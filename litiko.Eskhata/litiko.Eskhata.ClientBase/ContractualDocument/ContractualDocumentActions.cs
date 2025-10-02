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
    public override void ExportToABSlitiko(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.ExportToABSlitiko(e);
    }

    public override bool CanExportToABSlitiko(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Доступно роли «Администраторы» и «Ответственные за синхронизацию с учетными системами»
      return Users.Current.IncludedIn(Roles.Administrators) || Users.Current.IncludedIn(Integration.PublicConstants.Module.SynchronizationResponsibleRoleGuid);
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
      
      base.SendForApproval(e);
    }

    public override bool CanSendForApproval(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanSendForApproval(e);
    }

  }

}