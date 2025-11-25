using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Bank;

namespace litiko.Eskhata
{
  partial class BankClientHandlers
  {

    public override void CorrespondentAccountValueInput(Sungero.Presentation.StringValueInputEventArgs e)
    {
      var len = !string.IsNullOrWhiteSpace(e.NewValue) ? e.NewValue.Length : 0;
      if (len > 20)
        e.AddError(Resources.NoMoreThanXCharactersFormat(20));       
    }

  }
}