using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.SimpleDocument;

namespace litiko.Eskhata
{
  partial class SimpleDocumentServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      base.BeforeSave(e);
      
      // Добавить в связи, если основной документ - Приказ о сдаче документов в архив
      if (_obj.LeadingDocument != null && _obj.LeadingDocument.AccessRights.CanRead() 
          && _obj.LeadingDocument.DocumentKind != null 
          && Equals(_obj.LeadingDocument.DocumentKind, Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Archive.PublicConstants.Module.DocumentKindGuids.OrderArchive))
          && !_obj.Relations.GetRelatedFrom("Basis").Contains(_obj.LeadingDocument))
        _obj.Relations.AddFromOrUpdate("Basis", _obj.State.Properties.LeadingDocument.OriginalValue, _obj.LeadingDocument);
    }
  }

}