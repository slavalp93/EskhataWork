using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Bank;

namespace litiko.Eskhata.Client
{
  partial class BankActions
  {
    public virtual void FillFromABSlitiko(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (string.IsNullOrWhiteSpace(_obj.BIC))
      {
        e.AddError(Banks.Resources.ErrorNeedFillBIC);
        return;
      }      
      
      if (!string.IsNullOrEmpty(_obj.BIC))
        _obj.BIC = _obj.BIC.Trim();
      
      e.AddInformation("Интеграция с АБС в разработке...");
      
      //var response = Functions.CompanyBase.Remote.FillFromService(_obj, string.Empty);
      //var error = response.Message;
    }

    public virtual bool CanFillFromABSlitiko(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
       return _obj.AccessRights.CanUpdate() && _obj.IsCardReadOnly != true;
    }

  }

}