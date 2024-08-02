using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace litiko.Archive.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {      
      CreateDocumentTypes();
      CreateDocumentKinds();
      GrantRightsOnEntities();
      CreateApprovalRole(litiko.Archive.ApprovalRole.Type.Archivist, "Архивариус");
    }

    /// <summary>
    /// Создать типы документов.
    /// </summary>
    public static void CreateDocumentTypes()
    {
      InitializationLogger.Debug("Init: Create document types");      
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentType(Resources.ArchiveListType, ArchiveList.ClassTypeGuid, Sungero.Docflow.DocumentType.DocumentFlow.Inner, true);
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
        Sungero.Docflow.OfficialDocuments.Info.Actions.SendActionItem,
        Sungero.Docflow.OfficialDocuments.Info.Actions.SendForFreeApproval,
        Sungero.Docflow.OfficialDocuments.Info.Actions.SendForApproval,
        Sungero.Docflow.OfficialDocuments.Info.Actions.SendForAcquaintance };
      #endregion
      
      #region Приказ о сдаче документов в архив
      Sungero.Docflow.PublicInitializationFunctions.Module.
        CreateDocumentKind(litiko.Archive.Resources.OrderArchiveKind,
                           litiko.Archive.Resources.OrderArchiveKind,
                           registrable,
                           Sungero.Docflow.DocumentKind.DocumentFlow.Inner,
                           true,
                           false,
                           Constants.Module.DocumentTypeGuids.Order,
                           actions,
                           Constants.Module.DocumentKindGuids.OrderArchive,
                           false);
      #endregion

      #region График сдачи документов в архив
      Sungero.Docflow.PublicInitializationFunctions.Module.
        CreateDocumentKind(litiko.Archive.Resources.ArchiveScheduleKind,
                           litiko.Archive.Resources.ArchiveScheduleKind,
                           notNumerable,
                           Sungero.Docflow.DocumentKind.DocumentFlow.Inner,
                           true,
                           false,
                           Constants.Module.DocumentTypeGuids.SimpleDocument,
                           actions,
                           Constants.Module.DocumentKindGuids.ArchiveShedule,
                           false);
      #endregion

      #region Список документов для передачи в архив
      Sungero.Docflow.PublicInitializationFunctions.Module.
      CreateDocumentKind(litiko.Archive.Resources.ArchiveListKind,
                           litiko.Archive.Resources.ArchiveListKind,
                           numerable,
                           Sungero.Docflow.DocumentKind.DocumentFlow.Inner,
                           true,
                           true,
                           Constants.Module.DocumentTypeGuids.ArchiveList,
                           actions,
                           Constants.Module.DocumentKindGuids.ArchiveList,
                           true);
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
        Archives.AccessRights.Grant(roleAllUsers, DefaultAccessRightsTypes.Read);
        Archives.AccessRights.Save();
      }

      // Делопроизводители
      var clerks = Sungero.Docflow.PublicFunctions.DocumentRegister.Remote.GetClerks();
      if (clerks != null)
      {
        ArchiveLists.AccessRights.Grant(clerks, DefaultAccessRightsTypes.Create);
        ArchiveLists.AccessRights.Save();
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
