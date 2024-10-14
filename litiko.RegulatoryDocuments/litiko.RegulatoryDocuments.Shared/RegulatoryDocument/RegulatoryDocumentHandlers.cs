using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.RegulatoryDocuments.RegulatoryDocument;

namespace litiko.RegulatoryDocuments
{
  partial class RegulatoryDocumentSharedHandlers
  {

    public virtual void DateBeginChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      var dateBegin = e.NewValue;
      if (dateBegin.HasValue && _obj.DocumentKind != null)
      {
        var deadLine = litiko.RegulatoryDocuments.DeadlineForRevisions.GetAll(x => x.Status == litiko.RegulatoryDocuments.DeadlineForRevision.Status.Active &&
                                                                 Equals(x.DocumentKind, _obj.DocumentKind) &&
                                                                x.Deadline.HasValue)
          .FirstOrDefault();        
          
        if (deadLine != null)
        {
          var newRevisionDaate = Calendar.GetDate(dateBegin.Value.Year + deadLine.Deadline.Value, dateBegin.Value.Month, dateBegin.Value.Day);
          if (_obj.DateRevision != newRevisionDaate)
            _obj.DateRevision = newRevisionDaate;
        }      
      }
      else
        _obj.DateRevision = null;
    }

  }
}