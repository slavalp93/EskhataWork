using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.RegulatoryDocuments.ChangeStatusDocumentStage;

namespace litiko.RegulatoryDocuments.Server
{
  partial class ChangeStatusDocumentStageFunctions
  {
    /// <summary>
    /// Выполнить сценарий.
    /// </summary>
    /// <param name="approvalTask">Задача на согласование по регламенту.</param>
    /// <returns>Результат выполнения сценария.</returns>
    public override Sungero.Docflow.Structures.ApprovalFunctionStageBase.ExecutionResult Execute(Sungero.Docflow.IApprovalTask approvalTask)
    {
      var result = base.Execute(approvalTask);
      var document = approvalTask.DocumentGroup.OfficialDocuments.FirstOrDefault();
      if (document == null)
        return this.GetErrorResult(Resources.DocumentNotFound);            
        
      try
      {
        if (_obj.LifeCycleState != null && !Equals(document.LifeCycleState, _obj.LifeCycleState))
          document.LifeCycleState = _obj.LifeCycleState;
        
        if (_obj.InternalApprovalState != null && !Equals(document.InternalApprovalState, _obj.InternalApprovalState))
          document.InternalApprovalState = _obj.InternalApprovalState;
        
        if (document.State.IsChanged)
          document.Save();
      }
      catch (Exception ex)
      {
        result = this.GetRetryResult(string.Empty);
      }
      return result;
    }
  }
}