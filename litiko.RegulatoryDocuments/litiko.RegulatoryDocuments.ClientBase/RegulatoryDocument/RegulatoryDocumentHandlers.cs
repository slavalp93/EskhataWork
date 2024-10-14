using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.RegulatoryDocuments.RegulatoryDocument;

namespace litiko.RegulatoryDocuments
{
  partial class RegulatoryDocumentClientHandlers
  {

    public virtual void DateUpdateValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {      
      _obj.State.Controls.DaysUntilUpdaate.Refresh();
    }
  }

}