using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Department;

namespace litiko.Eskhata
{
  partial class DepartmentSharedHandlers
  {

    public virtual void ExternalCodelitikoChanged(Sungero.Domain.Shared.StringPropertyChangedEventArgs e)
    {
      // Вычисление кода подразделения по коду из внешней системы: в «Код» сохраняются все символы после 4-го.
      if(!string.IsNullOrEmpty(e.NewValue) && e.NewValue.Length > 4)
        _obj.Code = e.NewValue.Remove(0, 4);
      else
        _obj.Code = string.Empty;
    }

  }
}