using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Integration.IntegrationMethod;

namespace litiko.Integration
{
  partial class IntegrationMethodServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      // Настройки по умолчанию
      _obj.SaveRequestToIS = true;
      _obj.SaveResponseFromIS = true;
      _obj.SaveRequestToRX = true;
      _obj.SaveResponseFromRX = true;
    }
  }

}