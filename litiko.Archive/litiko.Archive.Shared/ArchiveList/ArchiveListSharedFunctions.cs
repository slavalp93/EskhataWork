using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Archive.ArchiveList;

namespace litiko.Archive.Shared
{
  partial class ArchiveListFunctions
  {
    /// <summary>
    /// Заполнить имя.
    /// </summary>
    public override void FillName()
    {
      // Не автоформируемое имя.
      if (_obj != null && _obj.DocumentKind != null && !_obj.DocumentKind.GenerateDocumentName.Value)
      {
        if (_obj.Name == Sungero.Docflow.OfficialDocuments.Resources.DocumentNameAutotext)
          _obj.Name = string.Empty;
        
        if (_obj.VerificationState != null && string.IsNullOrWhiteSpace(_obj.Name))
          _obj.Name = _obj.DocumentKind.ShortName; 
      }      
      
      if (_obj.DocumentKind == null || !_obj.DocumentKind.GenerateDocumentName.Value)
        return;
      
      // Автоформируемое имя.
      var name = string.Empty;
      
      /* Имя в формате:
        <Сокр. Наименование вида> №<Номер> от <Дата рег.> <По подразделению>
       */
      using (TenantInfo.Culture.SwitchTo())
      {       
        if (!string.IsNullOrWhiteSpace(_obj.RegistrationNumber))
          name += Sungero.Docflow.OfficialDocuments.Resources.Number + _obj.RegistrationNumber;
        
        if (_obj.RegistrationDate != null)
          name += Sungero.Docflow.OfficialDocuments.Resources.DateFrom + _obj.RegistrationDate.Value.ToString("dd.mm.yy");        
        
        if (_obj.ByDepartment != null)
          name += " " + _obj.ByDepartment.Name;
      }
      
      if (string.IsNullOrWhiteSpace(name))
      {
        name = _obj.VerificationState == null ? Sungero.Docflow.OfficialDocuments.Resources.DocumentNameAutotext : _obj.DocumentKind.ShortName;
      }
      else if (_obj.DocumentKind != null)
      {
        name = _obj.DocumentKind.ShortName + name;
      }
      
      name = Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols(name);
      
      _obj.Name = name;
    }
  }
}