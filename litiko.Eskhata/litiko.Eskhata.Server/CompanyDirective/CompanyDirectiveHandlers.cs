using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.CompanyDirective;

namespace litiko.Eskhata
{
  partial class CompanyDirectiveServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.ChiefAccountantApproving = false;
      base.Created(e);
    }
  }

}