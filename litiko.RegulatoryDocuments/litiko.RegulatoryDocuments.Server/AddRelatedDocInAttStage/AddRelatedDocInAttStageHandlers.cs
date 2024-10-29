using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.RegulatoryDocuments.AddRelatedDocInAttStage;

namespace litiko.RegulatoryDocuments
{
  partial class AddRelatedDocInAttStageDocumentKindPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> DocumentKindFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return query.Where(x => x.Status == Sungero.Docflow.DocumentKind.Status.Active);
    }
  }

  partial class AddRelatedDocInAttStageServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      _obj.CopyAccessRights = false;
    }
  }

}