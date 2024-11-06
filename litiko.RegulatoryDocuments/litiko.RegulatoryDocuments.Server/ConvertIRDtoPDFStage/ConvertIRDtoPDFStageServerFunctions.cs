using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.RegulatoryDocuments.ConvertIRDtoPDFStage;

namespace litiko.RegulatoryDocuments.Server
{
  partial class ConvertIRDtoPDFStageFunctions
  {
    public override Sungero.Docflow.Structures.ApprovalFunctionStageBase.ExecutionResult Execute(Sungero.Docflow.IApprovalTask approvalTask)
    {
      Logger.DebugFormat("ConvertIRDtoPDFStage. Start execute convert to pdf for task id: {0}, start id: {1}.", approvalTask.Id, approvalTask.StartId);
      
      var result = base.Execute(approvalTask);            
      
      var document = approvalTask.DocumentGroup.OfficialDocuments.FirstOrDefault();
      if (document == null)
      {
        Logger.ErrorFormat("ConvertIRDtoPDFStage. Primary document not found. task id: {0}, start id: {1}", approvalTask.Id, approvalTask.StartId);
        return this.GetErrorResult(Sungero.Docflow.Resources.PrimaryDocumentNotFoundError);
      }            
      
      if (!document.HasVersions)
      {
        Logger.ErrorFormat("ConvertIRDtoPDFStage. Document with Id {0} has no version.", document.Id);
        return this.GetErrorResult(string.Format("Document with Id {0} has no version.", document.Id));
      }

      var suitableVersions = new List<Sungero.Content.IElectronicDocumentVersions>();
      var versionOnRU = document.Versions.Where(x => x.Note == litiko.RegulatoryDocuments.RegulatoryDocuments.Resources.VersionNoteOnRU.ToString()).LastOrDefault();
      if (versionOnRU != null)
        suitableVersions.Add(versionOnRU);      
      var versionOnTJ = document.Versions.Where(x => x.Note == litiko.RegulatoryDocuments.RegulatoryDocuments.Resources.VersionNoteOnTJ.ToString()).LastOrDefault();
      if (versionOnTJ != null)
        suitableVersions.Add(versionOnTJ);
      
      if (!suitableVersions.Any())
      {
        Logger.ErrorFormat("ConvertIRDtoPDFStage. Document with Id {0} has no version with note version on RU or version on TG.", document.Id);
        return this.GetErrorResult(string.Format("Document with Id {0} has no version with note version on RU or version on TG.", document.Id));        
      }
      
      var versionsToConvert = new List<Sungero.Content.IElectronicDocumentVersions>();      
      foreach (var version in suitableVersions)
      {
        var versionExtension = version.BodyAssociatedApplication.Extension.ToLower();
        var versionExtensionIsSupported = Sungero.Docflow.PublicFunctions.OfficialDocument.CheckPdfConvertibilityByExtension(document, versionExtension);
        if (!versionExtensionIsSupported)
        {
          Logger.DebugFormat("ConvertIRDtoPDFStage. Document with Id {0} version Id {1} unsupported format {2}.", document.Id, version.Id, versionExtension);
          continue;
        }
        
        var lockInfo = Locks.GetLockInfo(version.Body);
        if (lockInfo.IsLocked)
        {
          Logger.DebugFormat("ConvertIRDtoPDFStage. Document with Id {0} locked {1}.", document.Id, lockInfo.OwnerName);
          return this.GetRetryResult(string.Format(Sungero.Docflow.ApprovalConvertPdfStages.Resources.ConvertPdfLockError, document.Name, document.Id, lockInfo.OwnerName));
        }        
        
        versionsToConvert.Add(version);
      }
      
      if (!versionsToConvert.Any())
      {
        Logger.ErrorFormat("ConvertIRDtoPDFStage. Document with Id {0} has no suitable versions with supported format to convert.", document.Id);
        return this.GetErrorResult(string.Format("Document with Id {0} has no suitable versions with supported format to convert.", document.Id));       
      }
      
      foreach (var version in versionsToConvert)
      {
        try
        {
          Logger.DebugFormat("ConvertIRDtoPDFStage. Start convert to pdf for document id {0} version id {1}.", document.Id, version.Id);
          var conversionResult = Functions.ConvertIRDtoPDFStage.ConvertToPdfVersion(_obj, document, version);
          if (conversionResult.HasErrors)
          {
            Logger.ErrorFormat("ConvertIRDtoPDFStage. Convert to pdf error {0}. Document Id {1}, Version Id {2}", conversionResult.ErrorMessage, document.Id, version.Id);
            result = this.GetRetryResult(string.Empty);
          }
        }
        catch (Exception ex)
        {
          Logger.ErrorFormat("ConvertIRDtoPDFStage. Convert to pdf error. Document Id {0}, Version Id {1}", ex, document.Id, version.Id);
          result = this.GetRetryResult(string.Empty);
        }        
      }            
      
      Logger.DebugFormat("ConvertIRDtoPDFStage. Done execute convert to pdf for task id {0}, success: {1}, retry: {2}", approvalTask.Id, result.Success, result.Retry);
      
      return result;
    }
    
    public virtual Eskhata.Module.Docflow.Structures.Module.IConversionToPdfResult ConvertToPdfVersion(Sungero.Docflow.IOfficialDocument document, Sungero.Content.IElectronicDocumentVersions version)
    {      
      var signature = Sungero.Docflow.PublicFunctions.OfficialDocument.GetSignatureForMark(document, version.Id);
      var signatureMark = (signature != null) ? Sungero.Docflow.PublicFunctions.OfficialDocument.GetSignatureMarkAsHtml(document, version.Id) : string.Empty;
      
      return litiko.Eskhata.Module.Docflow.PublicFunctions.Module.GeneratePublicBodyWithSignatureMarkEskhata(document, version.Id, signatureMark);      
    }    
  }
}