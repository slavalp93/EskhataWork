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
      
      // "Ответственные за синхронизацию с учетными системами"
      // "Ответственные за контрагентов"
      // "Администраторы"
      var canExecute = Users.Current.IncludedIn(Integration.PublicConstants.Module.RoleGuid.SynchronizationResponsibleRoleGuid) || Users.Current.IncludedIn(Roles.Administrators) || Users.Current.IncludedIn(Sungero.Docflow.PublicConstants.Module.RoleGuid.CounterpartiesResponsibleRole);
      if (!canExecute)
      {
        e.AddError(Integration.Resources.AvailableActionMessage);
        return;
      }
      
      string errorMessage = litiko.Integration.PublicFunctions.Module.IntegrationClientAction(_obj);
      if (!string.IsNullOrEmpty(errorMessage))
      {
        e.AddError(errorMessage);
        return;
      }
      else
        e.AddInformation(litiko.Integration.Resources.DataUpdatedSuccessfully);      
      
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
        
        foreach (var doc in documents.Where(d => !Equals(d, lastDocument)))
        {
          if (!approvalTask.OtherGroup.All.Contains(doc))
            approvalTask.OtherGroup.All.Add(doc);          
        }
        
        // Проверка: <Контрагент>
        approvalTask.Subject = Eskhata.Counterparties.Resources.VerificationTaskSubjectFormat(_obj.Name);
        
        // Прошу проверить контрагента и дать заключение.
        approvalTask.ActiveText = Eskhata.Counterparties.Resources.VerificationTaskActiveText;
        
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