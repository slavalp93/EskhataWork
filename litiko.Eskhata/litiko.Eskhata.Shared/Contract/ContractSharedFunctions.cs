using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Contract;

namespace litiko.Eskhata.Shared
{
  partial class ContractFunctions
  {

    /// <summary>
    /// Определить и установить значение поля "Типовой"
    /// </summary>       
    public void SetIsStandard()
    {
      // Нет, если для выбранного вида не настроен шаблон
      bool isStandard = false;
      var documentKind = _obj.DocumentKind;
      if (documentKind != null)
      {
        var aviabledTemplates = Sungero.Docflow.PublicFunctions.DocumentTemplate.GetDocumentTemplatesByDocumentKind(documentKind);
        if (aviabledTemplates != null && aviabledTemplates.Any())
          isStandard = true;
      }                  
      
      _obj.IsStandard = isStandard;      
    }
    /// <summary>
    /// Обновить карточку документа.
    /// </summary>
    public override void RefreshDocumentForm()
    {
      base.RefreshDocumentForm();
            
      _obj.State.Properties.RBOlitiko.IsVisible = _obj.DocumentGroup?.Name == "Аренда здания (МХБ, филиал, ГО)" && Eskhata.People.Is(_obj.Counterparty);
      _obj.State.Properties.AmountForPeriodlitiko.IsVisible = _obj.IsEqualPaymentlitiko.GetValueOrDefault() || _obj.IsPartialPaymentlitiko.GetValueOrDefault();
      _obj.State.Properties.AmountForPeriodInWordslitiko.IsVisible = _obj.State.Properties.AmountForPeriodlitiko.IsVisible;
      _obj.State.Properties.RegionOfRentallitiko.IsVisible = _obj.DocumentKind?.Name == "Прочие оплаты профессиональных услуг" && Eskhata.People.Is(_obj.Counterparty);
      _obj.State.Properties.PaymentRegionlitiko.IsVisible = _obj.DocumentKind?.Name == "Аренда" && Eskhata.People.Is(_obj.Counterparty);
    }
    
    /// <summary>
    /// Установить обязательность свойств в зависимости от заполненных данных.
    /// </summary>
    public override void SetRequiredProperties()
    {
      base.SetRequiredProperties();
      
      _obj.State.Properties.TotalAmountlitiko.IsRequired = !_obj.IsFrameworkContract.GetValueOrDefault();
      _obj.State.Properties.AmountForPeriodlitiko.IsRequired = _obj.State.Properties.AmountForPeriodlitiko.IsVisible && !_obj.IsFrameworkContract.GetValueOrDefault();
      _obj.State.Properties.RBOlitiko.IsRequired = _obj.State.Properties.RBOlitiko.IsVisible;
      _obj.State.Properties.PaymentMethodlitiko.IsRequired = true;
      _obj.State.Properties.FrequencyOfPaymentlitiko.IsRequired = true;
    }

    /// <summary>
    /// Заполнить имя.
    /// </summary>
    public override void FillName()
    {
      // Не автоформируемое имя.
      if (_obj != null && _obj.DocumentKind != null && !_obj.DocumentKind.GenerateDocumentName.Value)
      {
        if (_obj.Name == OfficialDocuments.Resources.DocumentNameAutotext)
          _obj.Name = string.Empty;
        
        if (_obj.VerificationState != null && string.IsNullOrWhiteSpace(_obj.Name))
          _obj.Name = _obj.DocumentKind.ShortName; 
      }         
      
      if (_obj.DocumentKind == null || !_obj.DocumentKind.GenerateDocumentName.Value)
        return;
      
      // Автоформируемое имя.
      var name = string.Empty;
      
      /* Имя в формате:
        <Вид документа> №<номер> от <дата> с <контрагент>.
       */
      using (TenantInfo.Culture.SwitchTo())
      {
        if (!string.IsNullOrWhiteSpace(_obj.RegistrationNumber))
          name += OfficialDocuments.Resources.Number + _obj.RegistrationNumber;
        
        if (_obj.RegistrationDate != null)
          name += OfficialDocuments.Resources.DateFrom + _obj.RegistrationDate.Value.ToString("d");
        
        if (_obj.Counterparty != null)
          name += Sungero.Contracts.ContractBases.Resources.NamePartForContractor + _obj.Counterparty.DisplayValue;
        
      }
      
      if (string.IsNullOrWhiteSpace(name))
      {
        name = _obj.VerificationState == null ? OfficialDocuments.Resources.DocumentNameAutotext : _obj.DocumentKind.ShortName;
      }
      else if (_obj.DocumentKind != null)
      {
        name = _obj.DocumentKind.ShortName + name;
      }
      
      _obj.Name = Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols(name);            
    }
    
    /// <summary>
    /// Заполнить сумму в нац. валюте.
    /// </summary>
    /// <param name="amount">Общая сумма.</param>
    /// <param name="currencyRate">Курс валюты.</param>
    /// <param name="currency">Валюта.</param>
    public override void FillTotalAmount(double? amount, litiko.NSI.ICurrencyRate currencyRate, Sungero.Commons.ICurrency currency)
    {                  
      if (_obj.AmountForPeriodlitiko > 0 && _obj.TotalAmountlitiko == 0)
        amount = _obj.AmountForPeriodlitiko;
      else
        amount = _obj.TotalAmountlitiko;
      
      base.FillTotalAmount(amount, _obj.CurrencyRatelitiko, _obj.CurrencyContractlitiko);
    }       
  }
}