using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.BusinessUnit;
using System.Text.RegularExpressions;

namespace litiko.Eskhata
{
  partial class BusinessUnitServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      // base.BeforeSave(e);
      
      #region Базовый обработчик без проверок ИНН, КПП, ОГРН, ОКПО
      if (!string.IsNullOrEmpty(_obj.TRRC))
        _obj.TRRC = _obj.TRRC.Trim();
      
      if (!string.IsNullOrEmpty(_obj.TIN))
        _obj.TIN = _obj.TIN.Trim();
      
      if (!string.IsNullOrEmpty(_obj.PSRN))
        _obj.PSRN = _obj.PSRN.Trim();
      
      if (!string.IsNullOrEmpty(_obj.NCEO))
        _obj.NCEO = _obj.NCEO.Trim();
      
      // Проверить код на пробелы, если свойство изменено.
      if (!string.IsNullOrEmpty(_obj.Code))
      {
        // При изменении кода e.AddError сбрасывается.
        var codeIsChanged = _obj.State.Properties.Code.IsChanged;
        _obj.Code = _obj.Code.Trim();
        
        if (codeIsChanged && Regex.IsMatch(_obj.Code, @"\s"))
          e.AddError(_obj.Info.Properties.Code, Sungero.Company.Resources.NoSpacesInCode);
      }
      
      #region Проверить дубли      
      
      var checkDuplicatesErrorText = Functions.BusinessUnit.GetCounterpartyDuplicatesErrorText(_obj);
      if (!string.IsNullOrEmpty(checkDuplicatesErrorText))
        e.AddWarning(checkDuplicatesErrorText, _obj.Info.Actions.ShowDuplicates);

      #endregion
      
      #region Проверить циклические ссылки в подчиненных НОР
      
      if (_obj.State.Properties.HeadCompany.IsChanged && _obj.HeadCompany != null)
      {
        var headCompany = _obj.HeadCompany;
        
        while (headCompany != null)
        {
          if (Equals(headCompany, _obj))
          {
            e.AddError(_obj.Info.Properties.HeadCompany, BusinessUnits.Resources.HeadCompanyCyclicReference, _obj.Info.Properties.HeadCompany);
            break;
          }
          
          headCompany = headCompany.HeadCompany;
        }
      }
      
      #endregion
      
      // Выставить параметр необходимости индексации сущности, при изменении индексируемых полей.
      var props = _obj.State.Properties;
      if (props.Name.IsChanged || props.LegalName.IsChanged || props.TIN.IsChanged || props.TRRC.IsChanged || props.PSRN.IsChanged || props.Status.IsChanged)
        e.Params.AddOrUpdate(Sungero.Commons.PublicConstants.Module.IsIndexedEntityInsertedParamKey, _obj.State.IsInserted);      
      #endregion
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      _obj.NUNonrezidentlitiko = false;
    }
  }

}