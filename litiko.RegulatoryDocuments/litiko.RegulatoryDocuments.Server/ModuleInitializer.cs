using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace litiko.RegulatoryDocuments.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      CreateDocumentTypes();
      GrantRightsOnEntities();
      CreateApprovalRole(litiko.RegulatoryDocuments.ApprovalRole.Type.ProcessManager, "Руководитель процесса");
    }
    
    /// <summary>
    /// Создать типы документов.
    /// </summary>
    public static void CreateDocumentTypes()
    {
      InitializationLogger.Debug("Init: Create document types");      
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentType(Resources.RegulatoryDocumentType, RegulatoryDocument.ClassTypeGuid, Sungero.Docflow.DocumentType.DocumentFlow.Inner, true);
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
        DeadlineForRevisions.AccessRights.Grant(roleAllUsers, DefaultAccessRightsTypes.Read);
        DeadlineForRevisions.AccessRights.Save();
        
        OrganForApprovings.AccessRights.Grant(roleAllUsers, DefaultAccessRightsTypes.Read);
        OrganForApprovings.AccessRights.Save();
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
  }
}
