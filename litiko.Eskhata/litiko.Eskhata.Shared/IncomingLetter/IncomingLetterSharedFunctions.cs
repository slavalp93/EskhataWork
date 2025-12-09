using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.IncomingLetter;

namespace litiko.Eskhata.Shared
{
  partial class IncomingLetterFunctions
  {
    /// <summary>
    /// Установить обязательность свойств.
    /// </summary>
    public override void SetRequiredProperties()
    {
      base.SetRequiredProperties();

      _obj.State.Properties.Dated.IsRequired = true;
      _obj.State.Properties.InNumber.IsRequired = true;
    }     
  }
}