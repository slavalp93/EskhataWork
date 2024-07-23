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
      var result = base.Execute(approvalTask);
      var document = approvalTask.DocumentGroup.OfficialDocuments.FirstOrDefault();
      if (document?.HasVersions != true)
      {
        result.Success = false;
        result.ErrorMessage = string.Format("Document ID={0} has no versions", document.Id);
        return result;
      }
      
      try
      {
        Logger.DebugFormat("ConvertOrdToPdf. Start convert to pdf for document id {0}.", document.Id);
        var conversionResult = ConvertToPdf(document);
        if (conversionResult.HasErrors)
        {
          Logger.ErrorFormat("ConvertOrdToPdf. Convert to pdf error {0}. Document Id {1}, Version Id {2}", conversionResult.ErrorMessage, document.Id, document.LastVersion.Id);
          result = this.GetRetryResult(string.Empty);
        }
        
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("ConvertOrdToPdf. Convert to pdf error. Document Id {0}, Version Id {1}", ex, document.Id, document.LastVersion.Id);
        result = this.GetRetryResult(string.Empty);
      }
      if (!result.Success)
        return result;
      
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
      return result;
    }
    
    public virtual Eskhata.Module.Docflow.Structures.Module.IConversionToPdfResult ConvertToPdf(Sungero.Docflow.IOfficialDocument document)
    {
      var lastVersionId = document.LastVersion.Id;
      var signature = Sungero.Docflow.PublicFunctions.OfficialDocument.GetSignatureForMark(document, lastVersionId);
      var signatureMark = (signature != null) ? Sungero.Docflow.PublicFunctions.OfficialDocument.GetSignatureMarkAsHtml(document, lastVersionId) : string.Empty;
      
      return litiko.Eskhata.Module.Docflow.PublicFunctions.Module.GeneratePublicBodyWithSignatureMarkEskhata(document, lastVersionId, signatureMark);
    }
  }
}