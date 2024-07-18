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
        var versionSignatures = Signatures.Get(version).Where(s => (showNotApproveSign || s.SignatureType != SignatureType.NotEndorsing)
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
            var additionalInfo = additionalInfos.FirstOrDefault();
            employeeName = string.Format("{1}{0}", signature.SignatoryFullName, AddEndOfLine(additionalInfo)).Trim();
          }
          else
          {
            if (additionalInfos.Count() == 3)
            {
              // Замещающий.
              var signatoryAdditionalInfo = additionalInfos[0];
              if (!string.IsNullOrEmpty(signatoryAdditionalInfo))
                signatoryAdditionalInfo = AddEndOfLine(signatoryAdditionalInfo);
              var signatoryText = AddEndOfLine(string.Format("{0}{1}", signatoryAdditionalInfo, signature.SignatoryFullName));
              
              // Замещаемый.
              var substitutedUserAdditionalInfo = additionalInfos[1];
              if (!string.IsNullOrEmpty(substitutedUserAdditionalInfo))
                substitutedUserAdditionalInfo = AddEndOfLine(substitutedUserAdditionalInfo);
              var substitutedUserText = string.Format("{0}{1}", substitutedUserAdditionalInfo, signature.SubstitutedUserFullName);
              
              // Замещающий за замещаемого.
              var onBehalfOfText = AddEndOfLine(Sungero.Docflow.ApprovalTasks.Resources.OnBehalfOf);
              employeeName = string.Format("{0}{1}{2}", signatoryText, onBehalfOfText, substitutedUserText);
            }
            else if (additionalInfos.Count() == 2)
            {
              // Замещающий / Система.
              var signatoryText = AddEndOfLine(signature.SignatoryFullName);
              
              // Замещаемый.
              var substitutedUserAdditionalInfo = additionalInfos[0];
              if (!string.IsNullOrEmpty(substitutedUserAdditionalInfo))
                substitutedUserAdditionalInfo = AddEndOfLine(substitutedUserAdditionalInfo);
              var substitutedUserText = string.Format("{0}{1}", substitutedUserAdditionalInfo, signature.SubstitutedUserFullName);
              
              // Система за замещаемого.
              var onBehalfOfText = AddEndOfLine(Sungero.Docflow.ApprovalTasks.Resources.OnBehalfOf);
              employeeName = string.Format("{0}{1}{2}", signatoryText, onBehalfOfText, substitutedUserText);
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
          row.Signature = string.Format("{0};{1}", signature.SignatoryFullName, Hyperlinks.Get(document));
          row.Date = signature.SigningDate.ToString();
          row.ReportSessionId = reportSessionId;
          approvalList.Add(row);
        }
      }
      return approvalList;
    }
    
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
  }
}