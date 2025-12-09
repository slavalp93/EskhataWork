using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Person;

namespace litiko.Eskhata.Client
{
  partial class PersonActions
  {
    public override void FillFromABSlitiko(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.FillFromABSlitiko(e);
      
      #region Подсветка измененных контролов
      var highlightColor = Colors.Common.Green;
      if (_obj.State.Properties.LastName.IsChanged)
        _obj.State.Properties.LastName.HighlightColor = highlightColor;      
      if (_obj.State.Properties.FirstName.IsChanged)
        _obj.State.Properties.FirstName.HighlightColor = highlightColor;      
      if (_obj.State.Properties.MiddleName.IsChanged)
        _obj.State.Properties.MiddleName.HighlightColor = highlightColor;      
      if (_obj.State.Properties.LastNameTGlitiko.IsChanged)
        _obj.State.Properties.LastNameTGlitiko.HighlightColor = highlightColor;      
      if (_obj.State.Properties.FirstNameTGlitiko.IsChanged)
        _obj.State.Properties.FirstNameTGlitiko.HighlightColor = highlightColor;      
      if (_obj.State.Properties.MiddleNameTGlitiko.IsChanged)
        _obj.State.Properties.MiddleNameTGlitiko.HighlightColor = highlightColor;      
      if (_obj.State.Properties.Sex.IsChanged)
        _obj.State.Properties.Sex.HighlightColor = highlightColor;      
      if (_obj.State.Properties.Inamelitiko.IsChanged)
        _obj.State.Properties.Inamelitiko.HighlightColor = highlightColor;      
      if (_obj.State.Properties.NUNonrezidentlitiko.IsChanged)
        _obj.State.Properties.NUNonrezidentlitiko.HighlightColor = highlightColor;      
      if (_obj.State.Properties.Nonresident.IsChanged)
        _obj.State.Properties.Nonresident.HighlightColor = highlightColor;      
      if (_obj.State.Properties.DateOfBirth.IsChanged)
        _obj.State.Properties.DateOfBirth.HighlightColor = highlightColor;      
      if (_obj.State.Properties.FamilyStatuslitiko.IsChanged)
        _obj.State.Properties.FamilyStatuslitiko.HighlightColor = highlightColor;      
      if (_obj.State.Properties.TIN.IsChanged)
        _obj.State.Properties.TIN.HighlightColor = highlightColor;      
      if (_obj.State.Properties.Citizenship.IsChanged)
        _obj.State.Properties.Citizenship.HighlightColor = highlightColor;      
      if (_obj.State.Properties.PostalAddress.IsChanged)
        _obj.State.Properties.PostalAddress.HighlightColor = highlightColor;      
      if (_obj.State.Properties.LegalAddress.IsChanged)
        _obj.State.Properties.LegalAddress.HighlightColor = highlightColor;      
      if (_obj.State.Properties.Phones.IsChanged)
        _obj.State.Properties.Phones.HighlightColor = highlightColor;      
      if (_obj.State.Properties.Email.IsChanged)
        _obj.State.Properties.Email.HighlightColor = highlightColor;
      if (_obj.State.Properties.Homepage.IsChanged)
        _obj.State.Properties.Homepage.HighlightColor = highlightColor;
      if (_obj.State.Properties.VATPayerlitiko.IsChanged)
        _obj.State.Properties.VATPayerlitiko.HighlightColor = highlightColor;
      if (_obj.State.Properties.SINlitiko.IsChanged)
        _obj.State.Properties.SINlitiko.HighlightColor = highlightColor;
      if (_obj.State.Properties.Account.IsChanged)
        _obj.State.Properties.Account.HighlightColor = highlightColor;
      if (_obj.State.Properties.AccountEskhatalitiko.IsChanged)
        _obj.State.Properties.AccountEskhatalitiko.HighlightColor = highlightColor;
      if (_obj.State.Properties.IdentityKind.IsChanged)
        _obj.State.Properties.IdentityKind.HighlightColor = highlightColor;
      if (_obj.State.Properties.IdentityDateOfIssue.IsChanged)
        _obj.State.Properties.IdentityDateOfIssue.HighlightColor = highlightColor;            
      if (_obj.State.Properties.IdentityExpirationDate.IsChanged)
        _obj.State.Properties.IdentityExpirationDate.HighlightColor = highlightColor; 
      if (_obj.State.Properties.IdentityNumber.IsChanged)
        _obj.State.Properties.IdentityNumber.HighlightColor = highlightColor; 
      if (_obj.State.Properties.IdentitySeries.IsChanged)
        _obj.State.Properties.IdentitySeries.HighlightColor = highlightColor; 
      if (_obj.State.Properties.IdentityAuthority.IsChanged)
        _obj.State.Properties.IdentityAuthority.HighlightColor = highlightColor;       
      #endregion      
    }

    public override bool CanFillFromABSlitiko(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanFillFromABSlitiko(e);
    }

  }


}