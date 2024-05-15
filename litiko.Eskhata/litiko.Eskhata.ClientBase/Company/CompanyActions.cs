using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Company;

namespace litiko.Eskhata.Client
{
  partial class CompanyActions
  {
    public virtual void FillFromABSlitiko(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (string.IsNullOrWhiteSpace(_obj.TIN))
      {
        e.AddError(Companies.Resources.ErrorNeedFillTin);
        return;
      }      
      
      if (!string.IsNullOrEmpty(_obj.TIN))
        _obj.TIN = _obj.TIN.Trim();
      
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