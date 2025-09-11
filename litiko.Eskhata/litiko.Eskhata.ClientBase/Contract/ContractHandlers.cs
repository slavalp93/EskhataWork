using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Contract;

namespace litiko.Eskhata
{
  partial class ContractClientHandlers
  {

    public override void IsFrameworkContractValueInput(Sungero.Presentation.BooleanValueInputEventArgs e)
    {
      base.IsFrameworkContractValueInput(e);
      
      _obj.State.Properties.TotalAmountlitiko.IsRequired = !_obj.IsFrameworkContract.GetValueOrDefault();
    }

    public override void ValidTillValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {
      base.ValidTillValueInput(e);
      
      if (Equals(_obj.LifeCycleState, LifeCycleState.Active) && !Equals(e.NewValue, e.OldValue))
        _obj.State.Properties.ReasonForChangelitiko.IsRequired = true;
    }

  }
}