using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.NSI.TaxRate;

namespace litiko.NSI.Server
{
  partial class TaxRateFunctions
  {
    /// <summary>
    /// Получить дубликаты
    /// </summary>
    /// <returns></returns>
    [Remote(IsPure = true)]
    public List<ITaxRate> GetDuplicates()
    {
      var duplicates = TaxRates.GetAll()
        .Where(x => x.Id != _obj.Id)
        .Where(x => x.Status != Sungero.CoreEntities.DatabookEntry.Status.Closed)
        .Where(x => Equals(x.DocumentKind, _obj.DocumentKind))
        .Where(x => Equals(x.Category, _obj.Category))
        .Where(x => Equals(x.CounterpartyType, _obj.CounterpartyType))
        .Where(x => Equals(x.TaxResident, _obj.TaxResident))
        .ToList();
      
      return duplicates;            
    }
  }
}