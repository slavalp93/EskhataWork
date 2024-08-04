using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.RecordManagementEskhata.Server
{
  public class ModuleFunctions
  {
    [Public]
    public List<Structures.ConvertOrdToPdf.IApprovalRow> GetApprovalSheetOrdReportTable(Sungero.Docflow.IOfficialDocument document, string reportSessionId)
    {
      var approvalList = new List<Structures.ConvertOrdToPdf.IApprovalRow>();
      var filteredSignatures = new Dictionary<string, Sungero.Domain.Shared.ISignature>();
      
      var setting = Sungero.Docflow.PublicFunctions.PersonalSetting.GetPersonalSettings(null);
      var showNotApproveSign = setting != null ? setting.ShowNotApproveSign == true : false;
      int i = 1;
      foreach (var version in document.Versions.OrderByDescending(v => v.Created))
      {
        var versionSignatures = Signatures.Get(version).Where(s => (showNotApproveSign || s.SignatureType == SignatureType.Endorsing)
                                                              && s.IsExternal != true
                                                              && !filteredSignatures.ContainsKey(GetSignatureKey(s, version.Number.Value)));
        var lastSignaturesInVersion = versionSignatures
          .GroupBy(v => GetSignatureKey(v, version.Number.Value))
          .Select(grouping => grouping.Where(s => s.SigningDate == grouping.Max(last => last.SigningDate)).First());
        
        foreach (Sungero.Domain.Shared.ISignature signature in lastSignaturesInVersion)
        {
          filteredSignatures.Add(GetSignatureKey(signature, version.Number.Value), signature);
          if (!signature.IsValid)
            foreach (var error in signature.ValidationErrors)
              Logger.DebugFormat("UpdateApprovalSheetReportTable: reportSessionId {0}, document {1}, version {2}, signatory {3}, substituted user {7}, signature {4}, with error {5} - {6}",
                                 reportSessionId, document.Id, version.Number,
                                 signature.Signatory != null ? signature.Signatory.Name : signature.SignatoryFullName, signature.Id, error.ErrorType, error.Message,
                                 signature.SubstitutedUser != null ? signature.SubstitutedUser.Name : string.Empty);
          
          // Dmitriev_IA: signature.AdditionalInfo формируется в Employee в действии "Получение информации о подписавшем".
          //              Может содержать лишние пробелы в должности сотрудника. US 89747.
          var employeeName = string.Empty;
          var additionalInfos = (signature.AdditionalInfo ?? string.Empty)
            .Split(new char[] { '|' }, StringSplitOptions.None)
            .Select(x => x.Trim())
            .ToList();
          if (signature.SubstitutedUser == null)
          {
            employeeName = signature.SignatoryFullName;
          }
          else
          {
            // Замещающий / Система.
            var signatoryText = AddEndOfLine(signature.SignatoryFullName);
            
            // Замещаемый.
            var substitutedUserText = signature.SubstitutedUserFullName;
            
            // Система за замещаемого.
            var onBehalfOfText = AddEndOfLine(Sungero.Docflow.ApprovalTasks.Resources.OnBehalfOf);
            employeeName = string.Format("{0}{1}{2}", signatoryText, onBehalfOfText, substitutedUserText);
            
          }
          var row = Structures.ConvertOrdToPdf.ApprovalRow.Create();
          row.RowIndex = i++;
          var signatory = Sungero.Company.Employees.As(signature.Signatory);
          if (signatory?.JobTitle != null)
            row.JobTitle = signatory?.JobTitle.Name;
          row.SignerName = employeeName;
          row.SignResult = GetEndorsingResultFromSignature(signature, false);
          row.Comment = HasApproveWithSuggestionsMark(signature.Comment)
            ? RemoveApproveWithSuggestionsMark(signature.Comment)
            : signature.Comment;
          string jobTitle = Eskhata.JobTitles.As(signatory?.JobTitle)?.NameTGlitiko;
          row.Signature = string.IsNullOrEmpty(jobTitle) ? 
            string.Format("{0};{1}{2}", signature.SignatoryFullName, Environment.NewLine, Hyperlinks.Get(document)) :
            string.Format("{0} {1};{2}{3}", jobTitle, signature.SignatoryFullName, Environment.NewLine, Hyperlinks.Get(document));
          row.Date = signature.SigningDate.ToString();
          row.ReportSessionId = reportSessionId;
          approvalList.Add(row);
        }
      }
      return approvalList;
    }
    
    [Public]
    public Eskhata.Module.Docflow.Structures.Module.IConversionToPdfResult ConvertToPdfWithSignatureMark(Sungero.Docflow.IOfficialDocument document, Sungero.Workflow.ITask task)
    {
      var versionId = document.LastVersion.Id;
      var info = ValidateDocumentBeforeConvertion(document, versionId);
      if (info.HasErrors)
        return info;
      var asyncConvertToPdf = RecordManagementEskhata.AsyncHandlers.ConvertAcquaintedDocToPDF.Create();
      asyncConvertToPdf.DocumentId = document.Id;
      asyncConvertToPdf.VersionId = versionId;
      asyncConvertToPdf.TaskId = task.Id;
      
      var startedNotificationText = Sungero.Docflow.OfficialDocuments.Resources.ConvertionInProgress;
      var completedNotificationText = litiko.RecordManagementEskhata.Resources.ConvertToPdfCompleteNotificationFormat(Hyperlinks.Get(document));
      var errorNotificationText = litiko.RecordManagementEskhata.Resources.ConvertionErrorNotificationFormat(Hyperlinks.Get(document), Environment.NewLine);

      asyncConvertToPdf.ExecuteAsync(startedNotificationText, completedNotificationText, errorNotificationText, Users.Current);
      info.IsOnConvertion = true;
      info.HasErrors = false;
      
      Sungero.Docflow.PublicFunctions.Module.LogPdfConverting("Signature mark. Added async", document, document.LastVersion);
      
      return info;
    }
    
    public virtual Eskhata.Module.Docflow.Structures.Module.IConversionToPdfResult ConvertAcquaintedDocToPDF(Sungero.Docflow.IOfficialDocument document, long versionId, Sungero.RecordManagement.IAcquaintanceTask task,
                                                                                                             string htmlStamp, bool isSignatureMark, double rightIndent, double bottomIndent)
    {
      // Предпроверки.
      var result = Eskhata.Module.Docflow.Structures.Module.ConversionToPdfResult.Create();
      result.HasErrors = true;
      var version = document.Versions.SingleOrDefault(v => v.Id == versionId);
      if (version == null)
      {
        result.HasConvertionError = true;
        result.ErrorMessage = Sungero.Docflow.OfficialDocuments.Resources.NoVersionWithNumberErrorFormat(versionId);
        return result;
      }
      
      Sungero.Docflow.PublicFunctions.Module.LogPdfConverting("Start converting to PDF", document, version);
      
      // Получить тело версии для преобразования в PDF.
      var body = Sungero.Docflow.PublicFunctions.OfficialDocument.GetBodyToConvertToPdf(document, version, isSignatureMark);
      if (body == null || body.Body == null || body.Body.Length == 0)
      {
        Sungero.Docflow.PublicFunctions.Module.LogPdfConverting("Cannot get version body", document, version);
        result.HasConvertionError = true;
        result.ErrorMessage = isSignatureMark ? Sungero.Docflow.OfficialDocuments.Resources.ConvertionErrorTitleBase : Sungero.Docflow.Resources.AddRegistrationStampErrorTitle;
        return result;
      }
      
      System.IO.Stream pdfDocumentStream = null;
      using (var inputStream = new System.IO.MemoryStream(body.Body))
      {
        try
        {
          pdfDocumentStream = Sungero.Docflow.IsolatedFunctions.PdfConverter.GeneratePdf(inputStream, body.Extension);
          if (!string.IsNullOrEmpty(htmlStamp))
          {
            pdfDocumentStream = isSignatureMark
              ? Sungero.Docflow.IsolatedFunctions.PdfConverter.AddSignatureStamp(pdfDocumentStream, body.Extension, htmlStamp, Sungero.Docflow.Resources.SignatureMarkAnchorSymbol,
                                                                                 Sungero.Docflow.Constants.Module.SearchablePagesLimit)
              : Sungero.Docflow.IsolatedFunctions.PdfConverter.AddRegistrationStamp(pdfDocumentStream, htmlStamp, 1, rightIndent, bottomIndent);
          }
        }
        catch (Exception ex)
        {
          if (ex is AppliedCodeException)
            Logger.Error(Sungero.Docflow.Resources.PdfConvertErrorFormat(document.Id), ex.InnerException);
          else
            Logger.Error(Sungero.Docflow.Resources.PdfConvertErrorFormat(document.Id), ex);
          
          result.HasConvertionError = true;
          result.HasLockError = false;
          result.ErrorMessage = isSignatureMark ? Sungero.Docflow.OfficialDocuments.Resources.ConvertionErrorTitleBase : Sungero.Docflow.Resources.AddRegistrationStampErrorTitle;
        }
      }
      
      if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
        return result;
      
      Sungero.Docflow.PublicFunctions.Module.LogPdfConverting(isSignatureMark ? "Generate public body" : "Create new version", document, version);
      
      var report = Reports.GetAcquaintanceApprovalSheet();
      report.Document = document;
      report.Task = task;
      try
      {
        using (var reportPdf = report.Export())
        {
          using (var mergedPdfStream = Sungero.Docflow.IsolatedFunctions.PdfConverter.MergePdf(pdfDocumentStream, reportPdf))
          {
            if (mergedPdfStream != null)
            {
              AccessRights.SuppressSecurityEvents(
                () =>
                {
                  if (isSignatureMark)
                  {
                    version.PublicBody.Write(mergedPdfStream);
                    version.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension(Sungero.Docflow.Constants.OfficialDocument.PdfExtension);
                  }
                  else
                  {
                    document.CreateVersionFrom(mergedPdfStream, Sungero.Docflow.Constants.OfficialDocument.PdfExtension);
                    
                    var lastVersion = document.LastVersion;
                  }
                });
              document.Save();
            }
          }
        }
      }
      catch (Exception ex)
      {
        result.HasErrors = true;
        result.ErrorMessage = string.Format("Failed to export approval sheet report or merge two PDFs. Document ID={0}. Error message = {1}", document.Id, ex.Message);
        Logger.ErrorFormat("Failed to export approval sheet report or merge two PDFs. Document ID={0}.", ex, document.Id);
      }
      result = this.SaveDocumentAfterConvertToPdf(document, isSignatureMark);
      Sungero.Docflow.PublicFunctions.Module.LogPdfConverting("End converting to PDF", document, version);
      pdfDocumentStream.Close();
      return result;
    }
    public virtual Eskhata.Module.Docflow.Structures.Module.IConversionToPdfResult SaveDocumentAfterConvertToPdf(Sungero.Docflow.IOfficialDocument document, bool isSignatureMark)
    {
      var result = Eskhata.Module.Docflow.Structures.Module.ConversionToPdfResult.Create();
      result.HasErrors = true;
      
      try
      {
        var paramToWriteInHistory = isSignatureMark
          ? Sungero.Docflow.PublicConstants.OfficialDocument.AddHistoryCommentAboutPDFConvert
          : Sungero.Docflow.PublicConstants.OfficialDocument.AddHistoryCommentAboutRegistrationStamp;
        ((Sungero.Domain.Shared.IExtendedEntity)document).Params[paramToWriteInHistory] = true;
        document.Save();
        ((Sungero.Domain.Shared.IExtendedEntity)document).Params.Remove(paramToWriteInHistory);
        
        Sungero.Docflow.PublicFunctions.OfficialDocument.PreparePreview(document);
        
        result.HasErrors = false;
      }
      catch (Sungero.Domain.Shared.Exceptions.RepeatedLockException e)
      {
        Logger.Error(e.Message);
        result.HasConvertionError = false;
        result.HasLockError = true;
        result.ErrorMessage = e.Message;
      }
      catch (Exception e)
      {
        Logger.Error(e.Message);
        result.HasConvertionError = true;
        result.HasLockError = false;
        result.ErrorMessage = e.Message;
      }
      
      return result;
    }
    
    #region private
    private string GetSignatureKey(Sungero.Domain.Shared.ISignature signature, int versionNumber)
    {
      // Если подпись не "несогласующая", она должна схлапываться вне версий.
      if (signature.SignatureType != SignatureType.NotEndorsing)
        versionNumber = 0;
      
      if (signature.Signatory != null)
      {
        if (signature.SubstitutedUser != null && !signature.SubstitutedUser.Equals(signature.Signatory))
          return string.Format("{0} - {1}:{2}:{3}", signature.Signatory.Id, signature.SubstitutedUser.Id, signature.SignatureType == SignatureType.Approval, versionNumber);
        else
          return string.Format("{0}:{1}:{2}", signature.Signatory.Id, signature.SignatureType == SignatureType.Approval, versionNumber);
      }
      else
        return string.Format("{0}:{1}:{2}", signature.SignatoryFullName, signature.SignatureType == SignatureType.Approval, versionNumber);
    }
    private string AddEndOfLine(string row)
    {
      return string.IsNullOrWhiteSpace(row) ? string.Empty : row + Environment.NewLine;
    }
    public string GetEndorsingResultFromSignature(Sungero.Domain.Shared.ISignature signature, bool emptyIfNotValid)
    {
      var result = string.Empty;
      
      if (emptyIfNotValid && !signature.IsValid)
        return result;
      
      switch (signature.SignatureType)
      {
        case SignatureType.Approval:
          result = Sungero.Docflow.ApprovalTasks.Resources.ApprovalFormApproved;
          break;
        case SignatureType.Endorsing:
          result = HasApproveWithSuggestionsMark(signature.Comment)
            ? Sungero.Docflow.ApprovalTasks.Resources.ApprovalFormEndorsedWithSuggestions
            : Sungero.Docflow.ApprovalTasks.Resources.ApprovalFormEndorsed;
          break;
        case SignatureType.NotEndorsing:
          result = Sungero.Docflow.ApprovalTasks.Resources.ApprovalFormNotEndorsed;
          break;
      }
      
      return result;
    }
    public bool HasApproveWithSuggestionsMark(string text)
    {
      return text.StartsWith(GetApproveWithSuggestionsMark(), StringComparison.OrdinalIgnoreCase);
    }
    public string RemoveApproveWithSuggestionsMark(string text)
    {
      if (string.IsNullOrEmpty(text) || !HasApproveWithSuggestionsMark(text))
        return text;
      
      var mark = GetApproveWithSuggestionsMark();
      // Удалить только первое вхождение метки. GetApproveWithSuggestionsMark проверяет наличие метки в начале текста.
      return text.Remove(0, mark.Length);
    }
    public string GetApproveWithSuggestionsMark()
    {
      var zeroWidthSpace = '\u200b';
      return new string(new char[] { zeroWidthSpace, zeroWidthSpace, zeroWidthSpace });
    }
    public Eskhata.Module.Docflow.Structures.Module.IConversionToPdfResult ValidateDocumentBeforeConvertion(Sungero.Docflow.IOfficialDocument document, long versionId)
    {
      var info = Eskhata.Module.Docflow.Structures.Module.ConversionToPdfResult.Create();
      info.HasErrors = true;
      
      // Документ МКДО.
      if (Sungero.Exchange.ExchangeDocumentInfos.GetAll().Any(x => Equals(x.Document, document) && x.VersionId == versionId))
      {
        info.HasErrors = false;
        return info;
      }
      
      // Формализованный документ.
      if (Sungero.Docflow.AccountingDocumentBases.Is(document) && Sungero.Docflow.AccountingDocumentBases.As(document).IsFormalized == true)
      {
        info.HasErrors = false;
        return info;
      }
      
      // Соглашение об аннулировании.
      if (Sungero.Exchange.CancellationAgreements.Is(document))
      {
        info.HasErrors = false;
        return info;
      }
      
      // Проверить наличие версии.
      var version = document.Versions.FirstOrDefault(x => x.Id == versionId);
      if (version == null)
      {
        info.ErrorTitle = Sungero.Docflow.OfficialDocuments.Resources.ConvertionErrorTitleBase;
        info.ErrorMessage = Sungero.Docflow.OfficialDocuments.Resources.NoVersionError;
        return info;
      }
      
      // Формат не поддерживается.
      var versionExtension = version.BodyAssociatedApplication.Extension.ToLower();
      if (!Sungero.Docflow.PublicFunctions.OfficialDocument.CheckPdfConvertibilityByExtension(document, versionExtension))
        return GetExtensionValidationError(versionExtension);
      
      info.HasErrors = false;
      return info;
    }
    public Eskhata.Module.Docflow.Structures.Module.IConversionToPdfResult GetExtensionValidationError(string extension)
    {
      var result = Eskhata.Module.Docflow.Structures.Module.ConversionToPdfResult.Create();
      result.HasErrors = true;
      result.ErrorTitle = Sungero.Docflow.OfficialDocuments.Resources.ConvertionErrorTitleBase;
      result.ErrorMessage = Sungero.Docflow.OfficialDocuments.Resources.ExtensionNotSupportedFormat(extension.ToUpper());
      return result;
    }
    public bool IsExchangeDocument(Sungero.Docflow.IOfficialDocument document,long versionId)
    {
      return Sungero.Exchange.ExchangeDocumentInfos.GetAll().Any(x => Equals(x.Document, document) && x.VersionId == versionId) ||
        Sungero.Docflow.AccountingDocumentBases.Is(document) && Sungero.Docflow.AccountingDocumentBases.As(document).IsFormalized == true ||
        Sungero.Exchange.CancellationAgreements.Is(document);
    }
    #endregion
  }
}