using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Counterparty;
using System.Text.RegularExpressions;

namespace litiko.Eskhata
{
  partial class CounterpartyServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      //base.BeforeSave(e);
      
      #region Базовый обработчик без проверок ИНН, КПП, ОГРН, ОКПО
      if (!string.IsNullOrWhiteSpace(_obj.Code))
      {
        _obj.Code = _obj.Code.Trim();
        if (Regex.IsMatch(_obj.Code, @"\s"))
          e.AddError(_obj.Info.Properties.Code, Sungero.Company.Resources.NoSpacesInCode);
      }
      
      // Проверить код на пробелы, если свойство изменено.
      if (!string.IsNullOrEmpty(_obj.Code))
      {
        // При изменении кода e.AddError сбрасывается.
        var codeIsChanged = _obj.State.Properties.Code.IsChanged;
        _obj.Code = _obj.Code.Trim();
        
        if (codeIsChanged && Regex.IsMatch(_obj.Code, @"\s"))
          e.AddError(_obj.Info.Properties.Code, Sungero.Company.Resources.NoSpacesInCode);
      }
      
      if (!_obj.AccessRights.CanChangeCard())
      {
        var exchangeBoxesProp = _obj.State.Properties.ExchangeBoxes;
        var canExchangeProp = _obj.State.Properties.CanExchange;
        
        if (_obj.State.Properties
            .Where(x => !Equals(x, exchangeBoxesProp) && !Equals(x, canExchangeProp))
            .Select(x => x as Sungero.Domain.Shared.IPropertyState)
            .Where(x => x != null)
            .Any(x => x.IsChanged))
        {
          e.AddError(Counterparties.Resources.NoRightsToChangeCard);
        }
      }
      
      // Трим пробелов в ИНН, ОГРН, ОКПО.
      if (!string.IsNullOrEmpty(_obj.TIN))
        _obj.TIN = _obj.TIN.Trim();

      if (!string.IsNullOrEmpty(_obj.PSRN))
        _obj.PSRN = _obj.PSRN.Trim();
      
      if (!string.IsNullOrEmpty(_obj.NCEO))
        _obj.NCEO = _obj.NCEO.Trim();
      
      // Проверка дублей контрагента.
      var saveFromUI = e.Params.Contains(Counterparties.Resources.ParameterSaveFromUIFormat(_obj.Id));
      var isForceDuplicateSave = e.Params.Contains(Counterparties.Resources.ParameterIsForceDuplicateSaveFormat(_obj.Id));
      if (saveFromUI && !isForceDuplicateSave)
      {
        var checkDuplicatesErrorText = Sungero.Parties.PublicFunctions.Counterparty.GetCounterpartyDuplicatesErrorText(_obj);
        if (!string.IsNullOrWhiteSpace(checkDuplicatesErrorText))
          e.AddError(checkDuplicatesErrorText, _obj.Info.Actions.ShowDuplicates, _obj.Info.Actions.ForceDuplicateSave);
      }
      
      // Проверка ящиков эл. обмена.
      foreach (var box in _obj.ExchangeBoxes.Select(x => x.Box).Distinct())
      {
        var boxLines = _obj.ExchangeBoxes.Where(x => Equals(x.Box, box)).ToList();
        if (boxLines.All(x => x.IsDefault == false))
        {
          foreach (var boxLine in boxLines)
            e.AddError(boxLine, _obj.Info.Properties.ExchangeBoxes.Properties.IsDefault,
                       Counterparties.Resources.NoDefaultBoxServiceFormat(boxLine.Box),
                       _obj.Info.Properties.ExchangeBoxes.Properties.IsDefault);
        }
      }      
      #endregion
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      _obj.NUNonrezidentlitiko = false;
      _obj.VATPayerlitiko = false;
    }
  }

}