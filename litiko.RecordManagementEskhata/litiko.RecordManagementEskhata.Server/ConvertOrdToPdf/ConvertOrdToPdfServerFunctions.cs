using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.RecordManagementEskhata.ConvertOrdToPdf;

namespace litiko.RecordManagementEskhata.Server
{
  partial class ConvertOrdToPdfFunctions
  {
    public override Sungero.Docflow.Structures.ApprovalFunctionStageBase.ExecutionResult Execute(Sungero.Docflow.IApprovalTask approvalTask)
    {
      var result = Sungero.Docflow.Structures.ApprovalFunctionStageBase.ExecutionResult.Create();
      var document = approvalTask.DocumentGroup.OfficialDocuments.FirstOrDefault();
      if (document?.HasVersions != true)
      {
        result.Success = false;
        result.ErrorMessage = string.Format("Document ID={0} has no versions", document.Id);
        return result;
      }
      
      try
      {
        var version = Sungero.Docflow.PublicFunctions.OfficialDocument.GetBodyToConvertToPdf(document, document.LastVersion, true);
        using (var bodyStream = new System.IO.MemoryStream(version.Body))
        {
          using (var pdfDocumentStream = Sungero.Docflow.IsolatedFunctions.PdfConverter.GeneratePdf(bodyStream, version.Extension))
          {
            if (pdfDocumentStream != null)
            {
              document.LastVersion.PublicBody.Write(pdfDocumentStream);
              document.LastVersion.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension(Sungero.Docflow.PublicConstants.OfficialDocument.PdfExtension);
              document.Save();
            }
          }
        }
      }
      catch (Exception ex)
      {
        result.Success = false;
        result.ErrorMessage = string.Format("Failed to generate PDF from last version body Document ID={0}. Error message = {1}", document.Id, ex.Message);
        Logger.ErrorFormat("Failed to generate PDF from last version body Document ID={0}.", ex, document.Id);
        return result;
      }
      
      try
      {
        var rep = Reports.GetApprovalSheetOrd();
        rep.Document = document;
        using (var approvalSheetPdf = rep.Export())
        {
          using (var pdfDocumentStream = Sungero.Docflow.IsolatedFunctions.PdfConverter.MergePdf(document.LastVersion.PublicBody.Read(), approvalSheetPdf))
          {
            if (pdfDocumentStream != null)
            {
              document.LastVersion.PublicBody.Write(pdfDocumentStream);
              document.LastVersion.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension(Sungero.Docflow.PublicConstants.OfficialDocument.PdfExtension);
              document.Save();
            }
          }
        }
      }
      catch (Exception ex)
      {
        result.Success = false;
        result.ErrorMessage = string.Format("Failed to export approval sheet report or merge two PDFs. Document ID={0}. Error message = {1}", document.Id, ex.Message);
        Logger.ErrorFormat("Failed to export approval sheet report or merge two PDFs. Document ID={0}.", ex, document.Id);
        return result;
      }
      return base.Execute(approvalTask);
    }
  }
}