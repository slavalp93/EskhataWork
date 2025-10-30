using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using litiko.Eskhata.Structures.Contracts.Contract;
using Sungero.Core;

namespace litiko.Eskhata.Server
{
  partial class ContractFunctions
  {
    /*[Remote, Public]
        public List<string> ImportContractsFromXml()
        {
          Logger.Debug("Import contracts from xml - Start");
    
          int addedCount = 0;
          int updatedCount = 0;
          int totalCount = 0;
          List<string> errorList = new List<string>();
    
          var xmlPathFile = "Contracts.xml";
    
          try
          {
            XDocument xDoc = XDocument.Load(xmlPathFile);
    
            var dataElements = xDoc.Descendants("Data");
            
            //var counterpartyElements = xDoc.Descendants("Data").Elements("Counterparty").ToArray;
    
            //var element = documentElements.FirstOrDefault();
            
            foreach (var dataElement in dataElements)
            {
              var documentElement = dataElement.Element("Document");
              var counterpartyElement = dataElement.Element("Counterparty");
              
              var isId = documentElement.Element("ID")?.Value;
              var isExternalD = documentElement.Element("ExternalD")?.Value;
              var isDocumentKind = documentElement.Element("DocumentKind")?.Value;
              var isDocumentGroup = documentElement.Element("DocumentGroup")?.Value;
              var isSubject = documentElement.Element("Subject")?.Value;
              var isName = documentElement.Element("Name")?.Value;
              var isCounterpartySignatory = documentElement.Element("CounterpartySignatory")?.Value;
              var isDepartment = documentElement.Element("Department")?.Value;
              var isResponsibleEmployee = documentElement.Element("ResponsibleEmployee")?.Value;
              var isAuthor = documentElement.Element("Author")?.Value;
              var isResponsibleAccountant = documentElement.Element("ResponsibleAccountant")?.Value;
              var isResponsibleDepartment = documentElement.Element("ResponsibleDepartment")?.Value;
              var isRBO = documentElement.Element("RBO")?.Value ?? "";
              var isValidFrom = documentElement.Element("ValidFrom")?.Value;
              var isValidTill = documentElement.Element("ValidTill")?.Value;
              var isChangeReason = documentElement.Element("ChangeReason")?.Value;
              var isAccountDebtCredt = documentElement.Element("AccountDebtCredt")?.Value;
              var isAccountFutureExpense = documentElement.Element("AccountFutureExpense")?.Value;
              var isInternalAcc = documentElement.Element("InternalAcc")?.Value;
              var isTotalAmount = documentElement.Element("TotalAmount")?.Value;
              var isCurrency = documentElement.Element("Currency")?.Value;
              var isCurrencyOperation = documentElement.Element("OperationCurrency")?.Value;
              var isVATApplicable = documentElement.Element("VATApplicable")?.Value;
              var isVATRate = documentElement.Element("VATRate")?.Value;
              var isVATAmount = documentElement.Element("VATAmount")?.Value;
              var isIncomeTaxRate = documentElement.Element("IncomeTaxRate")?.Value;
              var isPaymentRegion = documentElement.Element("PaymentRegion")?.Value;
              var isPaymentTaxRegion = documentElement.Element("PaymentTaxRegion")?.Value;
              //var isBatchProcessing = documentElement.Element("BatchProcessing")?.Value;
              var isPaymentMethod = documentElement.Element("PaymentMethod")?.Value;
              var isPaymentFrequency = documentElement.Element("PaymentFrequency")?.Value;
    
              var isPaymentBasis = documentElement.Element("PaymentBasis");
              if (isPaymentBasis != null)
              {
                var isPaymentContract = isPaymentBasis.Element("IsPaymentContract")?.Value;
                var isPaymentInvoice = isPaymentBasis.Element("IsPaymentInvoice")?.Value;
                var isPaymentTaxInvoice = isPaymentBasis.Element("IsPaymentTaxInvoice")?.Value;
                var isPaymentAct = isPaymentBasis.Element("IsPaymentAct")?.Value;
                var isPaymentOrder = isPaymentBasis.Element("IsPaymentOrder")?.Value;
              }
    
              var isPaymentClosureBasis = documentElement.Element("PaymentClosureBasis");
              if (isPaymentClosureBasis != null)
              {
                var isPaymentContract = isPaymentClosureBasis.Element("IsPaymentContract")?.Value;
                var isPaymentInvoice = isPaymentClosureBasis.Element("IsPaymentInvoice")?.Value;
                var isPaymentTaxInvoice = isPaymentClosureBasis.Element("IsPaymentTaxInvoice")?.Value;
                var isPaymentAct = isPaymentClosureBasis.Element("IsPaymentAct")?.Value;
                var isPaymentOrder = isPaymentClosureBasis.Element("IsPaymentOrder")?.Value;
                var isPaymentWaybill = isPaymentClosureBasis.Element("IsPaymentWaybill")?.Value;
              }
    
              var isPartialPayment = documentElement.Element("IsPartialPayment")?.Value;
              var isEqualPayment = documentElement.Element("IsEqualPayment")?.Value;
              var isAmountForPeriod = documentElement.Element("AmountForPeriod")?.Value;
              var isNote = documentElement.Element("Note")?.Value;
              var isRegistrationNumber = documentElement.Element("RegistrationNumber")?.Value;
              var isRegistrationDate = documentElement.Element("RegistrationDate")?.Value;
    
              var isCounterpartyPerson = counterpartyElement.Elements("Person");
              var isCounterpartyCompany = counterpartyElement.Elements("Company");
              
              try
              {
                var contract = Eskhata.Contracts.Null;
                contract = Eskhata.Contracts.GetAll(x => x.ExternalId == isExternalD).FirstOrDefault();
                bool isNew = false;
               
              
    
                if (documentElement != null)
                {
                  if (contract != null)
                    Logger.DebugFormat("contract with ExternalD:{0} was found. Id:{1}, Name:{2}", isExternalD, contract.Id,
                                       contract.Name);
                  else
                  {
                    contract = Eskhata.Contracts.Create();
    
                    addedCount++;
                    isNew = true;
    
                    contract.ExternalId = isExternalD;
    
                    var documentKind = litiko.Eskhata.DocumentKinds.GetAll()
                      .FirstOrDefault(k => k.ExternalIdlitiko == isDocumentKind);
    
                    // DocumentKind вид договора
                    documentKind.ExternalIdlitiko = isDocumentKind;
    
                    contract.DocumentKind = documentKind;
    
                    var documentGroup = litiko.Eskhata.DocumentGroupBases.GetAll()
                      .FirstOrDefault(d => d.ExternalIdlitiko == isDocumentGroup);
    
                    //DocumentGroup тип договора
                    documentGroup.ExternalIdlitiko = isDocumentGroup;
    
                    contract.DocumentGroup = documentGroup;
    
                    contract.Subject = isSubject;
                    contract.Name = isName;
    
    
                    var counterpartySignatory = litiko.Eskhata.Contacts.GetAll()
                      .FirstOrDefault(x => x.ExternalIdlitiko == isCounterpartySignatory);
                    contract.CounterpartySignatory = counterpartySignatory;
    
    
                    // Department
                    if (!string.IsNullOrWhiteSpace(isDepartment))
                    {
                      var department = litiko.Eskhata.Departments.GetAll()
                        .FirstOrDefault(d => d.ExternalCodelitiko == isDepartment);
                      contract.Department = department;
                    }
    
                    // ResponsibleEmployee
                    if (!string.IsNullOrWhiteSpace(isResponsibleEmployee))
                    {
                      var responsibleEmployee = litiko.Eskhata.Employees.GetAll()
                        .Where(e => e.ExternalId == isResponsibleEmployee).FirstOrDefault();
                      contract.ResponsibleEmployee = responsibleEmployee;
                    }
    
    
                    if (!string.IsNullOrWhiteSpace(isAuthor))
                    {
                      var author = litiko.Eskhata.Employees.GetAll().FirstOrDefault(x => x.ExternalId == isAuthor);
                      contract.Author = author;
                    }
    
                    var responsibilityAccountant = litiko.Eskhata.Employees.GetAll()
                      .FirstOrDefault(x => x.ExternalId == isResponsibleAccountant);
    
                    contract.ResponsibleEmployee = responsibilityAccountant;
    
                    contract.RBOlitiko = isRBO;
    
                    contract.ValidFrom = ParseDate(isValidFrom);
    
                    contract.ValidTill = ParseDate(isValidTill);
    
                    contract.ReasonForChangelitiko = isChangeReason;
    
                    contract.AccDebtCreditlitiko = isAccountDebtCredt;
    
                    contract.AccFutureExpenselitiko = isAccountFutureExpense;
    
                    //contract.InternalAcclitiko = isInternalAcc;
    
                    contract.TotalAmountlitiko = ParseDoubleSafe(isTotalAmount);
    
                    var currency = Sungero.Commons.Currencies.GetAll()
                      .FirstOrDefault(c => c.AlphaCode == isCurrency || c.NumericCode == isCurrency);
    
                    if (currency != null)
                      contract.Currency = currency;
    
    
                    if (!string.IsNullOrWhiteSpace(isCurrencyOperation))
                    {
                      var currencyOp = litiko.Eskhata.Currencies.GetAll()
                        .FirstOrDefault(c => c.AlphaCode == isCurrencyOperation || c.NumericCode == isCurrencyOperation);
    
                      if (currencyOp != null)
                        contract.CurrencyOperationlitiko = currencyOp;
                      else
                        Logger.Debug($"Currency Operation is not found by code '{isCurrencyOperation}'");
                    }
    
                    //contract.Vat = ParseBoolSafe(isVATApplicable); // TODO
    
                    if (!string.IsNullOrWhiteSpace(isVATRate))
                    {
                      double rateValue;
                      if (double.TryParse(isVATRate.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out rateValue))
                      {
                        var vatRate = Sungero.Commons.VatRates.GetAll().FirstOrDefault(r => r.Rate == rateValue);
                        contract.VatRate = vatRate;
                      }
                    }
                    contract.VatRatelitiko = ParseDoubleSafe(isVATRate);
    
                    contract.VatAmount = ParseDoubleSafe(isVATAmount);
    
                    contract.IncomeTaxRatelitiko = ParseDoubleSafe(isIncomeTaxRate);
    
                    if (!string.IsNullOrWhiteSpace(isPaymentRegion))
                    {
                      var paymentRegion = litiko.NSI.PaymentRegions.GetAll()
                        .Where(r => r.ExternalId == isPaymentRegion).FirstOrDefault();
    
                      if (paymentRegion != null)
                        contract.PaymentRegionlitiko = paymentRegion;
                      else
                        Logger.Debug($"Payment Region is not found by code '{isPaymentRegion}'");
                    }
    
    
                    if (!string.IsNullOrWhiteSpace(isPaymentTaxRegion))
                    {
                      var regionOfRental = litiko.NSI.RegionOfRentals.GetAll()
                        .FirstOrDefault(r => r.ExternalId == isPaymentTaxRegion);
    
                      if (regionOfRental != null)
                        contract.RegionOfRentallitiko = regionOfRental;
                      else
                        Logger.Debug($"Payment Tax Region is not found by code '{isPaymentRegion}'");
                    }
    
                    if (!string.IsNullOrWhiteSpace(isPaymentMethod))
                    {
                      Sungero.Core.Enumeration? paymentEnum = null;
    
                      switch (isPaymentMethod)
                      {
                        case "Предоплата":
                          paymentEnum = litiko.Eskhata.Contract.PaymentMethodlitiko.Prepayment;
                          break;
    
                        case "Постоплата":
                          paymentEnum = litiko.Eskhata.Contract.PaymentMethodlitiko.Postpay;
                          break;
                        default:
                          Logger.DebugFormat("Unknow Payment Method: {0}", isPaymentMethod);
                          break;
                      }
    
                      contract.PaymentMethodlitiko = paymentEnum;
                    }
                    // refactor here
    
                    if (!string.IsNullOrWhiteSpace(isPaymentFrequency))
                    {
                      var frequency = litiko.NSI.FrequencyOfPayments.GetAll()
                        .FirstOrDefault(f => f.Name.Equals(isPaymentFrequency, StringComparison.OrdinalIgnoreCase));
    
                      contract.FrequencyOfPaymentlitiko = frequency;
                    }
    
                    if (!string.IsNullOrWhiteSpace(isPartialPayment))
                    {
                      contract.IsPartialPaymentlitiko = ParseBoolSafe(isPartialPayment);
                    }
    
                    if (!string.IsNullOrWhiteSpace(isEqualPayment))
                    {
                      contract.IsEqualPaymentlitiko = ParseBoolSafe(isEqualPayment);
                    }
    
                    contract.AmountForPeriodlitiko = ParseDoubleSafe(isAmountForPeriod);
    
                    contract.AccDebtCreditlitiko = isAccountDebtCredt;
    
                    contract.AccFutureExpenselitiko = isAccountFutureExpense;
    
                    if (!string.IsNullOrWhiteSpace(isPaymentRegion))
                    {
                      var paymentRegion = litiko.NSI.PaymentRegions.GetAll()
                        .FirstOrDefault(r => r.ExternalId == isPaymentRegion);
    
                      if (paymentRegion == null)
                      {
                        contract.PaymentRegionlitiko = paymentRegion;
                        Logger.DebugFormat("Payment region with ExternalId {0} found {1}", isPaymentRegion,
                                           paymentRegion.Name);
                      }
                      else
                      {
                        Logger.DebugFormat("Payment region with ExternalId {0} not found", isPaymentRegion);
                      }
                    }
                    else
                    {
                      Logger.Debug("PaymentRegion element is missing or empty in XML.");
                    }
    
                    if (!string.IsNullOrWhiteSpace(isPartialPayment))
                    {
                      contract.IsPartialPaymentlitiko = ParseBoolSafe(isPartialPayment);
                    }
    
                    if (!string.IsNullOrWhiteSpace(isEqualPayment))
                    {
                      contract.IsEqualPaymentlitiko = ParseBoolSafe(isEqualPayment);
                    }
    
                    contract.AmountForPeriodlitiko = ParseDoubleSafe(isAmountForPeriod);
    
                    contract.Note = isNote;
    
                    contract.RegistrationNumber = isRegistrationNumber;
    
                    contract.RegistrationDate = DateTime.Parse(isRegistrationDate);
    
                    contract.FrequencyExpenseslitiko = contract.FrequencyOfPaymentlitiko;
    
                 
                    foreach (var isPerson in isCounterpartyPerson)
                    {
                      var isExternalId = isPerson.Element("ExternalId")?.Value;
    
                      if (!string.IsNullOrEmpty(isExternalId))
                      {
                        var counterpartyPerson = litiko.Eskhata.Counterparties.GetAll()
                          .Where(x => x.ExternalId == isExternalId).FirstOrDefault();
                        contract.Counterparty = counterpartyPerson;
                      }
                    }
              
                    foreach (var isCompany in isCounterpartyCompany)
                    {
                      var isExternalId = isCompany.Element("ExternalId")?.Value;
    
                      if (!string.IsNullOrEmpty(isExternalId))
                      {
                        var counterpartyCompany = litiko.Eskhata.Counterparties.GetAll()
                          .Where(x => x.ExternalId == isExternalId).FirstOrDefault();
                        contract.Counterparty = counterpartyCompany;
                      }
                    }
                    
                    
                    Logger.DebugFormat("Create new Contract with ExternalD:{0}. ID{1}. Name:{2}", isExternalD, contract.Id, contract.Name);
                    
                    contract.Save();
                    if (isNew)
                      addedCount++;
                    else
                      updatedCount++;
                  }
                }
              }
              catch (Exception ex)
              {
                var error = $"Error while reading XML: {ex.Message}";
                Logger.Error(error);
                errorList.Add(error);
              }
    
              Logger.DebugFormat("ImportContractsFromXML - End. CountAll {0}, Added {1}, Updated {2}, Errors {3}",
                                 totalCount, addedCount, updatedCount, errorList.Count);
              Logger.Debug("ImportContractsFromXML - Finish");
            }
          }
          catch (Exception e)
          {
            var error = "General error in load xml: " + e.Message;
            Logger.Error(error);
            errorList.Add(error);
          }
    
          return errorList;
        }
     */

