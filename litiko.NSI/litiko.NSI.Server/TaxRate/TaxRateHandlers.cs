using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.NSI.TaxRate;

namespace litiko.NSI
{
  partial class TaxRateServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.TaxResident = false;
    }
  }

  partial class TaxRateCategoryPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> CategoryFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return query.Where(x => x.DocumentKinds.Any(k => Equals(k.DocumentKind, _obj.DocumentKind)));
    }
  }

  partial class TaxRateDocumentKindPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> DocumentKindFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      var availableDocumentKinds = Sungero.Contracts.PublicFunctions.ContractCategory.GetAllowedDocumentKinds();      
      return query.Where(a => availableDocumentKinds.Contains(a));
    }
  }

}