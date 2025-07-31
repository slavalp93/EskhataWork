using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.Eskhata.Module.Docflow.Server
{
  partial class ModuleFunctions
  {
    [Public]
    public virtual Structures.Module.IConversionToPdfResult GeneratePublicBodyWithSignatureMarkEskhata(Sungero.Docflow.IOfficialDocument document, long versionId, string signatureMark)
    {
      var baseResult = this.GeneratePublicBodyWithSignatureMark(document, versionId, signatureMark);
      var result = Structures.Module.ConversionToPdfResult.Create();
      result.ErrorMessage = baseResult.ErrorMessage;
      result.ErrorTitle = baseResult.ErrorTitle;
      result.HasConvertionError = baseResult.HasConvertionError;
      result.HasErrors = baseResult.HasErrors;
      result.HasLockError = baseResult.HasLockError;
      result.IsFastConvertion = baseResult.IsFastConvertion;
      result.IsOnConvertion = baseResult.IsOnConvertion;
      return result;
    }
    
    public override Sungero.Docflow.Structures.OfficialDocument.ConversionToPdfResult GeneratePublicBodyWithSignatureMark(Sungero.Docflow.IOfficialDocument document, long versionId, string signatureMark)
    {
      return base.GeneratePublicBodyWithSignatureMark(document, versionId, signatureMark);
    }
    public override Sungero.Docflow.Structures.OfficialDocument.ConversionToPdfResult ConvertToPdfWithStamp(Sungero.Docflow.IOfficialDocument document, long versionId, string htmlStamp, bool isSignatureMark, double rightIndent, double bottomIndent)
    {
      // Предпроверки.
      var result = Sungero.Docflow.Structures.OfficialDocument.ConversionToPdfResult.Create();
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
        result.ErrorMessage = isSignatureMark ? Sungero.Docflow.OfficialDocuments.Resources.ConvertionErrorTitleBase : Docflow.Resources.AddRegistrationStampErrorTitle;
        return result;
      }
      
      System.IO.Stream pdfDocumentStream = null;
      using (var inputStream = new System.IO.MemoryStream(body.Body))
      {
        try
        {
          pdfDocumentStream = Sungero.Docflow.IsolatedFunctions.PdfConverter.GeneratePdf(inputStream, body.Extension);
          if (!string.IsNullOrEmpty(htmlStamp) && !isSignatureMark)
          {
            pdfDocumentStream = Sungero.Docflow.IsolatedFunctions.PdfConverter.AddRegistrationStamp(pdfDocumentStream, htmlStamp, 1, rightIndent, bottomIndent);
          }
          DateTime? incomingDate = null;
          var incomingNo = string.Empty;
          var response = OutgoingDocumentBases.As(document)?.InResponseTo;
          if(response != null)
          {
            incomingDate = response.Dated;
            incomingNo = response.InNumber;
          }
          pdfDocumentStream = DocflowEskhata.IsolatedFunctions.PdfConverter.AddRegistrationData(pdfDocumentStream, document.RegistrationNumber, document.RegistrationDate, incomingDate, incomingNo);
          
          #region ⚓^ - утверждающая подпись Подписанта
          var signatureForQR = Sungero.Docflow.PublicFunctions.OfficialDocument.GetSignatureForMark(document, version.Id);
          if (signatureForQR != null)
          {
            var signatory = Sungero.Company.Employees.As(signatureForQR.Signatory);
            string jobTitle = Eskhata.JobTitles.As(signatory.JobTitle)?.NameTGlitiko;
            var qrText = string.IsNullOrEmpty(jobTitle) ?
              string.Format("{0};{1} {2}", signatureForQR.SignatoryFullName, Environment.NewLine, Hyperlinks.Get(document)) :
              string.Format("{0} {1};{2} {3}", jobTitle, signatureForQR.SignatoryFullName, Environment.NewLine, Hyperlinks.Get(document));
            
            pdfDocumentStream = DocflowEskhata.IsolatedFunctions.PdfConverter.AddSignatureQRStamp(pdfDocumentStream, qrText, "⚓^");            
          }
          #endregion
          
          #region ⚓s - согласующая подпись Автора
          var endorsingAuthorSignature = litiko.Eskhata.PublicFunctions.OfficialDocument.GetEndorsingAuthorSignature(litiko.Eskhata.OfficialDocuments.As(document), version.Id, false);
          if (endorsingAuthorSignature != null)
          {
            var signatory = Sungero.Company.Employees.As(endorsingAuthorSignature.Signatory);
            string jobTitle = string.Empty;
            if (signatory != null && signatory.JobTitle != null && Eskhata.JobTitles.Is(signatory?.JobTitle))
            {
              jobTitle = Eskhata.JobTitles.As(signatory.JobTitle).NameTGlitiko;
            }
            var qrText = string.IsNullOrEmpty(jobTitle) ?
              string.Format("{0};{1} {2}", endorsingAuthorSignature.SignatoryFullName, Environment.NewLine, Hyperlinks.Get(document)) :
              string.Format("{0} {1};{2} {3}", jobTitle, endorsingAuthorSignature.SignatoryFullName, Environment.NewLine, Hyperlinks.Get(document));
            
            pdfDocumentStream = DocflowEskhata.IsolatedFunctions.PdfConverter.AddSignatureQRStamp(pdfDocumentStream, qrText, "⚓s");              
          }
          #endregion
          
        }
        catch (Exception ex)
        {
          if (ex is AppliedCodeException)
            Logger.Error(Docflow.Resources.PdfConvertErrorFormat(document.Id), ex.InnerException);
          else
            Logger.Error(Docflow.Resources.PdfConvertErrorFormat(document.Id), ex);
          
          result.HasConvertionError = true;
          result.HasLockError = false;
          result.ErrorMessage = isSignatureMark ? Sungero.Docflow.OfficialDocuments.Resources.ConvertionErrorTitleBase : Docflow.Resources.AddRegistrationStampErrorTitle;
        }
      }
      
      if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
        return result;
      
      // Выключить error-логирование при доступе к зашифрованным бинарным данным/версии.
      AccessRights.SuppressSecurityEvents(
        () =>
        {
          if (isSignatureMark)
          {
            version.PublicBody.Write(pdfDocumentStream);
            version.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension(Sungero.Docflow.Constants.OfficialDocument.PdfExtension);
          }
          else
          {
            document.CreateVersionFrom(pdfDocumentStream, Sungero.Docflow.Constants.OfficialDocument.PdfExtension);
            
            var lastVersion = document.LastVersion;
            lastVersion.Note = Sungero.Docflow.OfficialDocuments.Resources.VersionWithRegistrationStamp;
          }
        });
      
      pdfDocumentStream.Close();
      
      Sungero.Docflow.PublicFunctions.Module.LogPdfConverting(isSignatureMark ? "Generate public body" : "Create new version", document, version);
      result = this.SaveDocumentAfterConvertToPdf(document, isSignatureMark);
      Sungero.Docflow.PublicFunctions.Module.LogPdfConverting("End converting to PDF", document, version);
      
      return result;
    }
    
    public override Sungero.Docflow.Structures.OfficialDocument.ConversionToPdfResult SaveDocumentAfterConvertToPdf(Sungero.Docflow.IOfficialDocument document, bool isSignatureMark)
    {
      return base.SaveDocumentAfterConvertToPdf(document, isSignatureMark);
    }
  }
}