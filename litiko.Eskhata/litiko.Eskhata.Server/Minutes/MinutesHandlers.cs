using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Minutes;

namespace litiko.Eskhata
{
  partial class MinutesServerHandlers
  {

    public override void Saving(Sungero.Domain.SavingEventArgs e)
    {
      // 31.01.2025 убрано выдачу прав участникам совещания
      // base.Saving(e);
      
      #region копия из OfficialDocument
      // Заполнить регистрационный номер.
      if (_obj.RegistrationDate != null && _obj.DocumentRegister != null)
      {
        // Получить код подразделения.
        var departmentCode = string.Empty;
        var departmentId = 0L;
        if (_obj.Department != null)
        {
          departmentId = _obj.Department.Id;
          departmentCode = _obj.Department.Code;
        }
        
        // Получить ID и код НОР.
        var businessUnitCode = string.Empty;
        var businessUnitId = 0L;
        if (_obj.BusinessUnit != null)
        {
          businessUnitId = _obj.BusinessUnit.Id;
          businessUnitCode = _obj.BusinessUnit.Code;
        }
        
        var leadDocumentId = _obj.LeadingDocument != null ? _obj.LeadingDocument.Id : 0;
        
        if (string.IsNullOrEmpty(_obj.RegistrationNumber))
        {
          var registrationIndex = 0;
          var caseFileIndex = _obj.CaseFile != null ? _obj.CaseFile.Index : string.Empty;
          var docKindCode = _obj.DocumentKind != null ? _obj.DocumentKind.Code : string.Empty;
          var counterpartyCode = Sungero.Docflow.PublicFunctions.OfficialDocument.GetCounterpartyCode(_obj);
          do
          {
            // Для доп.соглашений и актов номер устанавливать в разрезе ведущего документа.
            registrationIndex = Sungero.Docflow.PublicFunctions.DocumentRegister.GetNextRegistrationNumber(_obj.DocumentRegister, _obj.RegistrationDate.Value, leadDocumentId, departmentId, businessUnitId);
            var registrationIndexWithLeadZero = registrationIndex.ToString();
            if (registrationIndexWithLeadZero.Length < _obj.DocumentRegister.NumberOfDigitsInNumber)
              registrationIndexWithLeadZero = string.Concat(Enumerable.Repeat("0", (_obj.DocumentRegister.NumberOfDigitsInNumber - registrationIndexWithLeadZero.Length) ?? 0)) +
                registrationIndexWithLeadZero;

            string registrationNumberPrefixValue;
            e.Params.TryGetValue(Sungero.Docflow.Constants.OfficialDocument.RegistrationNumberPrefix, out registrationNumberPrefixValue);
            string registrationNumberPostfixValue;
            e.Params.TryGetValue(Sungero.Docflow.Constants.OfficialDocument.RegistrationNumberPostfix, out registrationNumberPostfixValue);
            _obj.RegistrationNumber = registrationNumberPrefixValue + registrationIndexWithLeadZero +
              registrationNumberPostfixValue;
          } while (!Sungero.Docflow.PublicFunctions.DocumentRegister.Remote.IsRegistrationNumberUnique(_obj.DocumentRegister, _obj, _obj.RegistrationNumber, registrationIndex,
                                                                          _obj.RegistrationDate.Value, departmentCode, businessUnitCode,
                                                                          caseFileIndex, docKindCode, counterpartyCode, leadDocumentId));

          _obj.Index = registrationIndex;
        }
        else if (!string.IsNullOrEmpty(_obj.RegistrationNumber) && _obj.Index.HasValue && _obj.Index.Value > 0 &&
                 _obj.RegistrationNumber != _obj.State.Properties.RegistrationNumber.OriginalValue)
        {
          var currentCode = Functions.DocumentRegister.GetCurrentNumber(DocumentRegisters.As(_obj.DocumentRegister), _obj.RegistrationDate.Value, leadDocumentId, departmentId, businessUnitId);
          if (_obj.Index == (currentCode + 1))
          {
            Sungero.Docflow.PublicFunctions.DocumentRegister.Remote.SetCurrentNumber(_obj.DocumentRegister, _obj.Index.Value, leadDocumentId, departmentId, businessUnitId, _obj.RegistrationDate.Value);
          }
        }
        
      }
      
      if (e.Params.Contains(Sungero.Docflow.Constants.OfficialDocument.RepeatRegister))
        e.Params.Remove(Sungero.Docflow.Constants.OfficialDocument.RepeatRegister);      
      #endregion
    }
  }

}