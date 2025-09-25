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
      if (_obj.State.Properties.SWIFT.IsChanged)
        _obj.State.Properties.SWIFT.HighlightColor = Colors.Common.Green; 
      if (_obj.State.Properties.TIN.IsChanged)
        _obj.State.Properties.TIN.HighlightColor = Colors.Common.Green;         
      if (_obj.State.Properties.CorrespondentAccount.IsChanged)
        _obj.State.Properties.CorrespondentAccount.HighlightColor = Colors.Common.Green;        
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