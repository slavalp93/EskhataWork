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
      }
      return base.GetConditionName();
    }
  }
}