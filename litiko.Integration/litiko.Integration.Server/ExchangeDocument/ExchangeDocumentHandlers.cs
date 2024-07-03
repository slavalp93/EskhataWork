using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Integration.ExchangeDocument;

namespace litiko.Integration
{
  partial class ExchangeDocumentServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.StatusRequestToIS = ExchangeDocument.StatusRequestToIS.Created;
      _obj.RequestToRXPacketCount = 0;
    }
  }

}