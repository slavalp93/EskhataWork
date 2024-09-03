using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ApprovalStage;

namespace litiko.Eskhata
{
  partial class ApprovalStageClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);
      
      _obj.State.Properties.CheckIncludeInAgendalitiko.IsVisible = _obj.StageType == Sungero.Docflow.ApprovalStage.StageType.SimpleAgr;
    }
  }

}