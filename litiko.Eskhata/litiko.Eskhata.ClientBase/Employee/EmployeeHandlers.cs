using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Employee;

namespace litiko.Eskhata
{
  partial class EmployeeClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);
      
      // Поле "Учетная запись" показывать только Админам
      if (!Users.Current.IncludedIn(Roles.Administrators))
        _obj.State.Properties.Login.IsVisible = false;
    }

  }
}