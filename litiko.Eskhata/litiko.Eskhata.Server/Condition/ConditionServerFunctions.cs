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
      }
      return base.GetConditionName();
    }    
  }
}