    [Remote, Public]
    public IResultImportXml ImportContractsFromXml()
    {
      var result = Structures.Contracts.Contract.ResultImportXml.Create();
      result.Errors = new List<string>();
      result.ImportedCount = 0;
      
      Logger.Debug("Import contracts from xml - Start");
      
      var xmlPathFile = "Contracts.xml";

      try
      {
        XDocument xDoc = XDocument.Load(xmlPathFile);

        // Поддержка обоих вариантов: <Data> может быть корнем или вложенным
        var dataElements = xDoc.Element("Data");

        if (dataElements == null)
        {
          Logger.Debug("No <Data> elements found in XML.");
        }

        var documentElements = dataElements.Elements("Document").ToList();
        var counterpartyElements = dataElements.Elements("Counterparty").ToList();

        
        for (int i = 0; i < documentElements.Count; i++)
        {
          try
          {
            var documentElement = documentElements[i];
            var counterpartyElement = counterpartyElements.ElementAtOrDefault(i);

            if (documentElement == null)
              continue;

            var counterparty = ParseCounterparty(counterpartyElement);
            
            var contract = ParseContract(documentElement, counterparty);
            
            result.ImportedCount++;
            
            Logger.Debug($"Импортирован договор: {contract.Name}");
          }
          catch (Exception ex)
          {
            result.Errors.Add($"Ошибка при импорте документа №{i + 1}: {ex.Message}");
          }
        }
      }
      catch (Exception ex)
      {
        result.Errors.Add($"Общая ошибка импорта: {ex.Message}");
      }
      return result;
    }

