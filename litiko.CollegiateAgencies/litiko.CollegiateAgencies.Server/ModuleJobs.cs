using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.CollegiateAgencies.Server
{
  public class ModuleJobs
  {

    /// <summary>
    /// Рассылка уведомлений о приближении срока исполнения поручения
    /// </summary>
    public virtual void SendNoticeAboutActionItemExecutionTaskEnd()
    {
      Logger.Debug("Start SendNoticeAboutActionItemExecutionTaskEnd");
           
      var inFiveWorkDays = Calendar.AddWorkingDays(Calendar.Today, 5).Date;      
      var assignments = Sungero.RecordManagement.ActionItemExecutionAssignments.GetAll()
        .Where(a => a.Status == Sungero.Workflow.AssignmentBase.Status.InProcess)
        .Where(a => a.Deadline.HasValue && inFiveWorkDays.CompareTo(a.Deadline.Value.Date) == 0);
           
      Logger.DebugFormat("{0} matching assignments found", assignments.Count().ToString());      
      
      foreach(var assignment in assignments)
      {                
        string subject = litiko.CollegiateAgencies.Resources.ExecutionPeriodEndsInFiveDaysFormat(assignment.ActionItem);
        List<IRecipient> addressees = new List<IRecipient>();
        addressees.Add(assignment.Performer);
        var task = Sungero.RecordManagement.ActionItemExecutionTasks.As(assignment.Task);
        if (task.IsUnderControl.GetValueOrDefault() == true && task.Supervisor != null && !Equals(Users.As(task.Supervisor), assignment.Performer))
          addressees.Add(task.Supervisor);
        
        var notice = Sungero.Workflow.SimpleTasks.CreateWithNotices(subject, addressees.ToArray());
        notice.ActiveText = litiko.CollegiateAgencies.Resources.ActionItemAssignmentFormat(Hyperlinks.Get(assignment));
        notice.Attachments.Add(assignment);
        notice.Start();
        Logger.DebugFormat("Notice Task ID:{0} was sent succsesfully for actionItemAssignment ID:{1}", notice.Id, assignment.Id);        
      }
      Logger.Debug("Finish SendNoticeAboutActionItemExecutionTaskEnd");
    }

  }
}