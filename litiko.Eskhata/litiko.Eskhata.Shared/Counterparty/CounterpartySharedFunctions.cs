using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Counterparty;

namespace litiko.Eskhata.Shared
{
  partial class CounterpartyFunctions
  {
    /// <summary>
    /// Заполнить адрес.
    /// </summary>
    /// <param name="city">Населенный пункт</param>
    /// <param name="street">Улица.</param>
    /// <param name="houseNumber">Номер дома.</param> 
    public void FillLegalAddress(string city, string street, string houseNumber)
    {                              
      string address = Eskhata.Counterparties.Resources.AddressTemplateFormat(city, street, houseNumber);      
      
      if (_obj.LegalAddress != address)
        _obj.LegalAddress = address;
    }
  }
}