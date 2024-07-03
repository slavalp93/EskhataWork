using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Bank;

namespace litiko.Eskhata.Client
{
  partial class BankActions
  {
    public virtual void FillFromABSlitiko(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      // "Ответственные за синхронизацию с учетными системами"
      // "Ответственные за контрагентов"
      // "Администраторы"
      var canExecute = Users.Current.IncludedIn(Integration.PublicConstants.Module.SynchronizationResponsibleRoleGuid) || Users.Current.IncludedIn(Roles.Administrators) || Users.Current.IncludedIn(Sungero.Docflow.PublicConstants.Module.RoleGuid.CounterpartiesResponsibleRole);
      if (!canExecute)
      {
        e.AddError(Integration.Resources.AvailableActionMessage);
        return;
      }       
      
      if (string.IsNullOrWhiteSpace(_obj.BIC))
      {
        e.AddError(Banks.Resources.ErrorNeedFillBIC);
        return;
      }                             
      
      var integrationMethodName = "R_DR_GET_BANK";            
      var integrationMethod = Integration.IntegrationMethods.GetAll().Where(x => x.Name == integrationMethodName).FirstOrDefault();
      if (integrationMethod == null)
      {
        e.AddError(litiko.Integration.Resources.IntegrationMethodNotFoundFormat(integrationMethodName));
        return;        
      }       
            
      var exchDoc = Integration.PublicFunctions.Module.Remote.CreateExchangeDocument();
      exchDoc.IntegrationMethod = integrationMethod;
      exchDoc.Counterparty = _obj;
      exchDoc.Save();
      var exchDocId = exchDoc.Id;      
      
      var errorMessage =  Integration.PublicFunctions.Module.Remote.SendRequestToIS(integrationMethod, exchDoc, 0);      
      if (!string.IsNullOrEmpty(errorMessage))
      {
        //e.AddError(errorMessage);
        //return;
      }      

      bool successed = Integration.PublicFunctions.Module.Remote.WaitForGettingDataFromIS(exchDoc, 1000, 10);
      if (successed)
      {                
        var errorList = litiko.Integration.PublicFunctions.Module.Remote.R_DR_GET_BANK(exchDocId, _obj);
        if (errorList.Any())
          e.AddInformation(errorList.LastOrDefault());
        else
          e.AddInformation(litiko.Integration.Resources.DataUpdatedSuccessfully);
                
        #region Подсветка измененных контролов     
        if (_obj.State.Properties.Name.IsChanged)
          _obj.State.Properties.Name.HighlightColor = Colors.Common.Green;
        if (_obj.State.Properties.LegalName.IsChanged)
          _obj.State.Properties.LegalName.HighlightColor = Colors.Common.Green;        
        if (_obj.State.Properties.Inamelitiko.IsChanged)
          _obj.State.Properties.Inamelitiko.HighlightColor = Colors.Common.Green;        
        if (_obj.State.Properties.TRRC.IsChanged)
          _obj.State.Properties.TRRC.HighlightColor = Colors.Common.Green;              
        if (_obj.State.Properties.SWIFT.IsChanged)
          _obj.State.Properties.SWIFT.HighlightColor = Colors.Common.Green; 
        if (_obj.State.Properties.TIN.IsChanged)
          _obj.State.Properties.TIN.HighlightColor = Colors.Common.Green;         
        if (_obj.State.Properties.CorrespondentAccount.IsChanged)
          _obj.State.Properties.CorrespondentAccount.HighlightColor = Colors.Common.Green;        
        if (_obj.State.Properties.Countrylitiko.IsChanged)
          _obj.State.Properties.Countrylitiko.HighlightColor = Colors.Common.Green;
        if (_obj.State.Properties.PostalAddress.IsChanged)
          _obj.State.Properties.PostalAddress.HighlightColor = Colors.Common.Green;  
        if (_obj.State.Properties.LegalAddress.IsChanged)
          _obj.State.Properties.LegalAddress.HighlightColor = Colors.Common.Green;  
        if (_obj.State.Properties.Phones.IsChanged)
          _obj.State.Properties.Phones.HighlightColor = Colors.Common.Green;  
        if (_obj.State.Properties.Email.IsChanged)
          _obj.State.Properties.Email.HighlightColor = Colors.Common.Green;          
        #endregion
      }
      else
        e.AddInformation(litiko.Integration.Resources.ResponseNotReceived);
    }

    public virtual bool CanFillFromABSlitiko(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
       return _obj.AccessRights.CanUpdate() && _obj.IsCardReadOnly != true;
    }

  }

}