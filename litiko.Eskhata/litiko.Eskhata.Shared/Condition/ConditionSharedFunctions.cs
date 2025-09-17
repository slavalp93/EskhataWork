using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Condition;

namespace litiko.Eskhata.Shared
{
  partial class ConditionFunctions
  {
    /// <summary>
    /// Получить словарь поддерживаемых типов условий.
    /// </summary>
    /// <returns>
    /// Словарь.
    /// Ключ - GUID типа документа.
    /// Значение - список поддерживаемых условий.
    /// </returns>
    public override System.Collections.Generic.Dictionary<string, List<Enumeration?>> GetSupportedConditions()
    {
      var baseConditions = base.GetSupportedConditions();
      
      baseConditions[RecordManagementEskhata.PublicConstants.Module.DocumentTypeGuids.Order.ToString()].Add(Eskhata.Condition.ConditionType.ChiefAccountant);
      baseConditions[RecordManagementEskhata.PublicConstants.Module.DocumentTypeGuids.CompanyDirective.ToString()].Add(Eskhata.Condition.ConditionType.ChiefAccountant);
      
      baseConditions[RegulatoryDocuments.PublicConstants.Module.DocumentTypeGuids.RegulatoryDocument.ToString()].Add(Eskhata.Condition.ConditionType.IsRecommendat);
      baseConditions[RegulatoryDocuments.PublicConstants.Module.DocumentTypeGuids.RegulatoryDocument.ToString()].Add(Eskhata.Condition.ConditionType.IsRelatedStruct);
      baseConditions[RegulatoryDocuments.PublicConstants.Module.DocumentTypeGuids.RegulatoryDocument.ToString()].Add(Eskhata.Condition.ConditionType.IsRequirements);
      baseConditions[RegulatoryDocuments.PublicConstants.Module.DocumentTypeGuids.RegulatoryDocument.ToString()].Add(Eskhata.Condition.ConditionType.IRDType);
      baseConditions[RegulatoryDocuments.PublicConstants.Module.DocumentTypeGuids.RegulatoryDocument.ToString()].Add(Eskhata.Condition.ConditionType.OrganForApprov);
      
      // Vals 20250916
      foreach (var kvp in baseConditions)
      {
          kvp.Value.Add(Eskhata.Condition.ConditionType.IsDocumentRegis);
      }
      
      baseConditions[DocflowEskhata.PublicConstants.Module.DocumentTypeGuids.OutgoingLetter.ToString()].Add(Eskhata.Condition.ConditionType.StandardRespons);
      
      baseConditions[CollegiateAgencies.PublicConstants.Module.DocumentTypeGuids.ProjectSolution.ToString()].Add(Eskhata.Condition.ConditionType.MeetingCategorylitiko);
      
      return baseConditions;
    }
    
    /// <summary>
    /// Сменить доступность реквизитов.
    /// </summary>
    public override void ChangePropertiesAccess()
    {
      base.ChangePropertiesAccess();
      
      var isIRDType = _obj.ConditionType == ConditionType.IRDType;
      _obj.State.Properties.IRDTypelitiko.IsVisible = isIRDType;
      _obj.State.Properties.IRDTypelitiko.IsRequired = isIRDType;
      
      var isOrganForApprovType = _obj.ConditionType == ConditionType.OrganForApprov;
      _obj.State.Properties.OrganForApprovinglitiko.IsVisible = isOrganForApprovType;
      _obj.State.Properties.OrganForApprovinglitiko.IsRequired = isOrganForApprovType;

      var isStandardResponse = _obj.ConditionType == ConditionType.StandardRespons;
      
      var isMeetingCategoryType = _obj.ConditionType == ConditionType.MeetingCategorylitiko;
      _obj.State.Properties.MeetingCategorylitiko.IsVisible = isMeetingCategoryType;
      _obj.State.Properties.MeetingCategorylitiko.IsRequired = isMeetingCategoryType;
    }

