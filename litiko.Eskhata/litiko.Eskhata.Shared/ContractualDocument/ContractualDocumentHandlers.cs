using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ContractualDocument;

namespace litiko.Eskhata
{
  partial class ContractualDocumentSharedHandlers
  {

    public override void DocumentKindChanged(Sungero.Docflow.Shared.OfficialDocumentDocumentKindChangedEventArgs e)
    {
      base.DocumentKindChanged(e);
      
      _obj.IsIndividualPaymentlitiko = People.Is(_obj.Counterparty) && e.NewValue?.Name != "Аренда";
    }

    public virtual void FSZNAmountlitikoChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      Functions.ContractualDocument.FillAmountOfExpenses(_obj, _obj.Counterparty, _obj.TotalAmount, _obj.VatAmount, e.NewValue);
    }

    public virtual void IncomeTaxAmountlitikoChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      Functions.ContractualDocument.FillAmountToBePaid(_obj, _obj.TotalAmount, e.NewValue);
    }

    public override void VatAmountChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      Sungero.Docflow.PublicFunctions.ContractualDocumentBase.FillNetAmount(_obj, _obj.TotalAmount, e.NewValue);
      Functions.ContractualDocument.FillAmountOfExpenses(_obj, _obj.Counterparty, _obj.TotalAmount, e.NewValue, _obj.FSZNAmountlitiko);
    }

    public virtual void CurrencyContractlitikoChanged(litiko.Eskhata.Shared.ContractualDocumentCurrencyContractlitikoChangedEventArgs e)
    {
      Functions.ContractualDocument.FillCurrencyRate(_obj, e.NewValue, Calendar.Today);
      Functions.ContractualDocument.FillTotalAmount(_obj, _obj.TotalAmountlitiko, _obj.CurrencyRatelitiko, e.NewValue);      
    }

    public virtual void IsVATlitikoChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      Functions.ContractualDocument.FillVatAmount(_obj, _obj.TotalAmount, _obj.TaxRatelitiko, e.NewValue);
    }

    public virtual void IsIndividualPaymentlitikoChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      Functions.ContractualDocument.FillFSZNAmount(_obj, _obj.TotalAmount, _obj.TaxRatelitiko, e.NewValue);
      Functions.ContractualDocument.FillPennyAmount(_obj, _obj.TotalAmount, _obj.TaxRatelitiko, e.NewValue);
    }

    public virtual void CurrencyRatelitikoChanged(litiko.Eskhata.Shared.ContractualDocumentCurrencyRatelitikoChangedEventArgs e)
    {
      Functions.ContractualDocument.FillTotalAmount(_obj, _obj.TotalAmountlitiko, e.NewValue, _obj.CurrencyContractlitiko);
    }

    public virtual void TotalAmountlitikoChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      // Подставить валюту по умолчанию.
      if (_obj.CurrencyContractlitiko == null)
      {
        var defaultCurrency = Sungero.Commons.PublicFunctions.Currency.Remote.GetDefaultCurrency();
        if (defaultCurrency != null)
          _obj.CurrencyContractlitiko = defaultCurrency;
      }
      
      Functions.ContractualDocument.FillTotalAmount(_obj, e.NewValue, _obj.CurrencyRatelitiko, _obj.CurrencyContractlitiko);
    }

    public override void ValidFromChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      base.ValidFromChanged(e);
      
      //Functions.ContractualDocument.FillCurrencyRate(_obj, _obj.CurrencyContractlitiko, e.NewValue);
    }

    public override void TotalAmountChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      //base.TotalAmountChanged(e);
      
      Functions.ContractualDocument.FillVatAmount(_obj, e.NewValue, _obj.TaxRatelitiko, _obj.IsVATlitiko);
      Sungero.Docflow.PublicFunctions.ContractualDocumentBase.FillNetAmount(_obj, e.NewValue, _obj.VatAmount);
      Functions.ContractualDocument.FillPennyAmount(_obj, e.NewValue, _obj.TaxRatelitiko, _obj.IsIndividualPaymentlitiko);
      Functions.ContractualDocument.FillIncomeTaxAmount(_obj, e.NewValue, _obj.TaxRatelitiko);      
      Functions.ContractualDocument.FillFSZNAmount(_obj, e.NewValue, _obj.TaxRatelitiko, _obj.IsIndividualPaymentlitiko);
      Functions.ContractualDocument.FillAmountToBePaid(_obj, e.NewValue, _obj.IncomeTaxAmountlitiko);
      Functions.ContractualDocument.FillAmountOfExpenses(_obj, _obj.Counterparty, e.NewValue, _obj.VatAmount, _obj.FSZNAmountlitiko);
    }

    public virtual void TaxRatelitikoChanged(litiko.Eskhata.Shared.ContractualDocumentTaxRatelitikoChangedEventArgs e)
    {
      Functions.ContractualDocument.FillVatAmount(_obj, _obj.TotalAmount, e.NewValue, _obj.IsVATlitiko);
      Functions.ContractualDocument.FillPennyAmount(_obj, _obj.TotalAmount, e.NewValue, _obj.IsIndividualPaymentlitiko);
      Functions.ContractualDocument.FillIncomeTaxAmount(_obj, _obj.TotalAmount, e.NewValue);
      Functions.ContractualDocument.FillFSZNAmount(_obj, _obj.TotalAmount, e.NewValue, _obj.IsIndividualPaymentlitiko);
      
      _obj.VatRatelitiko = e.NewValue?.VAT;
      _obj.IncomeTaxRatelitiko = e.NewValue?.IncomeTax;      
      _obj.FSZNRatelitiko = e.NewValue?.FSZN;
      _obj.PennyRatelitiko = e.NewValue?.PensionContribution;
    }

    public virtual void VatRatelitikoChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      // Очистить базавое свойство
      _obj.VatRate = null;            
    }

    public override void CounterpartyChanged(Sungero.Docflow.Shared.ContractualDocumentBaseCounterpartyChangedEventArgs e)
    {
      base.CounterpartyChanged(e);
      
      var counterprty = litiko.Eskhata.Counterparties.As(e.NewValue);
      
      _obj.IsVATlitiko = counterprty?.VATPayerlitiko;
      _obj.IsIndividualPaymentlitiko = People.Is(counterprty) && _obj.DocumentKind?.Name != "Аренда";
      Functions.ContractualDocument.FillAmountOfExpenses(_obj, e.NewValue, _obj.TotalAmount, _obj.VatAmount, _obj.FSZNAmountlitiko);
    }

    public override void LeadingDocumentChanged(Sungero.Docflow.Shared.OfficialDocumentLeadingDocumentChangedEventArgs e)
    {
      base.LeadingDocumentChanged(e);
      
      #region Для доп. соглашений            
      if (e.NewValue != null && Sungero.Contracts.SupAgreements.Is(_obj))
      {
        var contract = Contracts.As(e.NewValue);
        
        if (!Equals(contract.ResponsibleEmployee, _obj.ResponsibleEmployee))
          _obj.ResponsibleEmployee = contract.ResponsibleEmployee;
                
        if (!Equals(contract.CurrencyContractlitiko, _obj.CurrencyContractlitiko))
          _obj.CurrencyContractlitiko = contract.CurrencyContractlitiko;
        
        if (!Equals(contract.TaxRatelitiko, _obj.TaxRatelitiko))
          _obj.TaxRatelitiko = contract.TaxRatelitiko;
        
        if (!Equals(contract.CurrencyOperationlitiko, _obj.CurrencyOperationlitiko))
          _obj.CurrencyOperationlitiko = contract.CurrencyOperationlitiko;
        
        if (!Equals(contract.IsVATlitiko, _obj.IsVATlitiko))
          _obj.IsVATlitiko = contract.IsVATlitiko;
      }
      #endregion
    }
  }

}