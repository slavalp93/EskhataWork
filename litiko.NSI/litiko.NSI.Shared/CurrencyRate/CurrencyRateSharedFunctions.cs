using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.NSI.CurrencyRate;

namespace litiko.NSI.Shared
{
  partial class CurrencyRateFunctions
  {
    /// <summary>
    /// Заполнить наименование.
    /// </summary>
    [Public]
    public virtual void FillName()
    {     
      // Курс <Буквенный код> на дату <Дата курса> - <Значение курса>
      string dateRate = _obj.Date.HasValue ? _obj.Date.Value.ToString("dd.MM.yyyy") : string.Empty;
      _obj.Name = string.Format("Курс {0} на дату {1} - {2}", _obj.Currency?.AlphaCode, dateRate, _obj.Rate.ToString());
    }
  }
}