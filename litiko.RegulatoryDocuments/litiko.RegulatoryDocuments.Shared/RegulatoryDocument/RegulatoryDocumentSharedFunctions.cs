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
    
    
  }
}