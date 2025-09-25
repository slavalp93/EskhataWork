using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.CounterpartyDocument;

namespace litiko.Eskhata.Shared
{
  partial class CounterpartyDocumentFunctions
  {
    /// <summary>
    /// Установить обязательность свойств в зависимости от заполненных данных.
    /// </summary>
    public override void SetRequiredProperties()
    {
      base.SetRequiredProperties();
      
      _obj.State.Properties.Subject.IsRequired = false;
    }
  }
}