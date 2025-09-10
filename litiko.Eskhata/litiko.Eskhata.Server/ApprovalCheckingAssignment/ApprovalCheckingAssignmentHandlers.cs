using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ApprovalCheckingAssignment;

namespace litiko.Eskhata
{
  partial class ApprovalCheckingAssignmentServerHandlers
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
      
      #region Договора. Контроль получения скана.
      if (stage != null && doc != null && litiko.Eskhata.ApprovalStages.As(stage.Stage).CustomStageTypelitiko == litiko.Eskhata.ApprovalStage.CustomStageTypelitiko.ScanReceivedCon &&
          ContractualDocuments.Is(doc) && _obj.Result == ApprovalCheckingAssignment.Result.Accept)
      {
        var contractualDocument = ContractualDocuments.As(doc);
        if (!contractualDocument.ScanReceivedlitiko.HasValue)
          e.AddError(litiko.ContractsEskhata.Resources.RequiredToFillIsScanReceived);
      }      
      #endregion      
    }
  }

}