    /// <summary>
    /// Очистка скрытых свойств.
    /// </summary>
    public override void ClearHiddenProperties()
    {
      base.ClearHiddenProperties();
      
      if (!_obj.State.Properties.IRDTypelitiko.IsVisible)
        _obj.IRDTypelitiko = null;
      
      if (!_obj.State.Properties.OrganForApprovinglitiko.IsVisible)
        _obj.OrganForApprovinglitiko = null;
      
      if (!_obj.State.Properties.MeetingCategorylitiko.IsVisible)
        _obj.MeetingCategorylitiko = null;
    }
    
    /// <summary>
    /// Проверить условие.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="task">Задача на согласование.</param>
    public override Sungero.Docflow.Structures.ConditionBase.ConditionResult CheckCondition(Sungero.Docflow.IOfficialDocument document, Sungero.Docflow.IApprovalTask task)
    {
      #region Согласование с Главным бухгалтером
      if (_obj.ConditionType == Eskhata.Condition.ConditionType.ChiefAccountant)
      {
        if (Eskhata.Orders.Is(document))
          return Sungero.Docflow.Structures.ConditionBase.ConditionResult.
            Create(Eskhata.Orders.As(document).ChiefAccountantApproving == true,
                   string.Empty);
        
        if (Eskhata.CompanyDirectives.Is(document))
          return Sungero.Docflow.Structures.ConditionBase.ConditionResult.
            Create(Eskhata.CompanyDirectives.As(document).ChiefAccountantApproving == true,
                   string.Empty);
        
        return Sungero.Docflow.Structures.ConditionBase.ConditionResult.
          Create(null, litiko.Eskhata.Conditions.Resources.CannotComputeCondition);
      }      
      #endregion
      
      #region Требования учетной политики Банка
      if (_obj.ConditionType == ConditionType.IsRequirements)
        return this.CheckIsRequirements(document, task);      
      #endregion
      
      #region Связано со структурой банка
      if (_obj.ConditionType == ConditionType.IsRelatedStruct)
        return this.CheckIsRelatedStruct(document, task);      
      #endregion
      
      #region Рекомендации внутреннего аудита
      if (_obj.ConditionType == ConditionType.IsRecommendat)
        return this.CheckIsRecommendat(document, task);      
      #endregion

      #region Тип ВНД
      if (_obj.ConditionType == ConditionType.IRDType)
      {
        var regulatoryDocument = litiko.RegulatoryDocuments.RegulatoryDocuments.As(document);
        if (regulatoryDocument != null)
          return Sungero.Docflow.Structures.ConditionBase.ConditionResult.Create(regulatoryDocument.Type == _obj.IRDTypelitiko, string.Empty);
        else
          return Sungero.Docflow.Structures.ConditionBase.ConditionResult.Create(null, litiko.Eskhata.Conditions.Resources.CannotComputeCondition);
      }      
      #endregion      
      
      #region Орган утверждения
      if (_obj.ConditionType == ConditionType.OrganForApprov)
      {
        var regulatoryDocument = litiko.RegulatoryDocuments.RegulatoryDocuments.As(document);
        if (regulatoryDocument != null)
          return Sungero.Docflow.Structures.ConditionBase.ConditionResult.Create(regulatoryDocument.OrganForApproving == _obj.OrganForApprovinglitiko, string.Empty);
        else
          return Sungero.Docflow.Structures.ConditionBase.ConditionResult.Create(null, litiko.Eskhata.Conditions.Resources.CannotComputeCondition);
      }      
      #endregion
      
      #region Стандартный ответ
      if (_obj.ConditionType == Eskhata.Condition.ConditionType.StandardRespons)
      {
        if (Eskhata.OutgoingDocumentBases.Is(document))
          return Sungero.Docflow.Structures.ConditionBase.ConditionResult.
            Create(Eskhata.OutgoingDocumentBases.As(document).StandardResponse == true,
                   string.Empty);
        
        return Sungero.Docflow.Structures.ConditionBase.ConditionResult.
          Create(null, litiko.Eskhata.Conditions.Resources.CannotComputeCondition);
      }      
      #endregion
      
      #region Категория заседания
      if (_obj.ConditionType == Eskhata.Condition.ConditionType.MeetingCategorylitiko)
      {
        if (CollegiateAgencies.Projectsolutions.Is(document))
          return Sungero.Docflow.Structures.ConditionBase.ConditionResult.
            Create(CollegiateAgencies.Projectsolutions.As(document).MeetingCategory == _obj.MeetingCategorylitiko,
                   string.Empty);
        
        return Sungero.Docflow.Structures.ConditionBase.ConditionResult.
          Create(null, litiko.Eskhata.Conditions.Resources.CannotComputeCondition);
      }            
      #endregion      
      
      #region Документ зарегистрирован
      // Vals 20250916
      if (_obj.ConditionType == Eskhata.Condition.ConditionType.IsDocumentRegis)
      {
        return Sungero.Docflow.Structures.ConditionBase.ConditionResult.
          Create(document.RegistrationState == Sungero.Docflow.OfficialDocument.RegistrationState.Registered,
                 string.Empty);
      }            
      #endregion        
      
      return base.CheckCondition(document, task);
    }
    
