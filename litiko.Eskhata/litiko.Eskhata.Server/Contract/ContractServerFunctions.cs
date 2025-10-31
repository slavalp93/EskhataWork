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
      contract.RegistrationNumber = isRegistrationNumber;
      contract.FrequencyExpenseslitiko = contract.FrequencyOfPaymentlitiko;
      
      var counterpartySignatory = litiko.Eskhata.Contacts.GetAll()
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
