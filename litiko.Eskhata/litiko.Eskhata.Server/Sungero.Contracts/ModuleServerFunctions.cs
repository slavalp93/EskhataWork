using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using litiko.Eskhata.Module.Contracts.Structures.Module;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Shared;

namespace litiko.Eskhata.Module.Contracts.Server
{
  partial class ModuleFunctions
  {
    /// <summary>
    /// Импорт договоров из XML (Синхронно, передача через Base64)
    /// </summary>
    [Remote, Public]
    public IResultImportXmlUI ImportContractsFromXmlUI(string fileBase64, string fileName)
    {
      var result = litiko.Eskhata.Module.Contracts.Structures.Module.ResultImportXmlUI.Create();
      result.Errors = new List<string>();
      result.ImportedCount = 0;
      result.TotalCount = 0;
      result.DuplicateCount = 0;

      Logger.DebugFormat("Start synchronous import from file: {0}", fileName);

      byte[] fileBytes;
      try
      {
        if (string.IsNullOrEmpty(fileBase64))
          throw new Exception("Переданы пустые данные файла.");
        fileBytes = Convert.FromBase64String(fileBase64);
      }
      catch (Exception ex)
      {
        result.Errors.Add($"Ошибка чтения файла (Base64): {ex.Message}");
        return result;
      }

      try
      {
        XDocument xDoc;
        using (var stream = new MemoryStream(fileBytes))
        {
          xDoc = XDocument.Load(stream);
        }

        var dataElements = xDoc.Element("Data");
        if (dataElements == null)
        {
          result.Errors.Add("Корневой элемент <Data> отсутствует в XML.");
          return result;
        }

        var documentElements = dataElements.Elements("Document").ToList();
        Logger.Debug($"Found {documentElements.Count} documents. Pre-loading cache...");

        // =================================================================================
        // ЭТАП 1: КЕШИРОВАНИЕ ДАННЫХ (ОПТИМИЗАЦИЯ)
        // Загружаем справочники в память 1 раз, чтобы не дергать БД
        // =================================================================================
        
        var xmlExternalIds = documentElements
          .Select(x => x.Element("ExternalD")?.Value?.Trim())
          .Where(x => !string.IsNullOrEmpty(x))
          .Distinct().ToList();
        
        // Собираем ExternalID контрагентов
        var xmlCounterpartyIds = documentElements
          .Select(x => x.Element("CounterpartyExternalId")?.Value?.Trim())
          .Where(x => !string.IsNullOrEmpty(x))
          .Distinct().ToList();

        // Загружаем ID существующих договоров для мгновенного поиска
        var existingContractsExtIds = Eskhata.Contracts.GetAll()
          .Where(x => xmlExternalIds.Contains(x.ExternalId))
          .Select(x => x.ExternalId)
          .ToList();

        // Загружаем справочники в списки (In-Memory Cache)
        var currencies = litiko.Eskhata.Currencies.GetAll().ToList();
        var docKinds = litiko.Eskhata.DocumentKinds.GetAll().ToList();
        var docGroups = litiko.Eskhata.DocumentGroupBases.GetAll().ToList();
        var departments = litiko.Eskhata.Departments.GetAll().ToList();
        var employees = litiko.Eskhata.Employees.GetAll().ToList();
        var paymentRegions = litiko.NSI.PaymentRegions.GetAll().ToList();
        var rentRegions = litiko.NSI.RegionOfRentals.GetAll().ToList();
        var frequencies = litiko.NSI.FrequencyOfPayments.GetAll().ToList();
        var contacts = Eskhata.Contacts.GetAll().ToList();

        // Грузим только нужных контрагентов
        var counterparties = Eskhata.Counterparties.GetAll()
          .Where(c => xmlCounterpartyIds.Contains(c.ExternalId))
          .ToList();

        var banks = litiko.Eskhata.Banks.GetAll()
          .Where(b => xmlCounterpartyIds.Contains(b.ExternalId))
          .ToList();

        // =================================================================================
        // ЭТАП 2: ЦИКЛ ОБРАБОТКИ
        // =================================================================================
        
        for (var i = 0; i < documentElements.Count; i++)
        {
          result.TotalCount++;
          var docXml = documentElements[i];
          var extId = docXml.Element("ExternalD")?.Value?.Trim();

          try
          {
            
            // Валидация ID
            if (string.IsNullOrEmpty(extId))
            {
              result.Errors.Add($"Документ №{i+1}: Отсутствует ExternalD");
              continue;
            }
            
            // Проверка на дубликат
            if (existingContractsExtIds.Contains(extId))
            {
              result.DuplicateCount++;
              Logger.Debug($"Дубликат пропущен: {extId}");
              continue;
            }

            // ВЫЗОВ ОПТИМИЗИРОВАННОГО МЕТОДА
            // Мы передаем туда все загруженные списки
            var contract = ParseContractOptimized(docXml, result,
                                                  currencies, docKinds, docGroups, departments,
                                                  employees, contacts, paymentRegions, rentRegions,
                                                  frequencies, counterparties, banks);
            
            
            
            if (contract != null)
            {
              result.ImportedCount++;
              existingContractsExtIds.Add(contract.ExternalId);
              Logger.Debug($"Импортирован: {contract.Name}");
            }
          }
          catch (Exception ex)
          {
            var docIdentifier = !string.IsNullOrEmpty(extId) ? $"ExtID: {extId}" : $"№{i + 1}";
            var errMsg = $"Ошибка в документе {docIdentifier}: {ex.Message}";
            result.Errors.Add(errMsg);
            Logger.Error(errMsg);
          }
        }
      }
      catch (Exception ex)
      {
        Logger.Error($"Critical Import Error: {ex.Message}");
        result.Errors.Add($"Критическая ошибка: {ex.Message}");
      }
      
      Logger.DebugFormat("End synchronous import from file: {0}", fileName);

      return result;
    }

