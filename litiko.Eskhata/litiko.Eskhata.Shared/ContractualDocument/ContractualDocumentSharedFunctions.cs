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
    /// Установить обязательность свойств в зависимости от заполненных данных.
    /// </summary>
    public override void SetRequiredProperties()
    {
      base.SetRequiredProperties();
      
      _obj.State.Properties.ValidTill.IsRequired = true;
      
      var defaultCurrency = Sungero.Commons.PublicFunctions.Currency.Remote.GetDefaultCurrency();
      _obj.State.Properties.CurrencyOperationlitiko.IsRequired = !Equals(_obj.CurrencyContractlitiko, defaultCurrency);
    } 

    /// <summary>
    /// Обновить карточку документа.
    /// </summary>
    public override void RefreshDocumentForm()
    {
      base.RefreshDocumentForm();                  
      
      _obj.State.Properties.VatAmount.IsEnabled = false;
      
      var canUpdate = _obj.AccessRights.CanUpdate();
      _obj.State.Properties.CaseFile.IsEnabled = canUpdate;
      _obj.State.Properties.PlacedToCaseFileDate.IsEnabled = canUpdate;
      _obj.State.Properties.StoredIn.IsEnabled = canUpdate;
      
      bool isCompany = Eskhata.Companies.Is(_obj.Counterparty);
      _obj.State.Properties.IsVATlitiko.IsVisible = isCompany;
      _obj.State.Properties.VatRatelitiko.IsVisible = isCompany;
      _obj.State.Properties.VatAmount.IsVisible = isCompany;
      
      _obj.State.Properties.TotalAmount.IsEnabled = false;
      
      // TODO убрать из обновления формы
      _obj.State.Controls.CounterpartryInfolitiko.Refresh();
    }   

    /// <summary>
    /// Заполнить сумму НДС.
    /// </summary>
    /// <param name="amount">Сумма в нац. валюте</param>
    /// <param name="taxRate">Ставка налогов.</param>
    /// <param name="IsVAT">Облагается НДС.</param>    
    public void FillVatAmount(double? amount, NSI.ITaxRate taxRate, bool? IsVAT)
    {      
      var rate = taxRate?.VAT;
      var method = taxRate?.VATMethod;      
      
      if (taxRate == null || !IsVAT.GetValueOrDefault() || amount.GetValueOrDefault() == 0 || rate.GetValueOrDefault() == 0 || method == null)
      {
        _obj.VatAmount = null;
        return;
      }      
      
      double? calculatedAmount;
      if (method.Value == NSI.TaxRate.VATMethod.Included)
      {
        // Включен в сумму договора
        calculatedAmount = Math.Round(
          amount.Value * (rate.Value / (100 + rate.Value)),
          2);
      }      
      else if (method.Value == NSI.TaxRate.VATMethod.OnTop)
      {
        // Начисляется сверх суммы договора
        calculatedAmount = Math.Round(
          amount.Value * (rate.Value / 100),
          2);
      }
      else if (method.Value == NSI.TaxRate.VATMethod.DuringCustoms)
      {
        // Оплачивается при проведении таможенных процедур
        calculatedAmount = null;
      }
      else
        calculatedAmount = null;
      
      if (!Equals(_obj.VatAmount, calculatedAmount))
        _obj.VatAmount = calculatedAmount;      
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
    public virtual void FillTotalAmount(double? amount, litiko.NSI.ICurrencyRate currencyRate, Sungero.Commons.ICurrency currency)
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
    /// <param name="taxRate">Ставка налогов.</param>
    public void FillIncomeTaxAmount(double? amount, NSI.ITaxRate taxRate)
    {            
      var rate = taxRate?.IncomeTax;
      var method = taxRate?.IncomeTaxMethod;
      if (taxRate == null || amount.GetValueOrDefault() == 0 || rate.GetValueOrDefault() == 0 || method == null)
      {
        _obj.IncomeTaxAmountlitiko = null;
        return;
      }
      
      double? calculatedAmount;      
      if (method.Value == NSI.TaxRate.IncomeTaxMethod.FromAmount1)
      {
        // Удерживается от суммы договора
        calculatedAmount = Math.Round(
          amount.Value * (rate.Value / 100),
          2);
      }
      else if (method.Value == NSI.TaxRate.IncomeTaxMethod.FromAmount2)
      {
        // Удерживается от суммы договора с лимитом        
        var limit = taxRate.IncomeTaxLimit.GetValueOrDefault();
        if (amount.Value > limit)
          calculatedAmount = Math.Round(
            (amount.Value - limit) * (rate.Value / 100),
            2);
        else
          calculatedAmount = null;
      }
      else if (method.Value == NSI.TaxRate.IncomeTaxMethod.FromAmount3)
      {
        // Удерживается от суммы договора за вычетом пенс. взноса
        calculatedAmount = Math.Round(
          (amount.Value - _obj.PennyAmountlitiko.GetValueOrDefault()) * (rate.Value / 100),
          2);
      }
      else
        calculatedAmount = null;
      
      if (!Equals(_obj.IncomeTaxAmountlitiko, calculatedAmount))
        _obj.IncomeTaxAmountlitiko = calculatedAmount;
    }    
    
    /// <summary>
    /// Заполнить сумму ФСЗН.
    /// </summary>
    /// <param name="amount">Сумма в нац. валюте</param>
    /// <param name="taxRate">Ставка налогов.</param>
    /// <param name="isIndividualPayment">Оплата труда физ лиц.</param> 
    public void FillFSZNAmount(double? amount, NSI.ITaxRate taxRate, bool? isIndividualPayment)
    {            
      var rate = taxRate?.FSZN;
      var method = taxRate?.FSZNMethod;
      if (amount.GetValueOrDefault() == 0 || rate.GetValueOrDefault() == 0 || !isIndividualPayment.GetValueOrDefault() || method == null)
      {
        _obj.FSZNAmountlitiko = null;
        return;
      }
            
      double? calculatedAmount;
      if (method.Value == NSI.TaxRate.FSZNMethod.AccruedOnTop)
      {
        // Начисляется сверх суммы договора 
        calculatedAmount = Math.Round(
          amount.Value * (rate.Value / 100),
          2);
      }
      else
        calculatedAmount = null;      
      
      if (!Equals(_obj.FSZNAmountlitiko, calculatedAmount))
        _obj.FSZNAmountlitiko = calculatedAmount;
    }     
    
    /// <summary>
    /// Заполнить сумму пенсионного взноса.
    /// </summary>
    /// <param name="amount">Сумма в нац. валюте</param>
    /// <param name="taxRate">Ставка налогов.</param>
    /// <param name="isIndividualPayment">Оплата труда физ лиц.</param> 
    public void FillPennyAmount(double? amount, NSI.ITaxRate taxRate, bool? isIndividualPayment)
    {            
      var rate = taxRate?.PensionContribution;
      var method = taxRate?.PensionContributionMethod;
      if (amount.GetValueOrDefault() == 0 || rate.GetValueOrDefault() == 0 || !isIndividualPayment.GetValueOrDefault() || method == null)
      {
        _obj.PennyAmountlitiko = null;
        return;
      }
            
      double? calculatedAmount;
      if (method.Value == NSI.TaxRate.PensionContributionMethod.FromAmount)
      {
        // Удерживается от суммы договора
        calculatedAmount = Math.Round(
          amount.Value * (rate.Value / 100),
          2);
      }
      else
        calculatedAmount = null;      
      
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
    
    /// <summary>
    /// Заполнить сумму к оплате.
    /// </summary>
    /// <param name="totalAmount">Сумма в нац. валюте</param>
    /// <param name="incomeTaxAmount">Сумма налога на доходы.</param>    
    public void FillAmountToBePaid(double? totalAmount, double? incomeTaxAmount)
    {            
      double? calculatedAmount = Math.Round(
          totalAmount.GetValueOrDefault() - incomeTaxAmount.GetValueOrDefault(),
          2);                  
      
      if (!Equals(_obj.AmountToBePaidlitiko, calculatedAmount))
        _obj.AmountToBePaidlitiko = calculatedAmount;
    }
    
    /// <summary>
    /// Заполнить сумму затрат по договору.
    /// </summary>
    /// <param name="counterparty">Контрагент</param>
    /// <param name="totalAmount">Сумма в нац. валюте.</param>
    /// <param name="vatAmount">Сумма НДС</param>
    /// <param name="fsznAmount">Сумма ФСЗН</param>
    public void FillAmountOfExpenses(Sungero.Parties.ICounterparty counterparty, double? totalAmount, double? vatAmount, double? fsznAmount)
    {            
      var nunrezident = Eskhata.Counterparties.As(counterparty)?.NUNonrezidentlitiko;
      if (nunrezident == null)
      {
        _obj.AmountOfExpenseslitiko = null;
        return;
      }
      
      double? calculatedAmount;
      if (nunrezident.GetValueOrDefault())
      {
        calculatedAmount = Math.Round(
          totalAmount.GetValueOrDefault() + vatAmount.GetValueOrDefault() + fsznAmount.GetValueOrDefault(),
          2);      
      }
      else
      {
        calculatedAmount = Math.Round(
          totalAmount.GetValueOrDefault() + fsznAmount.GetValueOrDefault(),
          2);       
      }
      
      if (!Equals(_obj.AmountOfExpenseslitiko, calculatedAmount))
        _obj.AmountOfExpenseslitiko = calculatedAmount;      
    }    
    
  }
}