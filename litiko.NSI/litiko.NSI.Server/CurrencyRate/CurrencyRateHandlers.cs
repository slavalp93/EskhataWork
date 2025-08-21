using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.NSI.CurrencyRate;

namespace litiko.NSI
{
  partial class CurrencyRateServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      #region Заполнить имя
      
      Functions.CurrencyRate.FillName(_obj);

      #endregion      
    }
  }

}