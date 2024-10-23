using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.RegulatoryDocuments.CreateApprovalSheetStage;

namespace litiko.RegulatoryDocuments.Server
{
  partial class CreateApprovalSheetStageFunctions
  {
    /// <summary>
    /// Выполнить сценарий.
    /// </summary>
    /// <param name="approvalTask">Задача на согласование по регламенту.</param>
    /// <returns>Результат выполнения сценария.</returns>
    public override Sungero.Docflow.Structures.ApprovalFunctionStageBase.ExecutionResult Execute(Sungero.Docflow.IApprovalTask approvalTask)
    {
      var result = base.Execute(approvalTask);
      var document = approvalTask.DocumentGroup.OfficialDocuments.FirstOrDefault();
      if (document == null)
        return this.GetErrorResult(Resources.DocumentNotFound);
      
      var regulatoryDocument = RegulatoryDocuments.As(document);
      if (regulatoryDocument == null)
        return this.GetErrorResult(Resources.DocumentIsNotIRG);
      
      var docKindApprovalSheetIRD = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(PublicConstants.Module.DocumentKindGuids.ApprovalSheetIRD);
      if (docKindApprovalSheetIRD == null)
        return this.GetErrorResult(CreateApprovalSheetStages.Resources.DocKindNotFound);
        
      try
      {
        var addendum = litiko.Eskhata.Addendums.Create();
        addendum.DocumentKind = docKindApprovalSheetIRD;
        addendum.LeadingDocument = regulatoryDocument;
        
        var report = Reports.GetApprovalSheetIRD();
        report.Entity = regulatoryDocument;
        report.ExportTo(addendum);
        
        addendum.Subject = null;
        addendum.Save();        
      }
      catch (Exception ex)
      {
        result = this.GetRetryResult(string.Empty);
      }
      return result;
    }
  }
}