using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using System.Xml.Linq;

namespace litiko.Eskhata.Server
{
  partial class ContractFunctions
  {
    [Remote, Public]
    public List<string> ImportContractsFromXml()
    {
      Logger.Debug("Import contracts from xml - Start");

      int addedCount = 0;
      int updatedCount = 0;
      int totalCount = 0;
      List<string> errorList = new List<string>();
      
      var xmlPathFile = "Contracts.xml";
      //var result = ResultImportXml.Create();

      try
      {
        XDocument xDoc = XDocument.Load(xmlPathFile);
        var documentElements = xDoc.Descendants("Data").Elements("Document");
//        var personElements = xDoc.Descendants("Counterparty").Elements("Person");
//        var companyElements = xDoc.Descendants("Counterparty").Elements("Company");
        
        //var documentType = Sungero.Docflow.DocumentTypes.GetAll(t => t.DocumentTypeGuid == Sungero.Contracts.PublicConstants.Module.ContractGuid).FirstOrDefault();
        
        foreach (var documentElement in documentElements)
        {
          var isId = documentElement.Element("Id")?.Value;
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

         /* var isPaymentBasis = documentElement.Element("PaymentBasis");
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
          }*/

          var isPartialPayment = documentElement.Element("IsPartialPayment")?.Value;
          var isEqualPayment = documentElement.Element("IsEqualPayment")?.Value;
          var isAmountForPeriod = documentElement.Element("AmountForPeriod")?.Value;
          var isNote = documentElement.Element("Note")?.Value;
          var isRegistrationNumber = documentElement.Element("RegistrationNumber")?.Value;
          var isRegistrationDate = documentElement.Element("RegistrationDate")?.Value;
          
          try
          {
            var contract = Eskhata.Contracts.Null;
            contract = Eskhata.Contracts.GetAll(x => x.ExternalId == isExternalD).FirstOrDefault();
            bool isNew = false;
            
            if (contract != null)
              Logger.DebugFormat("contract with ExternalD:{0} was found. Id:{1}, Name:{2}", isExternalD, contract.Id, contract.Name);
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

              
              var counterpartySignatory = litiko.Eskhata.Contacts.GetAll().FirstOrDefault(x=>x.ExternalIdlitiko == isCounterpartySignatory);
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
                var responsibleEmployee = litiko.Eskhata.Employees.GetAll().Where(e => e.ExternalId == isResponsibleEmployee).FirstOrDefault();
                contract.ResponsibleEmployee = responsibleEmployee;
              }

              
              if (!string.IsNullOrWhiteSpace(isAuthor))
              {
                var author = Sungero.Company.Employees.GetAll().FirstOrDefault(x => x.ExternalId == isAuthor);
                contract.Author = author;
              }

              var responsibilityAccountant = litiko.Eskhata.Employees.GetAll().FirstOrDefault(x=>x.ExternalId == isResponsibleAccountant);
              
              contract.ResponsibleEmployee = responsibilityAccountant;
              
              contract.RBOlitiko = isRBO;

              // validfrom
              var datePatternFrom = "dd.MM.yyyy";
              var dateStyleFrom = System.Globalization.DateTimeStyles.None;
              DateTime validFrom;
              if (!string.IsNullOrWhiteSpace(isValidFrom) &&
                  DateTime.TryParseExact(isValidFrom, datePatternFrom, null, dateStyleFrom, out validFrom))
                contract.ValidFrom = validFrom;

              
              //validtill
              var datePatternTill = "dd.MM.yyyy";
              var dateStyleTill = System.Globalization.DateTimeStyles.None;
              DateTime validTill;
              if (!string.IsNullOrWhiteSpace(isValidTill) &&
                  DateTime.TryParseExact(isValidTill, datePatternTill, null, dateStyleTill, out validTill))
                contract.ValidTill = validTill;

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
                if (double.TryParse(isVATRate.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out rateValue))
                {
                  var vatRate = Sungero.Commons.VatRates.GetAll().FirstOrDefault(r=>r.Rate == rateValue);
                  contract.VatRate = vatRate;
                }
              }
              
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

              if(!string.IsNullOrWhiteSpace(isPartialPayment))
              {
                contract.IsPartialPaymentlitiko = ParseBoolSafe(isPartialPayment);
              }
              
              if(!string.IsNullOrWhiteSpace(isEqualPayment))
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
                  Logger.DebugFormat("Payment region with ExternalId {0} found {1}", isPaymentRegion, paymentRegion.Name);
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
              
              if(!string.IsNullOrWhiteSpace(isPartialPayment))
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
              
              var counterpartyElement = documentElement.Element("Counterparty");
              if (counterpartyElement != null)
              {
                var personElement = counterpartyElement.Element("Person");
                var companyElement = counterpartyElement.Element("Company");

                if (personElement != null)
                {
                  var personExternalId = personElement.Element("ExternalID")?.Value;
                  
                  var person = litiko.Eskhata.Counterparties.GetAll()
                    .FirstOrDefault(x => x.ExternalId == personExternalId);
                  
                  ProcessPersonXml(person);
                  
                  if (person != null)
                    contract.Counterparty = person;
                  else
                    Logger.DebugFormat("Counterparty Person with ExternalId {0} not found", personExternalId);
                }
                else if (companyElement != null)
                {
                  var companyExternalId = companyElement.Element("ExternalID")?.Value;
                  
                  var company = litiko.Eskhata.Counterparties.GetAll()
                    .FirstOrDefault(x => x.ExternalId == companyExternalId);
                  
                    ProcessCompanyXml(company);
                    
                  if (company != null)
                    contract.Counterparty = company;
                  else
                    Logger.DebugFormat("Counterparty Company with ExternalId {0} not found", companyExternalId);
                }
                else
                {
                  Logger.Debug("No valid Counterparty element found in XML.");
                }
              }
              else
              {
                Logger.Debug("No valid Counterparty element found in XML.");

              }
              contract.Save();

              if (isNew)
                addedCount++;
              else
                updatedCount++;
              
              Logger.DebugFormat("Create new Contract with ExternalD:{0}. ID{1}. Name:{2}", isExternalD, contract.Id, contract.Name);

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

    private string ProcessPersonXml(Sungero.Parties.ICounterparty counterparty)
    {
      var person = litiko.Eskhata.People.As(counterparty);
      return person.ExternalId;
    }
    
    private string ProcessCompanyXml(Sungero.Parties.ICounterparty counterparty)
    {
      var company = litiko.Eskhata.Companies.As(counterparty);
      return company.ExternalId;
    }

    
    /// <summary>
    /// Создать юрид. заключение.
    /// </summary>
    /// <returns>Юридическое заключение.</returns>
    [Remote, Public]
    public static Sungero.Docflow.IAddendum CreateLegalOpinion()
    {
      var aviabledDocumentKinds = Sungero.Docflow.PublicFunctions.DocumentKind.GetAvailableDocumentKinds(typeof(Sungero.Docflow.IAddendum));
      var docKind = aviabledDocumentKinds
        .Where(x => x.Status == Sungero.CoreEntities.DatabookEntry.Status.Active && x.Name == "Юридическое заключение")
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
        documentName += Sungero.Docflow.OfficialDocuments.Resources.Number + _obj.RegistrationNumber;
      
      if (_obj.RegistrationDate != null)
        documentName += Sungero.Docflow.OfficialDocuments.Resources.DateFrom + _obj.RegistrationDate.Value.ToString("d");
      
      documentBlock.AddLabel(documentName);
      
      // Типовой/Не типовой, Рамочный.
      var isStandardLabel = _obj.IsStandard.Value ? Sungero.Contracts.ContractBases.Resources.isStandartContract : Sungero.Contracts.ContractBases.Resources.isNotStandartContract;
      var isframeworkContractLabel = _obj.IsFrameworkContract.Value ? _obj.Info.Properties.IsFrameworkContract.LocalizedName : string.Empty;
      
      if (string.IsNullOrEmpty(isframeworkContractLabel))
        documentBlock.AddLabel(string.Format("({0})", isStandardLabel));
      else
        documentBlock.AddLabel(string.Format("({0}, {1})", isStandardLabel, isframeworkContractLabel));
      documentBlock.AddLineBreak();
      documentBlock.AddLineBreak();
      
      // НОР.
      documentBlock.AddLabel(string.Format("{0}: ", _obj.Info.Properties.BusinessUnit.LocalizedName));
      if (_obj.BusinessUnit != null)
        documentBlock.AddLabel(Hyperlinks.Get(_obj.BusinessUnit));
      else
        documentBlock.AddLabel("-");
      
      documentBlock.AddLineBreak();
      
      // Контрагент.
      documentBlock.AddLabel(string.Format("{0}:", Sungero.Contracts.ContractBases.Resources.Counterparty));
      if (_obj.Counterparty != null)
      {
        documentBlock.AddLabel(Hyperlinks.Get(_obj.Counterparty));
        if (_obj.Counterparty.Nonresident == true)
          documentBlock.AddLabel(string.Format("({0})", _obj.Counterparty.Info.Properties.Nonresident.LocalizedName).ToLower());
      }
      else
      {
        documentBlock.AddLabel("-");
      }
      
      documentBlock.AddLineBreak();
      
      // Содержание.
      var subject = !string.IsNullOrEmpty(_obj.Subject) ? _obj.Subject : "-";
      documentBlock.AddLabel(string.Format("{0}: {1}", Sungero.Contracts.ContractBases.Resources.Subject, subject));
      documentBlock.AddLineBreak();
      
      // Сумма договора.
      var amount = this.GetTotalAmountDocumentSummary(_obj.TotalAmountlitiko);
      var amountText = string.Format("{0}: {1}", _obj.Info.Properties.TotalAmountlitiko.LocalizedName, amount);
      documentBlock.AddLabel(amountText);
      documentBlock.AddLineBreak();

      // Валюта.
      var currencyText = string.Format("{0}: {1}", _obj.Info.Properties.CurrencyContractlitiko.LocalizedName, _obj.CurrencyContractlitiko);
      documentBlock.AddLabel(currencyText);
      documentBlock.AddLineBreak();
      
      // Срок действия договора.
      var validity = "-";
      var validFrom = _obj.ValidFrom.HasValue ?
        string.Format("{0} {1} ", Sungero.Contracts.ContractBases.Resources.From, _obj.ValidFrom.Value.Date.ToShortDateString()) :
        string.Empty;
      
      var validTill = _obj.ValidTill.HasValue ?
        string.Format("{0} {1}", Sungero.Contracts.ContractBases.Resources.Till, _obj.ValidTill.Value.Date.ToShortDateString()) :
        string.Empty;
      
      var isAutomaticRenewal = _obj.IsAutomaticRenewal.Value &&  !string.IsNullOrEmpty(validTill) ?
        string.Format(", {0}", Sungero.Contracts.ContractBases.Resources.Renewal) :
        string.Empty;
      
      if (!string.IsNullOrEmpty(validFrom) || !string.IsNullOrEmpty(validTill))
        validity = string.Format("{0}{1}{2}", validFrom, validTill, isAutomaticRenewal);
      
      var validityText = string.Format("{0}:", Sungero.Contracts.ContractBases.Resources.Validity);
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
    
    private static bool ParseBoolSafe(string value)
    {
      bool result;
      if (bool.TryParse(value, out result))
        return result;
      Logger.DebugFormat("Unexpected boolean value: {0}", value);
      return false;
    }
    private static double ParseDoubleSafe(string value)
    {
      if(string.IsNullOrWhiteSpace(value))
        return 0.0;
      
      var result = 0.0;
      if(double.TryParse(value, System.Globalization.NumberStyles.Any,
                         System.Globalization.CultureInfo.InvariantCulture, out result))
        return result;
      return 0.0;
    }
  }
}