    /// <summary>
    /// Оптимизированный парсинг. Ищет данные в переданных списках, а не в БД.
    /// </summary>
    private IContract ParseContractOptimized(XElement docXml, IResultImportXmlUI result,
                                             List<litiko.Eskhata.ICurrency> currencies,
                                             List<litiko.Eskhata.IDocumentKind> docKinds,
                                             List<litiko.Eskhata.IDocumentGroupBase> docGroups,
                                             List<litiko.Eskhata.IDepartment> departments,
                                             List<litiko.Eskhata.IEmployee> employees,
                                             List<litiko.Eskhata.IContact> contacts,
                                             List<litiko.NSI.IPaymentRegion> payRegions,
                                             List<litiko.NSI.IRegionOfRental> rentRegions,
                                             List<litiko.NSI.IFrequencyOfPayment> frequencies,
                                             List<litiko.Eskhata.ICounterparty> counterparties,
                                             List<litiko.Eskhata.IBank> banks)
    {
      var externalId = docXml.Element("ExternalD")?.Value?.Trim();
      
      var name = docXml.Element("Name")?.Value;
      
      // Проверка Контрагента
      litiko.Eskhata.ICounterparty foundCounterparty = null;
      var counterpartyExternalId = docXml.Element("CounterpartyExternalId")?.Value?.Trim();
      
      if (!string.IsNullOrEmpty(counterpartyExternalId))
      {
        // А) Ищем в общем списке контрагентов
        foundCounterparty = counterparties.FirstOrDefault(c => c.ExternalId == counterpartyExternalId);
        
        // Б) Если не нашли, ищем в Банках
        if (foundCounterparty == null)
        {
          var foundBank = banks.FirstOrDefault(b => b.ExternalId == counterpartyExternalId);
          if (foundBank != null)
          {
            foundCounterparty = foundBank;
          }
        }

        if (foundCounterparty == null)
        {
          result.Errors.Add($"Договор {externalId}: Контрагент/Банк с ID '{counterpartyExternalId}' не найден.");
          return null;
        }
      }
      else
      {
        result.Errors.Add($"Договор {externalId}: Не указан ID контрагента.");
        return null;
      }

      litiko.Eskhata.IDocumentKind foundDocumentKind = null;
      var documentKindId = docXml.Element("DocumentKind")?.Value?.Trim();
      if (!string.IsNullOrEmpty(documentKindId))
      {
        foundDocumentKind = docKinds.FirstOrDefault(k => k.ExternalIdlitiko == documentKindId);
        if (foundDocumentKind == null)
        {
          // КРИТИЧЕСКАЯ ОШИБКА: Нет вида документа -> Нельзя создать договор
          result.Errors.Add($"Договор {externalId}: Вид документа '{documentKindId}' не найден.");
          return null;
        }
      }
      else
      {
        // Если вид обязателен
        result.Errors.Add($"Договор {externalId}: Не указан Вид документа.");
        return null;
      }


      var contract = Eskhata.Contracts.Create();
      contract.ExternalId = externalId;
      contract.Name = !string.IsNullOrEmpty(name) ? name : "Без имени";

      contract.Counterparty = foundCounterparty;
      contract.DocumentKind = foundDocumentKind;
      
      // Контрагент
      /*cpId = docXml.Element("CounterpartyExternalId")?.Value?.Trim();
      if(!string.IsNullOrEmpty(cpId))
      {
        var cp = counterparties.FirstOrDefault(c => c.ExternalId == cpId);
        if (cp != null) contract.Counterparty = cp;
        else result.Errors.Add($"Договор {extId}: Контрагент {cpId} не найден.");
      }

      // Вид документа
      var kindId = docXml.Element("DocumentKind")?.Value?.Trim();
      if(!string.IsNullOrEmpty(kindId))
      {
        var kind = docKinds.FirstOrDefault(k => k.ExternalIdlitiko == kindId);
        if (kind != null) contract.DocumentKind = kind;
        else result.Errors.Add($"Договор {extId}: Вид {kindId} не найден.");
      }*/

      // Группа
      var documentGroupId = docXml.Element("DocumentGroup")?.Value?.Trim();
      if(!string.IsNullOrEmpty(documentGroupId))
        contract.DocumentGroup = docGroups.FirstOrDefault(g => g.ExternalIdlitiko == documentGroupId);

      // Валюта
      var currency = docXml.Element("Currency")?.Value?.Trim();
      if (!string.IsNullOrEmpty(currency))
        contract.Currency = currencies.FirstOrDefault(c => c.AlphaCode == currency || c.NumericCode == currency);

      // Валюта операции
      var curOpCode = docXml.Element("OperationCurrency")?.Value?.Trim(); // если пусто, то будет TJS логика реализована в АБС при выгрузке
      if (!string.IsNullOrEmpty(curOpCode))
        contract.CurrencyOperationlitiko = currencies.FirstOrDefault(c => c.AlphaCode == curOpCode || c.NumericCode == curOpCode);

      //  Сначала ищем ОТВЕТСТВЕННОГО
      litiko.Eskhata.IEmployee responsibleObj = null;
      var empId = docXml.Element("ResponsibleEmployee")?.Value?.Trim();
      
      if (!string.IsNullOrEmpty(empId))
      {
        responsibleObj = employees.FirstOrDefault(e => e.ExternalId == empId);
        if (responsibleObj != null)
        {
          contract.ResponsibleEmployee = responsibleObj;
        }
      }

      var authId = docXml.Element("Author")?.Value?.Trim();

      if (!string.IsNullOrEmpty(authId))
      {
        var authorObj = employees.FirstOrDefault(e => e.ExternalId == authId);
        if (authorObj != null)
        {
          contract.Author = authorObj;
        }
      }
      else
      {
        if (responsibleObj != null)
        {
          contract.Author = responsibleObj;
        }
      }
      
      // Подразделение
      var department = docXml.Element("Department")?.Value?.Trim();
      if(!string.IsNullOrEmpty(department))
        contract.Department = departments.FirstOrDefault(d => d.ExternalId == department);

      // Подписант
      var counterpartySignatory = docXml.Element("CounterpartySignatory")?.Value?.Trim();
      if(!string.IsNullOrEmpty(counterpartySignatory))
        contract.CounterpartySignatory = contacts.FirstOrDefault(c => c.ExternalIdlitiko == counterpartySignatory);
      
      // Регионы
      var paymentRegion = docXml.Element("PaymentRegion")?.Value?.Trim();
      if (!string.IsNullOrEmpty(paymentRegion))
        contract.PaymentRegionlitiko = payRegions.FirstOrDefault(r => r.ExternalId == paymentRegion || r.Code == paymentRegion);
      
      var rentRegId = docXml.Element("PaymentTaxRegion")?.Value?.Trim();
      if (!string.IsNullOrEmpty(rentRegId))
        contract.RegionOfRentallitiko = rentRegions.FirstOrDefault(r => r.ExternalId == rentRegId || r.Code == rentRegId);

      // Периодичность
      var freqName = docXml.Element("PaymentFrequency")?.Value?.Trim();
      if (!string.IsNullOrEmpty(freqName))
        contract.FrequencyOfPaymentlitiko = frequencies.FirstOrDefault(f => f.Name == freqName);
      
      // Заполнение ПРОСТЫХ полей
      contract.Subject = docXml.Element("Subject")?.Value;
      contract.RBOlitiko = docXml.Element("RBO")?.Value;
      contract.ReasonForChangelitiko = docXml.Element("ChangeReason")?.Value;

      // contract.CorrAcc это поле берется из карточки контрагента
      contract.Note = docXml.Element("Note")?.Value;
      contract.RegistrationNumber = docXml.Element("RegistrationNumber")?.Value;

      // Числа
      contract.TotalAmountlitiko = ParseDoubleSafe(docXml.Element("TotalAmount")?.Value);
      contract.VatRatelitiko = ParseDoubleSafe(docXml.Element("VATRate")?.Value);
      contract.VatAmount = ParseDoubleSafe(docXml.Element("VATAmount")?.Value);
      contract.IncomeTaxRatelitiko = ParseDoubleSafe(docXml.Element("IncomeTaxRate")?.Value);
      contract.AmountForPeriodlitiko = ParseDoubleSafe(docXml.Element("AmountForPeriod")?.Value);

      // Булевы
      contract.IsVATlitiko = ParseBoolSafe(docXml.Element("VATApplicable")?.Value);
      contract.IsPartialPaymentlitiko = ParseBoolSafe(docXml.Element("IsPartialPayment")?.Value);
      contract.IsEqualPaymentlitiko = ParseBoolSafe(docXml.Element("IsEqualPayment")?.Value);

      // Даты
      contract.ValidFrom = TryParseDate(docXml.Element("ValidFrom")?.Value);
      contract.ValidTill = TryParseDate(docXml.Element("ValidTill")?.Value);
      contract.RegistrationDate = TryParseDate(docXml.Element("RegistrationDate")?.Value);

      var rawAccount = docXml.Element("AccountDebtCredt")?.Value?.Trim();
      var xmlPayMethod = docXml.Element("PaymentMethod")?.Value?.Trim();
      
      if (!string.IsNullOrEmpty(rawAccount))
      {
        if (rawAccount.StartsWith("17"))
        {
          contract.PaymentMethodlitiko = litiko.Eskhata.Contract.PaymentMethodlitiko.Prepayment;
        }
        else if (rawAccount.StartsWith("26"))
        {
          contract.PaymentMethodlitiko = litiko.Eskhata.Contract.PaymentMethodlitiko.Postpay;
        }
        else
        {
          if (xmlPayMethod == "Предоплата") contract.PaymentMethodlitiko = litiko.Eskhata.Contract.PaymentMethodlitiko.Prepayment;
          else if (xmlPayMethod == "Постоплата") contract.PaymentMethodlitiko = litiko.Eskhata.Contract.PaymentMethodlitiko.Postpay;
        }
      }
      else
      {
        if (xmlPayMethod == "Предоплата") contract.PaymentMethodlitiko = litiko.Eskhata.Contract.PaymentMethodlitiko.Prepayment;
        else if (xmlPayMethod == "Постоплата") contract.PaymentMethodlitiko = litiko.Eskhata.Contract.PaymentMethodlitiko.Postpay;
      }

      contract.Save();

      bool needSecondSave = false;

      if (!string.IsNullOrEmpty(rawAccount))
      {
        if (rawAccount.StartsWith("26"))
        {
          if (contract.AccDebtCreditlitiko != rawAccount)
          {
            contract.AccDebtCreditlitiko = rawAccount;
            contract.AccFutureExpenselitiko = null;
            needSecondSave = true;
          }
        }
        else if (rawAccount.StartsWith("17"))
        {
          if (contract.AccFutureExpenselitiko != rawAccount)
          {
            contract.AccFutureExpenselitiko = rawAccount;
            contract.AccDebtCreditlitiko = null;
            needSecondSave = true;
          }
        }
        else
        {
          if (contract.AccDebtCreditlitiko != rawAccount)
          {
            contract.AccDebtCreditlitiko = rawAccount;
            needSecondSave = true;
          }
        }
      }

      if (needSecondSave)
      {
        Logger.Debug($"Applying account '{rawAccount}' via second save.");
        contract.Save();
      }

      Logger.Debug($"Prepared Contract data: " +
                   $"ID: {contract.Id}| " +
                   $"ExternalID: {contract.ExternalId}| " +
                   $"DocumentKind: {contract.DocumentKind}| " +
                   $"DocumentGroup: {contract.DocumentGroup}| " +
                   $"Subject: {contract.Subject}| " +
                   $"Name: {contract.Name}| " +
                   $"CounterpartySignatory: {contract.CounterpartySignatory}| " +
                   $"Department: {contract.Department}| " +
                   $"ResponsibleEmployee: {contract.ResponsibleEmployee}| " +
                   $"Author: {contract.Author}| " +
                   $"RBO: {contract.RBOlitiko}| " +
                   $"ValidFrom: {contract.ValidFrom:dd.MM.yyyy}| " +
                   $"ValidTill: {contract.ValidTill:dd.MM.yyyy}| " +
                   $"ChangeReason: {contract.ReasonForChangelitiko}| " +
                   $"AccountDebtCredt: {contract.AccDebtCreditlitiko}| " +
                   $"AccountFutureExpense: {contract.AccFutureExpenselitiko}| " +
                   $"TotalAmount: {contract.TotalAmountlitiko}| " +
                   $"Currency: {contract.Currency} " +
                   $"OperationCurrency: {contract.CurrencyOperationlitiko}| " +
                   $"VATApplicable: {contract.IsVATlitiko}| " +
                   $"VATRate: {contract.VatRatelitiko}| " +
                   $"VATAmount: {contract.VatAmount}| " +
                   $"IncomeTaxRate: {contract.IncomeTaxRatelitiko}| " +
                   $"PaymentRegion: {contract.PaymentRegionlitiko}| " +
                   $"PaymentTaxRegion: {contract.RegionOfRentallitiko}| " +
                   $"PaymentMethod: {contract.PaymentMethodlitiko}| " +
                   $"PaymentFrequency: {contract.FrequencyOfPaymentlitiko}| " +
                   $"IsPartialPayment: {contract.IsPartialPaymentlitiko}| " +
                   $"IsEqual: {contract.IsEqualPaymentlitiko}| " +
                   $"AmountForPeriod: {contract.AmountForPeriodlitiko}| " +
                   $"Note: {contract.Note}| " +
                   $"RegistrationNumber: {contract.RegistrationNumber}| " +
                   $"RegistrationDate: {contract.RegistrationDate:dd.MM.yyyy}| " +
                   $"CounterpartyExternalId: {contract.Counterparty}| ");
      
      return contract;
    }

