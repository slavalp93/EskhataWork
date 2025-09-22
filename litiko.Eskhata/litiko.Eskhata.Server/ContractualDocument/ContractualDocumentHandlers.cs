using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ContractualDocument;

namespace litiko.Eskhata
{
  partial class ContractualDocumentServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      _obj.IsVATlitiko = false;
      _obj.IsIndividualPaymentlitiko = false;
      _obj.Currency = Sungero.Commons.PublicFunctions.Currency.Remote.GetDefaultCurrency();
    }
  }


}