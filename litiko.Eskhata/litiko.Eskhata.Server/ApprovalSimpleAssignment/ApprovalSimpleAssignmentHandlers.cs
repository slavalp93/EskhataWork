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
      
      #region КОУ. Контроль включения в повестку.            
      if (stage != null && _obj.Result == Sungero.Docflow.ApprovalCheckingAssignment.Result.Accept
          && litiko.Eskhata.ApprovalStages.As(stage.Stage).CustomStageTypelitiko == litiko.Eskhata.ApprovalStage.CustomStageTypelitiko.IncludeInMeet 
          && doc != null && litiko.CollegiateAgencies.Projectsolutions.Is(doc) && !litiko.CollegiateAgencies.Projectsolutions.As(doc).IncludedInAgenda.Value)
        e.AddError(litiko.CollegiateAgencies.Resources.DocumentIsNotIncludedInAgenda);
      #endregion
      
      #region КОУ. Контроль при голосовании.      
      var isVoiting = _obj.CustomStageTypelitiko == litiko.Eskhata.ApprovalSimpleAssignment.CustomStageTypelitiko.Voting;
      if (isVoiting && _obj.Votinglitiko.Any(d => !d.Yes.GetValueOrDefault() && !d.No.GetValueOrDefault() && !d.Abstained.GetValueOrDefault()))
        e.AddError(litiko.Eskhata.ApprovalSimpleAssignments.Resources.ErrorVoteAllDecisions);
      #endregion
      
      #region ВНД. Контроль заполнения полей: "Правовой акт" и "Введение в действие с". 
      if (stage != null && doc != null && litiko.Eskhata.ApprovalStages.As(stage.Stage).CustomStageTypelitiko == litiko.Eskhata.ApprovalStage.CustomStageTypelitiko.ControlIRD &&
          litiko.RegulatoryDocuments.RegulatoryDocuments.Is(doc))
      {
        var regulatoryDocument = litiko.RegulatoryDocuments.RegulatoryDocuments.As(doc);
        if (regulatoryDocument.LegalAct == null || !regulatoryDocument.DateBegin.HasValue)
          e.AddError(litiko.RegulatoryDocuments.Resources.NeedFillLegalActAndDateBegin);
      }
      #endregion      

    }
  }

}