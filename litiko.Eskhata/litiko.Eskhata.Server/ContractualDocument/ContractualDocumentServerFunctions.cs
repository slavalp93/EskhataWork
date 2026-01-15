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
      if (counterparty == null)
        return invalidProperties;

      var company = Companies.As(counterparty);
      var person = People.As(counterparty);
      var bank = Banks.As(counterparty);      

      // Кеш метаданных свойств по имени
      var propertiesByName = new Dictionary<string, Sungero.Metadata.PropertyMetadata>();

      var requsitesList = new List<string>();
      if (company != null)
      {
        propertiesByName = company.GetEntityMetadata().Properties.ToDictionary(p => p.Name, p => p);
        
        requsitesList.Add("Name");
        requsitesList.Add("LegalName");
        requsitesList.Add("TIN");
        requsitesList.Add("OKFSlitiko");
        requsitesList.Add("OKONHlitiko");
        requsitesList.Add("OKVEDlitiko");
        requsitesList.Add("EINlitiko");
        requsitesList.Add("OKOPFlitiko");
        requsitesList.Add("City");
        requsitesList.Add("AddressTypelitiko");
        requsitesList.Add("LegalAddress");
      }
      else if (person != null)
      {
        propertiesByName = person.GetEntityMetadata().Properties.ToDictionary(p => p.Name, p => p);
        
        requsitesList.Add("LastName");
        requsitesList.Add("FirstName");        
        requsitesList.Add("TIN");                                
        requsitesList.Add("Sex");
        requsitesList.Add("DateOfBirth");
        requsitesList.Add("City");
        requsitesList.Add("AddressTypelitiko");
        requsitesList.Add("LegalAddress");
        
        requsitesList.Add("IdentityKind");
        requsitesList.Add("IdentitySeries");
        requsitesList.Add("IdentityNumber");
        requsitesList.Add("IdentityDateOfIssue");
        requsitesList.Add("IdentityExpirationDate");
        requsitesList.Add("IdentityAuthority");
      }
      else if (bank != null)
      {
        propertiesByName = bank.GetEntityMetadata().Properties.ToDictionary(p => p.Name, p => p);
        
        requsitesList.Add("Name");
        requsitesList.Add("LegalName");
        requsitesList.Add("BIC");
        requsitesList.Add("City");
        requsitesList.Add("AddressTypelitiko");
        requsitesList.Add("LegalAddress");
      }
      
      // Оставляем в списке только реально существующие свойства
      requsitesList = requsitesList
        .Where(name => propertiesByName.ContainsKey(name))
        .ToList();

      // Базовая проверка обязательных реквизитов
      foreach (var propertyName in requsitesList)
      {
        if (!IsFilled(counterparty, propertiesByName, propertyName))
          invalidProperties.Add(GetLocalizedName(propertiesByName, propertyName));
      }

      // (Account + Bank) ИЛИ AccountEskhatalitiko для Companies/People
      if (Companies.Is(counterparty) || People.Is(counterparty))
      {
        var hasAccountEsk = IsFilled(counterparty, propertiesByName, "AccountEskhatalitiko");
        var hasAccount    = IsFilled(counterparty, propertiesByName, "Account");
        var hasBank       = IsFilled(counterparty, propertiesByName, "Bank");

        if (!hasAccountEsk)
        {
          if (!hasAccount)
            invalidProperties.Add(GetLocalizedName(propertiesByName, "Account"));

          if (!hasBank)
            invalidProperties.Add(GetLocalizedName(propertiesByName, "Bank"));
        }
      }

      // CorrespondentAccount ИЛИ AccountEskhatalitiko для Banks
      if (Banks.Is(counterparty))
      {
        var hasCorrAccount = IsFilled(counterparty, propertiesByName, "CorrespondentAccountlitiko");
        var hasAccountEsk  = IsFilled(counterparty, propertiesByName, "AccountEskhatalitiko");

        if (!hasCorrAccount && !hasAccountEsk)
        {
          invalidProperties.Add(GetLocalizedName(propertiesByName, "CorrespondentAccountlitiko"));
          invalidProperties.Add(GetLocalizedName(propertiesByName, "AccountEskhatalitiko"));
        }
      }
      
      // СИН не обязателен только для 'Аренда'. Во всех остальных случаях — обязателен.
      if (person != null)
      {
        var docKind = Sungero.Docflow.DocumentKinds.Null;
        if (SupAgreements.Is(_obj))
          docKind = SupAgreements.As(_obj).LeadingDocument?.DocumentKind;
        else
          docKind = _obj.DocumentKind;
        
        var kindName = (docKind?.Name ?? "").Trim();
        
        if (!kindName.Equals("Аренда", StringComparison.InvariantCultureIgnoreCase))
        {
          if (!IsFilled(person, propertiesByName, "SINlitiko"))
            invalidProperties.Add(GetLocalizedName(propertiesByName, "SINlitiko"));
        }

      }
      
      return invalidProperties;
    }

    private bool IsFilled(ICounterparty counterparty, IDictionary<string, Sungero.Metadata.PropertyMetadata> propertiesByName, string propertyName)
    {
      Sungero.Metadata.PropertyMetadata prop;
      if (!propertiesByName.TryGetValue(propertyName, out prop))
        return false;

      if (prop.PropertyType == Sungero.Metadata.PropertyType.Collection)
        return false;

      var propValue = prop.GetValue(counterparty);
      if (propValue == null)
        return false;

      if (propValue is string s)
        return !string.IsNullOrWhiteSpace(s);

      return true;
    }

    
    private string GetLocalizedName(IDictionary<string, Sungero.Metadata.PropertyMetadata> propertiesByName, string propertyName)
    {
      Sungero.Metadata.PropertyMetadata prop;
      
      if (propertiesByName.TryGetValue(propertyName, out prop))
        return prop.GetLocalizedName();
      
      return propertyName;
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

        if (Eskhata.Companies.Is(counterparty) || Eskhata.Banks.Is(counterparty))
        {
          block.AddLabel("Плательщик НДС:                       ", styleLabel);
          block.AddLabel(VATPayer);        
        }
        else
        {
          block.AddLabel("\t\t\t\t\t\t     ", styleLabel);
          block.AddLabel("      ");        
        }        
        block.AddLabel("\t\t\t\t");
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

        block.AddLabel("Ответственный бухгалтер: ", styleLabel);
        block.AddLabel(responsibleAccountantFIO);      
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