    private ICounterparty ParseCounterparty(XElement counterpartyElement)
    {
      if (counterpartyElement == null)
        return null;

      var person = counterpartyElement.Element("Person");
      var company = counterpartyElement.Element("Company");

      string externalID =
        person?.Element("ExternalID")?.Value ?? company?.Element("ExternalID")?.Value;

      if (string.IsNullOrEmpty(externalID))
        return null;

      return litiko
        .Eskhata.Counterparties.GetAll()
        .FirstOrDefault(x => x.ExternalId == externalID);
    }

    // --- вспомогательные функции ---
    private static DateTime? TryParseDate(string date)
    {
      var result = DateTime.MinValue;
      if (
        DateTime.TryParseExact(
          date,
          "dd.MM.yyyy",
          null,
          System.Globalization.DateTimeStyles.None,
          out result
         )
       )
        return result;
      return null;
    }

    private IContract ParseContract(XElement documentElement, ICounterparty counterparty)
    {
      int totalCount = 0;
      int addedCount = 0;
      int updatedCount = 0;

      if (documentElement == null)
      {
        Logger.Debug("Skipping <Data> without <Document>.");
      }

      totalCount++;

      var isExternalD = documentElement.Element("ExternalD")?.Value?.Trim();
      // прочие поля — берем по мере необходимости
      var isDocumentKind = documentElement.Element("DocumentKind")?.Value?.Trim();
      var isDocumentGroup = documentElement.Element("DocumentGroup")?.Value?.Trim();
      var isSubject = documentElement.Element("Subject")?.Value;
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
      var isPaymentFrequency = documentElement.Element("PaymentFrequency")?.Value?.Trim();
      var isPartialPayment = documentElement.Element("IsPartialPayment")?.Value?.Trim();
      var isEqualPayment = documentElement.Element("IsEqualPayment")?.Value?.Trim();
      var isAmountForPeriod = documentElement.Element("AmountForPeriod")?.Value?.Trim();
      var isRegistrationNumber = documentElement.Element("RegistrationNumber")?.Value?.Trim();
      var isRegistrationDate = documentElement.Element("RegistrationDate")?.Value?.Trim();


      // Найти контракт по externalId
      var contract = Eskhata.Contracts.GetAll().FirstOrDefault(x => x.ExternalId == isExternalD);

      var isNew = contract == null;
      if (isNew)
      {
        contract = Eskhata.Contracts.Create();
        contract.ExternalId = isExternalD;
      }

      contract.Counterparty = counterparty;
      // --- DocumentKind (назначаем только если нашли)
      if (!string.IsNullOrWhiteSpace(isDocumentKind))
      {
        var documentKind = litiko
          .Eskhata.DocumentKinds.GetAll()
          .FirstOrDefault(k => k.ExternalIdlitiko == isDocumentKind);
        if (documentKind != null)
          contract.DocumentKind = documentKind;
        else
          Logger.DebugFormat(
            "DocumentKind with ExternalId {0} not found, skipping assignment.",
            isDocumentKind
           );
      }

      // --- DocumentGroup
      if (!string.IsNullOrWhiteSpace(isDocumentGroup))
      {
        var documentGroup = litiko
          .Eskhata.DocumentGroupBases.GetAll()
          .FirstOrDefault(g => g.ExternalIdlitiko == isDocumentGroup);
        if (documentGroup != null)
          contract.DocumentGroup = documentGroup;
        else
          Logger.DebugFormat(
            "DocumentGroup with ExternalId {0} not found, skipping assignment.",
            isDocumentGroup
           );
      }

      // Простые поля
      contract.Subject = isSubject;
      contract.Name = isName;
      contract.RBOlitiko = isRBO;
      contract.ReasonForChangelitiko = isChangeReason;
      contract.AccDebtCreditlitiko = isAccountDebtCredt;
      contract.AccFutureExpenselitiko = isAccountFutureExpense;
      
      var counterpartySignatory = litiko.Eskhata.Employees.GetAll()
        .FirstOrDefault(x => x.ExternalIdlitiko == isCounterpartySignatory);
      contract.CounterpartySignatory = counterpartySignatory;
      
      // Department
      if (!string.IsNullOrWhiteSpace(isDepartment))
      {
        var dept = litiko
          .Eskhata.Departments.GetAll()
          .FirstOrDefault(d => d.ExternalCodelitiko == isDepartment);
        if (dept != null)
          contract.Department = dept;
      }

      // ResponsibleEmployee
      if (!string.IsNullOrWhiteSpace(isResponsibleEmployee))
      {
        var emp = litiko
          .Eskhata.Employees.GetAll()
          .FirstOrDefault(e => e.ExternalId == isResponsibleEmployee);
        if (emp != null)
          contract.ResponsibleEmployee = emp;
      }

      // Author
      if (!string.IsNullOrWhiteSpace(isAuthor))
      {
        var author = Sungero
          .Company.Employees.GetAll()
          .FirstOrDefault(u => u.ExternalId == isAuthor);
        if (author != null)
          contract.Author = author;
      }

      // Dates — безопасно
      var parsedFrom = TryParseDate(isValidFrom);
      if (parsedFrom.HasValue)
        contract.ValidFrom = parsedFrom.Value;
      var parsedTill = TryParseDate(isValidTill);
      if (parsedTill.HasValue)
        contract.ValidTill = parsedTill.Value;

      // Money / numeric
      contract.TotalAmountlitiko = ParseDoubleSafe(isTotalAmount);

      
      if (!string.IsNullOrWhiteSpace(isCurrencyOperation))
      {
        var currencyOp = litiko.Eskhata.Currencies.GetAll()
          .FirstOrDefault(c => c.AlphaCode == isCurrencyOperation || c.NumericCode == isCurrencyOperation);
        
        if (currencyOp != null)
          contract.CurrencyOperationlitiko = currencyOp;
        else
          Logger.Debug($"Currency Operation is not found by code '{isCurrencyOperation}'");
      }
      
      // Currency
      if (!string.IsNullOrWhiteSpace(isCurrency))
      {
        var currency = litiko.Eskhata.Currencies.GetAll()
          .FirstOrDefault(c => c.AlphaCode == isCurrency || c.NumericCode == isCurrency);
        if (currency != null)
          contract.Currency = currency;
      }
      
      contract.VatRatelitiko = ParseDoubleSafe(isVATRate);
      
      contract.VatAmount = ParseDoubleSafe(isVATAmount);
      
      contract.IncomeTaxRatelitiko = ParseDoubleSafe(isIncomeTaxRate);

      contract.IsVATlitiko = ParseBoolSafe(isVATApplicable);

      // PaymentRegion
      if (!string.IsNullOrWhiteSpace(isPaymentRegion))
      {
        var paymentRegion = litiko
          .NSI.PaymentRegions.GetAll()
          .FirstOrDefault(r =>
                          r.ExternalId == isPaymentRegion || r.Code == isPaymentRegion
                         );
        if (paymentRegion != null)
          contract.PaymentRegionlitiko = paymentRegion;
      }

      // PaymentTaxRegion / RegionOfRental
      if (!string.IsNullOrWhiteSpace(isPaymentTaxRegion))
      {
        var region = litiko
          .NSI.RegionOfRentals.GetAll()
          .FirstOrDefault(r =>
                          r.ExternalId == isPaymentTaxRegion || r.Code == isPaymentTaxRegion
                         );
        if (region != null)
          contract.RegionOfRentallitiko = region;
      }

      // PaymentMethod (enum) — сопоставь текст с вариантами
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

      // Frequency
      if (!string.IsNullOrWhiteSpace(isPaymentFrequency))
      {
        var frequency = litiko
          .NSI.FrequencyOfPayments.GetAll()
          .FirstOrDefault(f =>
                          f.Name.Equals(isPaymentFrequency, StringComparison.OrdinalIgnoreCase)
                         );
        if (frequency != null)
          contract.FrequencyOfPaymentlitiko = frequency;
      }

      // Bool fields
      if (!string.IsNullOrWhiteSpace(isPartialPayment))
        contract.IsPartialPaymentlitiko = ParseBoolSafe(isPartialPayment);
      if (!string.IsNullOrWhiteSpace(isEqualPayment))
        contract.IsEqualPaymentlitiko = ParseBoolSafe(isEqualPayment);

      // AmountForPeriod
      contract.AmountForPeriodlitiko = ParseDoubleSafe(isAmountForPeriod);

      // Registration
      contract.Note = documentElement.Element("Note")?.Value;
      contract.RegistrationNumber = isRegistrationNumber;
      var regDate = TryParseDate(isRegistrationDate);
      if (regDate.HasValue)
        contract.RegistrationDate = regDate.Value;

      // Сохранить
      contract.Save();

      if (isNew)
        addedCount++;
      else
        updatedCount++;

      return contract;
    }

