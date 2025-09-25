using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.NSI.Server
{
  public class ModuleFunctions
  {
    /// <summary>
    /// Получить запись матрицы ответственности по договору
    /// </summary>
    [Public]
    public NSI.IResponsibilityMatrix GetResponsibilityMatrix(litiko.Eskhata.IContract contract)
    {
      return NSI.ResponsibilityMatrices.GetAll()
        .Where(x => Equals(x.DocumentKind, contract.DocumentKind))
        .Where(x => x.ContractCategories.Any(c => Equals(c.Category, contract.DocumentGroup)))
        .FirstOrDefault();
    }    
    
    /// <summary>
    /// Получить запись справочника "Соответствие видов договоров и документов на оплату"
    /// </summary>
    [Public]
    public NSI.IContractsVsPaymentDoc GetContractsVsPaymentDoc(litiko.Eskhata.IContract contract, Sungero.Parties.ICounterparty counterparty)
    {
      Sungero.Core.Enumeration? counterpartyType;
      if (Sungero.Parties.Companies.Is(counterparty))
        counterpartyType = NSI.TaxRate.CounterpartyType.Company;
      else if (Sungero.Parties.People.Is(counterparty))
        counterpartyType = NSI.TaxRate.CounterpartyType.Person;
      else if (Sungero.Parties.Banks.Is(counterparty))
        counterpartyType = NSI.TaxRate.CounterpartyType.Bank;
      else
        counterpartyType = null;
      
      return NSI.ContractsVsPaymentDocs.GetAll()
        .Where(x => Equals(x.DocumentKind, contract.DocumentKind) && Equals(x.Category, contract.DocumentGroup) && Equals(x.CounterpartyType, counterpartyType))        
        .FirstOrDefault();
    }
  }
}