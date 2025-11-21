using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Shared;

namespace litiko.Eskhata.Module.Contracts.Server
{
  partial class ModuleFunctions
  {
    // ============================================================
    // 1. ИМПОРТ
    // ============================================================
    [Remote, Public]
    public void ImportContractsFromXmlUI()
    {
      // 1. Путь должен быть АБСОЛЮТНЫМ
      var xmlPathFile = "Contracts500.xml";

      if (!System.IO.File.Exists(xmlPathFile))
        throw new Exception($"Файл {xmlPathFile} не найден на сервере!");

      var asyncHandler = litiko.Eskhata.Module.Contracts.AsyncHandlers.ImportContractsAsynclitiko.Create();
      asyncHandler.XmlFilePath = xmlPathFile;
      
      // ВАЖНО: Передаем ID пользователя, чтобы он получил уведомление
      asyncHandler.UserId = Users.Current.Id; 

      asyncHandler.ExecuteAsync();
    }

    // ============================================================
    // 2. УДАЛЕНИЕ (Вариант через Клиентский цикл)
    // Используйте эти два метода в ModuleClientFunctionsш
    // ============================================================

    /// <summary>
    /// Получить список ID для удаления
    /// </summary>
    [Remote, Public]
    public List<long> GetTestContractIds()
    {
      // Возвращаем List<long>, так как Id в RX длинные
      return Eskhata.Contracts.GetAll()
        .Where(c => c.Author != null && c.Author.Id == 60)
        .Select(c => c.Id)
        .ToList();
    }

