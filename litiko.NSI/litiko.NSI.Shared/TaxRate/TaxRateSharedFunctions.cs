using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.NSI.TaxRate;

namespace litiko.NSI.Shared
{
  partial class TaxRateFunctions
  {
    /// <summary>
    /// Установить обязательность свойств в зависимости от заполненных данных.
    /// </summary>
    public void SetRequiredProperties()
    {
      _obj.State.Properties.VATMethod.IsRequired = _obj.VAT.GetValueOrDefault() > 0;
      _obj.State.Properties.IncomeTaxMethod.IsRequired = _obj.IncomeTax.GetValueOrDefault() > 0;
      _obj.State.Properties.IncomeTaxLimit.IsRequired = _obj.IncomeTaxMethod == NSI.TaxRate.IncomeTaxMethod.FromAmount2;
      _obj.State.Properties.PensionContributionMethod.IsRequired = _obj.PensionContribution.GetValueOrDefault() > 0;
      _obj.State.Properties.FSZNMethod.IsRequired = _obj.FSZN.GetValueOrDefault() > 0;
    }
    
    /// <summary>
    /// Установить видимость свойств в зависимости от заполненных данных.
    /// </summary>
    public void SetVisibleProperties()
    {
      _obj.State.Properties.IncomeTaxLimit.IsVisible = _obj.State.Properties.IncomeTaxLimit.IsRequired;
    }    
  }
}