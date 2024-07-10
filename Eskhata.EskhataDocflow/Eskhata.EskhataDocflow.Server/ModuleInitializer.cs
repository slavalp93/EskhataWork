using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace Eskhata.EskhataDocflow.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      CreateDocumentKinds();
      GrantRightsOnDatabooks();
    }
    
    public static void GrantRightsOnDatabooks()
    {
      InitializationLogger.Debug("Init: Grant rights on databooks.");
      EskhataDocflow.IncomingLettersCategories.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Read);
      EskhataDocflow.IncomingLettersCategories.AccessRights.Save();
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
        CreateDocumentKind(Eskhata.EskhataDocflow.Resources.IncomingCorrespondence,
                           Eskhata.EskhataDocflow.Resources.IncomingCorrespondence,
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
        CreateDocumentKind(Eskhata.EskhataDocflow.Resources.IncomingCorrespondenceExecutive,
                           Eskhata.EskhataDocflow.Resources.IncomingCorrespondenceExecutive,
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
        CreateDocumentKind(Eskhata.EskhataDocflow.Resources.IncomingCorrespondenceTax,
                           Eskhata.EskhataDocflow.Resources.IncomingCorrespondenceTax,
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
        CreateDocumentKind(Eskhata.EskhataDocflow.Resources.IncomingCorrespondenceNBT,
                           Eskhata.EskhataDocflow.Resources.IncomingCorrespondenceNBT,
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
        CreateDocumentKind(Eskhata.EskhataDocflow.Resources.IncomingCorrespondenceBranches,
                           Eskhata.EskhataDocflow.Resources.IncomingCorrespondenceBranches,
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
        CreateDocumentKind(Eskhata.EskhataDocflow.Resources.IncomingLettersCitizens,
                           Eskhata.EskhataDocflow.Resources.IncomingLettersCitizens,
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
        CreateDocumentKind(Eskhata.EskhataDocflow.Resources.IncomingLettersOrganisations,
                           Eskhata.EskhataDocflow.Resources.IncomingLettersOrganisations,
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
        CreateDocumentKind(Eskhata.EskhataDocflow.Resources.IncomingCorrespondenceHeadOffice,
                           Eskhata.EskhataDocflow.Resources.IncomingCorrespondenceHeadOffice,
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
        CreateDocumentKind(Eskhata.EskhataDocflow.Resources.OutgoingCorrespondence,
                           Eskhata.EskhataDocflow.Resources.OutgoingCorrespondence,
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
        CreateDocumentKind(Eskhata.EskhataDocflow.Resources.OutgoingCorrespondenceNBT,
                           Eskhata.EskhataDocflow.Resources.OutgoingCorrespondenceNBT,
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
        CreateDocumentKind(Eskhata.EskhataDocflow.Resources.ChecklistKindName,
                           Eskhata.EskhataDocflow.Resources.ChecklistKindName,
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
  }
}
