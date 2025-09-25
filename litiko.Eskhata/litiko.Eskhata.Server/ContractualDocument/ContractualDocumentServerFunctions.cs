using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ContractualDocument;
using Sungero.Domain.Shared;

namespace litiko.Eskhata.Server
{
  partial class ContractualDocumentFunctions
  {  
    
    /// <summary>
    /// Создать накладную.
    /// </summary>
    /// <returns>Созданный документ.</returns>
    [Remote]
    public static Sungero.FinancialArchive.IWaybill CreateWaybill()
    {            
      return Sungero.FinancialArchive.Waybills.Create();
    } 

    /// <summary>
    /// Проверка заполненности свойств контрагента
    /// </summary>
    [Remote(IsPure = true)]
    public List<string> CheckCounterpartyProperties(ICounterparty counterparty)
    {      
      var invalidProperties = new List<string>();      
      if (counterparty != null)
      {
        var objType = counterparty.GetType().GetFinalType();        
        var objMetadata = objType.GetEntityMetadata();        
        
        // Список отслеживаемых свойств
        var requsitesList = new List<string>();
        if (Companies.Is(counterparty))        
        {
          requsitesList.Add("TIN");
          requsitesList.Add("EINlitiko");
          requsitesList.Add("LegalAddress");
          requsitesList.Add("Phones");
          requsitesList.Add("Email");
          requsitesList.Add("Account");
          requsitesList.Add("Bank");
        }
        if (People.Is(counterparty))
        {
          requsitesList.Add("LastNameTGlitiko");
          requsitesList.Add("FirstNameTGlitiko");
          requsitesList.Add("MiddleNameTGlitiko");
          requsitesList.Add("LegalAddress");
          requsitesList.Add("TIN");
          requsitesList.Add("EINlitiko");                    
          requsitesList.Add("Account");
          requsitesList.Add("Bank");
        }        
                
        var properties = objMetadata.Properties.Where(p => requsitesList.Contains(p.Name));      
        foreach (var propertyMetadata in properties)
        {          
          if (propertyMetadata.PropertyType != Sungero.Metadata.PropertyType.Collection)
          {
            var propertyValue = propertyMetadata.GetValue(counterparty);
            if (propertyValue == null)
              invalidProperties.Add(propertyMetadata.GetLocalizedName());
          }
        }
      }
      
      return invalidProperties;
    }
    
