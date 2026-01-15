using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ApprovalTask;

namespace litiko.Eskhata.Shared
{
  partial class ApprovalTaskFunctions
  {
    /// <summary>
    /// Есть ли в схеме задачи этап с пользовательским типом.
    /// </summary>
    /// <param name="typeValue">Пользовательский тип этапа.</param>
    /// <returns>True, если этап есть. False, если этап отсутствует.</returns>
    [Public]
    public bool HasCustomStage(Sungero.Core.Enumeration typeValue)
    {
      if (_obj.ApprovalRule != null)
      {
        foreach (var stage in _obj.ApprovalRule.Stages)
        {
          var customStage = litiko.Eskhata.ApprovalStages.As(stage.Stage);
          if (customStage != null && customStage.CustomStageTypelitiko == typeValue)
            return true;
        }
      }
      return false;
    }
    
    /// <summary>
    /// Заполнить голосующих.
    /// </summary>
    public void FillVoters()
    {      
      var votingStage = litiko.Eskhata.ApprovalStage.CustomStageTypelitiko.Voting;
      var isVoting = this.HasCustomStage(votingStage) || _obj.ApprovalRule?.Name == litiko.CollegiateAgencies.PublicConstants.Module.VotingApprovalRuleName;
      
      if (isVoting)
      {
        var appRole = litiko.CollegiateAgencies.ApprovalRoles.GetAll().Where(r => Equals(r.Type, litiko.CollegiateAgencies.ApprovalRole.Type.MeetingPresentKOU)).FirstOrDefault();
        if (appRole != null)
        {
          var voters = litiko.CollegiateAgencies.PublicFunctions.ApprovalRole.Remote.GetRolePerformers(appRole, _obj);
          foreach (var employee in voters)
            _obj.Voterslitiko.AddNew().Employee = employee;            
        }
        
        var appRole2 = litiko.CollegiateAgencies.ApprovalRoles.GetAll().Where(r => Equals(r.Type, litiko.CollegiateAgencies.ApprovalRole.Type.MeetingPresentDOP)).FirstOrDefault();
        if (appRole2 != null)
        {
          var voters = litiko.CollegiateAgencies.PublicFunctions.ApprovalRole.Remote.GetRolePerformers(appRole2, _obj);
          foreach (var employee in voters)
            _obj.Voterslitiko.AddNew().Employee = employee;
        }        
      }
    }  

    public override void SetVisibleProperties(Sungero.Docflow.Structures.ApprovalTask.RefreshParameters refreshParameters)
    {
      base.SetVisibleProperties(refreshParameters);
                              
      bool isVoting = this.HasCustomStage(litiko.Eskhata.ApprovalStage.CustomStageTypelitiko.Voting) ||
        _obj.ApprovalRule?.Name == litiko.CollegiateAgencies.PublicConstants.Module.VotingApprovalRuleName;
      
      _obj.State.Properties.Voterslitiko.IsVisible = isVoting;
      _obj.State.Properties.Desigionslitiko.IsVisible = isVoting;
      _obj.State.Properties.ReqApprovers.IsVisible = isVoting ? false : true;      
    }    
  }
}