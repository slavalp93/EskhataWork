using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ApprovalTask;

namespace litiko.Eskhata
{
  partial class ApprovalTaskServerHandlers
  {

    public override void BeforeAbort(Sungero.Workflow.Server.BeforeAbortEventArgs e)
    {
      base.BeforeAbort(e);

      #region Голосование. Прекратить задачи Голосование по решению
      bool isVoting = _obj.ApprovalRule?.Name == litiko.CollegiateAgencies.PublicConstants.Module.VotingApprovalRuleName;
      if (isVoting)
      {
                
        var activeSubTasks = Eskhata.ApprovalTasks.GetAll()
          .Where(t => t.MainApprovalTasklitiko != null
                 && t.MainApprovalTasklitiko.Id == _obj.Id
                 && t.Status == Eskhata.ApprovalTask.Status.InProcess)
          .ToList();
        foreach (var task in activeSubTasks)
          task.Abort();        
      }
      #endregion
      
      #region Голосование. Снять главную задачу с паузы (если нужно)
      bool isSubVoting = _obj.ApprovalRule?.Name == litiko.CollegiateAgencies.PublicConstants.Module.VotingApprovalRule2Name;
      if (isSubVoting && _obj.MainApprovalTasklitiko != null)
      {        
        CollegiateAgencies.PublicFunctions.Module.TryUnPauseVotingTask(_obj.MainApprovalTasklitiko.Id, new List<long> { _obj.Id });        
      }      
      #endregion
    }
  }

}