    private static double ParseDoubleSafe(string value)
    {
      if (string.IsNullOrWhiteSpace(value))
        return 0.0;
      double r;
      if (
        double.TryParse(
          value.Trim().Replace(',', '.'),
          System.Globalization.NumberStyles.Any,
          System.Globalization.CultureInfo.InvariantCulture,
          out r
         )
       )
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

    /*private static bool ParseBoolSafe(string value)
        {
          bool result;
          if (bool.TryParse(value, out result))
            return result;
          Logger.DebugFormat("Unexpected boolean value: {0}", value);
          return false;
        }
    
        private static double ParseDoubleSafe(string value)
        {
          if (string.IsNullOrWhiteSpace(value))
            return 0.0;
    
          var result = 0.0;
          if (double.TryParse(value, System.Globalization.NumberStyles.Any,
                              System.Globalization.CultureInfo.InvariantCulture, out result))
            return result;
          return 0.0;
        }
        private DateTime? ParseDate(string date)
        {
          var result = DateTime.MinValue;
          if (DateTime.TryParseExact(date, "dd.MM.yyyy", null,
                                     System.Globalization.DateTimeStyles.None, out result))
            return result;
          return null;
        }*/

    /// <summary>
    /// Создать юрид. заключение.
    /// </summary>
    /// <returns>Юридическое заключение.</returns>
    [Remote, Public]
    public static Sungero.Docflow.IAddendum CreateLegalOpinion()
    {
      var aviabledDocumentKinds =
        Sungero.Docflow.PublicFunctions.DocumentKind.GetAvailableDocumentKinds(
          typeof(Sungero.Docflow.IAddendum)
         );
      var docKind = aviabledDocumentKinds
        .Where(x =>
               x.Status == Sungero.CoreEntities.DatabookEntry.Status.Active
               && x.Name == "Юридическое заключение"
              )
        .FirstOrDefault();

      if (docKind == null)
        return null;

      var newDoc = Sungero.Docflow.Addendums.Create();
      newDoc.DocumentKind = docKind;
      return newDoc;
    }

    /// <summary>
    /// Построить сводку по документу.
    /// </summary>
    /// <returns>Сводка по документу.</returns>
    [Remote(IsPure = true)]
    public override StateView GetDocumentSummary()
    {
      var documentSummary = StateView.Create();
      var documentBlock = documentSummary.AddBlock();

      // Краткое имя документа.
      var documentName = _obj.DocumentKind.Name;
      if (!string.IsNullOrWhiteSpace(_obj.RegistrationNumber))
        documentName +=
          Sungero.Docflow.OfficialDocuments.Resources.Number + _obj.RegistrationNumber;

      if (_obj.RegistrationDate != null)
        documentName +=
          Sungero.Docflow.OfficialDocuments.Resources.DateFrom
          + _obj.RegistrationDate.Value.ToString("d");

      documentBlock.AddLabel(documentName);

      // Типовой/Не типовой, Рамочный.
      var isStandardLabel = _obj.IsStandard.Value
        ? Sungero.Contracts.ContractBases.Resources.isStandartContract
        : Sungero.Contracts.ContractBases.Resources.isNotStandartContract;
      var isframeworkContractLabel = _obj.IsFrameworkContract.Value
        ? _obj.Info.Properties.IsFrameworkContract.LocalizedName
        : string.Empty;

      if (string.IsNullOrEmpty(isframeworkContractLabel))
        documentBlock.AddLabel(string.Format("({0})", isStandardLabel));
      else
        documentBlock.AddLabel(
          string.Format("({0}, {1})", isStandardLabel, isframeworkContractLabel)
         );
      documentBlock.AddLineBreak();
      documentBlock.AddLineBreak();

      // НОР.
      documentBlock.AddLabel(
        string.Format("{0}: ", _obj.Info.Properties.BusinessUnit.LocalizedName)
       );
      if (_obj.BusinessUnit != null)
        documentBlock.AddLabel(Hyperlinks.Get(_obj.BusinessUnit));
      else
        documentBlock.AddLabel("-");

      documentBlock.AddLineBreak();

      // Контрагент.
      documentBlock.AddLabel(
        string.Format("{0}:", Sungero.Contracts.ContractBases.Resources.Counterparty)
       );
      if (_obj.Counterparty != null)
      {
        documentBlock.AddLabel(Hyperlinks.Get(_obj.Counterparty));
        if (_obj.Counterparty.Nonresident == true)
          documentBlock.AddLabel(
            string.Format(
              "({0})",
              _obj.Counterparty.Info.Properties.Nonresident.LocalizedName
             )
            .ToLower()
           );
      }
      else
      {
        documentBlock.AddLabel("-");
      }

      documentBlock.AddLineBreak();

      // Содержание.
      var subject = !string.IsNullOrEmpty(_obj.Subject) ? _obj.Subject : "-";
      documentBlock.AddLabel(
        string.Format(
          "{0}: {1}",
          Sungero.Contracts.ContractBases.Resources.Subject,
          subject
         )
       );
      documentBlock.AddLineBreak();

      // Сумма договора.
      var amount = this.GetTotalAmountDocumentSummary(_obj.TotalAmountlitiko);
      var amountText = string.Format(
        "{0}: {1}",
        _obj.Info.Properties.TotalAmountlitiko.LocalizedName,
        amount
       );
      documentBlock.AddLabel(amountText);
      documentBlock.AddLineBreak();

      // Валюта.
      var currencyText = string.Format(
        "{0}: {1}",
        _obj.Info.Properties.CurrencyContractlitiko.LocalizedName,
        _obj.CurrencyContractlitiko
       );
      documentBlock.AddLabel(currencyText);
      documentBlock.AddLineBreak();

      // Срок действия договора.
      var validity = "-";
      var validFrom = _obj.ValidFrom.HasValue
        ? string.Format(
          "{0} {1} ",
          Sungero.Contracts.ContractBases.Resources.From,
          _obj.ValidFrom.Value.Date.ToShortDateString()
         )
        : string.Empty;

      var validTill = _obj.ValidTill.HasValue
        ? string.Format(
          "{0} {1}",
          Sungero.Contracts.ContractBases.Resources.Till,
          _obj.ValidTill.Value.Date.ToShortDateString()
         )
        : string.Empty;

      var isAutomaticRenewal =
        _obj.IsAutomaticRenewal.Value && !string.IsNullOrEmpty(validTill)
        ? string.Format(", {0}", Sungero.Contracts.ContractBases.Resources.Renewal)
        : string.Empty;

      if (!string.IsNullOrEmpty(validFrom) || !string.IsNullOrEmpty(validTill))
        validity = string.Format("{0}{1}{2}", validFrom, validTill, isAutomaticRenewal);

      var validityText = string.Format(
        "{0}:",
        Sungero.Contracts.ContractBases.Resources.Validity
       );
      documentBlock.AddLabel(validityText);
      documentBlock.AddLabel(validity);
      documentBlock.AddLineBreak();
      documentBlock.AddEmptyLine();

      // Примечание.
      var note = string.IsNullOrEmpty(_obj.Note) ? "-" : _obj.Note;
      var noteText = string.Format("{0}:", Sungero.Contracts.ContractBases.Resources.Note);
      documentBlock.AddLabel(noteText);
      documentBlock.AddLabel(note);

      return documentSummary;
    }
  }
}
