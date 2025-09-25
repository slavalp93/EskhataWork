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