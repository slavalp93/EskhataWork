using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace litiko.DocflowEskhata.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      CreateDocumentKinds();
      GrantRightsOnDatabooks();
      CreateApprovalRole(DocflowEskhata.UnitManagerApprovalRole.Type.UnitManager, "Глава департамента Инициатора");
    }
    
    public static void GrantRightsOnDatabooks()
    {
      InitializationLogger.Debug("Init: Grant rights on databooks.");
      DocflowEskhata.IncomingLettersCategories.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Read);
      DocflowEskhata.IncomingLettersCategories.AccessRights.Save();
    }
    
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
      
      #region Входящая корреспонденция
      Sungero.Docflow.PublicInitializationFunctions.Module.
        CreateDocumentKind(litiko.DocflowEskhata.Resources.IncomingCorrespondence,
                           litiko.DocflowEskhata.Resources.IncomingCorrespondence,
                           registrable,
                           Sungero.Docflow.DocumentKind.DocumentFlow.Incoming,
                           true,
                           false,
                           Constants.Module.DocumentTypeGuids.IncomingLetter,
                           actions,
                           Constants.Module.DocumentKindGuids.IncomingCorrespondence,
                           false);
      #endregion
      
      #region Входящая корреспонденция от исполнительных органов
      Sungero.Docflow.PublicInitializationFunctions.Module.
        CreateDocumentKind(litiko.DocflowEskhata.Resources.IncomingCorrespondenceExecutive,
                           litiko.DocflowEskhata.Resources.IncomingCorrespondenceExecutive,
                           registrable,
                           Sungero.Docflow.DocumentKind.DocumentFlow.Incoming,
                           true,
                           false,
                           Constants.Module.DocumentTypeGuids.IncomingLetter,
                           actions,
                           Constants.Module.DocumentKindGuids.IncomingCorrespondenceExecutive,
                           false);
      #endregion
      
      #region Входящая корреспонденция от налогового комитета
      Sungero.Docflow.PublicInitializationFunctions.Module.
        CreateDocumentKind(litiko.DocflowEskhata.Resources.IncomingCorrespondenceTax,
                           litiko.DocflowEskhata.Resources.IncomingCorrespondenceTax,
                           registrable,
                           Sungero.Docflow.DocumentKind.DocumentFlow.Incoming,
                           true,
                           false,
                           Constants.Module.DocumentTypeGuids.IncomingLetter,
                           actions,
                           Constants.Module.DocumentKindGuids.IncomingCorrespondenceTax,
                           false);
      #endregion
      
      #region Входящая корреспонденция от НБТ
      Sungero.Docflow.PublicInitializationFunctions.Module.
        CreateDocumentKind(litiko.DocflowEskhata.Resources.IncomingCorrespondenceNBT,
                           litiko.DocflowEskhata.Resources.IncomingCorrespondenceNBT,
                           registrable,
                           Sungero.Docflow.DocumentKind.DocumentFlow.Incoming,
                           true,
                           false,
                           Constants.Module.DocumentTypeGuids.IncomingLetter,
                           actions,
                           Constants.Module.DocumentKindGuids.IncomingCorrespondenceNBT,
                           false);
      #endregion
      
      #region Входящая корреспонденция от филиалов
      Sungero.Docflow.PublicInitializationFunctions.Module.
        CreateDocumentKind(litiko.DocflowEskhata.Resources.IncomingCorrespondenceBranches,
                           litiko.DocflowEskhata.Resources.IncomingCorrespondenceBranches,
                           registrable,
                           Sungero.Docflow.DocumentKind.DocumentFlow.Incoming,
                           true,
                           false,
                           Constants.Module.DocumentTypeGuids.IncomingLetter,
                           actions,
                           Constants.Module.DocumentKindGuids.IncomingCorrespondenceBranches,
                           false);
      #endregion
      
      #region Входящие письма/ обращения граждан
      Sungero.Docflow.PublicInitializationFunctions.Module.
        CreateDocumentKind(litiko.DocflowEskhata.Resources.IncomingLettersCitizens,
                           litiko.DocflowEskhata.Resources.IncomingLettersCitizens,
                           registrable,
                           Sungero.Docflow.DocumentKind.DocumentFlow.Incoming,
                           true,
                           false,
                           Constants.Module.DocumentTypeGuids.IncomingLetter,
                           actions,
                           Constants.Module.DocumentKindGuids.IncomingLettersCitizens,
                           false);
      #endregion
      
      #region Входящие письма/ обращения организаций
      Sungero.Docflow.PublicInitializationFunctions.Module.
        CreateDocumentKind(litiko.DocflowEskhata.Resources.IncomingLettersOrganisations,
                           litiko.DocflowEskhata.Resources.IncomingLettersOrganisations,
                           registrable,
                           Sungero.Docflow.DocumentKind.DocumentFlow.Incoming,
                           true,
                           false,
                           Constants.Module.DocumentTypeGuids.IncomingLetter,
                           actions,
                           Constants.Module.DocumentKindGuids.IncomingLettersOrganisations,
                           false);
      #endregion
      
      #region Входящая корреспонденция адресованная в ГО
      Sungero.Docflow.PublicInitializationFunctions.Module.
        CreateDocumentKind(litiko.DocflowEskhata.Resources.IncomingCorrespondenceHeadOffice,
                           litiko.DocflowEskhata.Resources.IncomingCorrespondenceHeadOffice,
                           registrable,
                           Sungero.Docflow.DocumentKind.DocumentFlow.Incoming,
                           true,
                           false,
                           Constants.Module.DocumentTypeGuids.IncomingLetter,
                           actions,
                           Constants.Module.DocumentKindGuids.IncomingCorrespondenceHeadOffice,
                           false);
      #endregion
      
      #region Исходящая корреспонденция
      Sungero.Docflow.PublicInitializationFunctions.Module.
        CreateDocumentKind(litiko.DocflowEskhata.Resources.OutgoingCorrespondence,
                           litiko.DocflowEskhata.Resources.OutgoingCorrespondence,
                           registrable,
                           Sungero.Docflow.DocumentKind.DocumentFlow.Outgoing,
                           true,
                           false,
                           Constants.Module.DocumentTypeGuids.OutgoingLetter,
                           actions,
                           Constants.Module.DocumentKindGuids.OutgoingCorrespondence,
                           false);
      #endregion
      
      #region Исходящая корреспонденция в НБТ
      Sungero.Docflow.PublicInitializationFunctions.Module.
        CreateDocumentKind(litiko.DocflowEskhata.Resources.OutgoingCorrespondenceNBT,
                           litiko.DocflowEskhata.Resources.OutgoingCorrespondenceNBT,
                           registrable,
                           Sungero.Docflow.DocumentKind.DocumentFlow.Outgoing,
                           true,
                           false,
                           Constants.Module.DocumentTypeGuids.OutgoingLetter,
                           actions,
                           Constants.Module.DocumentKindGuids.OutgoingCorrespondenceNBT,
                           false);
      #endregion
      
      
      #region Регистрационно-контрольный лист
      Sungero.Docflow.PublicInitializationFunctions.Module.
        CreateDocumentKind(litiko.DocflowEskhata.Resources.ChecklistKindName,
                           litiko.DocflowEskhata.Resources.ChecklistKindName,
                           notNumerable,
                           Sungero.Docflow.DocumentKind.DocumentFlow.Inner,
                           true,
                           false,
                           Constants.Module.DocumentTypeGuids.Addendum,
                           new Sungero.Domain.Shared.IActionInfo[]
                           {
                             
                           },
                           Constants.Module.DocumentKindGuids.Checklist,
                           false);
      #endregion
    }
    
    public static void CreateReportsTables()
    {
      var envelopesReportsTableName = Constants.EnvelopeB4Report.EnvelopesTableName;
      
      Sungero.Docflow.PublicFunctions.Module.DropReportTempTables(new[] {
                                                            envelopesReportsTableName
                                                          });
      
      Sungero.Docflow.PublicFunctions.Module.ExecuteSQLCommandFormat(Queries.EnvelopeB4Report.CreateEnvelopesTable, new[] { envelopesReportsTableName });
    }
    
    /// <summary>
    /// Создание роли.
    /// </summary>
    public static void CreateApprovalRole(Enumeration roleType, string description)
    {
      var role = UnitManagerApprovalRoles.GetAll().Where(r => Equals(r.Type, roleType)).FirstOrDefault();
      // Проверяет наличие роли.
      if (role == null)
      {
        role = UnitManagerApprovalRoles.Create();
        role.Type = roleType;
      }
      role.Description = description;
      role.Save();
      InitializationLogger.Debug($"Создана роль '{description}'");
    }
  }
}
