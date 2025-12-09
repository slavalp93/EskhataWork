using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.NSI.ContractsVsPaymentDoc;

namespace litiko.NSI.Server
{
  partial class ContractsVsPaymentDocFunctions
  {
    /// <summary>
    /// Получить дубликаты
    /// </summary>
    /// <returns></returns>
    [Remote(IsPure = true)]
    public List<IContractsVsPaymentDoc> GetDuplicates()
    {
      var duplicates = ContractsVsPaymentDocs.GetAll()
        .Where(x => x.Id != _obj.Id)
        .Where(x => x.Status != Sungero.CoreEntities.DatabookEntry.Status.Closed)
        .Where(x => Equals(x.DocumentKind, _obj.DocumentKind))
        .Where(x => Equals(x.Category, _obj.Category))
        .Where(x => Equals(x.CounterpartyType, _obj.CounterpartyType))
        .ToList();
      
      return duplicates;      
    }
  }
}