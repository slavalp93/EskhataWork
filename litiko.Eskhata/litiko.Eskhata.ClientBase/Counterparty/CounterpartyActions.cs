using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Counterparty;

namespace litiko.Eskhata.Client
{
  partial class CounterpartyActions
  {
    public virtual void FillFromABSlitiko(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      #region Предпроверки      
      // "Ответственные за синхронизацию с учетными системами"
      // "Ответственные за контрагентов"
      // "Администраторы"
      var canExecute = Users.Current.IncludedIn(Integration.PublicConstants.Module.SynchronizationResponsibleRoleGuid) || Users.Current.IncludedIn(Roles.Administrators) || Users.Current.IncludedIn(Sungero.Docflow.PublicConstants.Module.RoleGuid.CounterpartiesResponsibleRole);
      if (!canExecute)
      {
        e.AddError(Integration.Resources.AvailableActionMessage);
        return;
      }
      
      var company = Companies.As(_obj);
      var bank = Banks.As(_obj);
      var person = People.As(_obj);
      
      if ((company != null || bank != null) && string.IsNullOrEmpty(_obj.TIN))
      {
        e.AddError(Companies.Resources.ErrorNeedFillTin);
        return;
      }
      
      if (bank != null && string.IsNullOrEmpty(bank.BIC))
      {
        e.AddError(Banks.Resources.ErrorNeedFillBIC);
        return;
      }      
      
      var integrationMethodName = string.Empty;
      if (company != null)
        integrationMethodName = "R_DR_GET_COMPANY";
      else if (bank != null)
        integrationMethodName = "R_DR_GET_BANK";
      else if (person != null)
        integrationMethodName = "R_DR_GET_PERSON";
              
      var integrationMethod = Integration.IntegrationMethods.GetAll().Where(x => x.Name == integrationMethodName).FirstOrDefault();
      if (integrationMethod == null)
      {
        e.AddError(litiko.Integration.Resources.IntegrationMethodNotFoundFormat(integrationMethodName));
        return;        
      }       
      #endregion
      
      var exchDoc = Integration.PublicFunctions.Module.Remote.CreateExchangeDocument();
      exchDoc.IntegrationMethod = integrationMethod;
      exchDoc.Counterparty = _obj;
      exchDoc.Save();
      var exchDocId = exchDoc.Id;      
      
      var errorMessage =  Integration.PublicFunctions.Module.Remote.SendRequestToIS(integrationMethod, exchDoc, 0);      
      if (!string.IsNullOrEmpty(errorMessage))
      {
        e.AddError(errorMessage);
        return;
      }      

      bool successed = Integration.PublicFunctions.Module.Remote.WaitForGettingDataFromIS(exchDoc, 1000, 10);
      if (successed)
      {                
        var errorList = new List<string>();
        if (company != null)
          errorList = litiko.Integration.PublicFunctions.Module.Remote.R_DR_GET_COMPANY(exchDocId, _obj);
        else if (bank != null)
          errorList = litiko.Integration.PublicFunctions.Module.Remote.R_DR_GET_BANK(exchDocId, _obj);
        else if (person != null)
          //errorList = litiko.Integration.PublicFunctions.Module.Remote.R_DR_GET_PERSON(exchDocId, _obj);
        
        if (errorList.Any())
          e.AddInformation(errorList.LastOrDefault());
        else
          e.AddInformation(litiko.Integration.Resources.DataUpdatedSuccessfully);        
      }
      else
        e.AddInformation(litiko.Integration.Resources.ResponseNotReceived);      
    }

    public virtual bool CanFillFromABSlitiko(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.AccessRights.CanUpdate();
    }

    public virtual void SendToVerificationlitiko(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var documents = Sungero.Docflow.PublicFunctions.Module.Remote.GetCounterpartyDocuments(_obj);
      if (!documents.Any())
      {
        Dialogs.ShowMessage(Counterparties.Resources.InformationAboutCounterpartyIsRequired, MessageType.Warning);
        throw new OperationCanceledException();
      }
      
      var lastDocument = documents.OrderByDescending(d => d.Created).FirstOrDefault();
      // Если по документу ранее были запущены задачи, то вывести соответствующий диалог.
      if (!Sungero.Docflow.PublicFunctions.OfficialDocument.NeedCreateApprovalTask(lastDocument))
        return;
      
      var availableApprovalRules = Sungero.Docflow.PublicFunctions.OfficialDocument.Remote.GetApprovalRules(lastDocument).ToList();
      if (availableApprovalRules.Any())
      {
        var approvalTask = Sungero.Docflow.PublicFunctions.Module.Remote.CreateApprovalTask(lastDocument);
        if (!approvalTask.OtherGroup.All.Contains(_obj))
          approvalTask.OtherGroup.All.Add(_obj);
        
        approvalTask.Show();
        e.CloseFormAfterAction = true;
      }
      else
      {
        // Если по документу нет регламента, вывести сообщение.
        Dialogs.ShowMessage(OfficialDocuments.Resources.NoApprovalRuleWarning, MessageType.Warning);
        throw new OperationCanceledException();
      }      
    }

    public virtual bool CanSendToVerificationlitiko(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }


}