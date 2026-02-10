using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ApprovalReworkAssignment;

namespace litiko.Eskhata.Client
{
  partial class ApprovalReworkAssignmentActions
  {
    public override void AbortApprovingAction(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.AbortApprovingAction(e);
      SendNotificationToRegiastrator();
    }

    public override bool CanAbortApprovingAction(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanAbortApprovingAction(e);
    }
    
    private void SendNotificationToRegiastrator()
    {
      var document = _obj.DocumentGroup.OfficialDocuments.FirstOrDefault();
      if (document?.DocumentKind?.DocumentFlow != Sungero.Docflow.DocumentKind.DocumentFlow.Outgoing || document.RegistrationState != Sungero.Docflow.OfficialDocument.RegistrationState.Registered)
        return;
      
      var registrationAssignments = Sungero.Docflow.ApprovalRegistrationAssignments.GetAll(a => a.Task == _obj.Task).OrderBy(a => a.Id).ToList();
      
      if (registrationAssignments.Count == 0)
        return;
      var registrator = registrationAssignments.Last().Performer;
      
      
      var subject = string.Empty;
      var threadSubject = string.Empty;
      var abortingReason = ApprovalTasks.As(_obj.Task)?.AbortingReason ?? string.Empty;
      using (Sungero.Core.CultureInfoExtensions.SwitchTo(TenantInfo.Culture))
      {
        threadSubject = Sungero.Docflow.ApprovalTasks.Resources.AbortNoticeSubject;
        if (document != null)
          subject = string.Format(Sungero.Exchange.Resources.TaskSubjectTemplate, threadSubject, Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols(document.Name));
        else
        {
          var approvalTaskSubject = string.Format("{0}{1}", _obj.Subject.Substring(0, 1).ToLower(), _obj.Subject.Remove(0, 1));
          subject = string.Format("{0} {1}", Sungero.Docflow.ApprovalTasks.Resources.AbortApprovalTask, Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols(approvalTaskSubject));
        }
      }
      
      var performersList = new List<IUser>();
      performersList.Add(registrator);
      
      Sungero.Docflow.PublicFunctions.Module.Remote.SendNoticesAsSubtask(subject, performersList, _obj.Task, abortingReason, _obj.Performer, threadSubject);
    }

  }

}