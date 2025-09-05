using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using litiko.Eskhata.ApprovalTask;

namespace litiko.Eskhata.Server
{
  partial class ApprovalTaskRouteHandlers
  {

    public override void CompleteAssignment6(Sungero.Docflow.IApprovalAssignment assignment, Sungero.Docflow.Server.ApprovalAssignmentArguments e)
    {
      base.CompleteAssignment6(assignment, e);      
    }

    public override void CompleteAssignment30(Sungero.Docflow.IApprovalSimpleAssignment assignment, Sungero.Docflow.Server.ApprovalSimpleAssignmentArguments e)
    {
      base.CompleteAssignment30(assignment, e);
      
      var CustomAssignment = litiko.Eskhata.ApprovalSimpleAssignments.As(assignment);
      
      // Передача результатов голосования в проекты решений.
      if (CustomAssignment != null && CustomAssignment.CustomStageTypelitiko == litiko.Eskhata.ApprovalSimpleAssignment.CustomStageTypelitiko.Voting && assignment.Result == litiko.Eskhata.ApprovalSimpleAssignment.Result.Complete)
      {
        var asyncAddVoitingResults = litiko.CollegiateAgencies.AsyncHandlers.AddVoitingResults.Create();
        asyncAddVoitingResults.AssignmentId = assignment.Id;        
        asyncAddVoitingResults.ExecuteAsync();
      }      
    }

    public override void StartAssignment30(Sungero.Docflow.IApprovalSimpleAssignment assignment, Sungero.Docflow.Server.ApprovalSimpleAssignmentArguments e)
    {
      base.StartAssignment30(assignment, e);
      
      var stage = _obj.ApprovalRule.Stages
        .Where(s => s.Stage != null)
        .Where(s => s.Stage.StageType == Sungero.Docflow.ApprovalStage.StageType.SimpleAgr)
        .FirstOrDefault(s => s.Number == _obj.StageNumber);
      
      if (stage != null)
      {
        var CustomStage = litiko.Eskhata.ApprovalStages.As(stage.Stage);
        var CustomAssignment = litiko.Eskhata.ApprovalSimpleAssignments.As(assignment);
    
        #region Голосование
        if (CustomStage.CustomStageTypelitiko == litiko.Eskhata.ApprovalStage.CustomStageTypelitiko.Voting)
        {
          CustomAssignment.CustomStageTypelitiko = litiko.Eskhata.ApprovalSimpleAssignment.CustomStageTypelitiko.Voting;
          
          var task = litiko.Eskhata.ApprovalTasks.As(assignment.Task);
          int number = 1;
          foreach (var document in task.AddendaGroup.All)
          {
            var projectSolution = litiko.CollegiateAgencies.Projectsolutions.As(document);
            if (projectSolution != null)
            {
              var votingRecord = CustomAssignment.Votinglitiko.AddNew();
              votingRecord.Number = number++;
              votingRecord.Decision = projectSolution;
            }
          }

          var subject = CustomStage.Subject.TrimEnd(new[] { ' ', '.', ':' });
          var meeting = task.OtherGroup.All.Where(x => litiko.Eskhata.Meetings.Is(x)).FirstOrDefault();
          if (meeting != null)
            CustomAssignment.Subject = Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols("{0}: {1}", subject, meeting.DisplayValue);
        }                                          
        #endregion      
      }      
    }

  }
}