    /// <summary>
    /// Отображение информации о контрагенте
    /// </summary>       
    [Remote]
    public StateView GetCounterpartryInfo()
    {
      var stateView = StateView.Create();
      var counterparty = Counterparties.As(_obj.Counterparty);
      if (counterparty != null)
      {
        var styleLabel = StateBlockLabelStyle.Create();
        styleLabel.Color = Colors.Common.DarkBlue;                     
        var styleError = StateBlockLabelStyle.Create();
        styleError.Color = Colors.Common.Red;               
        
        var block = stateView.AddBlock();
        block.ShowBorder = false;
        block.DockType = DockType.Bottom;
        string VATPayer = counterparty.VATPayerlitiko.HasValue ? (counterparty.VATPayerlitiko.Value ? "Да " : "Нет") : "???";
        string NUNRezident = counterparty.NUNonrezidentlitiko.HasValue ? (counterparty.NUNonrezidentlitiko.Value ? "Да " : "Нет") : "???";
        string reliability = counterparty.Reliabilitylitiko.HasValue ? counterparty.Info.Properties.Reliabilitylitiko.GetLocalizedValue(counterparty.Reliabilitylitiko) : string.Empty;
        string reviewDate = counterparty.DateOfInspectionlitiko.HasValue ? counterparty.DateOfInspectionlitiko.Value.ToString("dd.MM.yyyy") : string.Empty;        
        
        //block.AddLabel(ContractualDocuments.Resources.CounterpartryInfoFormat(VATPayer, NUNRezident, reliability, reviewDate, "\t\t\t\t"));        

        block.AddLabel("Плательщик НДС:                       ", styleLabel);
        block.AddLabel(VATPayer);
        block.AddLabel("\t\t\t\t ");
        block.AddLabel("Благонадежность:", styleLabel);
        block.AddLabel(reliability);
        block.AddLineBreak();
        
        block.AddLabel("Налоговый нерезидент:             ", styleLabel);
        block.AddLabel(NUNRezident);
        block.AddLabel("\t\t\t\t");
        block.AddLabel("Дата проверки:     ", styleLabel);
        block.AddLabel(reviewDate);
        
        if (Contracts.Is(_obj))
        {
          var contract = Contracts.As(_obj);                                                  
          block.ShowBorder = false;
          block.AddLineBreak();
          block.AddLabel("Заключение ДКР обязательное:", styleLabel);
          
          string conclusionDKRValue = string.Empty;
          var matrix = NSI.PublicFunctions.Module.GetResponsibilityMatrix(contract);
          if (matrix != null && matrix.ConclusionDKR.HasValue)
            conclusionDKRValue = matrix.ConclusionDKR.Value ? "Да" : "Нет";          
          block.AddLabel(conclusionDKRValue);          
          block.AddLineBreak();
          
          var doc = Sungero.Docflow.CounterpartyDocuments.GetAll()
            .Where(x => Equals(x.Counterparty, counterparty))
            .Where(x => x.DocumentKind.Name == "Заключение ДКР")
            .OrderByDescending(x => x.DocumentDate)
            .FirstOrDefault();
          if (doc != null)            
            block.AddHyperlink(doc.Name, Hyperlinks.Get(doc));
          else
          {
            if (conclusionDKRValue == "Да")
              block.AddLabel("Не найден документ Заключение ДКР по контагенту!", styleError);
          }            
        }        
      }      
      
      return stateView;
    }    

