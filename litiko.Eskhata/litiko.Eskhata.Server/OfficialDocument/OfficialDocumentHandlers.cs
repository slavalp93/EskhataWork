using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.OfficialDocument;

namespace litiko.Eskhata
{
  partial class OfficialDocumentCreatingFromServerHandler
  {

    public override void CreatingFrom(Sungero.Domain.CreatingFromEventArgs e)
    {
      base.CreatingFrom(e);
      
      e.Without(_info.Properties.IntegrationStatuslitiko);
    }
  }

  partial class OfficialDocumentServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      base.BeforeSave(e);
      
      if (_obj.State.Properties.Archivelitiko.IsChanged && _obj.Archivelitiko != null && _obj.TransferredToArchivelitiko.HasValue)
      {
        var newRecord = _obj.Tracking.AddNew();
        newRecord.Action = litiko.Eskhata.OfficialDocumentTracking.Action.Archivinglitiko;
        newRecord.DeliveredTo = _obj.Archivelitiko.Archivist;
        newRecord.IsOriginal = true;
        newRecord.DeliveryDate = _obj.TransferredToArchivelitiko;
        newRecord.ReturnDeadline = null;
        newRecord.ReturnDate = null;

        _obj.LocationState = litiko.Archive.Resources.DocInArchiveLocationStateFormat(_obj.Archivelitiko.Name);        
      }
    }
  }

}