using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.RegulatoryDocuments
{
  partial class ActOnTheRelevanceServerHandlers
  {

    public virtual IQueryable<litiko.RegulatoryDocuments.IRegulatoryDocument> GetDoc()
    {
      return litiko.RegulatoryDocuments.RegulatoryDocuments.GetAll(x => x.Id == ActOnTheRelevance.Entity.Id);
    }

  }
}