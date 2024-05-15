using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Bank;

namespace litiko.Eskhata
{
  partial class BankServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      _obj.SettlParticipantlitiko = false;
      _obj.LoroCorrespondentlitiko = false;
      _obj.NostroCorrespondentlitiko = false;
    }
  }

}