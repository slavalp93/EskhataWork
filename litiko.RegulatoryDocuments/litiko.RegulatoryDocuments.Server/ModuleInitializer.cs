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
      CreateDocumentKinds();
      GrantRightsOnEntities();
      CreateApprovalRole(litiko.RegulatoryDocuments.ApprovalRole.Type.ProcessManager, "Руководитель процесса");      
      CreateActOnRelevanceStage();
      CreateApprovalSheetStage();
      CreateReportsTables();
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
        Sungero.Docflow.OfficialDocuments.Info.Actions.SendForFreeApproval        
      };
      #endregion
      
      #region Акт об актуальности ВНД
      Sungero.Docflow.PublicInitializationFunctions.Module.
        CreateDocumentKind(Resources.ActOnTheRelevance,
                           Resources.ActOnTheRelevance,
                           notNumerable,
                           Sungero.Docflow.DocumentKind.DocumentFlow.Inner,
                           true,
                           false,
                           litiko.CollegiateAgencies.PublicConstants.Module.DocumentTypeGuids.Addendum,
                           actions,
                           Constants.Module.DocumentKindGuids.ActOnTheRelevance,
                           false);
      #endregion
      
      #region Лист согласования ВНД
      Sungero.Docflow.PublicInitializationFunctions.Module.
        CreateDocumentKind(Resources.ApprovalSheetIRD,
                           Resources.ApprovalSheetIRD,
                           notNumerable,
                           Sungero.Docflow.DocumentKind.DocumentFlow.Inner,
                           true,
                           false,
                           litiko.CollegiateAgencies.PublicConstants.Module.DocumentTypeGuids.Addendum,
                           actions,
                           Constants.Module.DocumentKindGuids.ApprovalSheetIRD,
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
    
    /// <summary>
    /// Создание записи нового типа сценария "Этап формирования Акта об актуальности ВНД".
    /// </summary>
    public static void CreateActOnRelevanceStage()
    {
      InitializationLogger.DebugFormat("Init: Create Stage of Creating act on relevance.");
      if (CreateActOnRelevanceStages.GetAll().Any())
        return;
      var stage = CreateActOnRelevanceStages.Create();
      stage.Name = "Формирование Акта об актуальности ВНД";
      stage.TimeoutInHours = 4;
      stage.Save();
    }
    
    /// <summary>
    /// Создание записи нового типа сценария "Этап формирования Листа согласования ВНД".
    /// </summary>
    public static void CreateApprovalSheetStage()
    {
      InitializationLogger.DebugFormat("Init: Create Stage of Creating approval sheet IRD.");
      if (CreateApprovalSheetStages.GetAll().Any())
        return;
      var stage = CreateApprovalSheetStages.Create();
      stage.Name = "Формирование Листа согласования ВНД";
      stage.TimeoutInHours = 4;
      stage.Save();
    }    
    
    /// <summary>
    /// Создание таблиц для отчетов
    /// </summary>
    public static void CreateReportsTables()
    {
      var approvalSheetIRDTableName = Constants.Module.SourceTableName;      
      Sungero.Docflow.PublicFunctions.Module.DropReportTempTables(new[] { approvalSheetIRDTableName});
      Sungero.Docflow.PublicFunctions.Module.ExecuteSQLCommandFormat(Queries.Module.CreateApprovalSheetIRDTable, new[] { approvalSheetIRDTableName });      
    }    
  }
}
