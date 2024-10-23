using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Addendum;

namespace litiko.Eskhata.Shared
{
  partial class AddendumFunctions
  {
    public override void FillName()
    {
      if (_obj.DocumentKind != null && !_obj.DocumentKind.GenerateDocumentName.Value && _obj.Name == OfficialDocuments.Resources.DocumentNameAutotext)
        _obj.Name = string.Empty;
      
      if (_obj.DocumentKind == null || !_obj.DocumentKind.GenerateDocumentName.Value)
        return;
      
      var name = string.Empty;
      
      /* Имя в формате:
        <Вид документа> №<номер> "<содержание>" к <имя ведущего документа>.
       */
      using (TenantInfo.Culture.SwitchTo())
      {
        if (!string.IsNullOrWhiteSpace(_obj.RegistrationNumber))
          name += OfficialDocuments.Resources.Number + _obj.RegistrationNumber;
                
        var docKindExtractProtocol = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(litiko.CollegiateAgencies.PublicConstants.Module.DocumentKindGuids.ExtractProtocol);
        var docKindResolution = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(litiko.CollegiateAgencies.PublicConstants.Module.DocumentKindGuids.Resolution);                  
        if (!((docKindExtractProtocol != null && Equals(_obj.DocumentKind, docKindExtractProtocol)) || (docKindResolution != null && Equals(_obj.DocumentKind, docKindResolution))) && !string.IsNullOrEmpty(_obj.Subject))        
          name += " " + _obj.Subject;
        
        if (_obj.LeadingDocument != null)
        {
          name += OfficialDocuments.Resources.NamePartForLeadDocument;
          name += Sungero.Docflow.PublicFunctions.Module.ReplaceFirstSymbolToLowerCase(GetDocumentName(_obj.LeadingDocument));
        }
      }
      
      if (string.IsNullOrWhiteSpace(name))
        name = OfficialDocuments.Resources.DocumentNameAutotext;
      else if (_obj.DocumentKind != null)
        name = _obj.DocumentKind.ShortName + name;
      
      name = Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols(name);
      
      _obj.Name = Sungero.Docflow.PublicFunctions.OfficialDocument.AddClosingQuote(name, _obj);
      
    }
    
    /// <summary>
    /// Установить обязательность свойств в зависимости от заполненных данных.
    /// </summary>
    public override void SetRequiredProperties()
    {
      base.SetRequiredProperties();

      // Для Постановлений и Выписок из протокола - Содержание не обязательное поле
      // + Акт об актуальности ВНД и Лист согласования ВНД 
      bool isSubjectrequired = _obj.State.Properties.Subject.IsRequired;
      var docKindExtractProtocol = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(litiko.CollegiateAgencies.PublicConstants.Module.DocumentKindGuids.ExtractProtocol);
      var docKindResolution = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(litiko.CollegiateAgencies.PublicConstants.Module.DocumentKindGuids.Resolution);
      var docKindActOnTheRelevance = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(litiko.RegulatoryDocuments.PublicConstants.Module.DocumentKindGuids.ActOnTheRelevance);
      var docKindApprovalSheetIRD = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(litiko.RegulatoryDocuments.PublicConstants.Module.DocumentKindGuids.ApprovalSheetIRD);
      if ((docKindExtractProtocol != null && Equals(_obj.DocumentKind, docKindExtractProtocol)) || 
          (docKindActOnTheRelevance != null && Equals(_obj.DocumentKind, docKindActOnTheRelevance)) ||
          (docKindApprovalSheetIRD != null && Equals(_obj.DocumentKind, docKindApprovalSheetIRD))
         )
        isSubjectrequired = false;
      
      _obj.State.Properties.Subject.IsRequired = isSubjectrequired;
    }    
  }
}