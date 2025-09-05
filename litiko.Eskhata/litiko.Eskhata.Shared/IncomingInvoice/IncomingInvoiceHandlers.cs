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
      _obj.CurrencyOperationlitiko = contract?.CurrencyOperationlitiko;
      
      _obj.TaxRatelitiko = contract?.TaxRatelitiko;
      
      if (contract != null)
      {
        var matrix = NSI.PublicFunctions.Module.GetResponsibilityMatrix(contract);
        _obj.OurSignatory = Sungero.Company.Employees.As(matrix?.ResponsibleAccountant);
      }            
    }

  }
}