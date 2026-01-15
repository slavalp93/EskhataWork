using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.CollegiateAgencies.ApprovalSentNoticeStage;

namespace litiko.CollegiateAgencies.Server
{
  partial class ApprovalSentNoticeStageFunctions
  {
    /// <summary>
    /// Создание уведомлений.
    /// </summary>
    /// <param name="approvalTask">Задача на согласование по регламенту.</param>
    /// <returns>Результат выполнения кода.</returns>
    public override Sungero.Docflow.Structures.ApprovalFunctionStageBase.ExecutionResult Execute(Sungero.Docflow.IApprovalTask approvalTask)
    {
      Logger.DebugFormat("ApprovalSentNoticeStage. Start, approval task (ID={0}) (StartId={1}) (Iteration={2}) (StageNumber={3}).",
                         approvalTask.Id, approvalTask.StartId, approvalTask.Iteration, approvalTask.StageNumber);            
      
      #region Предпроверки      
      var document = approvalTask.DocumentGroup.OfficialDocuments.SingleOrDefault();
      if (document == null)
      {
        Logger.ErrorFormat("ApprovalSentNoticeStage. Primary document not found. Approval task (ID={0}) (StartId={1}) (Iteration={2}) (StageNumber={3}).",
                           approvalTask.Id, approvalTask.StartId, approvalTask.Iteration, approvalTask.StageNumber);
        return this.GetErrorResult(Sungero.Docflow.Resources.PrimaryDocumentNotFoundError);
      }            
      #endregion
      
      var meeting = Eskhata.Meetings.Null;
      if (Eskhata.Agendas.Is(document))
        meeting = Eskhata.Meetings.As(Eskhata.Agendas.As(document).Meeting);
      else if (Eskhata.Minuteses.Is(document))
        meeting = Eskhata.Meetings.As(Eskhata.Minuteses.As(document).Meeting);
      else if (CollegiateAgencies.Projectsolutions.Is(document))
        meeting = CollegiateAgencies.Projectsolutions.As(document).Meeting;
      
      if (meeting == null)
      {
        Logger.ErrorFormat("ApprovalSentNoticeStage. Meeting not found. Approval task (ID={0}) (StartId={1}) (Iteration={2}) (StageNumber={3}).",
                           approvalTask.Id, approvalTask.StartId, approvalTask.Iteration, approvalTask.StageNumber);
        return this.GetErrorResult(CollegiateAgencies.ApprovalSentNoticeStages.Resources.MeetingNotFound);      
      }
      
      foreach (var element in meeting.ProjectSolutionslitiko.Where(x => x.ProjectSolution != null))
      {
        var projectSolution = element.ProjectSolution;
        var author = projectSolution.PreparedBy;
        if (author != null)
        {          
          var subject = Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols("{0}: {1}", _obj.Subject.TrimEnd(new[] { ' ', '.', ':' }), projectSolution.Name);
          var newTask = Sungero.Workflow.SimpleTasks.CreateWithNotices(subject, author);
          newTask.NeedsReview = false;                    
          newTask.Attachments.Add(projectSolution);                    
          newTask.Start();                      
        }
      }
                 
      Logger.DebugFormat("ApprovalSentNoticeStage. Finish, approval task (ID={0}) (StartId={1}) (Iteration={2}) (StageNumber={3}).",
                         approvalTask.Id, approvalTask.StartId, approvalTask.Iteration, approvalTask.StageNumber);
      return this.GetSuccessResult();
    }
  }
}