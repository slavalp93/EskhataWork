using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace litiko.CollegiateAgencies.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {    
      CreateDocumentTypes();
      CreateDocumentKinds();
      CreateRoles();
      GrantRightsOnEntities();
      
      CreateApprovalRole(litiko.CollegiateAgencies.ApprovalRole.Type.Speaker, "Докладчик");
      CreateApprovalRole(litiko.CollegiateAgencies.ApprovalRole.Type.MeetingSecretary, "Секретарь заседания");
      CreateApprovalRole(litiko.CollegiateAgencies.ApprovalRole.Type.MeetingPresident, "Председатель заседания");
      CreateApprovalRole(litiko.CollegiateAgencies.ApprovalRole.Type.MeetingMembers, "Участники заседания");
      CreateApprovalRole(litiko.CollegiateAgencies.ApprovalRole.Type.MeetingInvited, "Приглашенные сотрудники");
      CreateApprovalRole(litiko.CollegiateAgencies.ApprovalRole.Type.MeetingPresentKOU, "Присутствующие члены КОУ");
      CreateApprovalRole(litiko.CollegiateAgencies.ApprovalRole.Type.MeetingPresentDOP, "Присутствующие доп. члены КОУ");
      
      CreateVotingDefaultApprovalRule();
      CreateApprovalVotingUpdateStage();
    }
    
    /// <summary>
    /// Создать типы документов.
    /// </summary>
    public static void CreateDocumentTypes()
    {
      InitializationLogger.Debug("Init: Create document types");      
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentType(Resources.ProjectsolutionType, Projectsolution.ClassTypeGuid, Sungero.Docflow.DocumentType.DocumentFlow.Inner, false);
    }    
    
    /// <summary>
    /// Создать виды документов.
    /// </summary>    
    public static void CreateDocumentKinds()
    {
      
      #region Settings
      InitializationLogger.Debug("Init: Create document kinds.");
      
      var registrable = Sungero.Docflow.DocumentKind.NumberingType.Registrable;
      var numerable = Sungero.Docflow.DocumentKind.NumberingType.Numerable;
      var notNumerable = Sungero.Docflow.DocumentKind.NumberingType.NotNumerable;
      
      var actions = new Sungero.Domain.Shared.IActionInfo[] {
        Sungero.Docflow.OfficialDocuments.Info.Actions.SendForFreeApproval,
        Sungero.Docflow.OfficialDocuments.Info.Actions.SendForApproval
      };
      #endregion
      
      #region Проект решения
      Sungero.Docflow.PublicInitializationFunctions.Module.
        CreateDocumentKind(Resources.ProjectSolutionKind,
                           Resources.ProjectSolutionKind,
                           notNumerable,
                           Sungero.Docflow.DocumentKind.DocumentFlow.Inner,
                           true,
                           false,
                           Constants.Module.DocumentTypeGuids.ProjectSolution,
                           actions,
                           Constants.Module.DocumentKindGuids.ProjectSolution,
                           true);
      #endregion    
      
      #region Пояснительная записка
      Sungero.Docflow.PublicInitializationFunctions.Module.
        CreateDocumentKind(Resources.ExplanatoryNoteKind,
                           Resources.ExplanatoryNoteKind,
                           notNumerable,
                           Sungero.Docflow.DocumentKind.DocumentFlow.Inner,
                           true,
                           false,
                           Constants.Module.DocumentTypeGuids.Addendum,
                           actions,
                           Constants.Module.DocumentKindGuids.ExplanatoryNote,
                           false);
      #endregion     
      
      #region Выписка из протокола 
      Sungero.Docflow.PublicInitializationFunctions.Module.
        CreateDocumentKind(Resources.ExtractProtocolKind,
                           Resources.ExtractProtocolKind,
                           notNumerable,
                           Sungero.Docflow.DocumentKind.DocumentFlow.Inner,
                           true,
                           false,
                           Constants.Module.DocumentTypeGuids.Addendum,
                           new Sungero.Domain.Shared.IActionInfo[] { Sungero.Docflow.OfficialDocuments.Info.Actions.SendForFreeApproval, Sungero.Docflow.OfficialDocuments.Info.Actions.SendForAcquaintance },
                           Constants.Module.DocumentKindGuids.ExtractProtocol,
                           false);
      #endregion      
      
      #region Постановление 
      Sungero.Docflow.PublicInitializationFunctions.Module.
        CreateDocumentKind(Resources.ResolutionKind,
                           Resources.ResolutionKind,
                           registrable,
                           Sungero.Docflow.DocumentKind.DocumentFlow.Inner,
                           true,
                           false,
                           Constants.Module.DocumentTypeGuids.Addendum,
                           new Sungero.Domain.Shared.IActionInfo[] { Sungero.Docflow.OfficialDocuments.Info.Actions.SendForFreeApproval, Sungero.Docflow.OfficialDocuments.Info.Actions.SendForApproval, Sungero.Docflow.OfficialDocuments.Info.Actions.SendForAcquaintance },
                           Constants.Module.DocumentKindGuids.Resolution,
                           false);
      #endregion       
      
      #region Протокол заседания Правления        
      var externalLink = Sungero.Docflow.PublicFunctions.Module.GetExternalLink(Sungero.Docflow.Server.DocumentKind.ClassTypeGuid, Constants.Module.DocumentKindGuids.ProtocolOfBoardMeeting);      
      if (externalLink == null)
      {
        var documentKind = Sungero.Docflow.DocumentKinds.GetAll().Where(k => k.Name == Resources.ProtocolOfBoardMeetingKind).FirstOrDefault();
        if (documentKind != null)
          Sungero.Docflow.PublicFunctions.Module.CreateExternalLink(documentKind, Constants.Module.DocumentKindGuids.ProtocolOfBoardMeeting);
        else
        {
          Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(Resources.ProtocolOfBoardMeetingKind,
                           Resources.ProtocolOfBoardMeetingKind,
                           registrable,
                           Sungero.Docflow.DocumentKind.DocumentFlow.Inner,
                           true,
                           false,
                           Constants.Module.DocumentTypeGuids.Minutes,
                           new Sungero.Domain.Shared.IActionInfo[] { Sungero.Docflow.OfficialDocuments.Info.Actions.SendForFreeApproval, 
                             Sungero.Docflow.OfficialDocuments.Info.Actions.SendForApproval, 
                             Sungero.Docflow.OfficialDocuments.Info.Actions.SendForAcquaintance,
                             Sungero.Docflow.OfficialDocuments.Info.Actions.SendActionItem
                           },
                           Constants.Module.DocumentKindGuids.ProtocolOfBoardMeeting,
                           false);          
        }
      }
      #endregion
      
    }
    
    /// <summary>
    /// Выдать права на сущности модуля.
    /// </summary>
    public static void GrantRightsOnEntities()
    {
      InitializationLogger.Debug("Init: Grant rights on entities.");
      
      // "Все пользователи"
      var roleAllUsers = Roles.AllUsers;
      if (roleAllUsers != null)
      {
        //MeetingCategories.AccessRights.Grant(roleAllUsers, DefaultAccessRightsTypes.Read);
        //MeetingCategories.AccessRights.Save();

        //MeetingMethods.AccessRights.Grant(roleAllUsers, DefaultAccessRightsTypes.Read);
        //MeetingMethods.AccessRights.Save();

        Projectsolutions.AccessRights.Grant(roleAllUsers, DefaultAccessRightsTypes.Create);
        Projectsolutions.AccessRights.Save();

        NSI.AbsentReasons.AccessRights.Grant(roleAllUsers, DefaultAccessRightsTypes.Read);
        NSI.AbsentReasons.AccessRights.Save();
        
        //QuestionGroups.AccessRights.Grant(roleAllUsers, DefaultAccessRightsTypes.Read);
        //QuestionGroups.AccessRights.Save();
      }
      
      // "Ответственные за совещания"
      var meetingResponsible = Roles.GetAll().Where(n => n.Sid == Sungero.Meetings.PublicConstants.Module.MeetingResponsibleRole).FirstOrDefault();
      if (meetingResponsible != null)
      {
        MeetingCategories.AccessRights.Grant(meetingResponsible, DefaultAccessRightsTypes.FullAccess);
        MeetingCategories.AccessRights.Save();
        
        MeetingMethods.AccessRights.Grant(meetingResponsible, DefaultAccessRightsTypes.FullAccess);
        MeetingMethods.AccessRights.Save();
      }
      
      // "Секретари КОУ"
      var secretariesKOU = Roles.GetAll().Where(r => r.Sid == Constants.Module.RoleGuid.Secretaries).FirstOrDefault();
      if (secretariesKOU != null)
      {
        QuestionGroups.AccessRights.Grant(secretariesKOU, DefaultAccessRightsTypes.FullAccess);
        QuestionGroups.AccessRights.Save();
        
        Reports.AccessRights.Grant(Reports.GetMeetingMinutesReport().Info, secretariesKOU, DefaultReportAccessRightsTypes.Execute);
        
        NSI.AbsentReasons.AccessRights.Grant(secretariesKOU, DefaultAccessRightsTypes.FullAccess);
        NSI.AbsentReasons.AccessRights.Save();
      }      
      
    }    
    
    /// <summary>
    /// Создание роли.
    /// </summary>
    public static void CreateApprovalRole(Enumeration roleType, string description)
    {

      var role = ApprovalRoles.GetAll().Where(r => Equals(r.Type, roleType)).FirstOrDefault();
      if (role == null)
      {
        role = ApprovalRoles.Create();
        role.Type = roleType;
      }
      if (role.Description != description)
        role.Description = description;
      
      if (role.State.IsChanged || role.State.IsInserted)
      {
        role.Save();
        InitializationLogger.DebugFormat("Создана/обновлена роль {0}", description);        
      }

    }
    
    /// <summary>
    /// Создать предопределенные роли.
    /// </summary>
    public static void CreateRoles()
    {
      InitializationLogger.Debug("Init: Create Default Roles");
            
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(Resources.RoleSecretaries, Resources.DescriptionRoleSecreteries, Constants.Module.RoleGuid.Secretaries);
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(Resources.RolePresidents, Resources.DescriptionRolePresidents, Constants.Module.RoleGuid.Presidents);
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(Resources.RoleCreationResolutions, Resources.DescriptionRoleCreationResolutions, Constants.Module.RoleGuid.CreationResolutions);
      CreateSingleUserRole(Resources.RoleTranslator, Resources.RoleTranslator, Constants.Module.RoleGuid.Translator);
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(Resources.RoleAdditionalBoardMembers, Resources.DescriptionAdditionalBoardMembers, Constants.Module.RoleGuid.AdditionalBoardMembers);
      CreateSingleUserRole(Resources.ResponsibleEmployeeAHD, Resources.ResponsibleEmployeeAHDDescription, Constants.Module.RoleGuid.ResponsibleEmployeeAHD);            
    }
    
    /// <summary>
    /// Создать правило Голосование.
    /// </summary>
    public static void CreateVotingDefaultApprovalRule()
    {
      InitializationLogger.Debug("Init: Create default voting approval rule");
            
      var isRulealreadyCreated = Sungero.Docflow.ApprovalRuleBases.GetAll().Any(r => r.IsDefaultRule == true && r.Name == Constants.Module.VotingApprovalRuleName);            
      var docKindAgenda = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Sungero.Meetings.PublicConstants.Module.AgendaKind);
      if (isRulealreadyCreated || docKindAgenda == null)
        return;
            
      var stages = new List<Enumeration> { Sungero.Docflow.ApprovalStage.StageType.SimpleAgr, Sungero.Docflow.ApprovalStage.StageType.Notice };
      var rule = Sungero.Docflow.ApprovalRules.Create();
      rule.Status = Sungero.Docflow.ApprovalRuleBase.Status.Active;
      rule.Name = Constants.Module.VotingApprovalRuleName;
      rule.DocumentFlow = Sungero.Docflow.ApprovalRuleBase.DocumentFlow.Inner;
      rule.IsDefaultRule = true;
      rule.DocumentKinds.AddNew().DocumentKind = docKindAgenda;
      
      Sungero.Docflow.PublicInitializationFunctions.Module.SetRuleStages(rule, stages);
      Sungero.Docflow.PublicFunctions.ApprovalRuleBase.CreateAutoTransitions(rule);
      rule.Save();     
    }


    /// <summary>
    /// Создание записи нового типа сценария "Этап обновления результатов голосования в совещании".
    /// </summary>
    public static void CreateApprovalVotingUpdateStage()
    {
      InitializationLogger.DebugFormat("Init: Create Stage of Updating voting results in meeting.");
      if (litiko.CollegiateAgencies.ApprovalVotingUpdateStages.GetAll().Any())
        return;
      var stage = litiko.CollegiateAgencies.ApprovalVotingUpdateStages.Create();
      stage.Name = "Обновление результатов голосования в совещании";
      stage.TimeoutInHours = 4;
      stage.Save();
    }    
    
    /// <summary>
    /// Создать роль с одним участником.
    /// </summary>
    /// <param name="roleName">Название роли.</param>
    /// <param name="roleDescription">Описание роли.</param>
    /// <param name="roleGuid">Guid роли. Игнорирует имя.</param>
    /// <returns>Новая роль.</returns>
    [Public]
    public static IRole CreateSingleUserRole(string roleName, string roleDescription, Guid roleGuid)
    {
      InitializationLogger.DebugFormat("Init: Create Role {0}", roleName);
      var role = Roles.GetAll(r => r.Sid == roleGuid).FirstOrDefault();
      
      if (role == null)
      {
        role = Roles.Create();
        role.Name = roleName;
        role.Description = roleDescription;
        role.Sid = roleGuid;
        role.IsSystem = true;
        role.IsSingleUser = true;
        role.RecipientLinks.AddNew().Member = Users.Current;
        role.Save();
      }
      else
      {
        if (role.Name != roleName)
        {
          InitializationLogger.DebugFormat("Role '{0}'(Sid = {1}) renamed as '{2}'", role.Name, role.Sid, roleName);
          role.Name = roleName;
          role.Save();
        }
        if (role.Description != roleDescription)
        {
          InitializationLogger.DebugFormat("Role '{0}'(Sid = {1}) update Description '{2}'", role.Name, role.Sid, roleDescription);
          role.Description = roleDescription;
          role.Save();
        }
      }
      return role;
    }    
  }
}
