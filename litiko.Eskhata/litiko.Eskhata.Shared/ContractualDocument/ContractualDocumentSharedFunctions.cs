using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ContractualDocument;
using Sungero.Domain.Shared;

namespace litiko.Eskhata.Shared
{
  partial class ContractualDocumentFunctions
  {
    /// <summary>
    /// Обновить карточку документа.
    /// </summary>
    public override void RefreshDocumentForm()
    {
      base.RefreshDocumentForm();                  
      
      _obj.State.Properties.VatAmount.IsEnabled = false;
      
      // TODO убрать из обновления формы
      _obj.State.Controls.CounterpartryInfolitiko.Refresh();
    }

    /// <summary>
    /// Заполнить сумму НДС.
    /// </summary>
    /// <param name="totalAmount">Сумма.</param>
    /// <param name="vatRate">Ставка НДС.</param>
    /// <param name="vatRate">Облагается НДС.</param>    
    public void FillVatAmount(double? totalAmount, double? vatRate, bool? IsVAT)
    {      
      if (totalAmount == null || vatRate == null || !IsVAT.GetValueOrDefault())
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
    /// Заполнить ссылку на Ставки налогов.
    /// </summary>
    /// <param name="DocumentKind">Вид договора.</param>
    /// <param name="category">Тип договора (категория).</param>    
    /// <param name="counterparty">Контрагент.</param>
    [Public]
    public void FillTaxRate(IDocumentKind DocumentKind, Sungero.Contracts.IContractCategory category, Sungero.Parties.ICounterparty counterparty)
    {      
      if (DocumentKind == null || counterparty == null)
      {
        _obj.TaxRatelitiko = null;
        return;
      }      
      
      Sungero.Core.Enumeration? counterpartyType;
      if (Companies.Is(counterparty))
        counterpartyType = NSI.TaxRate.CounterpartyType.Company;
      else if (People.Is(counterparty))
        counterpartyType = NSI.TaxRate.CounterpartyType.Person;
      else if (Banks.Is(counterparty))
        counterpartyType = NSI.TaxRate.CounterpartyType.Bank;
      else
        counterpartyType = null;      
            
      var taxRate = NSI.TaxRates.Null;            
      bool? isNonResident = litiko.Eskhata.Counterparties.As(counterparty)?.NUNonrezidentlitiko;
      
      // Базовый запрос
      var query = NSI.TaxRates.GetAll()
        .Where(x => Equals(x.DocumentKind, DocumentKind))
        .Where(x => Equals(x.CounterpartyType, counterpartyType));
      
      // Фильтруем по признаку нерезидент (только если он не null)
      if (isNonResident.HasValue)
        query = query.Where(x => Equals(x.TaxResident, isNonResident));
      
      // Фильтруем по категории
      if (category != null)
        query = query.Where(x => Equals(x.Category, category));
      
      taxRate = query.FirstOrDefault();      
      
      if (!Equals(_obj.TaxRatelitiko, taxRate))
        _obj.TaxRatelitiko = taxRate;
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
    /// <param name="isIndividualPayment">Оплата труда физ лиц.</param> 
    public void FillFSZNAmount(double? amount, double? fsznTaxRate, bool? isIndividualPayment)
    {            
      if (amount == null || fsznTaxRate == null || !isIndividualPayment.GetValueOrDefault())
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
    /// <param name="isIndividualPayment">Оплата труда физ лиц.</param> 
    public void FillPennyAmount(double? amount, double? pennyTaxRate, bool? isIndividualPayment)
    {            
      if (amount == null || pennyTaxRate == null || !isIndividualPayment.GetValueOrDefault())
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
    /// Заполнить ссылку на матрицу ответственности.
    /// </summary>
    [Public]
    public void FillResponsibilityMatrix()
    {            
      var contract = Contracts.Null;
      var matrix = NSI.ResponsibilityMatrices.Null;
      
      if (Contracts.Is(_obj))
        contract = Contracts.As(_obj);
      else if (SupAgreements.Is(_obj) && _obj.LeadingDocument != null)
        contract = Contracts.As(_obj.LeadingDocument);
      
      if (contract != null)
        matrix = NSI.PublicFunctions.Module.GetResponsibilityMatrix(contract);
      
      if (!Equals(_obj.ResponsibilityMatrixlitiko, matrix))
        _obj.ResponsibilityMatrixlitiko = matrix;
    }
    
  }
}