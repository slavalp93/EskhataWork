using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ApprovalAssignment;

namespace litiko.Eskhata
{
  partial class ApprovalAssignmentClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);
      
      // Скрывать результат выполнения "Не согласен" в случаях когда он отключен в настройках этапа.      
      var stageAllowsNotAgreeResult = Functions.ApprovalTask.Remote.GetApprovalWithResultNotAgreeParameter(ApprovalTasks.As(_obj.Task), _obj.StageNumber.Value);
      if (!stageAllowsNotAgreeResult)
        e.HideAction(_obj.Info.Actions.NotAgreelitiko);
            
    }

  }
}