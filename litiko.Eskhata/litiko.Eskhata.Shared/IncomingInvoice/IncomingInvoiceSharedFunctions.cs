using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.IncomingInvoice;

namespace litiko.Eskhata.Shared
{
  partial class IncomingInvoiceFunctions
  {
    /// <summary>
    /// Установить обязательность свойств в зависимости от заполненных данных.
    /// </summary>
    public override void SetRequiredProperties()
    {
      base.SetRequiredProperties();
      
      _obj.State.Properties.TotalAmount.IsRequired = false;
      _obj.State.Properties.TotalAmountlitiko.IsRequired = true;
    }
  }
}