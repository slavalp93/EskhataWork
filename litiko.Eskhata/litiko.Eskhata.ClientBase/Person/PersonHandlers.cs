using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Person;

namespace litiko.Eskhata
{
  partial class PersonClientHandlers
  {

    public override void TINValueInput(Sungero.Presentation.StringValueInputEventArgs e)
    {
      var len = !string.IsNullOrWhiteSpace(e.NewValue) ? e.NewValue.Length : 0;
      if (_obj.Nonresident == true)
      {
        if (len > 12)
          e.AddError(Resources.INNNonRezidentPersonError);
      }
      else
      {
        if (len > 9)
          e.AddError(Resources.INNRezidentPersonError);
      }
    }
  }
}