    /// <summary>
    /// Удалить один договор по ID
    /// </summary>
    [Remote, Public]
    public void DeleteContractById(long id) // Исправил int на long
    {
      var contract = Eskhata.Contracts.GetAll().FirstOrDefault(c => c.Id == id);
      
      if (contract == null) return;

      // Если заблокирован - кидаем ошибку
      if (Locks.GetLockInfo(contract).IsLocked)
        throw new Exception($"Договор {id} заблокирован пользователем {Locks.GetLockInfo(contract).OwnerName}.");

      Eskhata.Contracts.Delete(contract);
    }
  }
}
/*[Remote, Public]
    public IResultImportXmlUI ImportContractsFromXmlUI()
    {
      //var result = Eskhata.Structures.Contracts.Contract.ResultImportXml.Create();
      var result = litiko.Eskhata.Module.Contracts.Structures.Module.ResultImportXmlUI.Create();
      result.Errors = new List<string>();
      result.ImportedCount = 0;
      result.TotalCount = 0;

      Logger.Debug("Import contracts from XML - Start");

      var xmlPathFile = "Contracts1.xml";

      if (!System.IO.File.Exists(xmlPathFile))
      {
        result.Errors.Add($"Файл '{xmlPathFile}' не найден");
        Logger.Error($"XML File {xmlPathFile} is not found");
        return result;
      }

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
      var contacts = litiko.Eskhata.Contacts.GetAll().ToList();
      var counterparties = Eskhata.Counterparties.GetAll()
        .Where(c => allCounterpartyIds.Contains(c.ExternalId))
        .ToList();

      var contracts = Eskhata.Contracts.GetAll().ToList();

      for (var i = 0; i < documentElements.Count; i++)
      {
        result.TotalCount++;
        var docXml = documentElements[i];

        try
        {
          // --- Парсинг базовых полей ---
          var xmlExternalId = docXml.Element("ExternalD")?.Value?.Trim();
          var xmlName = docXml.Element("Name")?.Value;

          if (string.IsNullOrWhiteSpace(xmlExternalId))
          {
            result.Errors.Add($"Документ №{i + 1} ({xmlName}): пропущен ExternalId.");
            continue;
          }

          // Проверка на дубликат (в памяти)
          if (existingContracts.Contains(xmlExternalId))
          {
            // Раскомментируйте лог ниже, если нужно видеть пропуски
            // Logger.Debug($"Contract {xmlExternalId} already exists. Skipping.");
            // result.Errors.Add($"Договор {xmlName} ({xmlExternalId}) уже существует.");
            continue;
          }

          // Создание сущности
          var contract = Eskhata.Contracts.Create();
          contract.ExternalId = xmlExternalId;
          contract.Name = !string.IsNullOrEmpty(xmlName) ? xmlName : "Без имени";

          // --- Заполнение справочных полей (Поиск в памяти) ---

          // Контрагент
          var cpExtId = docXml.Element("CounterpartyExternalId")?.Value?.Trim();
          if (!string.IsNullOrEmpty(cpExtId))
          {
            var cp = counterparties.FirstOrDefault(c => c.ExternalId == cpExtId);
            if (cp != null) contract.Counterparty = cp;
            else result.Errors.Add($"Договор {xmlExternalId}: Контрагент {cpExtId} не найден.");
          }

          // Вид документа
          var kindExtId = docXml.Element("DocumentKind")?.Value?.Trim();
          if (!string.IsNullOrEmpty(kindExtId))
          {
            var kind = docKinds.FirstOrDefault(k => k.ExternalIdlitiko == kindExtId);
            if (kind != null) contract.DocumentKind = kind;
            else result.Errors.Add($"Договор {xmlExternalId}: Вид {kindExtId} не найден.");
          }

          // Группа документов
          var groupExtId = docXml.Element("DocumentGroup")?.Value?.Trim();
          if (!string.IsNullOrEmpty(groupExtId))
          {
            var grp = docGroups.FirstOrDefault(g => g.ExternalIdlitiko == groupExtId);
            if (grp != null) contract.DocumentGroup = grp;
          }

          // Валюта (Currency)
          var curCode = docXml.Element("Currency")?.Value?.Trim();
          if (!string.IsNullOrEmpty(curCode))
          {
            // Ищем по коду или AlphaCode
            var cur = currencies.FirstOrDefault(c => c.AlphaCode == curCode || c.NumericCode == curCode);
            if (cur != null) contract.Currency = cur;
          }

          // Валюта операции (OperationCurrency)
          var curOpCode = docXml.Element("OperationCurrency")?.Value?.Trim();
          if (!string.IsNullOrEmpty(curOpCode))
          {
            var curOp = currencies.FirstOrDefault(c => c.AlphaCode == curOpCode || c.NumericCode == curOpCode);
            if (curOp != null) contract.CurrencyOperationlitiko = curOp;
          }

          // Подразделение (Department)
          var depCode = docXml.Element("Department")?.Value?.Trim();
          if (!string.IsNullOrEmpty(depCode))
          {
            var dep = departments.FirstOrDefault(d => d.ExternalCodelitiko == depCode);
            if (dep != null) contract.Department = dep;
          }

          // Сотрудники (Responsible, Author)
          var respId = docXml.Element("ResponsibleEmployee")?.Value?.Trim();
          if (!string.IsNullOrEmpty(respId))
            contract.ResponsibleEmployee = employees.FirstOrDefault(e => e.ExternalId == respId);

          var authId = docXml.Element("Author")?.Value?.Trim();
          if (!string.IsNullOrEmpty(authId))
            contract.Author = employees.FirstOrDefault(e => e.ExternalId == authId);

          // Подписант (CounterpartySignatory) - ищем в Контактах
          var signId = docXml.Element("CounterpartySignatory")?.Value?.Trim();
          if (!string.IsNullOrEmpty(signId))
            contract.CounterpartySignatory = contacts.FirstOrDefault(c => c.ExternalIdlitiko == signId);

          // Регионы
          var payRegId = docXml.Element("PaymentRegion")?.Value?.Trim();
          if (!string.IsNullOrEmpty(payRegId))
            contract.PaymentRegionlitiko =
              paymentRegions.FirstOrDefault(r => r.ExternalId == payRegId || r.Code == payRegId);

          var rentRegId = docXml.Element("PaymentTaxRegion")?.Value?.Trim();
          if (!string.IsNullOrEmpty(rentRegId))
            contract.RegionOfRentallitiko =
              rentRegions.FirstOrDefault(r => r.ExternalId == rentRegId || r.Code == rentRegId);

          // Периодичность (PaymentFrequency)
          var freqName = docXml.Element("PaymentFrequency")?.Value?.Trim();
          if (!string.IsNullOrEmpty(freqName))
            contract.FrequencyOfPaymentlitiko = frequencies.FirstOrDefault(f => f.Name == freqName);

          // --- Простые поля ---
          contract.Subject = docXml.Element("Subject")?.Value?.Trim();
          contract.RBOlitiko = docXml.Element("RBO")?.Value ?? "";
          contract.ReasonForChangelitiko = docXml.Element("ChangeReason")?.Value?.Trim();
          contract.AccDebtCreditlitiko = docXml.Element("AccountDebtCredt")?.Value?.Trim();
          contract.AccFutureExpenselitiko = docXml.Element("AccountFutureExpense")?.Value?.Trim();
          contract.RegistrationNumber = docXml.Element("RegistrationNumber")?.Value?.Trim();
          contract.Note = docXml.Element("Note")?.Value?.Trim();

          // Числа
          contract.TotalAmountlitiko = ParseDoubleSafe(docXml.Element("TotalAmount")?.Value);
          contract.VatRatelitiko = ParseDoubleSafe(docXml.Element("VATRate")?.Value);
          contract.VatAmount = ParseDoubleSafe(docXml.Element("VATAmount")?.Value);
          contract.IncomeTaxRatelitiko = ParseDoubleSafe(docXml.Element("IncomeTaxRate")?.Value);
          contract.AmountForPeriodlitiko = ParseDoubleSafe(docXml.Element("AmountForPeriod")?.Value);

          // Даты
          contract.ValidFrom = TryParseDate(docXml.Element("ValidFrom")?.Value);
          contract.ValidTill = TryParseDate(docXml.Element("ValidTill")?.Value);
          contract.RegistrationDate = TryParseDate(docXml.Element("RegistrationDate")?.Value);

          // Булевы
          contract.IsVATlitiko = ParseBoolSafe(docXml.Element("VATApplicable")?.Value);
          contract.IsPartialPaymentlitiko = ParseBoolSafe(docXml.Element("IsPartialPayment")?.Value);
          contract.IsEqualPaymentlitiko = ParseBoolSafe(docXml.Element("IsEqualPayment")?.Value);

          // Перечисления (Enums)
          var payMethod = docXml.Element("PaymentMethod")?.Value?.Trim();
          if (!string.IsNullOrEmpty(payMethod))
          {
            if (payMethod == "Предоплата")
              contract.PaymentMethodlitiko = litiko.Eskhata.Contract.PaymentMethodlitiko.Prepayment;
            else if (payMethod == "Постоплата")
              contract.PaymentMethodlitiko = litiko.Eskhata.Contract.PaymentMethodlitiko.Postpay;
          }

          // СОХРАНЕНИЕ
          contract.Save();

          // Добавляем в кеш, чтобы избежать дублей внутри одного XML файла
          existingContracts.Add(xmlExternalId);

          result.ImportedCount++;

          // Логируем кратко, чтобы не забивать память
          if (result.ImportedCount % 50 == 0)
            Logger.Debug($"Imported {result.ImportedCount} contracts...");
        }
        catch (Exception ex)
        {
          // Ловим ошибку конкретного договора и идем дальше!
          var err = $"Ошибка импорта договора №{i + 1}: {ex.Message}";
          Logger.Error(err); // Можно добавить ex.StackTrace для дебага
          result.Errors.Add(err);
          // НЕТ throw - цикл продолжается
        }
      }

      Logger.Debug(
        $"Import finished. Total: {result.TotalCount}, Imported: {result.ImportedCount}, Errors: {result.Errors.Count}");
      return result;
    }

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
      if (double.TryParse(value.Trim().Replace(',', '.'), System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out r))
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

/* private IContract ParseContract(XElement documentElement, IResultImportXmlUI result)
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
 }*/

// --- вспомогательные функции ---



