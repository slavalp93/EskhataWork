using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Addendum;

namespace litiko.Eskhata
{
  partial class AddendumClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);
            
      bool isSubjectrequired = _obj.State.Properties.Subject.IsRequired;
      var docKindExtractProtocol = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(litiko.CollegiateAgencies.PublicConstants.Module.DocumentKindGuids.ExtractProtocol);
      var docKindResolution = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(litiko.CollegiateAgencies.PublicConstants.Module.DocumentKindGuids.Resolution);                  
      if ((docKindExtractProtocol != null && Equals(_obj.DocumentKind, docKindExtractProtocol)) || (docKindResolution != null && Equals(_obj.DocumentKind, docKindResolution)))
        isSubjectrequired = false;
      
      _obj.State.Properties.Subject.IsRequired = isSubjectrequired;
    }
  }

}