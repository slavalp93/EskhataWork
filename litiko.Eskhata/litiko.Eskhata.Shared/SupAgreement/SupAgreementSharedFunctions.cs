using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.SupAgreement;

namespace litiko.Eskhata.Shared
{
  partial class SupAgreementFunctions
  {
    /// <summary>
    /// Заполнить сумму в нац. валюте.
    /// </summary>
    /// <param name="amount">Общая сумма.</param>
    /// <param name="currencyRate">Курс валюты.</param>
    /// <param name="currency">Валюта.</param>
    public override void FillTotalAmount(double? amount, litiko.NSI.ICurrencyRate currencyRate, Sungero.Commons.ICurrency currency)
    {                        
      if (_obj.AmountForPeriodlitiko > 0 && _obj.TotalAmountlitiko == 0)
        amount = _obj.AmountForPeriodlitiko;
      else
        amount = _obj.TotalAmountlitiko;
      
      base.FillTotalAmount(amount, _obj.CurrencyRatelitiko, _obj.CurrencyContractlitiko);
    } 
  }
}