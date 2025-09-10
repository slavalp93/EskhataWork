using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.Eskhata.Module.FinancialArchiveUI.Server
{
  partial class FinancialDocumentslitikoFolderHandlers
  {

    public virtual IQueryable<litiko.Eskhata.IAccountingDocumentBase> FinancialDocumentslitikoDataQuery(IQueryable<litiko.Eskhata.IAccountingDocumentBase> query)
    {      
      if (_filter == null)
        return query;
      
      #region Фильтр "Виды документов"
      if ((_filter.Actslitiko || _filter.Invoiceslitiko || _filter.TaxInvoiceslitiko || _filter.Waybillslitiko) &&
          !(_filter.Actslitiko && _filter.Invoiceslitiko && _filter.TaxInvoiceslitiko && _filter.Waybillslitiko))
      {
        var IncomingInvoiceKind     = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Sungero.Contracts.PublicConstants.Module.Initialize.IncomingInvoiceKind);
        var OutgoingInvoiceKind     = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Sungero.Contracts.PublicConstants.Module.Initialize.OutgoingInvoiceKind);
        var IncomingTaxInvoiceKind  = DocumentKinds.GetAll().Where(x => x.Status == Sungero.CoreEntities.DatabookEntry.Status.Active && x.Name == Constants.Module.IncomingTaxInvoiceKindName).FirstOrDefault();
        var OutgoingTaxInvoiceKind  = DocumentKinds.GetAll().Where(x => x.Status == Sungero.CoreEntities.DatabookEntry.Status.Active && x.Name == Constants.Module.OutgoingingTaxInvoiceKindName).FirstOrDefault();
        
        query = query.Where(x => (_filter.Actslitiko && Sungero.FinancialArchive.ContractStatements.Is(x)) ||
                                 (_filter.Invoiceslitiko && (Equals(x.DocumentKind, IncomingInvoiceKind) || Equals(x.DocumentKind, OutgoingInvoiceKind))) ||
                                 (_filter.TaxInvoiceslitiko && (Equals(x.DocumentKind, IncomingTaxInvoiceKind) || Equals(x.DocumentKind, OutgoingTaxInvoiceKind))) ||
                                 (_filter.Waybillslitiko && Sungero.FinancialArchive.Waybills.Is(x))
                           );      
      }      
        
      #endregion
      
      #region Фильтр "Подразделение"
      if (_filter.Departmentlitiko != null)
        query = query.Where(c => Equals(c.Department, _filter.Departmentlitiko));
      #endregion
      
      #region Фильтр "Контрагент"      
      if (_filter.Counterpartylitiko != null)
        query = query.Where(c => Equals(c.Counterparty, _filter.Counterpartylitiko));
      #endregion
      
      #region Фильтр "Период"
      var beginDate = Calendar.UserToday.BeginningOfMonth();
      var endDate = Calendar.UserToday.EndOfMonth();

      if (_filter.PreviousMonthlitiko)
      {
        beginDate = Calendar.UserToday.AddMonths(-1).BeginningOfMonth();
        endDate = Calendar.UserToday.AddMonths(-1).EndOfMonth();
      }
      if (_filter.CurrentQuarterlitiko)
      {
        beginDate = Sungero.Docflow.PublicFunctions.AccountingDocumentBase.BeginningOfQuarter(Calendar.UserToday);
        endDate = Sungero.Docflow.PublicFunctions.AccountingDocumentBase.EndOfQuarter(Calendar.UserToday);
      }
      if (_filter.PreviousQuarterlitiko)
      {
        beginDate = Sungero.Docflow.PublicFunctions.AccountingDocumentBase.BeginningOfQuarter(Calendar.UserToday.AddMonths(-3));
        endDate = Sungero.Docflow.PublicFunctions.AccountingDocumentBase.EndOfQuarter(Calendar.UserToday.AddMonths(-3));
      }

      if (_filter.ManualPeriodlitiko)
      {
        beginDate = _filter.DateRangelitikoFrom ?? Calendar.SqlMinValue;
        endDate = _filter.DateRangelitikoTo ?? Calendar.SqlMaxValue;
      }
      
      var serverPeriodBegin = Equals(Calendar.SqlMinValue, beginDate) ? beginDate : Sungero.Docflow.PublicFunctions.Module.Remote.GetTenantDateTimeFromUserDay(beginDate);
      var serverPeriodEnd = Equals(Calendar.SqlMaxValue, endDate) ? endDate : endDate.EndOfDay().FromUserTime();
      var clientPeriodEnd = !Equals(Calendar.SqlMaxValue, endDate) ? endDate.AddDays(1) : Calendar.SqlMaxValue;
      query = query.Where(j => (j.DocumentDate.Between(serverPeriodBegin, serverPeriodEnd) ||
                                        j.DocumentDate == beginDate) && j.DocumentDate != clientPeriodEnd);      
      #endregion
      
      return query;
    }
  }

  partial class FinancialArchiveUIHandlers
  {
  }
}