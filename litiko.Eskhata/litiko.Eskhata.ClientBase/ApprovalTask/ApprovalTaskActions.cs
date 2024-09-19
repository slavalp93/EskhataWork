using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ApprovalTask;

namespace litiko.Eskhata.Client
{
  partial class ApprovalTaskActions
  {
    public override void Start(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      #region Голосование
      
      // Проверка на наличие активных задач по проектам решений, содержащих этап голосование.      
      var hasVotingStage = Functions.ApprovalTask.HasCustomStage(_obj, litiko.Eskhata.ApprovalStage.CustomStageTypelitiko.Voting);
      var projectSolutionIDs = _obj.AddendaGroup.All.Where(d => litiko.CollegiateAgencies.Projectsolutions.Is(d)).Select(d => d.Id).ToList();
      if (hasVotingStage && projectSolutionIDs.Any() && litiko.CollegiateAgencies.PublicFunctions.Module.Remote.AnyVoitingTasks(projectSolutionIDs))
      {                
        e.AddWarning(litiko.CollegiateAgencies.Resources.HasActiveVotingTasks);        
        return;
      }
      
      #endregion
      
      base.Start(e);
    }

    public override bool CanStart(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanStart(e);
    }

  }

}