using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.OutgoingDocumentBase;

namespace litiko.Eskhata.Client
{
  partial class OutgoingDocumentBaseCollectionActions
  {
    public override void PrintEnvelope(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Functions.OutgoingDocumentBase.ShowSelectEnvelopeFormatDialog(_objs.ToList(), null, null);
    }

    public override bool CanPrintEnvelope(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanPrintEnvelope(e);
    }

  }

}