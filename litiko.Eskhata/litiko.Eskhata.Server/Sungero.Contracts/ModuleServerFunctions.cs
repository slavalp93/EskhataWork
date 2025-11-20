using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using System.Xml.XPath;
using litiko.Eskhata.Module.Contracts.Structures.Module;
using Sungero.Core;
//using litiko.Eskhata.Structures.Contracts.Contract;
using Sungero.CoreEntities;
using Sungero.Domain.Shared;
using Sungero.Workflow.TaskSchemeValidators;

namespace litiko.Eskhata.Module.Contracts.Server
{
  partial class ModuleFunctions
  {
    [Remote, Public]
    public IResultImportXmlUI ImportContractsFromXmlUI()
    {
      //var result = Eskhata.Structures.Contracts.Contract.ResultImportXml.Create();
      var result = litiko.Eskhata.Module.Contracts.Structures.Module.ResultImportXmlUI.Create();
      result.Errors = new List<string>();
      result.ImportedCount = 0;
      result.TotalCount = 0;

      Logger.Debug("Import contracts from XML - Start");

      var xmlPathFile = "Contracts.xml";

      if (!System.IO.File.Exists(xmlPathFile))
      {
        result.Errors.Add($"Файл '{xmlPathFile}' не найден");
        Logger.Error($"XML File {xmlPathFile} is not found");
        return result;
      }

      try
      {
        XDocument xDoc = XDocument.Load(xmlPathFile);

        var dataElements = xDoc.Element("Data");

        if (dataElements == null)
        {
          result.Errors.Add("Корневой элемент <Data> отсутствует в XML.");
          Logger.Error("No <Data> root element found in XML.");
          return result;
        }

        var documentElements = dataElements.Elements("Document").ToList();
        Logger.Debug($"Found {documentElements.Count} documents in XML.");

        var allXmlExternalIds = documentElements
          .Select(x => x.Element("ExternalD")?.Value?.Trim())
          .Where(x => !string.IsNullOrEmpty(x))
          .Distinct()
          .ToList();
        
        var allCounterpartyIds = documentElements
          .Select(x => x.Element("CounterpartyExternalId")?.Value?.Trim())
          .Where(x => !string.IsNullOrEmpty(x))
          .Distinct()
          .ToList();

        var existingContracts = Eskhata.Contracts.GetAll()
          .Where(x => allXmlExternalIds.Contains(x.ExternalId))
          .Select(x => x.ExternalId)
          .ToList();

        var currencies = litiko.Eskhata.Currencies.GetAll().ToList();
        var docKinds = litiko.Eskhata.DocumentKinds.GetAll().ToList();
        var docGroups = litiko.Eskhata.DocumentGroupBases.GetAll().ToList();
        var departments = litiko.Eskhata.Departments.GetAll().ToList();
        var employees = litiko.Eskhata.Employees.GetAll().ToList();
        var paymentRegions = litiko.NSI.PaymentRegions.GetAll().ToList();
        var rentRegions = litiko.NSI.RegionOfRentals.GetAll().ToList();
        var frequencies = litiko.NSI.FrequencyOfPayments.GetAll().ToList();

        var counterparties = Eskhata.Counterparties.GetAll()
          .Where(c => allCounterpartyIds.Contains(c.ExternalId))
          .ToList();

        var contracts = Eskhata.Contracts.GetAll().ToList();
        
        for (var i = 0; i < documentElements.Count; i++)
        {
          result.TotalCount++;
          
          try
          {
            var documentElement = documentElements[i];

            if (documentElements[i] == null)
            {
              result.Errors.Add($"Элемент <Document> №{i + 1} пустой, пропущен");
              continue;
            }
            
            var contract = ParseContract(documentElement, result);
            if (contract == null)
            {
              var name = documentElement.Element("Name")?.Value;
              var externalId = documentElement.Element("ExternalD")?.Value;
              result.Errors.Add($"Документ '{name}' с Внешним идентификатором {externalId} был пропущен (дубликат или не соответствует условиям).");
              continue;
            }
            
            result.ImportedCount++;
            Logger.DebugFormat(
                "Created new Contract with: " +
                $"Id={contract.Id}, " +
                $"Name={contract.Name}, " +
                $"ExternalId={contract.ExternalId}, " +
                $"Subject={contract.Subject}, " +
                $"DocumentKind={contract.DocumentKind} " +
                $"DocumentGroup={contract.DocumentGroup}, " +
                $"Counterparty={contract.Counterparty} " +
                $"CounterpartySignatory={contract.CounterpartySignatory}, " +
                $"Department={contract.Department}, " +
                $"ResponsibleEmployee={contract.ResponsibleEmployee}, " +
                $"Author={contract.Author} " +
                $"RBO={contract.RBOlitiko}, " +
                $"ValidFrom={contract.ValidFrom}, " +
                $"ValidTill={contract.ValidTill}, " +
                $"ReasonForChange={contract.ReasonForChangelitiko}, " +
                $"AccDebtCredit={contract.AccDebtCreditlitiko}, " +
                $"AccFutureExpense={contract.AccFutureExpenselitiko}, " +
                $"TotalAmount={contract.TotalAmountlitiko}, " +
                $"Currency={contract.Currency}, " +
                $"CurrencyOperation={contract.CurrencyOperationlitiko}, " +
                $"IsVAT={contract.IsVATlitiko}, " +
                $"VatRate={contract.VatRatelitiko}, " +
                $"VatAmount={contract.VatAmount}, " +
                $"IncomeTaxRate={contract.IncomeTaxRatelitiko}, " +
                $"PaymentRegion={contract.PaymentRegionlitiko}, " +
                $"RegionOfRental={contract.RegionOfRentallitiko}, " +
                $"PaymentMethod={contract.PaymentMethodlitiko}, " +
                $"PaymentFrequency={contract.FrequencyOfPaymentlitiko}, " +
                $"IsPartialPayment={contract.IsPartialPaymentlitiko}, " +
                $"IsEqualPayment={contract.IsEqualPaymentlitiko}, " +
                $"AmountForPeriod={contract.AmountForPeriodlitiko}, " +
                $"RegistrationNumber={contract.RegistrationNumber}, " +
                $"RegistrationDate={contract.RegistrationDate}, " +
                $"Note={contract.Note}"
);


            Logger.Debug($"Импортирован договор: {contract.Name}");
          }
          catch (Exception ex)
          {
            Logger.Error($"Error due parsing Counterparty or Document №{i + 1}: {ex.Message}");
            result.Errors.Add($"Ошибка при импорте документа №{i + 1}: {ex.Message}");
          }
        }
      }
      catch (Exception ex)
      {
        Logger.Error($"Error due parsing Counterparty or Document: {ex.Message}");
        result.Errors.Add($"Общая ошибка импорта: {ex.Message}");
      }
      return result;
    }
    
