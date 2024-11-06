using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.RegulatoryDocuments.ConvertIRDtoPDFStage;

namespace litiko.RegulatoryDocuments
{
  partial class ConvertIRDtoPDFStageClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);
      
      e.AddInformation(litiko.RegulatoryDocuments.ConvertIRDtoPDFStages.Resources.InfoMessageFormat(
        litiko.RegulatoryDocuments.RegulatoryDocuments.Resources.VersionNoteOnRU, 
        litiko.RegulatoryDocuments.RegulatoryDocuments.Resources.VersionNoteOnTJ)
       );
    }

  }
}