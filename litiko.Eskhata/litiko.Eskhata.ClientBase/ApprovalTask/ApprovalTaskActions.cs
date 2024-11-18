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
      

      var document = _obj.DocumentGroup.OfficialDocuments.FirstOrDefault();
      
      // TODO Возможны ошибки при блокировках...
      #region При отправке Повестки совещания на Проекты решений: выдать права Председателю и Членам комитета и !!! включить строгий доступ !!!
      if (document != null && litiko.Eskhata.Agendas.Is(document))
      {               
        var agenda = litiko.Eskhata.Agendas.As(document);
        var meeting = litiko.Eskhata.Meetings.As(agenda?.Meeting);
        if (meeting != null)        
        {         
          var president = meeting.President;
          var categoryMembers = meeting.MeetingCategorylitiko?.Members.Where(x => x.Member != null);
          foreach (var psDocument in meeting.ProjectSolutionslitiko.Where(x => x.ProjectSolution != null))
          {
            var projectSolution = psDocument.ProjectSolution;                            
            if (projectSolution.AccessRights.StrictMode == AccessRightsStrictMode.None && projectSolution.AccessRights.CanManage())
            {                
              // Выдать права Председателю и Членам комитета
              if (president != null && !projectSolution.AccessRights.IsGrantedDirectly(DefaultAccessRightsTypes.Change, president))
              {
                projectSolution.AccessRights.Grant(president, DefaultAccessRightsTypes.Change);
                projectSolution.AccessRights.Save();
              }
  
              foreach (var element in categoryMembers)
              {
                var member = element.Member;
                if (member != null && !projectSolution.AccessRights.IsGrantedDirectly(DefaultAccessRightsTypes.Change, member))
                {
                  projectSolution.AccessRights.Grant(member, DefaultAccessRightsTypes.Change);
                  projectSolution.AccessRights.Save();
                }
              }
                
              // Установить усиленный строгий доступ
              if (projectSolution.AccessRights.CanSetStrictMode(AccessRightsStrictMode.Enhanced))
              {
                projectSolution.AccessRights.SetStrictMode(AccessRightsStrictMode.Enhanced);
                projectSolution.AccessRights.Save();
              }                
            }              
          }        
        }        
      }
      #endregion
      
      #region При отправке Протокола совещания на Проекты решений: выдать права Председателю и Членам комитета (Присутствующие сотрудники)
      if (document != null && litiko.Eskhata.Minuteses.Is(document))
      {               
        var minutes = litiko.Eskhata.Minuteses.As(document);
        var meeting = litiko.Eskhata.Meetings.As(minutes?.Meeting);
        if (meeting != null)        
        {         
          var president = meeting.President;
          var categoryMembers = meeting.Presentlitiko.Where(x => x.Employee != null);
          foreach (var psDocument in meeting.ProjectSolutionslitiko.Where(x => x.ProjectSolution != null))
          {
            var projectSolution = psDocument.ProjectSolution;                            
            if (projectSolution.AccessRights.CanManage())
            {                
              // Выдать права Председателю и Членам комитета
              if (president != null && !projectSolution.AccessRights.IsGrantedDirectly(DefaultAccessRightsTypes.Change, president))
              {
                projectSolution.AccessRights.Grant(president, DefaultAccessRightsTypes.Change);
                projectSolution.AccessRights.Save();
              }
  
              foreach (var element in categoryMembers)
              {
                var member = element.Employee;
                if (member != null && !projectSolution.AccessRights.IsGrantedDirectly(DefaultAccessRightsTypes.Change, member))
                {
                  projectSolution.AccessRights.Grant(member, DefaultAccessRightsTypes.Change);
                  projectSolution.AccessRights.Save();
                }
              }                
            }              
          }        
        }        
      }
      #endregion      

    }

    public override bool CanStart(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanStart(e);
    }

  }

}