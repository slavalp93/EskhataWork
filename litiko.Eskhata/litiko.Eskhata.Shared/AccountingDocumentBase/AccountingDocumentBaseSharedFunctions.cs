using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.AccountingDocumentBase;

namespace litiko.Eskhata.Shared
{
  partial class AccountingDocumentBaseFunctions
  {

    /// <summary>
    /// Установить обязательность свойств в зависимости от заполненных данных.
    /// </summary>
    public override void SetRequiredProperties()
    {
      base.SetRequiredProperties();
      
      _obj.State.Properties.TotalAmount.IsRequired = false;
      _obj.State.Properties.TotalAmountlitiko.IsRequired = true;
    } 
    
    /// <summary>
    /// Заполнить курс валюты.
    /// </summary>
    /// <param name="currency">Валюта.</param>
    /// <param name="date">Дата.</param>
    public void FillCurrencyRate(Sungero.Commons.ICurrency currency, DateTime? date)
    {            
      if (currency == null || date == null)
      {
        _obj.CurrencyRatelitiko = null;
        return;
      }
      
      var currencyRate = NSI.CurrencyRates.GetAll()
        .Where(x => Equals(x.Currency, currency) && Equals(x.Date, date))
        .FirstOrDefault();
      
      if (!Equals(_obj.CurrencyRatelitiko, currencyRate))
        _obj.CurrencyRatelitiko = currencyRate;
    }
    
    /// <summary>
    /// Заполнить сумму НДС.
    /// </summary>
    /// <param name="totalAmount">Сумма.</param>
    /// <param name="vatRate">Ставка НДС.</param>    
    public void FillVatAmount(double? totalAmount, double? vatRate)
    {      
      if (totalAmount == null || vatRate == null)
      {
        _obj.VatAmount = null;
        return;
      }      
      
      if (totalAmount.HasValue && vatRate.HasValue)
      {
        var rateValue = Math.Round((double)vatRate.Value / 100, 2);
        _obj.VatAmount = Math.Round(totalAmount.Value * rateValue / (1 + rateValue), 2);      
      }      
    }    
    
    /// <summary>
    /// Заполнить сумму налога на доходы.
    /// </summary>
    /// <param name="amount">Сумма в нац. валюте</param>
    /// <param name="incomeTaxRate">Ставка налога на доходы.</param>
    public void FillIncomeTaxAmount(double? amount, double? incomeTaxRate)
    {            
      if (amount == null || incomeTaxRate == null)
      {
        _obj.IncomeTaxAmountlitiko = null;
        return;
      }
            
      var rateValue = Math.Round(incomeTaxRate.Value / 100, 2);
      var calculatedAmount = Math.Round(amount.Value * rateValue, 2);
      
      if (!Equals(_obj.IncomeTaxAmountlitiko, calculatedAmount))
        _obj.IncomeTaxAmountlitiko = calculatedAmount;
    }
    
    /// <summary>
    /// Заполнить сумму ФСЗН.
    /// </summary>
    /// <param name="amount">Сумма в нац. валюте</param>
    /// <param name="fsznTaxRate">Ставка ФСЗН.</param>        
    public void FillFSZNAmount(double? amount, double? fsznTaxRate)
    {            
      if (amount == null || fsznTaxRate == null)
      {
        _obj.FSZNAmountlitiko = null;
        return;
      }
            
      var rateValue = Math.Round(fsznTaxRate.Value / 100, 2);
      var calculatedAmount = Math.Round(amount.Value * rateValue, 2);
      
      if (!Equals(_obj.FSZNAmountlitiko, calculatedAmount))
        _obj.FSZNAmountlitiko = calculatedAmount;
    }
    
    /// <summary>
    /// Заполнить сумму пенсионного взноса.
    /// </summary>
    /// <param name="amount">Сумма в нац. валюте</param>
    /// <param name="pennyTaxRate">Ставка пенс. взноса</param>    
    public void FillPennyAmount(double? amount, double? pennyTaxRate)
    {            
      if (amount == null || pennyTaxRate == null)
      {
        _obj.PennyAmountlitiko = null;
        return;
      }
            
      var rateValue = Math.Round(pennyTaxRate.Value / 100, 2);
      var calculatedAmount = Math.Round(amount.Value * rateValue, 2);
      
      if (!Equals(_obj.PennyAmountlitiko, calculatedAmount))
        _obj.PennyAmountlitiko = calculatedAmount;
    }
    
    /// <summary>
    /// Заполнить сумму в нац. валюте.
    /// </summary>
    /// <param name="amount">Общая сумма.</param>
    /// <param name="currencyRate">Курс валюты.</param>
    /// <param name="currency">Валюта.</param>
    public void FillTotalAmount(double? amount, litiko.NSI.ICurrencyRate currencyRate, Sungero.Commons.ICurrency currency)
    {                  
      if (amount == null)
      {
        _obj.TotalAmount = null;
        return;
      }
      
      double? calculatedAmount;
      var defaultCurrency = Sungero.Commons.PublicFunctions.Currency.Remote.GetDefaultCurrency();
      if (Equals(defaultCurrency, currency))
        calculatedAmount = amount;
      else
      {
        if (currencyRate == null)
        {
          _obj.TotalAmount = null;
          return;
        }
        
        int UnitOfMeasurement = currencyRate?.Currency?.UnitOfMeasurementlitiko ?? 1;
        calculatedAmount = Math.Round(amount.Value * (double)currencyRate?.Rate / UnitOfMeasurement, 2);      
      }            
      
      if (!Equals(_obj.TotalAmount, calculatedAmount))
        _obj.TotalAmount = calculatedAmount;
    }    
  }
}