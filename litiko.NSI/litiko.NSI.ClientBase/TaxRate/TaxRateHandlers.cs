using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.NSI.TaxRate;

namespace litiko.NSI
{
  partial class TaxRateClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {      
      Functions.TaxRate.SetRequiredProperties(_obj);
      Functions.TaxRate.SetVisibleProperties(_obj);
    }

  }
}