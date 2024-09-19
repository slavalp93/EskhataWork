using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ApprovalTask;

namespace litiko.Eskhata
{
  partial class ApprovalTaskSharedHandlers
  {

    public override void ApprovalRuleChanged(Sungero.Docflow.Shared.ApprovalTaskApprovalRuleChangedEventArgs e)
    {
      base.ApprovalRuleChanged(e);
      
      Functions.ApprovalTask.FillVoters(_obj);
    }

  }
}