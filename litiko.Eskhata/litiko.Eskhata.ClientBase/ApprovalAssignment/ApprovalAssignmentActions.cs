using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ApprovalAssignment;

namespace litiko.Eskhata.Client
{
  partial class ApprovalAssignmentActions
  {
    public virtual void NotAgreelitiko(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (string.IsNullOrWhiteSpace(_obj.ActiveText))
      {
        e.AddError(litiko.Eskhata.ApprovalAssignments.Resources.CommentNeeded);
        return;
      }      
      
      var action = new Sungero.Workflow.Client.ExecuteResultActionArgs(e.FormType, e.Entity, e.Action);
      base.Approved(action);
      _obj.IsNotAgreeResultlitiko = true;
      e.CloseFormAfterAction = true;
      _obj.Complete(Result.Approved);      
    }

    public virtual bool CanNotAgreelitiko(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.Addressee == null && _obj.DocumentGroup.OfficialDocuments.Any();
    }

  }

}