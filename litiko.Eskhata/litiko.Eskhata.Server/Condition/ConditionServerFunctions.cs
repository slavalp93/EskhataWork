using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Condition;

namespace litiko.Eskhata.Server
{
  partial class ConditionFunctions
  {
    public override string GetConditionName()
    {
      using (TenantInfo.Culture.SwitchTo())
      {
        if (_obj.ConditionType == ConditionType.ChiefAccountant)
          return litiko.Eskhata.Conditions.Resources.ChiefAccountantConditionName;
        
        if (_obj.ConditionType == ConditionType.IsRequirements)
          return litiko.Eskhata.Conditions.Resources.IsRequirementsConditionName;
        
        if (_obj.ConditionType == ConditionType.IsRelatedStruct)
          return litiko.Eskhata.Conditions.Resources.IsRelatedStructConditionName;

        if (_obj.ConditionType == ConditionType.IsRecommendat)
          return litiko.Eskhata.Conditions.Resources.IsRecommendatConditionName;
        
        if (_obj.ConditionType == ConditionType.StandardRespons)
          return litiko.Eskhata.Conditions.Resources.StandardResponseConditionName;
        
        // "Тип ВНД – {0}?"
        if (_obj.ConditionType == ConditionType.IRDType)
          return litiko.Eskhata.Conditions.Resources.IRDTypeConditionNameFormat(Conditions.Info.Properties.IRDTypelitiko.GetLocalizedValue(_obj.IRDTypelitiko));
        
        // "Орган утверждения – {0}?"
        if (_obj.ConditionType == ConditionType.OrganForApprov)
          return litiko.Eskhata.Conditions.Resources.OrganForApprovConditionNameFormat(_obj.OrganForApprovinglitiko);

        if (_obj.ConditionType == ConditionType.StandardRespons)
          return litiko.Eskhata.Conditions.Resources.StandardResponseConditionName;
        
        // "Категория – {0}?"
        if (_obj.ConditionType == ConditionType.MeetingCategorylitiko)
          return litiko.Eskhata.Conditions.Resources.MeetingCategoryConditionFormat(_obj.MeetingCategorylitiko);
        
        // Документ зарегистрирован         // Vals 20250916
        if (_obj.ConditionType == ConditionType.IsDocumentRegis)
          return litiko.Eskhata.Conditions.Resources.IsDocumentRegistered;
        
      }
      return base.GetConditionName();
    }
  }
}