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
    public override System.Collections.Generic.Dictionary<string, List<Enumeration?>> GetSupportedConditions()
    {
      var baseConditions = base.GetSupportedConditions();
      
      baseConditions[RecordManagementEskhata.PublicConstants.Module.DocumentTypeGuids.Order.ToString()].Add(Eskhata.Condition.ConditionType.ChiefAccountant);
      baseConditions[RecordManagementEskhata.PublicConstants.Module.DocumentTypeGuids.CompanyDirective.ToString()].Add(Eskhata.Condition.ConditionType.ChiefAccountant);
      return baseConditions;
    }
    public override Sungero.Docflow.Structures.ConditionBase.ConditionResult CheckCondition(Sungero.Docflow.IOfficialDocument document, Sungero.Docflow.IApprovalTask task)
    {
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
          Create(null, "Условие не может быть вычислено. Отправляемый документ не того вида.");
      }
      return base.CheckCondition(document, task);
    }
  }
}