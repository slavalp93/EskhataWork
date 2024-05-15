using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.BusinessUnit;

namespace litiko.Eskhata
{
  partial class BusinessUnitServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      _obj.NUNonrezidentlitiko = false;
    }
  }

}