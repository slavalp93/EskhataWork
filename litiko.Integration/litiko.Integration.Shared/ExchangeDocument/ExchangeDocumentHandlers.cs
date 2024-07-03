using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Integration.ExchangeDocument;

namespace litiko.Integration
{
  partial class ExchangeDocumentSharedHandlers
  {

    public virtual void IntegrationMethodChanged(litiko.Integration.Shared.ExchangeDocumentIntegrationMethodChangedEventArgs e)
    {
      if (e.NewValue != null)
        _obj.IntegrationSystem = e.NewValue.IntegrationSystem;
      
      Functions.ExchangeDocument.FillName(_obj);
    }

    public virtual void IntegrationSystemChanged(litiko.Integration.Shared.ExchangeDocumentIntegrationSystemChangedEventArgs e)
    {
      Functions.ExchangeDocument.FillName(_obj);
    }

  }
}