    /// <summary>
    /// Проверить условие "Требования учетной политики Банка".
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="task">Задача на согласование.</param>
    public virtual Sungero.Docflow.Structures.ConditionBase.ConditionResult CheckIsRequirements(Sungero.Docflow.IOfficialDocument document, Sungero.Docflow.IApprovalTask task)
    {
      if (litiko.RegulatoryDocuments.RegulatoryDocuments.Is(document))
      {
        var regulatoryDocument = litiko.RegulatoryDocuments.RegulatoryDocuments.As(document);
        
        if (!regulatoryDocument.IsRequirements.HasValue)
          return Sungero.Docflow.Structures.ConditionBase.ConditionResult.Create(null, Conditions.Resources.IsRequirementsIsNotFilledInIRG);
        
        return Sungero.Docflow.Structures.ConditionBase.ConditionResult.Create(regulatoryDocument.IsRequirements.Value, string.Empty);
      }

      return Sungero.Docflow.Structures.ConditionBase.ConditionResult.Create(null, Conditions.Resources.SelectApprovalRuleWithoutIsRequirementsCondition);
    }

    /// <summary>
    /// Проверить условие "Рекомендации внутреннего аудита".
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="task">Задача на согласование.</param>
    public virtual Sungero.Docflow.Structures.ConditionBase.ConditionResult CheckIsRecommendat(Sungero.Docflow.IOfficialDocument document, Sungero.Docflow.IApprovalTask task)
    {
      if (litiko.RegulatoryDocuments.RegulatoryDocuments.Is(document))
      {
        var regulatoryDocument = litiko.RegulatoryDocuments.RegulatoryDocuments.As(document);
        
        if (!regulatoryDocument.IsRecommendations.HasValue)
          return Sungero.Docflow.Structures.ConditionBase.ConditionResult.Create(null, Conditions.Resources.IsRecommendationsIsNotFilledInIRG);
        
        return Sungero.Docflow.Structures.ConditionBase.ConditionResult.Create(regulatoryDocument.IsRecommendations.Value, string.Empty);
      }

      return Sungero.Docflow.Structures.ConditionBase.ConditionResult.Create(null, Conditions.Resources.SelectApprovalRuleWithoutIsRecommendationsCondition);
    }
    
    /// <summary>
    /// Проверить условие "Связано со структурой банка".
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="task">Задача на согласование.</param>
    public virtual Sungero.Docflow.Structures.ConditionBase.ConditionResult CheckIsRelatedStruct(Sungero.Docflow.IOfficialDocument document, Sungero.Docflow.IApprovalTask task)
    {
      if (litiko.RegulatoryDocuments.RegulatoryDocuments.Is(document))
      {
        var regulatoryDocument = litiko.RegulatoryDocuments.RegulatoryDocuments.As(document);
        
        if (!regulatoryDocument.IsRelatedToStructure.HasValue)
          return Sungero.Docflow.Structures.ConditionBase.ConditionResult.Create(null, Conditions.Resources.IsRelatedToStructureIsNotFilledInIRG);
        
        return Sungero.Docflow.Structures.ConditionBase.ConditionResult.Create(regulatoryDocument.IsRelatedToStructure.Value, string.Empty);
      }

      return Sungero.Docflow.Structures.ConditionBase.ConditionResult.Create(null, Conditions.Resources.SelectApprovalRuleWithoutIsRelatedToStructureCondition);
    }
    
  }
}