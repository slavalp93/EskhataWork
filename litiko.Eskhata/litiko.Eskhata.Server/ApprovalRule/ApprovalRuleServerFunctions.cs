using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ApprovalRule;

namespace litiko.Eskhata.Server
{
  partial class ApprovalRuleFunctions
  {
    public override bool CheckRoutePossibility(List<Sungero.Docflow.Structures.ApprovalRuleBase.RouteStep> route,
                                               List<Sungero.Docflow.Structures.ApprovalRuleBase.ConditionRouteStep> ruleConditions,
                                               Sungero.Docflow.Structures.ApprovalRuleBase.RouteStep conditionStep)
    {
      var possibleStage = base.CheckRoutePossibility(route, ruleConditions, conditionStep);
      var conditionType = _obj.Conditions.First(c => c.Number == conditionStep.StepNumber).Condition.ConditionType;
      
      if (conditionType == Eskhata.Condition.ConditionType.ChiefAccountant)
      {
        var conditions = this.GetChiefAccountantConditionsInRoute(route).Where(c => c.StepNumber != conditionStep.StepNumber).ToList();
        possibleStage = true;
      }
      
      if (conditionType == Eskhata.Condition.ConditionType.IsRecommendat || conditionType == Eskhata.Condition.ConditionType.IsRelatedStruct || conditionType == Eskhata.Condition.ConditionType.IsRequirements)
      {                
        possibleStage = true;
      }      
      
      return possibleStage;
    }
    
    public List<Sungero.Docflow.Structures.ApprovalRuleBase.RouteStep> GetChiefAccountantConditionsInRoute(List<Sungero.Docflow.Structures.ApprovalRuleBase.RouteStep> route)
    {
      return route.Where(e => _obj.Conditions.Any(c => Equals(c.Number, e.StepNumber) && c.Condition.ConditionType ==
                                                  Eskhata.Condition.ConditionType.ChiefAccountant)).ToList();
    }        
  }
}