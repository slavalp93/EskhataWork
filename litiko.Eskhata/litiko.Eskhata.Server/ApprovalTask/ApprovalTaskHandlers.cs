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
    }
  }

}