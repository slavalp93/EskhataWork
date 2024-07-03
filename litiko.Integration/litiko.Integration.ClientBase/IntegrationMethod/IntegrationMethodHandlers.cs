using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Integration.IntegrationMethod;

namespace litiko.Integration
{
  partial class IntegrationMethodClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      _obj.State.Properties.SaveRequestToRX.IsEnabled = false;
    }

  }
}