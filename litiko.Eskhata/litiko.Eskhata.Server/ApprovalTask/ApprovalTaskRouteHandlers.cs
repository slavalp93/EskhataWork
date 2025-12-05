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

    public override void StartBlock31(Sungero.Docflow.Server.ApprovalCheckingAssignmentArguments e)
    {
      base.StartBlock31(e);
      
      if (_obj.ApprovalRule?.Name == Eskhata.PublicConstants.Parties.Counterparty.VerificationApprovalRuleName)
      {
        var counterparty = _obj.OtherGroup.All.FirstOrDefault(x => Eskhata.Counterparties.Is(x));
        if (counterparty != null)
          e.Block.Subject = "Проверьте контрагента и дайте заключение: " + Eskhata.Counterparties.As(counterparty).Name;
      }      
    }

    public override void StartBlock33(Sungero.Docflow.Server.ApprovalSimpleNotificationArguments e)
    {
      base.StartBlock33(e);

      if (_obj.ApprovalRule?.Name == litiko.CollegiateAgencies.PublicConstants.Module.VotingApprovalRuleName)     
        e.Block.Subject = _obj.Subject.Replace("Голосование:", "Завершено голосование:");
    }

    public override void CompleteAssignment31(Sungero.Docflow.IApprovalCheckingAssignment assignment, Sungero.Docflow.Server.ApprovalCheckingAssignmentArguments e)
    {
      base.CompleteAssignment31(assignment, e);
      
      var CustomAssignment = litiko.Eskhata.ApprovalCheckingAssignments.As(assignment);
      
      // Передача результатов голосования в проекты решений.
      if (CustomAssignment != null && CustomAssignment.CustomStageTypelitiko == litiko.Eskhata.ApprovalCheckingAssignment.CustomStageTypelitiko.Voting && assignment.Result == litiko.Eskhata.ApprovalCheckingAssignment.Result.Accept)
      {
        var asyncAddVoitingResults = litiko.CollegiateAgencies.AsyncHandlers.AddVoitingResults.Create();
        asyncAddVoitingResults.AssignmentId = assignment.Id;        
        asyncAddVoitingResults.ExecuteAsync();
      }      
    }

    public override void StartAssignment31(Sungero.Docflow.IApprovalCheckingAssignment assignment, Sungero.Docflow.Server.ApprovalCheckingAssignmentArguments e)
    {
      base.StartAssignment31(assignment, e);
      
      var stage = _obj.ApprovalRule.Stages
        .Where(s => s.Stage != null)
        .Where(s => s.Stage.StageType == Sungero.Docflow.ApprovalStage.StageType.SimpleAgr)
        .FirstOrDefault(s => s.Number == _obj.StageNumber);
      
      if (stage != null)
      {
        var CustomStage = litiko.Eskhata.ApprovalStages.As(stage.Stage);
        var CustomAssignment = litiko.Eskhata.ApprovalCheckingAssignments.As(assignment);
    
        #region Голосование
        if (CustomStage.CustomStageTypelitiko == litiko.Eskhata.ApprovalStage.CustomStageTypelitiko.Voting)
        {
          CustomAssignment.CustomStageTypelitiko = litiko.Eskhata.ApprovalCheckingAssignment.CustomStageTypelitiko.Voting;
          
          var task = litiko.Eskhata.ApprovalTasks.As(assignment.Task);          
          var firstDecision = task.Desigionslitiko.FirstOrDefault();
          if (firstDecision != null)
          {
            var projectSolution = litiko.CollegiateAgencies.Projectsolutions.As(firstDecision.Desigion);
            if (projectSolution != null)
            {
              var votingRecord = CustomAssignment.Votinglitiko.AddNew();
              votingRecord.Number = 1;
              votingRecord.Decision = projectSolution;
            }              
          }                      
        }                                          
        #endregion      
      }      
    }

    public override void StartBlock52(Sungero.Workflow.Server.Route.MonitoringStartBlockEventArguments e)
    {
      base.StartBlock52(e);
      
      // TODO Вынести в настройку частоту мониторинга, например, в карточку метода интеграции
      if (_obj.ExchangeDocIdlitiko != null)
        e.Block.Period = TimeSpan.FromMinutes(5);
      
      if (_obj.ApprovalRule?.Name == litiko.CollegiateAgencies.PublicConstants.Module.VotingApprovalRuleName)
        e.Block.Period = TimeSpan.FromMinutes(5);
    }

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
          var firstDecision = task.Desigionslitiko.FirstOrDefault();
          if (firstDecision != null)
          {
            var projectSolution = litiko.CollegiateAgencies.Projectsolutions.As(firstDecision.Desigion);
            if (projectSolution != null)
            {
              var votingRecord = CustomAssignment.Votinglitiko.AddNew();
              votingRecord.Number = 1;
              votingRecord.Decision = projectSolution;
            }              
          }          
        }                                          
        #endregion      
        
        #region Пауза
        if (CustomStage.CustomStageTypelitiko == litiko.Eskhata.ApprovalStage.CustomStageTypelitiko.Pause)
        {
          CustomAssignment.CustomStageTypelitiko = litiko.Eskhata.ApprovalSimpleAssignment.CustomStageTypelitiko.Pause;
        }
        #endregion
      }      
    }

  }
}