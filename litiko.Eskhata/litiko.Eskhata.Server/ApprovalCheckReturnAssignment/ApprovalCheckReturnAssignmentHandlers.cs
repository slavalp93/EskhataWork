using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ApprovalCheckReturnAssignment;

namespace litiko.Eskhata
{
  partial class ApprovalCheckReturnAssignmentServerHandlers
  {

    public override void BeforeComplete(Sungero.Workflow.Server.BeforeCompleteEventArgs e)
    {
      base.BeforeComplete(e);

      var doc = _obj.DocumentGroup.OfficialDocuments.FirstOrDefault();
      var task = Sungero.Docflow.ApprovalTasks.As(_obj.Task);
      var stage = task.ApprovalRule.Stages
        .Where(s => s.Stage != null)
        .Where(s => s.Stage.StageType == Sungero.Docflow.ApprovalStage.StageType.CheckReturn)
        .FirstOrDefault(s => s.Number == _obj.StageNumber); 
      
      #region Договора. Контроль получения оригинала.
      if (stage != null && doc != null && litiko.Eskhata.ApprovalStages.As(stage.Stage).CustomStageTypelitiko == litiko.Eskhata.ApprovalStage.CustomStageTypelitiko.OrigReceivedCon &&
          ContractualDocuments.Is(doc) && _obj.Result == ApprovalCheckReturnAssignment.Result.Signed)
      {
        var contractualDocument = ContractualDocuments.As(doc);
        if (!contractualDocument.OriginalReceivedlitiko.HasValue)
        {
          e.AddError(litiko.ContractsEskhata.Resources.RequiredToFillIsOriginalReceived);
          return;
        }
      }
      #endregion                  
    }
  }

}