using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.BusinessUnit;

namespace litiko.Eskhata.Shared
{
  partial class BusinessUnitFunctions
  {
    /// <summary>
    /// Получить текст ошибки о наличии дублей.
    /// </summary>
    /// <returns>Текст ошибки.</returns>
    public override string GetCounterpartyDuplicatesErrorText()
    {
      return base.GetCounterpartyDuplicatesErrorText();
    }
  }
}