using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Xml.Serialization;
using System.IO;

using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Parties;           // Для работы с контрагентами
using Sungero.Company;           // Для работы с сотрудниками

namespace litiko.ContractsEskhata.Structures.Module
{

  /// <summary>
  /// 
  /// </summary>
  partial class ContractData
  {
    [XmlElement("ContractNumber")]
    public string ContractNumber {get; set;}
    
    [XmlElement("ContractDate")]
    public string ContractDate { get; set; }
    
    [XmlElement("Amount")]
    public decimal Amount { get; set; }

    [XmlElement("CurrencyAlphaCode")]
    public string CurrencyAlphaCode { get; set; }

    [XmlElement("CounterpartyInn")]
    public string CounterpartyInn { get; set; }

    [XmlElement("CounterpartyName")]
    public string CounterpartyName { get; set; }
  }

}