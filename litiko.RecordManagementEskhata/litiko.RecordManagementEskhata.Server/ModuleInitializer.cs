using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace litiko.RecordManagementEskhata.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      CreateDocumentKinds();
      CreateReportsTables();
      CreateConvertOrdToPdfStage();
      Reports.AccessRights.Grant(Reports.GetDocflowReport().Info, Roles.AllUsers, DefaultReportAccessRightsTypes.Execute);
      Reports.AccessRights.Grant(Reports.GetAcquaintanceApprovalSheet().Info, Roles.AllUsers, DefaultReportAccessRightsTypes.Execute);
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
      
      #region Приказ от имени Председателя Правления
      Sungero.Docflow.PublicInitializationFunctions.Module.
        CreateDocumentKind(litiko.RecordManagementEskhata.Resources.CharmanOrder,
                           litiko.RecordManagementEskhata.Resources.CharmanOrder,
                           registrable,
                           Sungero.Docflow.DocumentKind.DocumentFlow.Inner,
                           true,
                           false,
                           Constants.Module.DocumentTypeGuids.Order,
                           actions,
                           Constants.Module.DocumentKindGuids.CharmanOrder,
                           true);
      #endregion
      
      #region Приказ по утверждению нормативного документа
      Sungero.Docflow.PublicInitializationFunctions.Module.
        CreateDocumentKind(litiko.RecordManagementEskhata.Resources.NormativeOrder,
                           litiko.RecordManagementEskhata.Resources.NormativeOrder,
                           registrable,
                           Sungero.Docflow.DocumentKind.DocumentFlow.Inner,
                           true,
                           false,
                           Constants.Module.DocumentTypeGuids.Order,
                           actions,
                           Constants.Module.DocumentKindGuids.NormativeOrder,
                           false);
      #endregion
      
      #region Распоряжение по департаменту
      Sungero.Docflow.PublicInitializationFunctions.Module.
        CreateDocumentKind(litiko.RecordManagementEskhata.Resources.DepartmentalDirective,
                           litiko.RecordManagementEskhata.Resources.DepartmentalDirective,
                           registrable,
                           Sungero.Docflow.DocumentKind.DocumentFlow.Inner,
                           true,
                           false,
                           Constants.Module.DocumentTypeGuids.CompanyDirective,
                           actions,
                           Constants.Module.DocumentKindGuids.DepartmentalDirective,
                           true);
      #endregion

      #region Приказ по филиалу
      Sungero.Docflow.PublicInitializationFunctions.Module.
        CreateDocumentKind(litiko.RecordManagementEskhata.Resources.BranchOrder,
                           litiko.RecordManagementEskhata.Resources.BranchOrder,
                           registrable,
                           Sungero.Docflow.DocumentKind.DocumentFlow.Inner,
                           true,
                           false,
                           Constants.Module.DocumentTypeGuids.Order,
                           actions,
                           Constants.Module.DocumentKindGuids.BranchOrder,
                           false);
      #endregion
    }
    
    public static void CreateReportsTables()
    {
      var approvalSheetOrdTableName = Constants.ApprovalSheetOrd.SourceTableName;
      var docflowReportTableName = Constants.DocflowReport.SourceTableName;
      Sungero.Docflow.PublicFunctions.Module.DropReportTempTables(new[] { approvalSheetOrdTableName, docflowReportTableName });

      Sungero.Docflow.PublicFunctions.Module.ExecuteSQLCommandFormat(Queries.ApprovalSheetOrd.CreateApprovalSheetOrdSourceTable, new[] { approvalSheetOrdTableName });
      Sungero.Docflow.PublicFunctions.Module.ExecuteSQLCommandFormat(Queries.DocflowReport.CreateDocflowReportSourceTable, new[] { docflowReportTableName });
    }
    
    public static void CreateConvertOrdToPdfStage()
    {
      if (ConvertOrdToPdfs.GetAll().Any())
        return;
      var record = ConvertOrdToPdfs.Create();
      record.Name = "Преобразование в PDF ОРД";
      record.TimeoutInHours = 4;
      record.Save();
    }
  }
}
