using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ContractualDocument;

namespace litiko.Eskhata
{
  partial class ContractualDocumentClientHandlers
  {

    public override void TotalAmountValueInput(Sungero.Presentation.DoubleValueInputEventArgs e)
    {      
      if (e.NewValue < 0)
        e.AddError(Sungero.Docflow.Resources.TotalAmountMustBePositive);      
      
      this._obj.State.Properties.TotalAmount.HighlightColor = Sungero.Core.Colors.Empty;
    }

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);
      
      // TODO убрать из обновления формы
      var counterparty = litiko.Eskhata.Counterparties.As(_obj.Counterparty);
      if (counterparty != null)
      {
        List<string> invalidProperties = Functions.ContractualDocument.Remote.CheckCounterpartyProperties(_obj, counterparty);
        if (invalidProperties.Any())        
          e.AddWarning(ContractualDocuments.Resources.CheckCounterpartyPropertiesMessageFormat(string.Join(", ", invalidProperties)));
      }      
    }

  }
}