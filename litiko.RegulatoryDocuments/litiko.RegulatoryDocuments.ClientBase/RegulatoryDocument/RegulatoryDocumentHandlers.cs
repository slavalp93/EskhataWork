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

    public virtual void TypeValueInput(Sungero.Presentation.EnumerationValueInputEventArgs e)
    {
      Functions.RegulatoryDocument.SetRequiredProperties(_obj);
    }

    public virtual void DateUpdateValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {      
      _obj.State.Controls.DaysUntilUpdaate.Refresh();
    }
  }

}