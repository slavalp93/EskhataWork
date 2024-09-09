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
      CreateApprovalRole(litiko.CollegiateAgencies.ApprovalRole.Type.SecretaryByCat, "Секретарь по категории заседания");
      CreateApprovalRole(litiko.CollegiateAgencies.ApprovalRole.Type.PresidentByCat, "Председатель по категории заседания");
      CreateApprovalRole(litiko.CollegiateAgencies.ApprovalRole.Type.MeetingMembers, "Участники заседания");
      CreateApprovalRole(litiko.CollegiateAgencies.ApprovalRole.Type.MeetingInvited, "Приглашенные сотрудники");
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
        MeetingCategories.AccessRights.Grant(roleAllUsers, DefaultAccessRightsTypes.Read);
        MeetingCategories.AccessRights.Save();

        MeetingMethods.AccessRights.Grant(roleAllUsers, DefaultAccessRightsTypes.Read);
        MeetingMethods.AccessRights.Save();

        Projectsolutions.AccessRights.Grant(roleAllUsers, DefaultAccessRightsTypes.Create);
        Projectsolutions.AccessRights.Save();
      }
      
      // "Ответственные за совещания"
      var meetingResponsible = Roles.GetAll().Where(n => n.Sid == Sungero.Meetings.PublicConstants.Module.MeetingResponsibleRole).FirstOrDefault();
      if (meetingResponsible != null)
      {
        MeetingCategories.AccessRights.Grant(meetingResponsible, DefaultAccessRightsTypes.Create);
        MeetingCategories.AccessRights.Save();
        
        MeetingMethods.AccessRights.Grant(meetingResponsible, DefaultAccessRightsTypes.Create);
        MeetingMethods.AccessRights.Save();
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
    }
  }
}
