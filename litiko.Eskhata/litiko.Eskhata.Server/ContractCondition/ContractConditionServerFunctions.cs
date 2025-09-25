using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ContractCondition;

namespace litiko.Eskhata.Server
{
  partial class ContractConditionFunctions
  {

    /// <summary>
    /// Получить текст условия.
    /// </summary>
    /// <returns>Текст условия.</returns>    
    public override string GetConditionName()
    {
      using (TenantInfo.Culture.SwitchTo())
      {
        if (_obj.ConditionType == Sungero.Contracts.ContractCondition.ConditionType.AmountIsMore)
        {
          return litiko.Eskhata.ContractConditions.Resources.ContractIsMoreThanFormat(_obj.Info.Properties.AmountOperator.GetLocalizedValue(_obj.AmountOperator),
                                                                   Sungero.Docflow.PublicFunctions.ConditionBase.AmountFormat(_obj.Amount));
        }

        if (_obj.ConditionType == ConditionType.PaymentBasedOn)
        {
          return litiko.Eskhata.ContractConditions.Resources.PaymentBasedOnFormat(_obj.Info.Properties.PaymentBasedOnlitiko.GetLocalizedValue(_obj.PaymentBasedOnlitiko));
        }
        
        if (_obj.ConditionType == ConditionType.DocumentGroup)
        {          
          return litiko.Eskhata.ContractConditions.Resources.DocumentGroupFormat(ConditionMultiSelectNameBuilder(_obj.DocumentGroupslitiko.Select(x => x.DocumentGroup.Name).ToList()));
          
        }        
      }
      
      return base.GetConditionName();
    }
  }
}