    /// <summary>
    /// Отображение дополнительной информации
    /// </summary>      
    [Remote]
    public StateView GetDopInfo()
    {
      var YES_VALUE = "Да ";
      var NO_VALUE = "Нет";
      var NULL_VALUE = "???";
      
      var stateView = StateView.Create();
      var styleLabel = StateBlockLabelStyle.Create();
      styleLabel.Color = Colors.Common.DarkBlue;                                   
        
      var block = stateView.AddBlock();
      block.ShowBorder = false;
      block.DockType = DockType.Bottom;
      
      if (Contracts.Is(_obj))
      {
        var contract = Contracts.As(_obj);        
        var matrix = NSI.PublicFunctions.Module.GetResponsibilityMatrix(contract);
                
        var responsibleEmployee =
            Employees.As(matrix?.ResponsibleAccountant)            
            ?? Roles.As(matrix?.ResponsibleAccountant)?
                   .RecipientLinks
                   .Select(l => Employees.As(l.Member))
                   .FirstOrDefault(e => e != null);
        
        var responsibleAccountantFIO = responsibleEmployee?.Name ?? string.Empty;
        var responsibleDepartment = responsibleEmployee?.Department?.Name ?? string.Empty;

        block.AddLabel("Ответственный бухгалтер: ", styleLabel);
        block.AddLabel(responsibleAccountantFIO);      
        block.AddLineBreak();                
        block.AddLabel("Ответственное подразделение: ", styleLabel);
        block.AddLabel(responsibleDepartment);      
        block.AddLineBreak();
        
        string accountEskhata = string.Empty;
        string accountAnother = string.Empty;
        var counterparty = litiko.Eskhata.Counterparties.As(contract.Counterparty);
        if (counterparty != null)
        {
          accountEskhata = counterparty.AccountEskhatalitiko;
          accountAnother = counterparty.Account;
        }
        block.AddLabel("Счет клиента в своем банке: ", styleLabel);
        block.AddLabel(accountEskhata);
        block.AddLineBreak();                
        block.AddLabel("Счет клиентов в других банках: ", styleLabel);
        block.AddLabel(accountAnother);      
        block.AddLineBreak();
                
        var batchProcessing = matrix?.BatchProcessing == true ? YES_VALUE
                    : matrix?.BatchProcessing == false ? NO_VALUE
                    : NULL_VALUE;        
        block.AddLabel("Групповая обработка: ", styleLabel);
        block.AddLabel(batchProcessing);      
        block.AddLineBreak();
                
        var matrix2 = NSI.PublicFunctions.Module.GetContractsVsPaymentDoc(contract, _obj.Counterparty);
        
        var block2 = stateView.AddBlock();
        block2.DockType = DockType.Bottom;
        block2.AddLabel("ОПЛАТА НА ОСНОВАНИИ");
        block2.AddLineBreak();
        block2.AddLabel("Договор", styleLabel);
        block2.AddLabel(matrix2?.PBIsPaymentContract == true ? YES_VALUE : matrix2?.PBIsPaymentContract == false ? NO_VALUE : NULL_VALUE);
        block2.AddLineBreak();
        
        block2.AddLabel("Счет", styleLabel);
        block2.AddLabel(matrix2?.PBIsPaymentInvoice == true ? YES_VALUE : matrix2?.PBIsPaymentInvoice == false ? NO_VALUE : NULL_VALUE);
        block2.AddLineBreak();
        
        block2.AddLabel("Счет-фактура", styleLabel);
        block2.AddLabel(matrix2?.PBIsPaymentTaxInvoice == true ? YES_VALUE : matrix2?.PBIsPaymentTaxInvoice == false ? NO_VALUE : NULL_VALUE);
        block2.AddLineBreak();
        
        block2.AddLabel("Акт выполненных работ", styleLabel);
        block2.AddLabel(matrix2?.PBIsPaymentAct == true ? YES_VALUE : matrix2?.PBIsPaymentAct == false ? NO_VALUE : NULL_VALUE);
        block2.AddLineBreak();
        
        block2.AddLabel("Заказ ", styleLabel);
        block2.AddLabel(matrix2?.PBIsPaymentOrder == true ? YES_VALUE : matrix2?.PBIsPaymentOrder == false ? NO_VALUE : NULL_VALUE);
        
        var block3 = stateView.AddBlock();
        block3.DockType = DockType.Bottom;
        block3.AddLabel("ЗАКРЫТИЕ ПЛАТЕЖА НА ОСНОВАНИИ");
        block3.AddLineBreak();
        block3.AddLabel("Договор", styleLabel);
        block3.AddLabel(matrix2?.PCBIsPaymentContract == true ? YES_VALUE : matrix2?.PCBIsPaymentContract == false ? NO_VALUE : NULL_VALUE);
        block3.AddLineBreak();
        
        block3.AddLabel("Счет", styleLabel);
        block3.AddLabel(matrix2?.PCBIsPaymentInvoice == true ? YES_VALUE : matrix2?.PCBIsPaymentInvoice == false ? NO_VALUE : NULL_VALUE);
        block3.AddLineBreak();
        
        block3.AddLabel("Счет-фактура", styleLabel);
        block3.AddLabel(matrix2?.PCBIsPaymentTaxInvoice == true ? YES_VALUE : matrix2?.PCBIsPaymentTaxInvoice == false ? NO_VALUE : NULL_VALUE);
        block3.AddLineBreak();
        
        block3.AddLabel("Акт выполненных работ", styleLabel);
        block3.AddLabel(matrix2?.PCBIsPaymentAct == true ? YES_VALUE : matrix2?.PCBIsPaymentAct == false ? NO_VALUE : NULL_VALUE);
        block3.AddLineBreak();
        
        block3.AddLabel("Расходная накладная", styleLabel);
        block3.AddLabel(matrix2?.PCBIsPaymentWaybill == true ? YES_VALUE : matrix2?.PCBIsPaymentWaybill == false ? NO_VALUE : NULL_VALUE);
        block3.AddLineBreak();        
        
        block3.AddLabel("Страховой полис", styleLabel);
        block3.AddLabel(matrix2?.PCBIsPaymentInsurance == true ? YES_VALUE : matrix2?.PCBIsPaymentInsurance == false ? NO_VALUE : NULL_VALUE);
      }      
      
      return stateView;
    }
       
  }
}