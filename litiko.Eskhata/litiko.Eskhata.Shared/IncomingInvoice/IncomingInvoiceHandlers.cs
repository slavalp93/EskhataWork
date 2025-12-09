using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.IncomingInvoice;

namespace litiko.Eskhata
{
  partial class IncomingInvoiceSharedHandlers
  {

    public override void ContractChanged(Sungero.Contracts.Shared.IncomingInvoiceContractChangedEventArgs e)
    {
      base.ContractChanged(e);
      
      var contract = Contracts.As(e.NewValue);
      var supAgreement = SupAgreements.As(e.NewValue);
      
      if (contract == null && supAgreement != null && supAgreement.LeadingDocument != null)
        contract = Contracts.As(supAgreement.LeadingDocument);
      
      _obj.CurrencyOperationlitiko = contract?.CurrencyOperationlitiko;
      _obj.Currency = contract?.CurrencyContractlitiko;
      
      _obj.TaxRatelitiko = contract?.TaxRatelitiko;
      
      if (contract != null)
      {
        var matrix = NSI.PublicFunctions.Module.GetResponsibilityMatrix(contract);
        _obj.OurSignatory = Sungero.Company.Employees.As(matrix?.ResponsibleAccountant);
      }            
    }

  }
}