using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ApprovalStage;

namespace litiko.Eskhata
{
  partial class ApprovalStageServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      _obj.AllowResultNotAgreelitiko = false;
    }
  }


}