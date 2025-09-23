using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Company;
using System.Xml;
using System.Xml.Linq;

namespace litiko.Eskhata.Client
{
  partial class CompanyActions
  {
    public override void FillFromABSlitiko(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.FillFromABSlitiko(e);            
      
      #region Подсветка измененных контролов        
      if (_obj.State.Properties.Name.IsChanged)
        _obj.State.Properties.Name.HighlightColor = Colors.Common.Green;
      if (_obj.State.Properties.LegalName.IsChanged)
        _obj.State.Properties.LegalName.HighlightColor = Colors.Common.Green;        
      if (_obj.State.Properties.Inamelitiko.IsChanged)
        _obj.State.Properties.Inamelitiko.HighlightColor = Colors.Common.Green;        
      if (_obj.State.Properties.TRRC.IsChanged)
        _obj.State.Properties.TRRC.HighlightColor = Colors.Common.Green;        
      if (_obj.State.Properties.NCEO.IsChanged)
        _obj.State.Properties.NCEO.HighlightColor = Colors.Common.Green;        
      if (_obj.State.Properties.OKOPFlitiko.IsChanged)
        _obj.State.Properties.OKOPFlitiko.HighlightColor = Colors.Common.Green;
      if (_obj.State.Properties.OKFSlitiko.IsChanged)
        _obj.State.Properties.OKFSlitiko.HighlightColor = Colors.Common.Green;
      if (_obj.State.Properties.OKONHlitiko.IsChanged)
        _obj.State.Properties.OKONHlitiko.HighlightColor = Colors.Common.Green;
      if (_obj.State.Properties.OKVEDlitiko.IsChanged)
        _obj.State.Properties.OKVEDlitiko.HighlightColor = Colors.Common.Green;
      if (_obj.State.Properties.RegNumlitiko.IsChanged)
        _obj.State.Properties.RegNumlitiko.HighlightColor = Colors.Common.Green;
      if (_obj.State.Properties.Numberslitiko.IsChanged)
        _obj.State.Properties.Numberslitiko.HighlightColor = Colors.Common.Green;
      if (_obj.State.Properties.Businesslitiko.IsChanged)
        _obj.State.Properties.Businesslitiko.HighlightColor = Colors.Common.Green;
      if (_obj.State.Properties.EnterpriseTypelitiko.IsChanged)
        _obj.State.Properties.EnterpriseTypelitiko.HighlightColor = Colors.Common.Green;
      if (_obj.State.Properties.Countrylitiko.IsChanged)
        _obj.State.Properties.Countrylitiko.HighlightColor = Colors.Common.Green;
      if (_obj.State.Properties.PostalAddress.IsChanged)
        _obj.State.Properties.PostalAddress.HighlightColor = Colors.Common.Green;  
      if (_obj.State.Properties.LegalAddress.IsChanged)
        _obj.State.Properties.LegalAddress.HighlightColor = Colors.Common.Green;  
      if (_obj.State.Properties.Phones.IsChanged)
        _obj.State.Properties.Phones.HighlightColor = Colors.Common.Green;  
      if (_obj.State.Properties.Email.IsChanged)
        _obj.State.Properties.Email.HighlightColor = Colors.Common.Green;          
      #endregion
    }

    public override bool CanFillFromABSlitiko(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanFillFromABSlitiko(e);
    }

  }

}