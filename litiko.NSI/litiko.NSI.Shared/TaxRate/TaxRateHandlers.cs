using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.NSI.TaxRate;

namespace litiko.NSI
{
  partial class TaxRateSharedHandlers
  {

    public virtual void PensionContributionChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      Functions.TaxRate.SetRequiredProperties(_obj);
    }

    public virtual void FSZNChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      Functions.TaxRate.SetRequiredProperties(_obj);
    }

    public virtual void IncomeTaxMethodChanged(Sungero.Domain.Shared.EnumerationPropertyChangedEventArgs e)
    {
      if (e.NewValue != NSI.TaxRate.IncomeTaxMethod.FromAmount2)
        _obj.IncomeTaxLimit = null;
        
      Functions.TaxRate.SetVisibleProperties(_obj);
    }

    public virtual void IncomeTaxChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      Functions.TaxRate.SetRequiredProperties(_obj);
      Functions.TaxRate.SetVisibleProperties(_obj);
    }

    public virtual void VATChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      Functions.TaxRate.SetRequiredProperties(_obj);
    }
  }

}