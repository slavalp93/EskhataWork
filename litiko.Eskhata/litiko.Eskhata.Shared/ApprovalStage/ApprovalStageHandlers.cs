using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ApprovalStage;

namespace litiko.Eskhata
{
  partial class ApprovalStageSharedHandlers
  {

    public override void AllowSendToReworkChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      base.AllowSendToReworkChanged(e);
      
      if (e.NewValue.Value == e.OldValue)
        return;
      
      if (e.NewValue == true)
      {
        if (_obj.CustomStageTypelitiko.GetValueOrDefault() == litiko.Eskhata.ApprovalStage.CustomStageTypelitiko.Voting)
          _obj.CustomStageTypelitiko = null;
      }
    }

  }
}