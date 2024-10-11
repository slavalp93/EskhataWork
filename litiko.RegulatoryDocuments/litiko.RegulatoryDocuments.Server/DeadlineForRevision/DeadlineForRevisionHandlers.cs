using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.RegulatoryDocuments.DeadlineForRevision;

namespace litiko.RegulatoryDocuments
{
  partial class DeadlineForRevisionDocumentKindPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> DocumentKindFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return query.Where(x => x.DocumentType.DocumentTypeGuid == Constants.Module.DocumentTypeGuids.RegulatoryDocument.ToString());
    }
  }

}