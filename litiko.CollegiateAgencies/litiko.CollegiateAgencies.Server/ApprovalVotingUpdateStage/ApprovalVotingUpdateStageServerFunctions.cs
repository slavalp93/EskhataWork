using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.CollegiateAgencies.ApprovalVotingUpdateStage;

namespace litiko.CollegiateAgencies.Server
{
  partial class ApprovalVotingUpdateStageFunctions
  {
    /// <summary>
    /// Выполнить сценарий.
    /// </summary>
    /// <param name="approvalTask">Задача на согласование по регламенту.</param>
    /// <returns>Результат выполнения сценария.</returns>
    public override Sungero.Docflow.Structures.ApprovalFunctionStageBase.ExecutionResult Execute(Sungero.Docflow.IApprovalTask approvalTask)
    {
      Logger.DebugFormat("ApprovalVotingUpdateStage: Start. ApprovalTask Id: {0}", approvalTask.Id);
      var result = base.Execute(approvalTask);
      
      var meeting = litiko.Eskhata.Meetings.As(approvalTask.OtherGroup.All.Where(x => litiko.Eskhata.Meetings.Is(x)).FirstOrDefault());
      if (meeting == null)
        return this.GetErrorResult(litiko.CollegiateAgencies.ApprovalVotingUpdateStages.Resources.MeetingNotFound);
            
      List<CollegiateAgencies.IProjectsolution> desigions = Eskhata.ApprovalTasks.As(approvalTask).Desigionslitiko
        .Select(x => x.Desigion)
        .ToList();
      
      if (!desigions.Any())
        return this.GetErrorResult(litiko.CollegiateAgencies.ApprovalVotingUpdateStages.Resources.DesigionsNotFound);
      
      if (!Locks.TryLock(meeting))
        this.GetRetryResult(litiko.CollegiateAgencies.ApprovalVotingUpdateStages.Resources.MeetingIsLockedFormat(meeting.Id));
      
      try
      {        
        // Учитывать голосования по доп. голосующим
        bool needAdditionalVoters = false;
        var roleAdditionalBoardMembers = Roles.GetAll(x => x.Sid == litiko.CollegiateAgencies.PublicConstants.Module.RoleGuid.AdditionalBoardMembers).FirstOrDefault();
        if (meeting.MeetingCategorylitiko?.Name == "Заседание Правления" && roleAdditionalBoardMembers != null)
          needAdditionalVoters = true;
        
        var documentIDs = desigions.Select(x => x.Id).ToList();
        foreach (var element in meeting.ProjectSolutionslitiko.Where(x => x.ProjectSolution != null && documentIDs.Contains(x.ProjectSolution.Id)))
        {
          var projectSolution = element.ProjectSolution;            
                    
          var votedYes = needAdditionalVoters ? projectSolution.Voting.Where(x => !x.Member.IncludedIn(roleAdditionalBoardMembers)).Count(x => x.Yes.GetValueOrDefault()) : projectSolution.Voting.Count(x => x.Yes.GetValueOrDefault());
          var votedNo = needAdditionalVoters ? projectSolution.Voting.Where(x => !x.Member.IncludedIn(roleAdditionalBoardMembers)).Count(x => x.No.GetValueOrDefault()) : projectSolution.Voting.Count(x => x.No.GetValueOrDefault());
          var votedAbstained = needAdditionalVoters ? projectSolution.Voting.Where(x => !x.Member.IncludedIn(roleAdditionalBoardMembers)).Count(x => x.Abstained.GetValueOrDefault()) : projectSolution.Voting.Count(x => x.Abstained.GetValueOrDefault());
          var accepted = votedYes > votedNo ? true : false;
            
          if (element.Yes.GetValueOrDefault() != votedYes)
            element.Yes = votedYes;
          if (element.No.GetValueOrDefault() != votedNo)
            element.No = votedNo;
          if (element.Abstained.GetValueOrDefault() != votedAbstained)
            element.Abstained = votedAbstained;
          if (element.Accepted.GetValueOrDefault() != accepted)
            element.Accepted = accepted;            
        }                              

        if (meeting.State.IsChanged)
        {
          meeting.Save();
          Logger.DebugFormat("ApprovalVotingUpdateStage: Updated successfully. Meeting Id: {0}, ApprovalTask Id: {1}", meeting.Id, approvalTask.Id);
        }
        else
          Logger.DebugFormat("ApprovalVotingUpdateStage: No data to update. Meeting Id: {0}, ApprovalTask Id: {1}", meeting.Id, approvalTask.Id);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("ApprovalVotingUpdateStage: Error: {0}. Stack trace: {1}", ex.Message, ex.StackTrace);
        result = this.GetRetryResult(ex.Message);
      }
      finally
      {
        Locks.Unlock(meeting);
      }
      
      return result;      
    }
  }
}