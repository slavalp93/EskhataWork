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
      if (!_obj.IsPartialPaymentlitiko.HasValue || _obj.IsPartialPaymentlitiko.GetValueOrDefault() == e.NewValue)
        _obj.IsPartialPaymentlitiko = !e.NewValue;
    }

    public virtual void IsPartialPaymentlitikoChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      if (!_obj.IsEqualPaymentlitiko.HasValue || _obj.IsEqualPaymentlitiko.GetValueOrDefault() == e.NewValue)
        _obj.IsEqualPaymentlitiko = !e.NewValue;
      
      if (e.NewValue.GetValueOrDefault())
        _obj.AmountForPeriodlitiko = null;
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