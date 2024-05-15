using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.BusinessUnit;

namespace litiko.Eskhata
{
  partial class BusinessUnitClientHandlers
  {

    public override void NCEOValueInput(Sungero.Presentation.StringValueInputEventArgs e)
    {      
      var len = !string.IsNullOrWhiteSpace(e.NewValue) ? e.NewValue.Length : 0;
      if (len > 16)
        e.AddError(Resources.OKPOerror);
    }

    public override void NonresidentValueInput(Sungero.Presentation.BooleanValueInputEventArgs e)
    {      
      var len = !string.IsNullOrWhiteSpace(_obj.TIN) ? _obj.TIN.Length : 0;
      if (e.NewValue == true)
      {
        if (len > 12)
          e.AddError(Resources.INNNonRezidentError);
      }
      else
      {
        if (len > 9)
          e.AddError(Resources.INNRezidentError);        
      }
    }

    public override void TINValueInput(Sungero.Presentation.StringValueInputEventArgs e)
    {      
      var len = !string.IsNullOrWhiteSpace(e.NewValue) ? e.NewValue.Length : 0;
      if (_obj.Nonresident == true)
      {                
        if (len > 12)
          e.AddError(Resources.INNNonRezidentError);
      }
      else
      {
        if (len > 9)
          e.AddError(Resources.INNRezidentError);
      }
    }
  }

}