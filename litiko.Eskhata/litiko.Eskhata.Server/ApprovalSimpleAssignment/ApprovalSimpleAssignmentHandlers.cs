using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ApprovalSimpleAssignment;

namespace litiko.Eskhata
{
  partial class ApprovalSimpleAssignmentServerHandlers
  {

    public override void BeforeComplete(Sungero.Workflow.Server.BeforeCompleteEventArgs e)
    {
      base.BeforeComplete(e);
      
      var doc = _obj.DocumentGroup.OfficialDocuments.FirstOrDefault();
      var task = Sungero.Docflow.ApprovalTasks.As(_obj.Task);
      var stage = task.ApprovalRule.Stages
        .Where(s => s.Stage != null)
        .Where(s => s.Stage.StageType == Sungero.Docflow.ApprovalStage.StageType.SimpleAgr)
        .FirstOrDefault(s => s.Number == _obj.StageNumber);                  
      
      if (stage != null && litiko.Eskhata.ApprovalStages.As(stage.Stage).CheckIncludeInAgendalitiko.Value && doc != null && litiko.CollegiateAgencies.Projectsolutions.Is(doc) && !litiko.CollegiateAgencies.Projectsolutions.As(doc).IncludedInAgenda.Value)
        e.AddError(litiko.CollegiateAgencies.Resources.DocumentIsNotIncludedInAgenda);

    }
  }

}