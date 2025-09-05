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
      
      _obj.State.Properties.IsStandard.IsEnabled = false;                  
      _obj.State.Properties.RBOlitiko.IsVisible = _obj.DocumentKind?.Name == "Аренда";
      _obj.State.Properties.AmountForPeriodlitiko.IsVisible = _obj.IsEqualPaymentlitiko.GetValueOrDefault();
    }
    
    /// <summary>
    /// Установить обязательность свойств в зависимости от заполненных данных.
    /// </summary>
    public override void SetRequiredProperties()
    {
      base.SetRequiredProperties();
      
      _obj.State.Properties.TotalAmountlitiko.IsRequired = !_obj.IsFrameworkContract.GetValueOrDefault();
      _obj.State.Properties.AmountForPeriodlitiko.IsRequired = _obj.IsEqualPaymentlitiko.GetValueOrDefault();
      //_obj.State.Properties.PaymentRegionlitiko.IsRequired = _obj.IsIndividualPaymentlitiko.GetValueOrDefault();
    }    
  }
}