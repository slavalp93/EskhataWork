using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.RegulatoryDocuments.RegulatoryDocument;

namespace litiko.RegulatoryDocuments.Shared
{
  partial class RegulatoryDocumentFunctions
  {
    /// <summary>
    /// Установить обязательность свойств в зависимости от заполненных данных.
    /// </summary>
    public override void SetRequiredProperties()
    {
      base.SetRequiredProperties();
        
      _obj.State.Properties.LeadingDocument.IsRequired = Equals(_obj.Type, litiko.RegulatoryDocuments.RegulatoryDocument.Type.IsChange) || 
        Equals(_obj.Type, litiko.RegulatoryDocuments.RegulatoryDocument.Type.IsUpdate);
      
      _obj.State.Properties.PreparedBy.IsRequired = true;
      _obj.State.Properties.Department.IsRequired = true;
      _obj.State.Properties.BusinessUnit.IsRequired = true;
    }
    
    /// <summary>
    /// Обновить карточку документа.
    /// </summary>
    public override void RefreshDocumentForm()
    {
      base.RefreshDocumentForm();
      
      // "Введение в действие с", "Дата пересмотра" - доступно для изменения Группе регистрации "Общий отдел"
      bool isEnabled = false;
      var regGroup = Sungero.Docflow.RegistrationGroups.GetAll(x => x.Status == Sungero.Docflow.RegistrationGroup.Status.Active && 
                                                               x.Name == litiko.RegulatoryDocuments.Constants.Module.RegGroupGeneralDepartment)
        .FirstOrDefault();
      
      if (regGroup != null && Users.Current.IncludedIn(regGroup))
        isEnabled = true;
      
      _obj.State.Properties.DateBegin.IsEnabled = isEnabled;
      _obj.State.Properties.DateRevision.IsEnabled = isEnabled;
      
      // "Дата актуализации" - доступно для изменения Группе регистрации "Общий отдел", Автору, Руководителю процесса
      _obj.State.Properties.DateUpdate.IsEnabled = isEnabled || Equals(Users.Current, Users.As(_obj.PreparedBy)) || Equals(Users.Current, Users.As(_obj.ProcessManager));
    }
    
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
        <Рег.номер> <Вид> <Содержание> (Версия <Номер версии>) <Язык документа>
       */
      using (TenantInfo.Culture.SwitchTo())
      {       
        if (!string.IsNullOrWhiteSpace(_obj.RegistrationNumber))
          name += _obj.RegistrationNumber;
        
        if (!string.IsNullOrWhiteSpace(_obj.DocumentKind.ShortName))
          name += " " + _obj.DocumentKind.ShortName;
        else
          name += " " + _obj.DocumentKind.Name;
        
        if (!string.IsNullOrWhiteSpace(_obj.Subject))
          name += " " + _obj.Subject;
        
        if (_obj.VersionNumber.HasValue)
          name += " " + litiko.RegulatoryDocuments.RegulatoryDocuments.Resources.VersionInNameFormat(_obj.VersionNumber.Value);
        
        if (_obj.Language.HasValue)
          name += " " + _obj.Info.Properties.Language.GetLocalizedValue(_obj.Language);
      }
      
      if (string.IsNullOrWhiteSpace(name))
      {
        name = _obj.VerificationState == null ? Sungero.Docflow.OfficialDocuments.Resources.DocumentNameAutotext : _obj.DocumentKind.ShortName;
      }      
      
      name = Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols(name);
      
      _obj.Name = name;
    }    
    
    
  }
}