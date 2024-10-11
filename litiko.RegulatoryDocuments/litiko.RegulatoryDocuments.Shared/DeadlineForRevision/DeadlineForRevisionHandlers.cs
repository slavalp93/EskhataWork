using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.RegulatoryDocuments.DeadlineForRevision;

namespace litiko.RegulatoryDocuments
{
  partial class DeadlineForRevisionSharedHandlers
  {

    public virtual void DeadlineChanged(Sungero.Domain.Shared.IntegerPropertyChangedEventArgs e)
    {
      Functions.DeadlineForRevision.FillName(_obj);
    }

    public virtual void DocumentKindChanged(litiko.RegulatoryDocuments.Shared.DeadlineForRevisionDocumentKindChangedEventArgs e)
    {
      Functions.DeadlineForRevision.FillName(_obj);
    }
  }

}