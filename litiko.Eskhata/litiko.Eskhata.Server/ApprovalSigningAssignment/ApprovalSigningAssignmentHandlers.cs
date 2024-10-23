using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ApprovalSigningAssignment;

namespace litiko.Eskhata
{
  partial class ApprovalSigningAssignmentServerHandlers
  {

    public override void BeforeComplete(Sungero.Workflow.Server.BeforeCompleteEventArgs e)
    {
      if (_obj.Result.Value == Result.Sign || _obj.Result.Value == Result.ConfirmSign)
      {
        var doc = _obj.DocumentGroup.OfficialDocuments.FirstOrDefault();
        var task = Sungero.Docflow.ApprovalTasks.As(_obj.Task);
        var stage = task.ApprovalRule.Stages
          .Where(s => s.Stage != null)
          .Where(s => s.Stage.StageType == Sungero.Docflow.ApprovalStage.StageType.Sign)
          .FirstOrDefault(s => s.Number == _obj.StageNumber);                  
        
        #region В карточке документа необходимо заполнить поля: "Правовой акт" и "Введение в действие с". 
        if (stage != null && doc != null && litiko.Eskhata.ApprovalStages.As(stage.Stage).CustomStageTypelitiko == litiko.Eskhata.ApprovalStage.CustomStageTypelitiko.ControlIRD &&
            litiko.RegulatoryDocuments.RegulatoryDocuments.Is(doc))
        {
          var regulatoryDocument = litiko.RegulatoryDocuments.RegulatoryDocuments.As(doc);
          if (regulatoryDocument.LegalAct == null || !regulatoryDocument.DateBegin.HasValue)
            e.AddError(litiko.RegulatoryDocuments.Resources.NeedFillLegalActAndDateBegin);
        }
        #endregion
      }      
      
      base.BeforeComplete(e);
    }
  }

}