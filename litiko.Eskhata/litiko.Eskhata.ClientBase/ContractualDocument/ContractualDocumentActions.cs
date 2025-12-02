using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ContractualDocument;

namespace litiko.Eskhata.Client
{
  partial class ContractualDocumentActions
  {
    public override void CreateFromTemplate(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.CreateFromTemplate(e);
    }

    public override bool CanCreateFromTemplate(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanCreateFromTemplate(e) && _obj.IsStandard.GetValueOrDefault();
    }

    public override void ExportToABSlitiko(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.ExportToABSlitiko(e);
    }

    public override bool CanExportToABSlitiko(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Доступно роли «Администраторы» и «Ответственные за синхронизацию с учетными системами» и «Менеджеры модуля "Договоры"»
      return Users.Current.IncludedIn(Roles.Administrators) || 
        Users.Current.IncludedIn(Integration.PublicConstants.Module.RoleGuid.SynchronizationResponsibleRoleGuid) ||
        Users.Current.IncludedIn(ContractsEskhata.PublicConstants.Module.RoleGuid.ContractsManagers);
    }

    public virtual void CreateWaybilllitiko(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var waybill = litiko.Eskhata.Functions.ContractualDocument.Remote.CreateWaybill();      
      waybill.LeadingDocument = _obj;      
      waybill.Show();
    }

    public virtual bool CanCreateWaybilllitiko(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public override void SendForApproval(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (_obj.IsStandard.GetValueOrDefault() && string.IsNullOrEmpty(_obj.RegistrationNumber))
      {
        Dialogs.ShowMessage(litiko.Eskhata.ContractualDocuments.Resources.ReservtionNumberIsRequired, MessageType.Warning);
        throw new OperationCanceledException();
      }
      
      List<string> invalidProperties = Functions.ContractualDocument.Remote.CheckCounterpartyProperties(_obj, litiko.Eskhata.Counterparties.As(_obj.Counterparty));
      if (invalidProperties.Any())
      {        
        Dialogs.ShowMessage(Eskhata.ContractualDocuments.Resources.NeedToFillCounterpartyPropertiesFormat(string.Join(", ", invalidProperties)), MessageType.Warning);
        throw new OperationCanceledException();        
      }
      
      base.SendForApproval(e);
    }

    public override bool CanSendForApproval(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanSendForApproval(e);
    }

  }

}