    private static DateTime? TryParseDate(string date)
    {
      if (string.IsNullOrWhiteSpace(date))
        return null;

      DateTime result;

      string[] formats = { "dd.MM.yyyy", "d.MM.yyyy", "dd.M.yyyy", "d.M.yyyy" };

      if (DateTime.TryParseExact(date, formats,System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None,out result))
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
      if (string.IsNullOrWhiteSpace(value)) return false;
      var norm = value.Trim().ToLowerInvariant();
      return norm == "1" || norm == "true" || norm == "yes" || norm == "да";
    }

    /// <summary>
    /// Поиск ID договоров по ключевому слову (Имя, Тема или ExternalId)
    /// </summary>
    [Remote, Public]
    public List<long> GetContractIdsByKeyword(string keyword)
    {
      if (string.IsNullOrWhiteSpace(keyword))
        return new List<long>();

      return Eskhata.Contracts.GetAll()
        .Where(c => (c.Name != null && c.Name.Contains(keyword)) ||
               (c.Subject != null && c.Subject.Contains(keyword)) ||
               (c.ExternalId != null && c.ExternalId.Contains(keyword)))
        .Select(c => c.Id)
        .ToList();
    }

    [Remote, Public]
    public void DeleteContractById(long id)
    {
      var contract = Eskhata.Contracts.GetAll().FirstOrDefault(c => c.Id == id);
      if (contract == null) return;

      if (Locks.GetLockInfo(contract).IsLocked)
        Locks.Unlock(contract);

      Eskhata.Contracts.Delete(contract);
    }
  }
}