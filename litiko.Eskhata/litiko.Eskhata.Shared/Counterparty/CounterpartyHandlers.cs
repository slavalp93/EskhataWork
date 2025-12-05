using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Counterparty;

namespace litiko.Eskhata
{
  partial class CounterpartySharedHandlers
  {

    public virtual void HouseNumberlitikoChanged(Sungero.Domain.Shared.StringPropertyChangedEventArgs e)
    {
      Functions.Counterparty.FillLegalAddress(_obj, _obj.City?.Name, _obj.Streetlitiko, e.NewValue);
    }

    public virtual void StreetlitikoChanged(Sungero.Domain.Shared.StringPropertyChangedEventArgs e)
    {
      Functions.Counterparty.FillLegalAddress(_obj, _obj.City?.Name, e.NewValue, _obj.HouseNumberlitiko);
    }

    public override void CityChanged(Sungero.Parties.Shared.CounterpartyCityChangedEventArgs e)
    {
      base.CityChanged(e);
      
      Functions.Counterparty.FillLegalAddress(_obj, e.NewValue?.Name, _obj.Streetlitiko, _obj.HouseNumberlitiko);
    }

    public override void RegionChanged(Sungero.Parties.Shared.CounterpartyRegionChangedEventArgs e)
    {
      // Ничего не проверять
      // base.RegionChanged(e);
    }

  }
}