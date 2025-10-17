using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using litiko.Eskhata.Contract;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Linq;


namespace litiko.Eskhata.Server
{
  partial class ContractFunctions
  {
    
    //string xmlPathFile = "C:\\RxData\\git_repository\\Contracts.xml"

    [Remote, Public]
    public List<string> ImportContractsFromXml()
    {
      Logger.Debug("ImportContractsFromXml - Start");

      var xmlPathFile = "Contracts.xml";
      var errorList = new List<string>();
      int countAll = 0;
      int countChanged = 0;
      int countNotChanged = 0;
      int countErrors = 0;
      bool isNew = false;

      try
      {
        XDocument xDoc = XDocument.Load(xmlPathFile);
        var documentElements = xDoc.Descendants("Data").Elements("Document");
        var counterpartyElements = xDoc.Descendants("Counterparty").Elements("Person");

        foreach (var documentElement in documentElements)
        {
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
          var isChangeReason = documentElement.Element("СhangeReason")?.Value;
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
          var isBatchProcessing = documentElement.Element("BatchProcessing")?.Value;
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

          try
          {
            var contract = Eskhata.Contracts.GetAll().FirstOrDefault(x => x.ExternalId == isExternalD);
            if (contract != null)
              Logger.DebugFormat("contract with ExternalD:{0} was found. Id:{1}, Name:{2}", isExternalD, contract.Id,
                                 contract.Name);
            else
            {
              contract = Eskhata.Contracts.Create();
              contract.ExternalId = isExternalD;
              contract.DeliveryAddresslitiko = "";
              contract.DeliveryDatelitiko = null;
              contract.RBOlitiko = isRBO;
              contract.ReasonForChangelitiko = isChangeReason;

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

                if (paymentEnum != null)
                  contract.PaymentMethodlitiko = paymentEnum;
              }

              if (!string.IsNullOrWhiteSpace(isPaymentFrequency))
              {
                var frequency = litiko.NSI.FrequencyOfPayments.GetAll()
                  .FirstOrDefault(f => f.Name.Equals(isPaymentFrequency, StringComparison.OrdinalIgnoreCase));

                if (frequency != null)
                  contract.FrequencyOfPaymentlitiko = frequency;
                else
                  Logger.DebugFormat("Payment frequency with name {0} was not found", isPaymentFrequency);
              }

              contract.IsPartialPaymentlitiko = bool.Parse(isPartialPayment);
              contract.IsEqualPaymentlitiko = bool.Parse(isEqualPayment);
              contract.AmountForPeriodlitiko = double.Parse(isAmountForPeriod);
              contract.AccDebtCreditlitiko = isAccountDebtCredt;
              contract.AccFutureExpenselitiko = isAccountFutureExpense;

              if (!string.IsNullOrWhiteSpace(isPaymentRegion))
              {
                var paymentRegion = litiko.NSI.PaymentRegions.GetAll()
                  .FirstOrDefault(r => r.ExternalId == isPaymentRegion);

                if (paymentRegion != null)
                {
                  contract.PaymentRegionlitiko = paymentRegion;
                  Logger.DebugFormat("Payment region with ExternalId {0} found {1}", isPaymentRegion, paymentRegion.Name);
                }
                else
                {
                  Logger.DebugFormat("Payment region with ExternalId {0} not found", isPaymentRegion);
                  /*paymentRegion = litiko.NSI.PaymentRegions.Create();
                  paymentRegion.ExternalId = isPaymentRegion;
                  paymentRegion.Name = $"{}";*/
                }
              }
              else
              {
                Logger.Debug("PaymentRegion element is missing or empty in XML.");
              }
              
              
              
              
              
              var totalAmount = 0.0;
              if (!string.IsNullOrWhiteSpace(isTotalAmount))
              {
                if (double.TryParse(isTotalAmount.Replace(',', '.'),
                                    System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    out totalAmount))
                {
                  contract.TotalAmountlitiko = totalAmount;
                  Logger.DebugFormat("Assigned TotalAmount = {0}", totalAmount);
                }
                else
                {
                  Logger.DebugFormat("Invalid TotalAmount format: {0}", isTotalAmount);
                }
              }
              else
              {
                Logger.Debug("TotalAmount is null or whitespace.");
              }
              
              Logger.DebugFormat("Create new Contract with ExternalD:{0}. ID{1}", isExternalD, contract.Id);
              contract.Save();
            }

            ////////////////////////////////////////////////////DocumentKind//////////////////////////////////////////////////////////////
            var documentKind = litiko.Eskhata.DocumentKinds.GetAll()
              .FirstOrDefault(k => k.ExternalIdlitiko == isDocumentKind);
            if (documentKind != null)
              Logger.DebugFormat("Document kind with ExternalId:{0} was found. Id{1}, Name:{2}", isDocumentKind,
                                 documentKind.Id, documentKind.Name);
            else
            {
              documentKind = litiko.Eskhata.DocumentKinds.Create();
              documentKind.ExternalIdlitiko = isDocumentKind;
              documentKind.Name = "Imported data";
              Logger.DebugFormat("Create new Document kind with ExternalId:{0}. ID{1}", isDocumentKind,
                                 documentKind.Id);
              documentKind.Save();
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            var documentGroup = litiko.Eskhata.DocumentGroupBases.GetAll()
              .FirstOrDefault(d => d.ExternalIdlitiko == isDocumentGroup);
            if (documentGroup == null)
            {
              documentGroup = litiko.Eskhata.DocumentGroupBases.Create();
              documentGroup.ExternalIdlitiko = isDocumentGroup;
            }

            contract.Subject = isSubject;
            contract.Name = isName;
            contract.CounterpartySignatorylitiko = int.Parse(isCounterpartySignatory);

            if (!string.IsNullOrWhiteSpace(isDepartment)) // Department
            {
              var department = litiko.Eskhata.Departments.GetAll()
                .FirstOrDefault(d => d.ExternalCodelitiko == isDepartment);

              if (department == null)
              {
                department = litiko.Eskhata.Departments.Create();
                contract.Department = department;
              }
              else
                Logger.DebugFormat("Department with ExternalCode:{0} not found", isDepartment);
            }

            if (!string.IsNullOrWhiteSpace(isResponsibleEmployee)) // ResponsibleEmployee
            {
              var employee = litiko.Eskhata.Employees.GetAll()
                .FirstOrDefault(e => e.ExternalId == isResponsibleEmployee);

              if (employee == null)
              {
                employee = litiko.Eskhata.Employees.Create();
                contract.ResponsibleEmployee = employee;
              }
              else
                Logger.DebugFormat("Employee with ExternalId:{0} not found", isResponsibleEmployee);
            }

            var authorId = 0;
            if (!string.IsNullOrWhiteSpace(isAuthor) && int.TryParse(isAuthor, out authorId))
            {
              var author = Sungero.CoreEntities.Users.GetAll().FirstOrDefault(x => x.Id == authorId);

              if (author != null)
                contract.Author = author;
            }

            contract.ResponsibleAccountantlitiko = isResponsibleAccountant;
            contract.ResponsibleDepartmentlitiko = isResponsibleDepartment;
            contract.RBOlitiko = isRBO;

            var datePatternFrom = "dd.MM.yyyy";
            var dateStyleFrom = System.Globalization.DateTimeStyles.None;
            DateTime validFrom;
            if (!string.IsNullOrWhiteSpace(isValidFrom) &&
                DateTime.TryParseExact(isValidFrom, datePatternFrom, null, dateStyleFrom, out validFrom))
              contract.ValidFrom = validFrom;

            var datePatternTill = "dd.MM.yyyy";
            var dateStyleTill = System.Globalization.DateTimeStyles.None;
            DateTime validTill;
            if (!string.IsNullOrWhiteSpace(isValidFrom) &&
                DateTime.TryParseExact(isValidFrom, datePatternTill, null, dateStyleTill, out validTill))
              contract.ValidFrom = validTill;

            //contract.ReasonForChangelitiko = isChaneReason;
            contract.AccDebtCreditlitiko = isAccountDebtCredt;
            contract.AccFutureExpenselitiko = isAccountFutureExpense;
            contract.InternalAcclitiko = isInternalAcc;
            contract.TotalAmountlitiko = double.Parse(isTotalAmount);

            if (!string.IsNullOrWhiteSpace(isCurrency))
            {
              var currency = Sungero.Commons.Currencies.GetAll()
                .FirstOrDefault(c => c.AlphaCode == isCurrency || c.NumericCode == isCurrency);

              if (currency != null)
                contract.Currency = currency;
              else
                Logger.Debug($"Currency is not found by code '{isCurrency}'");
            }

            if (!string.IsNullOrWhiteSpace(isCurrencyOperation))
            {
              var currencyOp = litiko.Eskhata.Currencies.GetAll()
                .FirstOrDefault(c => c.AlphaCode == isCurrencyOperation || c.NumericCode == isCurrencyOperation);

              if (currencyOp != null)
                contract.CurrencyOperationlitiko = currencyOp;
              else
                Logger.Debug($"Currency Operation is not found by code '{isCurrencyOperation}'");
            }

            contract.VATApplicablelitiko = bool.Parse(isVATApplicable);

            /*if (!string.IsNullOrWhiteSpace(isVATRate))
            {
              var vatRate = Sungero.Commons.VatRates.GetAll()
                .FirstOrDefault(r => r.Rate == double.Parse(isVATRate));

              if(vatRate != null)
                contract.VatRatelitiko
            }*/

            contract.VatRatelitiko = double.Parse(isVATRate);
            contract.VatAmount = double.Parse(isVATAmount);
            contract.IncomeTaxRatelitiko = double.Parse(isIncomeTaxRate);

            if (!string.IsNullOrWhiteSpace(isPaymentRegion))
            {
              var paymentRegion = litiko.NSI.PaymentRegions.GetAll()
                .FirstOrDefault(r => r.ExternalId == isPaymentRegion || r.Code == isPaymentRegion);

              if (paymentRegion != null)
                contract.PaymentRegionlitiko = paymentRegion;
              else
                Logger.Debug($"Payment Region is not found by code '{isPaymentRegion}'");
            }
          }
          catch (Exception ex)
          {
            var error = $"{ex.Message}";
            Logger.Error(error);
            errorList.Add(error);
            countErrors++;
          }
          Logger.DebugFormat("ImportContractsFromXML - End. CountAll {0}, Updated {1}, NotUpdated {2}, Errors {3}",
                             countAll, countChanged, countNotChanged, countErrors);
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
  }
}