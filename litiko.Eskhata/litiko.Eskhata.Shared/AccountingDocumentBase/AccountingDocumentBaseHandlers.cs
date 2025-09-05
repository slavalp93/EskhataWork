using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.AccountingDocumentBase;

namespace litiko.Eskhata
{
  partial class AccountingDocumentBaseSharedHandlers
  {

    public virtual void PennyRatelitikoChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      Functions.AccountingDocumentBase.FillPennyAmount(_obj, _obj.TotalAmount, e.NewValue);
    }

    public virtual void FSZNRatelitikoChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      Functions.AccountingDocumentBase.FillFSZNAmount(_obj, _obj.TotalAmount, e.NewValue);
    }

    public virtual void IncomeTaxRatelitikoChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      Functions.AccountingDocumentBase.FillIncomeTaxAmount(_obj, _obj.TotalAmount, e.NewValue);
    }

    public virtual void TaxRatelitikoChanged(litiko.Eskhata.Shared.AccountingDocumentBaseTaxRatelitikoChangedEventArgs e)
    {
      _obj.VatRatelitiko = e.NewValue?.VAT;
      _obj.IncomeTaxRatelitiko = e.NewValue?.IncomeTax;
      _obj.FSZNRatelitiko = e.NewValue?.FSZN;
      _obj.PennyRatelitiko = e.NewValue?.PensionContribution;
    }

    public override void TotalAmountChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      base.TotalAmountChanged(e);
      
      Functions.AccountingDocumentBase.FillVatAmount(_obj, e.NewValue, _obj.VatRatelitiko);
      Sungero.Docflow.PublicFunctions.AccountingDocumentBase.FillNetAmount(_obj, e.NewValue, _obj.VatAmount);
      Functions.AccountingDocumentBase.FillIncomeTaxAmount(_obj, e.NewValue, _obj.IncomeTaxRatelitiko);
      Functions.AccountingDocumentBase.FillFSZNAmount(_obj, e.NewValue, _obj.FSZNRatelitiko);
      Functions.AccountingDocumentBase.FillPennyAmount(_obj, e.NewValue, _obj.PennyRatelitiko);
    }

    public virtual void VatRatelitikoChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      _obj.VatRate = null;
      Functions.AccountingDocumentBase.FillVatAmount(_obj, _obj.TotalAmount, e.NewValue);
    }

    public override void RegistrationDateChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      base.RegistrationDateChanged(e);
      
      Functions.AccountingDocumentBase.FillCurrencyRate(_obj, _obj.Currency, e.NewValue);
    }

    public override void CurrencyChanged(Sungero.Docflow.Shared.AccountingDocumentBaseCurrencyChangedEventArgs e)
    {
      base.CurrencyChanged(e);
      
      Functions.AccountingDocumentBase.FillCurrencyRate(_obj, e.NewValue, _obj.RegistrationDate);
    }

    public override void LeadingDocumentChanged(Sungero.Docflow.Shared.OfficialDocumentLeadingDocumentChangedEventArgs e)
    {
      base.LeadingDocumentChanged(e);
      
      var contract = Contracts.As(e.NewValue);
      _obj.Counterparty = contract?.Counterparty;
      _obj.CurrencyOperationlitiko = contract?.CurrencyOperationlitiko;
      
      _obj.TaxRatelitiko = contract?.TaxRatelitiko;
      
      if (Sungero.Contracts.IncomingInvoices.Is(_obj) && contract != null)
      {
        var matrix = NSI.PublicFunctions.Module.GetResponsibilityMatrix(contract);
        _obj.OurSignatory = Sungero.Company.Employees.As(matrix?.ResponsibleAccountant);
      }
      
      if (Sungero.FinancialArchive.ContractStatements.Is(_obj))      
        _obj.OurSignatory = contract?.ResponsibleEmployee;
    }

  }
}