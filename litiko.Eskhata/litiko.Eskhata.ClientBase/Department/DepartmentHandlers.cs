using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Department;

namespace litiko.Eskhata
{
  partial class DepartmentClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);
      
      _obj.State.Properties.ExternalId.IsEnabled = false;
      _obj.State.Properties.ExternalCodelitiko.IsEnabled = false;
    }

  }
}