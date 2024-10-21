using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ApprovalAssignment;

namespace litiko.Eskhata
{
  partial class ApprovalAssignmentServerHandlers
  {

    public override void BeforeComplete(Sungero.Workflow.Server.BeforeCompleteEventArgs e)
    {
      base.BeforeComplete(e);
      
      if (_obj.IsNotAgreeResultlitiko.GetValueOrDefault())
        e.Result = litiko.Eskhata.ApprovalAssignments.Resources.NotAgreeResult;
    }
  }

}