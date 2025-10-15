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
    /*//    /// <summary>
    //    /// Импорт договоров из файла экспортированных из АБС
    //    /// </summary>
    //    [Remote, Public]
    //    public List<string> ImportContractsFromXml()
    //    {
    //      var errorList = new List<string>();
//
    //      string filePath = "C:\\RxData\\git_repository\\Contracts.xml";
//
    //      if(!File.Exists(filePath))
    //      {
    //        Logger.Error("XML file is not exist");
    //        return errorList;
    //      }
//
    //      try
    //      {
    //       // var documentKind = litiko.Eskhata.Contract
    //       //var person = litiko.Eskhata.Contracts.As()
    //        XDocument xmlDoc = XDocument.Load(filePath);
//
    //        XElement dataElement = xmlDoc.Descendants("Data").FirstOrDefault();
//
    //        if(dataElement == null)
    //        {
    //          Logger.Error("<DATA> tag is not found.");
    //          return errorList;
    //        }
//
    //        foreach (var docElement in dataElement.Elements("Document"))
    //        {
    //          var contract = litiko.Eskhata.Contracts.Create();
//
    //          contract.ExternalId = docElement.Element("ExternalD")?.Value;
//
    //          var documentKind = docElement.Element("DocumentKind")?.Value;
    //          var docKind = Sungero.Docflow.DocumentKinds.GetAll().FirstOrDefault(k=>k.Code == documentKind);
    //          if(docKind != null)
    //            contract.DocumentKind = docKind;
    //          else
    //            Logger.Error("DocumentKind not found");
//
    //          var documentGroup = docElement.Element("DocumentGroup").Value;
    //          if(documentGroup != null)
    //            contract.DocumentGroup = documentGroup;
    //          else
    //            Logger.Error("Document group not found");
//
    //          contract.Subject = docElement.Element("Subject").Value;
    //          contract.Name = docElement.Element("Name").Value;
//
    //          /*var counterpartySignatory = (Sungero.Parties.IContact)(docElement.Element("CounterpartySignatory"));
    //          contract.CounterpartySignatory = counterpartySignatory;
              contract.Department = (Sungero.Company.IDepartment)docElement.Element("Department");
              contract.ResponsibleEmployee = (Sungero.Company.IEmployee)docElement.Element("ResponsibleEmployee");
              contract.Author = (IUser)docElement.Element("Author");/*
              contract.ResponsibleAccountantlitiko = docElement.Element("ResponsibleAccountant").Value;
              contract.ResponsibleDepartmentlitiko = docElement.Element("ResponsibleDepartment").Value;
              contract.RBOlitiko = docElement.Element("RBO").Value;
              contract.ValidFrom = docElement.Element("ValidFrom").Value;
              contract.ValidTill = docElement.Element("ValidTill").Value;
              contract.ReasonForChangelitiko = docElement.Element("СhangeReason").Value;
              contract.AccDebtCreditlitiko = docElement.Element("AccountDebtCredt").Value;
              contract.AccFutureExpenselitiko = docElement.Element("AccountFutureExpense").Value;
              contract.InternalAcclitikoternalAcc = docElement.Element("InternalAcc").Value; // create
              contract.TotalAmountlitiko = docElement.Element("TotalAmount").Value;
              contract.Currency = docElement.Element("Currency").Value;
              contract.CurrencyOperationlitiko = docElement.Element("OperationCurrency").Value;
              contract.IsVATlitiko = docElement.Element("VATApplicable").Value;
              contract.VatRatelitiko = docElement.Element("VATRate").Value;
              contract.VatAmount = docElement.Element("VATAmount").Value;
              contract.IncomeTaxRatelitiko = docElement.Element("IncomeTaxRate").Value;
              contract.PaymentRegionlitiko = docElement.Element("PaymentRegion").Value;
              contract.PaymentTaxRegionlitiko = docElement.Element("PaymentTaxRegion").Value;
              contract.BatchProcessinglitiko = docElement.Element("BatchProcessing").Value;
              contract.PaymentMethodlitiko = docElement.Element("PaymentMethod").Value;
              contract.FrequencyOfPaymentlitiko = docElement.Element("PaymentFrequency").Value;
              

              var paymentBasisElement = docElement.Element("PaymentBasis");
              if(paymentBasisElement != null)
              {
                var item = contract.PaymentBasislitiko.AddNew();

                item.IsPaymentContract   = (bool)paymentBasisElement.Element("IsPaymentContract");
                item.IsPaymentInvoice    = (bool)paymentBasisElement.Element("IsPaymentInvoice");
                item.IsPaymentTaxInvoice = (bool)paymentBasisElement.Element("IsPaymentTaxInvoice");
                item.IsPaymentAct        = (bool)paymentBasisElement.Element("IsPaymentAct");
                item.IsPaymentOrder      = (bool)paymentBasisElement.Element("IsPaymentOrder");
              }

              var paymentClosureBasis = docElement.Element("PaymentClosureBasis");
              if (paymentClosureBasis != null)
              {
                var item = contract.PaymentClosureBasislitiko.AddNew();

                item.IsPaymentContract   = (bool)paymentClosureBasis.Element("IsPaymentContract");
                item.IsPaymentInvoice    = (bool)paymentClosureBasis.Element("IsPaymentInvoice");
                item.IsPaymentTaxInvoice = (bool)paymentClosureBasis.Element("IsPaymentTaxInvoice");
                item.IsPaymentAct        = (bool)paymentClosureBasis.Element("IsPaymentAct");
                item.IsPaymentOrder      = (bool)paymentClosureBasis.Element("IsPaymentOrder");
                item.IsPaymentWaybill    = (bool)paymentClosureBasis.Element("IsPaymentWaybill");
              }
              
              contract.IsPartialPaymentlitiko = docElement.Element("IsPartialPayment");
              contract.FrequencyOfPaymentlitiko = docElement.Element("PaymentFrequency").Value;
              contract.FrequencyOfPaymentlitiko = docElement.Element("PaymentFrequency").Value;
              contract.FrequencyOfPaymentlitiko = docElement.Element("PaymentFrequency").Value;
              contract.FrequencyOfPaymentlitiko = docElement.Element("PaymentFrequency").Value;
              contract.FrequencyOfPaymentlitiko = docElement.Element("PaymentFrequency").Value;
              contract.FrequencyOfPaymentlitiko = docElement.Element("PaymentFrequency").Value;
              contract.FrequencyOfPaymentlitiko = docElement.Element("PaymentFrequency").Value;
              contract.FrequencyOfPaymentlitiko = docElement.Element("PaymentFrequency").Value;
              


             contract.Save();

            }
          }
          catch (Exception ex)
          {
            Console.WriteLine("{0}", ex);

          }

          return errorList;
        }


    // Серверный метод (на DDS в .NET 6)
    
    [Remote, Public]
    public List<string> ImportContractsFromXml()
    {
      var errorList = new List<string>();

      string filePath = "C:\\RxData\\git_repository\\Contracts.xml";

      if (!System.IO.File.Exists(filePath))
      {
        Logger.Error("XML файл не найден: " + filePath);
        return errorList;
      }

      var xmlDoc = new System.Xml.XmlDocument();
      xmlDoc.Load(filePath); // загружаем файл, а не текст!

      var dataNodes = xmlDoc.GetElementsByTagName("Data");

      foreach (System.Xml.XmlNode dataNode in dataNodes)
      {
        try
        {
          var documentNode = dataNode.SelectSingleNode("Document");
          var counterpartyNode = dataNode.SelectSingleNode("Counterparty/Person");

          // --- Контрагент ---
          var counterpartyInn = GetNodeValue(counterpartyNode, "INN");
          var counterpartyExternalId = GetNodeValue(counterpartyNode, "ExternalID");
          var lastName = GetNodeValue(counterpartyNode, "LastName");
          var firstName = GetNodeValue(counterpartyNode, "FirstName");
          var middleName = GetNodeValue(counterpartyNode, "MiddleName");
          var fullName = $"{lastName} {firstName} {middleName}".Trim();

          

          // --- Договор ---
          var externalId = GetNodeValue(documentNode, "ExternalID");
          var name = GetNodeValue(documentNode, "Name");
          var subject = GetNodeValue(documentNode, "Subject");
          var docKindId = GetNodeValue(documentNode, "DocumentKind");
          var departmentId = GetNodeValue(documentNode, "Department");
          var validFromStr = GetNodeValue(documentNode, "ValidFrom");
          var validTillStr = GetNodeValue(documentNode, "ValidTill");
          var regDateStr = GetNodeValue(documentNode, "RegistrationDate");
          var regNumber = GetNodeValue(documentNode, "RegistrationNumber");
          var totalAmountStr = GetNodeValue(documentNode, "TotalAmount");
          var currencyCode = GetNodeValue(documentNode, "Currency");
          var vatApplicableStr = GetNodeValue(documentNode, "VATApplicable");
          var note = GetNodeValue(documentNode, "Note");

          
          
          // Проверка на дубликат
          var existing = Sungero.Contracts.ContractBases.GetAll()
            .FirstOrDefault(d => d.Note == externalId);
          if (existing != null)
            continue;

          var contract = Sungero.Contracts.ContractualDocuments.Create();
          contract.Name = name;
          contract.Subject = subject;
          contract.Note = externalId;

          // Вид документа
          int docKindInt;
          if (int.TryParse(docKindId, out docKindInt))
            contract.DocumentKind = Sungero.Docflow.DocumentKinds.GetAll().FirstOrDefault(k => k.Id == docKindInt);

          // Подразделение
          int deptInt;
          if (int.TryParse(departmentId, out deptInt))
            contract.Department = Sungero.Company.Departments.GetAll().FirstOrDefault(d => d.Id == deptInt);

          // Даты
          contract.ValidFrom = ParseDate(validFromStr);
          contract.ValidTill = ParseDate(validTillStr);
          contract.RegistrationDate = ParseDate(regDateStr);
          contract.RegistrationNumber = regNumber;

          
          var counterparty = null as Sungero.Parties.ICounterparty;

          if (!string.IsNullOrWhiteSpace(counterpartyInn))
          {
            try
            {
              counterparty = Sungero.Parties.Counterparties.GetAll()
                .FirstOrDefault(c => c.TIN == counterpartyInn.Trim());
            }
            catch (Exception ex)
            {
              Logger.Error($"Ошибка при поиске контрагента по ИНН '{counterpartyInn}': {ex.Message}");
            }
          }

          // Если контрагент не найден — создаём
          if (counterparty == null)
          {
            try
            {
              counterparty = Sungero.Parties.Counterparties.Create();
              counterparty.Name = firstName;
              
              if (!string.IsNullOrWhiteSpace(counterpartyInn))
                counterparty.TIN = counterpartyInn.Trim();

              counterparty.Save();
            }
            catch (Exception ex)
            {
              Logger.Error($"Ошибка при создании контрагента '{fullName}': {ex.Message}");
            }
          contract.Counterparty = counterparty;
            
          }
          
          
          
          // Валюта
          var currency = Sungero.Financials.Currencies.GetAll().FirstOrDefault(c => c.AlphaCode == currencyCode);
          if (currency != null)
            contract.Currency = currency;

    // Сумма
    //contract.TotalAmount = ParseDecimal(totalAmountStr);

    // НДС
    bool vatApplicable;
          if (bool.TryParse(vatApplicableStr, out vatApplicable))
            contract.vat = vatApplicable;

          contract.LifeCycleState = Sungero.Docflow.OfficialDocument.LifeCycleState.Draft;

          contract.Save();
        }
        catch (Exception ex)
        {
          Logger.Debug($"Ошибка при импорте договора: {ex.Message}");
        }
      }
      return errorList;
      
    }*/

    //string xmlPathFile = "C:\\RxData\\git_repository\\Contracts.xml"

    [Remote, Public]
    public List<string> ImportContractsFromXml(IContract contractParam, string xmlPathFile)
    {
      Logger.Debug("ImportContractsFromXml - Start");

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
            var isDepartment = int.Parse(documentElement.Element("Department")?.Value);
            var isResponsibleEmployee = int.Parse(documentElement.Element("ResponsibleEmployee")?.Value);
            var isAuthor = int.Parse(documentElement.Element("Author")?.Value);
            var isResponsibleAccountant = int.Parse(documentElement.Element("ResponsibleAccountant")?.Value);
            var isRBO = documentElement.Element("RBO")?.Value ?? "";
            var isValidFrom = documentElement.Element("ValidFrom")?.Value;
            var isValidTill = documentElement.Element("ValidTill")?.Value;
            var isChaneReason = documentElement.Element("ChangeReason")?.Value;
            var isAccountDebtCredit = documentElement.Element("AccountDebtCredt")?.Value;
            var isAccountFutureExpense = documentElement.Element("AccountFutureExpense")?.Value;
            var isTotalAmount = documentElement.Element("TotalAmount")?.Value;
            var isCurrency = documentElement.Element("Currency")?.Value;
            var isCurrencyOperation = documentElement.Element("CurrencyOperation")?.Value;
            var isVATApplicable = bool.Parse(documentElement.Element("VATApplicable")?.Value);
            var isVATRate = documentElement.Element("VATRate")?.Value;
            var isVATAmount = double.Parse(documentElement.Element("VATAmount")?.Value);
            var isIncomeTaxRate = documentElement.Element("IncomeTaxRate")?.Value;
            var isPaymentRegion = int.Parse(documentElement.Element("PaymentRegion")?.Value);
            var isPaymentTaxRegion = documentElement.Element("PaymentTaxRegion")?.Value;
            var isBatchProcessing = bool.Parse(documentElement.Element("BatchProcessing")?.Value);
            var isPaymentMethod = int.Parse(documentElement.Element("PaymentMethod")?.Value);
            var isPaymentFrequency = int.Parse(documentElement.Element("PaymentFrequency")?.Value);
            var isPaymentBasis = documentElement.Element("PaymentBasis");

            try
            {
              var contract = Eskhata.Contracts.GetAll().Where(x=>x.ExternalId == isExternalD).FirstOrDefault();
              if (contract != null)
                Logger.DebugFormat("contract with ExternalD:{0} was found. Id:{1}, Name:{2}", isExternalD, contract.Id, contract.Name);
              else
              {
                contract = Eskhata.Contracts.Create();
                contract.ExternalId = isExternalD;
                Logger.DebugFormat("Create new Contract with ExternalD:{0}. ID{1}", isExternalD, contract.Id);
              }

              var documentKind = litiko.Eskhata.DocumentKinds.GetAll().FirstOrDefault(k => k.ExternalIdlitiko == isDocumentKind);
              if(documentKind != null)
                Logger.DebugFormat("Document kind with ExternalId:{0} was found. Id{1}, Name:{2}", isDocumentKind, documentKind.Id, documentKind.Name);
              else
              {
                documentKind = litiko.Eskhata.DocumentKinds.Create();
                documentKind.ExternalIdlitiko = isDocumentKind;
                Logger.DebugFormat("Create new Document kind with ExternalId:{0}. ID{1}", isDocumentKind, documentKind.Id);
              }

              var documentGroup = litiko.Eskhata.DocumentGroupBases.GetAll().FirstOrDefault(d => d.ExternalIdlitiko == isDocumentGroup);
              if (documentGroup != null)
              {
                documentGroup = litiko.Eskhata.DocumentGroupBases.Create();
                documentGroup.ExternalIdlitiko = isDocumentGroup;
              }

              contract.Subject = isSubject;
              contract.Name = isName;
              
              
              
              
              
              
            var docExternal = documentElement.Element("ExternalD")?.Value?.Trim();
            var counterpartyElement = counterpartyElements
              .FirstOrDefault(c => string.Equals(c.Element("ExternalID")?.Value?.Trim(), docExternal, StringComparison.OrdinalIgnoreCase));

            string externalID = documentElement.Element("ExternalD")?.Value.Trim();
            if (string.IsNullOrWhiteSpace(externalID))
              throw AppliedCodeException.Create("ExternalD отсутствует в документе.");

            
            if (contract == null)
            {
              contract = Contracts.Create();
              contract.ExternalId = externalID;
              isNew = true;
              Logger.DebugFormat("Создаём новый договор ExternalD={0}", externalID);
            }
            else
            {
              Logger.DebugFormat("Найден существующий договор ExternalD={0}", externalID);
            }

            // Простые текстовые поля
            contract.Name = documentElement.Element("Name")?.Value;
            contract.Subject = documentElement.Element("Subject")?.Value;
            if (!string.IsNullOrWhiteSpace(isCounterpartySignatory))
            {
              int cps;
              if (int.TryParse(isCounterpartySignatory, out cps))
                contract.CounterpartySignatorylitiko = cps;
            }
            
            contract.RegistrationNumber = documentElement.Element("RegistrationNumber")?.Value;
            contract.Note = documentElement.Element("Note")?.Value;
            var changeReason = documentElement.Element("СhangeReason")?.Value ?? documentElement.Element("ChangeReason")?.Value;
            if (!string.IsNullOrWhiteSpace(changeReason))
              contract.ReasonForChangelitiko = changeReason;
            var rbo = documentElement.Element("RBO")?.Value;
            if (!string.IsNullOrWhiteSpace(rbo))
              contract.RBOlitiko = rbo;
            var accDebt = documentElement.Element("AccountDebtCredt")?.Value ?? documentElement.Element("AccountDebtCredit")?.Value;
            if (!string.IsNullOrWhiteSpace(accDebt))
              contract.AccDebtCreditlitiko = accDebt;
            var accFuture = documentElement.Element("AccountFutureExpense")?.Value;
            if (!string.IsNullOrWhiteSpace(accFuture))
              contract.AccFutureExpenselitiko = accFuture;
            var internalAcc = documentElement.Element("InternalAcc")?.Value;
            if (!string.IsNullOrWhiteSpace(internalAcc))
              contract.InternalAcclitiko = internalAcc;

            var validFromStr = documentElement.Element("ValidFrom")?.Value?.Trim();
            if (!string.IsNullOrWhiteSpace(validFromStr))
            {
              try
              {
                // Парсим строку формата dd.MM.yyyy
                DateTime df = DateTime.ParseExact(validFromStr, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
                contract.ValidFrom = Calendar.FromUserTime(df);
              }
              catch
              {
                Logger.DebugFormat("Не удалось распарсить ValidFrom: '{0}'", validFromStr);
              }
            }

            var validTillStr = documentElement.Element("ValidTill")?.Value?.Trim();
            if (!string.IsNullOrWhiteSpace(validTillStr))
            {
              try
              {
                DateTime dt = DateTime.ParseExact(validTillStr, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
                contract.ValidTill = Calendar.FromUserTime(dt);
              }
              catch
              {
                Logger.DebugFormat("Не удалось распарсить ValidTill: '{0}'", validTillStr);
              }
            }
            var regDateStr = documentElement.Element("RegistrationDate")?.Value?.Trim();
            if (!string.IsNullOrWhiteSpace(regDateStr))
            {
              DateTime rd;
              if (DateTime.TryParseExact(regDateStr, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out rd))
                contract.RegistrationDate = Calendar.FromUserTime(rd);
              else
                Logger.DebugFormat("Не удалось распарсить RegistrationDate: '{0}'", regDateStr);
            }

            // Сумма
            var totalAmountStr = documentElement.Element("TotalAmount")?.Value?.Trim();
            if (!string.IsNullOrWhiteSpace(totalAmountStr))
            {
              double total;
              if (double.TryParse(totalAmountStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out total))
                contract.TotalAmountlitiko = total;
              else
                Logger.DebugFormat("Не удалось распарсить TotalAmount: '{0}'", totalAmountStr);
            }
            // Валюта
            string currencyCode = documentElement.Element("Currency")?.Value?.Trim();
            if (!string.IsNullOrWhiteSpace(currencyCode))
            {
              var currency = Sungero.Commons.Currencies.GetAll().FirstOrDefault(c => string.Equals(c.AlphaCode, currencyCode, StringComparison.OrdinalIgnoreCase));
              if (currency != null)
                contract.CurrencyContractlitiko = currency;
              else
                Logger.DebugFormat("Валюта по коду '{0}' не найдена", currencyCode);
            }

            // Контрагент
            if (counterpartyElement != null)
            {
              // В текущей структуре counterpartyElement — это уже Person
              var cpExternalId = counterpartyElement.Element("ExternalID")?.Value?.Trim();
              var firstName = counterpartyElement.Element("FirstName")?.Value?.Trim();
              var lastName = counterpartyElement.Element("LastName")?.Value?.Trim();

              Sungero.Parties.ICounterparty counterparty = null;
              if (!string.IsNullOrWhiteSpace(cpExternalId))
              {
                counterparty = Sungero.Parties.Counterparties.GetAll()
                  .FirstOrDefault(c => c.ExternalId == cpExternalId);
              }

              if (counterparty == null)
              {
                var created = Sungero.Parties.Companies.Create();
                created.ExternalId = cpExternalId;
                created.Name = string.Format("{0} {1}", lastName, firstName).Trim();
                created.Save();
                counterparty = created;
                Logger.DebugFormat("Создан контрагент: {0}", created.Name);
              }

              contract.Counterparty = counterparty;
            }

            // Сохраняем договор
            contract.Save();

            if (isNew) countChanged++; else countNotChanged++;
            
          }
          catch (Exception ex)
          {
            var error = $"{ex.Message}";
            Logger.Error(error);
            errorList.Add(error);
            countErrors++;
          }
        }
      }
      
      catch (Exception e)
      {
        var error = "General error in load xml: " + e.Message;
        Logger.Error(error);
        errorList.Add(error);
      }

      Logger.DebugFormat("ImportContractsFromXML - End. CountAll {0}, Updated {1}, NotUpdated {2}, Errors {3}",
                         countAll, countChanged, countNotChanged, countErrors);
      Logger.Debug("ImportContractsFromXML - Finish");

      return errorList;
    }
    

    /*
public Sungero.CoreEntities.Shared.WorkingTimeCalendarState ParseDateSafe(string dateStr)
{
    if (string.IsNullOrWhiteSpace(dateStr))
        return null;

    DateTime dt;
    if (DateTime.TryParseExact(dateStr, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out dt))
        return dt;

    if (DateTime.TryParse(dateStr, out dt))
        return dt;

    Logger.Debug("Не удалось распарсить дату: '" + dateStr + "'");
    return null;
}
     */

  


    
    
    
    
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