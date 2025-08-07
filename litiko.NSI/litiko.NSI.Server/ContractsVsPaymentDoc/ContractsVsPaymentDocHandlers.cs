using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.NSI.ContractsVsPaymentDoc;

namespace litiko.NSI
{
  partial class ContractsVsPaymentDocServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      #region Оплата на основании
      _obj.PBIsPaymentContract = false;
      _obj.PBIsPaymentAct = false;
      _obj.PBIsPaymentInvoice = false;
      _obj.PBIsPaymentTaxInvoice = false;
      _obj.PBIsPaymentOrder = false;      
      #endregion
      
      #region Закрытие платежа на основании
      _obj.PCBIsPaymentContract = false;
      _obj.PCBIsPaymentAct = false;
      _obj.PCBIsPaymentInvoice = false;
      _obj.PCBIsPaymentTaxInvoice = false;
      _obj.PCBIsPaymentWaybill = false;
      _obj.PCBIsPaymentInsurance = false;
      #endregion
    }
  }

  partial class ContractsVsPaymentDocCategoryPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> CategoryFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {      
      return query.Where(x => x.DocumentKinds.Any(k => Equals(k.DocumentKind, _obj.DocumentKind)));
    }
  }

  partial class ContractsVsPaymentDocDocumentKindPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> DocumentKindFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      var availableDocumentKinds = Sungero.Contracts.PublicFunctions.ContractCategory.GetAllowedDocumentKinds();      
      return query.Where(a => availableDocumentKinds.Contains(a));
    }
  }

}