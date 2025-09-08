using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ContractCondition;

namespace litiko.Eskhata.Shared
{
  partial class ContractConditionFunctions
  {
    /// <summary>
    /// Сменить доступность реквизитов.
    /// </summary>
    public override void ChangePropertiesAccess()
    {
      base.ChangePropertiesAccess();
      
      var isPaymentBasedOn = _obj.ConditionType == ConditionType.PaymentBasedOn;
      var isDocumentGroup = _obj.ConditionType == ConditionType.DocumentGroup;
      
      _obj.State.Properties.PaymentBasedOnlitiko.IsVisible = isPaymentBasedOn;
      _obj.State.Properties.DocumentGroupslitiko.IsVisible = isDocumentGroup;
    }
    
    /// <summary>
    /// Очистка скрытых свойств.
    /// </summary>
    public override void ClearHiddenProperties()
    {
      base.ClearHiddenProperties();
      
      if (!_obj.State.Properties.PaymentBasedOnlitiko.IsVisible)
        _obj.PaymentBasedOnlitiko = null;
      
      if (!_obj.State.Properties.DocumentGroupslitiko.IsVisible)
        _obj.DocumentGroupslitiko.Clear();
                
    }
    
    /// <summary>
    /// Получить словарь поддерживаемых типов условий.
    /// </summary>
    public override System.Collections.Generic.Dictionary<string, List<Enumeration?>> GetSupportedConditions()
    {
      var baseSupport = base.GetSupportedConditions();
          
      baseSupport[Sungero.Contracts.PublicConstants.Module.ContractGuid].Add(ConditionType.PaymentBasedOn);
      baseSupport[Sungero.Contracts.PublicConstants.Module.ContractGuid].Add(ConditionType.DocumentGroup);
         
      return baseSupport;
    }
    
    /// <summary>
    /// Проверить условие.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="task">Задача на согласование.</param>
    /// <returns>Результат проверки условия. Структуру формата - выполнение условия, сообщение об ошибке.</returns>
    public override Sungero.Docflow.Structures.ConditionBase.ConditionResult CheckCondition(Sungero.Docflow.IOfficialDocument document, Sungero.Docflow.IApprovalTask task)
    {
      #region Оплата на основании
      if (_obj.ConditionType == ConditionType.PaymentBasedOn)
      {
        var contract = litiko.Eskhata.Contracts.As(document);
        if (contract != null)
        {
          var matrix = litiko.NSI.PublicFunctions.Module.GetContractsVsPaymentDoc(contract, contract.Counterparty);
          if (matrix != null)
          {
            bool conditionResult = false;                                   
            
            if (_obj.PaymentBasedOnlitiko == litiko.Eskhata.ContractCondition.PaymentBasedOnlitiko.Contract)
              conditionResult = matrix.PBIsPaymentContract.GetValueOrDefault();            
            
            else if (_obj.PaymentBasedOnlitiko == litiko.Eskhata.ContractCondition.PaymentBasedOnlitiko.Invoice)
              conditionResult = matrix.PBIsPaymentInvoice.GetValueOrDefault();
            
            else if (_obj.PaymentBasedOnlitiko == litiko.Eskhata.ContractCondition.PaymentBasedOnlitiko.TaxInvoice)
              conditionResult = matrix.PBIsPaymentTaxInvoice.GetValueOrDefault();
            
            else if (_obj.PaymentBasedOnlitiko == litiko.Eskhata.ContractCondition.PaymentBasedOnlitiko.Act)
              conditionResult = matrix.PBIsPaymentAct.GetValueOrDefault();            
            
            else if (_obj.PaymentBasedOnlitiko == litiko.Eskhata.ContractCondition.PaymentBasedOnlitiko.Order)
              conditionResult = matrix.PBIsPaymentOrder.GetValueOrDefault();
            
            Sungero.Docflow.Structures.ConditionBase.ConditionResult.Create(conditionResult, string.Empty);
          }
          else
            Sungero.Docflow.Structures.ConditionBase.ConditionResult.Create(null, litiko.Eskhata.ContractConditions.Resources.ContractsVsPaymentDocIsNotFound);
        }
        
        return Sungero.Docflow.Structures.ConditionBase.ConditionResult.Create(null, Sungero.Docflow.ConditionBases.Resources.CannotPerformConditionCheck);
      }      
      #endregion

      #region Тип договора
      if (_obj.ConditionType == ConditionType.DocumentGroup)
      {
        var contract = litiko.Eskhata.Contracts.As(document);
        if (contract != null)
          return Sungero.Docflow.Structures.ConditionBase.ConditionResult.Create(_obj.DocumentGroupslitiko.Any(x => Equals(x.DocumentGroup, contract.DocumentGroup)), string.Empty);
        
        return Sungero.Docflow.Structures.ConditionBase.ConditionResult.Create(null, Sungero.Docflow.ConditionBases.Resources.CannotPerformConditionCheck);
      }
      #endregion
      
      return base.CheckCondition(document, task);
    }    
  }
}