    private IContract ParseContract(XElement documentElement, IResultImportXmlUI result)
    {
      if (documentElement == null)
      {
        Logger.Debug("Skipping <Data> without <Document>.");
        return null;
      }

      var isExternalD = documentElement.Element("ExternalD")?.Value?.Trim();
      var isDocumentKind = documentElement.Element("DocumentKind")?.Value?.Trim();
      var isDocumentGroup = documentElement.Element("DocumentGroup")?.Value?.Trim();
      var isSubject = documentElement.Element("Subject")?.Value?.Trim();
      var isName = documentElement.Element("Name")?.Value;
      var isCounterpartySignatory = documentElement.Element("CounterpartySignatory")?.Value?.Trim();
      var isDepartment = documentElement.Element("Department")?.Value?.Trim();
      var isResponsibleEmployee = documentElement.Element("ResponsibleEmployee")?.Value?.Trim();
      var isAuthor = documentElement.Element("Author")?.Value?.Trim();
      var isRBO = documentElement.Element("RBO")?.Value ?? "";
      var isValidFrom = documentElement.Element("ValidFrom")?.Value?.Trim();
      var isValidTill = documentElement.Element("ValidTill")?.Value?.Trim();
      var isChangeReason = documentElement.Element("ChangeReason")?.Value.Trim();
      var isAccountDebtCredt = documentElement.Element("AccountDebtCredt")?.Value.Trim();
      var isAccountFutureExpense = documentElement.Element("AccountFutureExpense")?.Value.Trim();
      var isTotalAmount = documentElement.Element("TotalAmount")?.Value?.Trim();
      var isCurrency = documentElement.Element("Currency")?.Value?.Trim();
      var isCurrencyOperation = documentElement.Element("OperationCurrency")?.Value.Trim();
      var isVATApplicable = documentElement.Element("VATApplicable")?.Value.Trim();
      var isVATRate = documentElement.Element("VATRate")?.Value.Trim();
      var isVATAmount = documentElement.Element("VATAmount")?.Value.Trim();
      var isIncomeTaxRate = documentElement.Element("IncomeTaxRate")?.Value.Trim();
      var isPaymentRegion = documentElement.Element("PaymentRegion")?.Value?.Trim();
      var isPaymentTaxRegion = documentElement.Element("PaymentTaxRegion")?.Value?.Trim();
      var isPaymentMethod = documentElement.Element("PaymentMethod")?.Value?.Trim();
      var isNote = documentElement.Element("Note")?.Value?.Trim();
      var isPaymentFrequency = documentElement.Element("PaymentFrequency")?.Value?.Trim();
      var isPartialPayment = documentElement.Element("IsPartialPayment")?.Value?.Trim();
      var isEqualPayment = documentElement.Element("IsEqualPayment")?.Value?.Trim();
      var isAmountForPeriod = documentElement.Element("AmountForPeriod")?.Value?.Trim();
      var isRegistrationNumber = documentElement.Element("RegistrationNumber")?.Value?.Trim();
      var isRegistrationDate = documentElement.Element("RegistrationDate")?.Value?.Trim();
      var isCounterpartyExternalId = documentElement.Element("CounterpartyExternalId")?.Value?.Trim();
      
      if(string.IsNullOrWhiteSpace(isExternalD))
      {
        result.Errors.Add("У документа отсутствует (Внешний идентификатор)");
        Logger.Error("Missing ExternalID in XML File");
        throw new KeyNotFoundException("У документа отсутствует (ExternalID)");
      }
      // Найти контракт по externalId
      var contract = Eskhata.Contracts.GetAll().FirstOrDefault(x =>
                                                               (!string.IsNullOrEmpty(isExternalD) && x.ExternalId == isExternalD));
      
      if(contract != null)
      {
        Logger.DebugFormat($"Contract with ExternalId {isExternalD} found, continue.");
        return null;
      }
      
      
      if (string.IsNullOrWhiteSpace(isExternalD))
      {
        result.Errors.Add($"У документа с именем {isName} пропущен ExternalId");
        Logger.Error("Missing ExternalId in XML File");
        throw new KeyNotFoundException("У документа отсутствует внешний идентификатор");
      }
      
      contract = Eskhata.Contracts.Create();

      if(!string.IsNullOrEmpty(isExternalD))
      {
        contract.ExternalId = isExternalD;
        Logger.DebugFormat("Create contract with ID:{0}, ExternalD:{1}", contract.ExternalId, contract.Id);
      }
      else
        result.Errors.Add($"Произошла ошибка при добавлении внешнего идентификатора договора с внешним идентификатором {contract.ExternalId}");

      var counterpartyExternalId = documentElement.Element("CounterpartyExternalId")?.Value?.Trim();

      if (!string.IsNullOrWhiteSpace(counterpartyExternalId))
      {
        var cp = Eskhata.Counterparties.GetAll()
          .FirstOrDefault(c => c.ExternalId == counterpartyExternalId);

        if (cp != null)
        {
          contract.Counterparty = cp;
        }
        else
        {
          result.Errors.Add($"Контрагент с ExternalId={counterpartyExternalId} не найден при импорте договора {isExternalD}");
          Logger.DebugFormat("Counterparty with ExternalId {0} not found when importing contract {1}.", counterpartyExternalId, isExternalD);
        }
      }
      // --- DocumentKind (назначаем только если нашли)
      if (!string.IsNullOrWhiteSpace(isDocumentKind))
      {
        var documentKind = litiko.Eskhata.DocumentKinds.GetAll().FirstOrDefault(k => k.ExternalIdlitiko == isDocumentKind);
        if (documentKind != null)
          contract.DocumentKind = documentKind;
        else
        {
          Logger.DebugFormat("DocumentKind with ExternalId {0} not found, skipping assignment.", isDocumentKind);
          result.Errors.Add($"Вид договора с ExternalId:{isDocumentKind} не найден при импорте договора:{contract.Id}");
        }
      }

      // --- DocumentGroup
      if (!string.IsNullOrWhiteSpace(isDocumentGroup))
      {
        var documentGroup = litiko.Eskhata.DocumentGroupBases.GetAll().FirstOrDefault(g => g.ExternalIdlitiko == isDocumentGroup);
        if (documentGroup != null)
          contract.DocumentGroup = documentGroup;
        else
        {
          Logger.DebugFormat("DocumentGroup with ExternalId {0} not found, skipping assignment.", isDocumentGroup);
          result.Errors.Add($"Тип договора с ExternalId:{isDocumentGroup} не найден при импорте договора:{contract.Id}");
        }
      }

      // Простые поля
      if(!string.IsNullOrEmpty(isSubject))
        contract.Subject = isSubject;
      else
      {
        Logger.DebugFormat("Subject is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add("Пустое поле Содержание в договоре с Id: " + contract.Id);
      }
      
      if(!string.IsNullOrEmpty(isName))
        contract.Name = isName;
      else
      {
        Logger.DebugFormat("Name is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле Имя в договоре с Id: {contract.Id}");
      }
      
      if(!string.IsNullOrEmpty(isRBO))
        contract.RBOlitiko = isRBO;
      else
      {
        Logger.DebugFormat("RBO is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле РБО в договоре с Id: {contract.Id}");
      }
      
      if(!string.IsNullOrEmpty(isChangeReason))
        contract.ReasonForChangelitiko = isChangeReason;
      else
      {
        Logger.DebugFormat("ChangeReason is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле Основание изменений в договоре с Id: {contract.Id}");
      }
      
      if(!string.IsNullOrEmpty(isAccountDebtCredt))
        contract.AccDebtCreditlitiko = isAccountDebtCredt;
      else
      {
        Logger.DebugFormat("AccountDebtCredt is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле 'Счет расчетов с дебиторами и кредиторами' в договоре с Id: {contract.Id}");
      }
      
      if(!string.IsNullOrEmpty(isAccountFutureExpense))
        contract.AccFutureExpenselitiko = isAccountFutureExpense;
      else
      {
        Logger.DebugFormat("AccountFutureExpense is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле 'Счет будущих расходов' в договоре с Id: {contract.Id}");
      }
      
      if(!string.IsNullOrEmpty(isRegistrationDate))
        contract.RegistrationNumber = isRegistrationNumber;
      else
      {
        Logger.DebugFormat("RegistrationNumber is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле 'Регистрационный номер' в договоре с Id: {contract.Id}");
      }

      if (!string.IsNullOrEmpty(isPaymentFrequency))
      {
        var frequency = litiko.NSI.FrequencyOfPayments.GetAll().FirstOrDefault(f => f.Name.Equals(isPaymentFrequency));
        contract.FrequencyOfPaymentlitiko = frequency;
      }
      else
      {
        Logger.DebugFormat("PaymentFrequency is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле 'Периодичность оплаты' в договоре с Id: {contract.Id}");
      }
      
      //contract.FrequencyExpenseslitiko = contract.FrequencyOfPaymentlitiko; //TODO: проверить с бизнесом

      if (!string.IsNullOrEmpty(isCounterpartySignatory))
      {
        var counterpartySignatory = Eskhata.Contacts.GetAll().FirstOrDefault(x => x.ExternalIdlitiko == isCounterpartySignatory);
        contract.CounterpartySignatory = counterpartySignatory;
      }
      else
      {
        Logger.DebugFormat("CounterpartySignatory is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле 'Подписал' в договоре с Id: {contract.Id}");
      }

      // Department
      if (!string.IsNullOrWhiteSpace(isDepartment))
      {
        var dept = litiko.Eskhata.Departments.GetAll().FirstOrDefault(d => d.ExternalCodelitiko == isDepartment);
        if (dept != null)
          contract.Department = dept;
      }
      else
      {
        Logger.DebugFormat("Department is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле 'Подразделение' в договоре с Id: {contract.Id}");
      }

      // ResponsibleEmployee
      if (!string.IsNullOrWhiteSpace(isResponsibleEmployee))
      {
        var emp = litiko.Eskhata.Employees.GetAll().FirstOrDefault(e => e.ExternalId == isResponsibleEmployee);
        if (emp != null)
          contract.ResponsibleEmployee = emp;
      }

      else
      {
        Logger.DebugFormat("ResponsibleEmployee is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле 'Ответственный' в договоре с Id: {contract.Id}");
      }

      // Author
      if (!string.IsNullOrWhiteSpace(isAuthor))
      {
        var author = litiko.Eskhata.Employees.GetAll().FirstOrDefault(u => u.ExternalId == isAuthor);
        if (author != null)
          contract.Author = author;
      }
      else
      {
        Logger.DebugFormat("Author is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле 'Автор' в договоре с Id: {contract.Id}");
      }

      
      // Dates — безопасно
      if (!string.IsNullOrEmpty(isValidFrom))
      {
        var parsedFrom = TryParseDate(isValidFrom);
        if (parsedFrom.HasValue)
          contract.ValidFrom = parsedFrom.Value;
      }
      else
      {
        Logger.DebugFormat("ValidFrom is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле 'Действителен с' в договоре с Id: {contract.Id}");
      }

      if (!string.IsNullOrEmpty(isValidTill))
      {
        var parsedTill = TryParseDate(isValidTill);
        if (parsedTill.HasValue)
          contract.ValidTill = parsedTill.Value;
      }
      else
      {
        Logger.DebugFormat("ValidTill is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле 'Действителен по' в договоре с Id: {contract.Id}");
      }

      if (!string.IsNullOrEmpty(isTotalAmount))
        contract.TotalAmountlitiko = ParseDoubleSafe(isTotalAmount);
      else
      {
        Logger.DebugFormat("TotalAmount is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле 'Общая сумма' в договоре с Id: {contract.Id}");
      }
      
      if (!string.IsNullOrEmpty(isCurrencyOperation))
      {
        var currencyOp = litiko.Eskhata.Currencies.GetAll().FirstOrDefault(c => c.AlphaCode == isCurrencyOperation || c.NumericCode == isCurrencyOperation);
        if (currencyOp != null)
          contract.CurrencyOperationlitiko = currencyOp;
      }
      else
      {
        Logger.DebugFormat("CurrencyOperation is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле 'Валюта операции' в договоре с Id: {contract.Id}");
      }

      // Currency
      if (!string.IsNullOrEmpty(isCurrency))
      {
        var currency = litiko.Eskhata.Currencies.GetAll().FirstOrDefault(c => c.AlphaCode == isCurrency || c.NumericCode == isCurrency);
        if (currency != null)
          contract.Currency = currency;
      }
      else
      {
        Logger.DebugFormat("Currency is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле 'Валюта' в договоре с Id: {contract.Id}");
      }

      if(!string.IsNullOrEmpty(isVATRate))
        contract.VatRatelitiko = ParseDoubleSafe(isVATRate);
      else
      {
        Logger.DebugFormat("VATRate is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле 'Ставка НДС' в договоре с Id: {contract.Id}");
      }

      if(!string.IsNullOrEmpty(isVATAmount))
        contract.VatAmount = ParseDoubleSafe(isVATAmount);
      else
      {
        Logger.DebugFormat("VATAmount is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле 'Сумма НДС' в договоре с Id: {contract.Id}");
      }

      if(!string.IsNullOrEmpty(isIncomeTaxRate))
        contract.IncomeTaxRatelitiko = ParseDoubleSafe(isIncomeTaxRate);
      else
      {
        Logger.DebugFormat("IncomeTaxRate is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле 'Налог на доходы' в договоре с Id: {contract.Id}");
      }

      if(!string.IsNullOrEmpty(isVATApplicable))
        contract.IsVATlitiko = ParseBoolSafe(isVATApplicable);
      else
      {
        Logger.DebugFormat("VATApplicable is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле 'Облагается НДС' в договоре с Id: {contract.Id}");
      }

      // PaymentRegion
      if (!string.IsNullOrWhiteSpace(isPaymentRegion))
      {
        var paymentRegion = litiko.NSI.PaymentRegions.GetAll().FirstOrDefault(r => r.ExternalId == isPaymentRegion || r.Code == isPaymentRegion);
        if (paymentRegion != null)
          contract.PaymentRegionlitiko = paymentRegion;
      }
      else
      {
        Logger.DebugFormat("PaymentRegion is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле 'Регион оплаты' в договоре с Id: {contract.Id}");
      }

      if (!string.IsNullOrWhiteSpace(isPaymentTaxRegion))
      {
        var region = litiko.NSI.RegionOfRentals.GetAll().FirstOrDefault(r => r.ExternalId == isPaymentTaxRegion || r.Code == isPaymentTaxRegion);
        if (region != null)
          contract.RegionOfRentallitiko = region;
      }
      else
      {
        Logger.DebugFormat("PaymentTaxRegion is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле 'Регион оплаты' в договоре с Id: {contract.Id}");
      }

      if (!string.IsNullOrWhiteSpace(isPaymentMethod))
      {
        Sungero.Core.Enumeration? paymentEnum = null;
        switch (isPaymentMethod.Trim())
        {
          case "Предоплата":
            paymentEnum = litiko.Eskhata.Contract.PaymentMethodlitiko.Prepayment;
            break;
          case "Постоплата":
            paymentEnum = litiko.Eskhata.Contract.PaymentMethodlitiko.Postpay;
            break;
          default:
            Logger.DebugFormat("Unknown Payment Method: {0}", isPaymentMethod);
            break;
        }
        if (paymentEnum.HasValue)
          contract.PaymentMethodlitiko = paymentEnum;
      }
      else
      {
        Logger.DebugFormat("PaymentMethod is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле 'Способ оплаты' в договоре с Id: {contract.Id}");
      }

      if (!string.IsNullOrWhiteSpace(isPaymentFrequency))
      {
        var frequency = litiko.NSI.FrequencyOfPayments.GetAll().FirstOrDefault(f => f.Name.Equals(isPaymentFrequency));
        if (frequency != null)
          contract.FrequencyOfPaymentlitiko = frequency;
      }
      else
      {
        Logger.DebugFormat("PaymentFrequency is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле 'Периодичность оплаты' в договоре с Id: {contract.Id}");
      }

      if (!string.IsNullOrWhiteSpace(isPartialPayment))
        contract.IsPartialPaymentlitiko = ParseBoolSafe(isPartialPayment);
      else
      {
        Logger.DebugFormat("IsPartialPayment is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле 'Оплата частями' в договоре с Id: {contract.Id}");
      }
      
      if (!string.IsNullOrWhiteSpace(isEqualPayment))
        contract.IsEqualPaymentlitiko = ParseBoolSafe(isEqualPayment);
      else
      {
        Logger.DebugFormat("IsEqualPayment is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле 'Равномерная оплата' в договоре с Id: {contract.Id}");
      }
      
      if(!string.IsNullOrEmpty(isAmountForPeriod))
        contract.AmountForPeriodlitiko = ParseDoubleSafe(isAmountForPeriod);
      else
      {
        Logger.DebugFormat("AmountForPeriod is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле 'Сумма за период' в договоре с Id: {contract.Id}");
      }
      
      if(!string.IsNullOrEmpty(isRegistrationNumber))
        contract.RegistrationNumber = isRegistrationNumber;
      else
      {
        Logger.DebugFormat("RegistrationNumber is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле 'Регистрационный номер' в договоре с Id: {contract.Id}");
      }
      
      if(!string.IsNullOrEmpty(isNote))
        contract.Note = isNote;
      else
      {
        Logger.DebugFormat("Note is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле 'Примечание' в договоре с Id: {contract.Id}");
      }

      if (!string.IsNullOrEmpty(isRegistrationDate))
        contract.RegistrationDate = TryParseDate(isRegistrationDate);
      else
      {
        Logger.DebugFormat("RegistrationDate is empty for Contract with Id {0}, ExternalId{1}", contract.Id, isExternalD);
        result.Errors.Add($"Пустое поле 'Дата документа' в договоре с Id: {contract.Id}");
      }

      // Сохранить
      contract.Save();
      return contract;
    }

    // --- вспомогательные функции ---
    private static DateTime? TryParseDate(string date)
    {
      if (string.IsNullOrWhiteSpace(date))
        return null;

      DateTime result;

      // Поддерживаем оба формата: 1.12.2025 и 01.12.2025
      string[] formats = { "dd.MM.yyyy", "d.MM.yyyy", "dd.M.yyyy", "d.M.yyyy" };

      if (DateTime.TryParseExact(date, formats,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out result))
        return result;

      return null;
    }


    private static double ParseDoubleSafe(string value)
    {
      if (string.IsNullOrWhiteSpace(value))
        return 0.0;
      double r;
      if (double.TryParse(value.Trim().Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out r))
        return r;
      return 0.0;
    }

    private static bool ParseBoolSafe(string value)
    {
      if (string.IsNullOrWhiteSpace(value))
        return false;
      bool b;
      if (bool.TryParse(value.Trim(), out b))
        return b;
      // также допускаем "1"/"0", "yes"/"no"
      var norm = value.Trim().ToLowerInvariant();
      if (norm == "1" || norm == "true" || norm == "yes")
        return true;
      return false;
    }
  }
}
