using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.NSI.ResponsibilityMatrix;

namespace litiko.NSI
{
  partial class ResponsibilityMatrixSharedHandlers
  {

    public virtual void DocumentKindChanged(litiko.NSI.Shared.ResponsibilityMatrixDocumentKindChangedEventArgs e)
    {
      if (e.NewValue != null && ! Equals(e.NewValue, e.OldValue))
        _obj.ContractCategories.Clear();
    }

  }
}