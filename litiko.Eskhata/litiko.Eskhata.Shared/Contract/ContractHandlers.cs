using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Contract;

namespace litiko.Eskhata
{
  partial class ContractSharedHandlers
  {

    public virtual void AmountForPeriodlitikoChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      Functions.Contract.FillTotalAmount(_obj, e.NewValue, _obj.CurrencyRatelitiko, _obj.Currency);
    }

    public override void CounterpartyChanged(Sungero.Docflow.Shared.ContractualDocumentBaseCounterpartyChangedEventArgs e)
    {
      base.CounterpartyChanged(e);
      
      PublicFunctions.ContractualDocument.FillTaxRate(_obj, DocumentKinds.As(_obj.DocumentKind), Sungero.Contracts.ContractCategories.As(_obj.DocumentGroup), e.NewValue);      
    }

    public override void DocumentGroupChanged(Sungero.Docflow.Shared.OfficialDocumentDocumentGroupChangedEventArgs e)
    {
      base.DocumentGroupChanged(e);
      
      PublicFunctions.ContractualDocument.FillTaxRate(_obj, DocumentKinds.As(_obj.DocumentKind), Sungero.Contracts.ContractCategories.As(e.NewValue), _obj.Counterparty);
      PublicFunctions.ContractualDocument.FillResponsibilityMatrix(_obj);
    }

    public virtual void IsEqualPaymentlitikoChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {      
      if (e.NewValue.GetValueOrDefault())
        _obj.IsPartialPaymentlitiko = false;
    }

    public virtual void IsPartialPaymentlitikoChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {      
      if (e.NewValue.GetValueOrDefault())
        _obj.IsEqualPaymentlitiko = false;           
    }

    public override void DocumentKindChanged(Sungero.Docflow.Shared.OfficialDocumentDocumentKindChangedEventArgs e)
    {
      base.DocumentKindChanged(e);
      
      Functions.Contract.SetIsStandard(_obj);            
      PublicFunctions.ContractualDocument.FillTaxRate(_obj, DocumentKinds.As(e.NewValue), Sungero.Contracts.ContractCategories.As(_obj.DocumentGroup), _obj.Counterparty);      
      PublicFunctions.ContractualDocument.FillResponsibilityMatrix(_obj);
    }

  }
}