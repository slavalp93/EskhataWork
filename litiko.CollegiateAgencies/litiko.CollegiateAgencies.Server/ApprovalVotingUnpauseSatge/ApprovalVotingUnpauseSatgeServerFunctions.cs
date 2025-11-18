using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.CollegiateAgencies.ApprovalVotingUnpauseSatge;

namespace litiko.CollegiateAgencies.Server
{
  partial class ApprovalVotingUnpauseSatgeFunctions
  {
    /// <summary>
    /// Снятие задачи по голосованию с паузы.
    /// </summary>
    /// <param name="approvalTask">Задача на согласование по регламенту.</param>
    /// <returns>Результат выполнения кода.</returns>
    public override Sungero.Docflow.Structures.ApprovalFunctionStageBase.ExecutionResult Execute(Sungero.Docflow.IApprovalTask approvalTask)
    {
      Logger.DebugFormat("ApprovalVotingUnpauseSatge. Start, approval task (ID={0}) (StartId={1}) (Iteration={2}) (StageNumber={3}).",
                         approvalTask.Id, approvalTask.StartId, approvalTask.Iteration, approvalTask.StageNumber);            
      
      #region Предпроверки      
      var custumApprovalTask = Eskhata.ApprovalTasks.As(approvalTask);
      if (custumApprovalTask.MainApprovalTasklitiko == null)
      {
        Logger.DebugFormat("ApprovalVotingUnpauseSatge. MainApprovalTask is null. Approval task (ID={0}) (StartId={1}) (Iteration={2}) (StageNumber={3}).",
                           approvalTask.Id, approvalTask.StartId, approvalTask.Iteration, approvalTask.StageNumber);
        return this.GetErrorResult("Не найдено главную задачу по голосованию.");
      }
      #endregion
      
      try
      {
        CollegiateAgencies.Functions.Module.TryUnPauseVotingTask(custumApprovalTask.MainApprovalTasklitiko.Id, new List<long> { custumApprovalTask.Id });
      }
      catch (Exception ex)
      {
        return this.GetErrorResult($"Ошибка при снятии задачи с паузы:{ex.Message}");
      }
        
      Logger.DebugFormat("ApprovalVotingUnpauseSatge. Finish. Approval task (ID={0}) (MainApprovalTasklitiko={1}). ", approvalTask.Id, custumApprovalTask.MainApprovalTasklitiko.Id);      
      return this.GetSuccessResult();
    }
  }
}