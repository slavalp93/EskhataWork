using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Employee;

namespace litiko.Eskhata
{
  partial class EmployeeSharedHandlers
  {

    public override void DepartmentChanged(Sungero.Company.Shared.EmployeeDepartmentChangedEventArgs e)
    {
      base.DepartmentChanged(e);
      
      if (e.NewValue != null)
      {
        if (_obj.DepCodelitiko != e.NewValue.Code)
          _obj.DepCodelitiko = e.NewValue.Code;
      }
      else
        _obj.DepCodelitiko = null;
    }
  }



}
