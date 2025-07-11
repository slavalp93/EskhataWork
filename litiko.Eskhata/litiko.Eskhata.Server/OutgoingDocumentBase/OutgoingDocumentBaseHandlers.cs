using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.OutgoingDocumentBase;

namespace litiko.Eskhata
{
  partial class OutgoingDocumentBaseServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.StandardResponse = false;
    }
  }

}