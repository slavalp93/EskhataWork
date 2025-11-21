using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Shared;

namespace litiko.Eskhata.Module.Contracts.Server
{
  public partial class ModuleAsyncHandlers
  {
    // ВНИМАНИЕ: Убедитесь, что в Development Studio у обработчика ImportContractsAsync
    // создан строковый параметр XmlFilePath.
    
    public virtual void ImportContractsAsynclitiko(litiko.Eskhata.Module.Contracts.Server.AsyncHandlerInvokeArgs.ImportContractsAsynclitikoInvokeArgs args)
    {
      Logger.Debug("AsyncHandler: ImportContractsAsync started.");
      
      var filePath = args.XmlFilePath;
      var user = Sungero.CoreEntities.Users.GetAll().FirstOrDefault(u => u.Id == args.UserId);

      // 1. Валидация файла
      if (!System.IO.File.Exists(filePath))
      {
        var err = $"Файл импорта не найден по пути: {filePath}";
        Logger.Error(err);
        SendNotice("Ошибка импорта договоров", err, user);
        return; // Завершаем работу, так как файла нет
      }

      XDocument xDoc;
      try
      {
        xDoc = XDocument.Load(filePath);
      }
      catch (Exception ex)
      {
        var err = $"Не удалось прочитать XML файл. Ошибка: {ex.Message}";
        Logger.Error(err);
        SendNotice("Ошибка импорта договоров", err, user);
        return;
      }

      var dataElement = xDoc.Element("Data");
      if (dataElement == null)
      {
        SendNotice("Ошибка импорта", "В XML отсутствует корневой узел <Data>.", user);
        return;
      }

      var documentElements = dataElement.Elements("Document").ToList();
      if (!documentElements.Any())
      {
        SendNotice("Импорт завершен", "Файл пуст, договоры не найдены.", user);
        return;
      }

      // ======================================================================================
      // ЭТАП 1: ПРЕДЗАГРУЗКА ДАННЫХ (КЕШИРОВАНИЕ)
      // Загружаем справочники в память ОДИН РАЗ, чтобы не делать запросы в цикле.
      // ======================================================================================
      Logger.Debug("AsyncHandler: Pre-loading reference data...");

      // Собираем все ExternalD из файла, чтобы проверить дубликаты
      var xmlExternalIds = documentElements
        .Select(x => x.Element("ExternalD")?.Value?.Trim())
        .Where(x => !string.IsNullOrEmpty(x))
        .Distinct()
        .ToList();

      var xmlCounterpartyIds = documentElements
        .Select(x => x.Element("CounterpartyExternalId")?.Value?.Trim())
        .Where(x => !string.IsNullOrEmpty(x))
        .Distinct()
        .ToList();

      // Загружаем существующие договоры (только ID), чтобы пропускать дубли
      // Используем HashSet для мгновенного поиска
      var existingContractsIds = Eskhata.Contracts.GetAll()
        .Where(x => xmlExternalIds.Contains(x.ExternalId))
        .Select(x => x.ExternalId)
        .ToList();

      // Загружаем справочники
      var currencies = litiko.Eskhata.Currencies.GetAll().ToList();
      var docKinds = litiko.Eskhata.DocumentKinds.GetAll().ToList();
      var docGroups = litiko.Eskhata.DocumentGroupBases.GetAll().ToList();
      var departments = litiko.Eskhata.Departments.GetAll().ToList();
      // Если сотрудников слишком много (>5000), можно оптимизировать фильтрацией, но для асинхронника 5-10к норм.
      var employees = litiko.Eskhata.Employees.GetAll().ToList(); 
      var contacts = Eskhata.Contacts.GetAll().ToList();
      
      var paymentRegions = litiko.NSI.PaymentRegions.GetAll().ToList();
      var rentRegions = litiko.NSI.RegionOfRentals.GetAll().ToList();
      var frequencies = litiko.NSI.FrequencyOfPayments.GetAll().ToList();

      // Грузим только нужных контрагентов
      var counterparties = Eskhata.Counterparties.GetAll()
        .Where(c => xmlCounterpartyIds.Contains(c.ExternalId))
        .ToList();

      Logger.Debug("AsyncHandler: Data loaded. Starting processing loop...");

      // Статистика
      int total = documentElements.Count;
      int imported = 0;
      int errorsCount = 0;
      int skipped = 0;
      var errorMessages = new List<string>();

      // ======================================================================================
      // ЭТАП 2: ЦИКЛ С МИКРО-ТРАНЗАКЦИЯМИ
      // ======================================================================================
      
      for (int i = 0; i < total; i++)
      {
        var docXml = documentElements[i];
        var xmlExtId = docXml.Element("ExternalD")?.Value?.Trim();
        var xmlName = docXml.Element("Name")?.Value ?? "No Name";

        // Проверка на пустоту ID
        if (string.IsNullOrEmpty(xmlExtId))
        {
          errorsCount++;
          errorMessages.Add($"Row {i+1}: Empty ExternalD");
          continue;
        }

        // Проверка на дубликат
        if (existingContractsIds.Contains(xmlExtId))
        {
          skipped++;
          continue;
        }

        try
        {
          // --- ТРАНЗАКЦИЯ НА ОДИН ДОКУМЕНТ ---
          // Sungero.Domain.Transactions.Execute создает изолированную транзакцию.
          // Это позволяет серверу сбрасывать блокировки и освобождать память после каждого договора.
          Transactions.Execute(() =>
          {
            var contract = Eskhata.Contracts.Create();
            contract.ExternalId = xmlExtId;
            contract.Name = xmlName;

            // --- Заполнение ссылочных полей из КЕША (без запросов к БД) ---

            // Контрагент
            var cpId = docXml.Element("CounterpartyExternalId")?.Value?.Trim();
            if (!string.IsNullOrEmpty(cpId))
              contract.Counterparty = counterparties.FirstOrDefault(c => c.ExternalId == cpId);

            // Вид документа
            var kindId = docXml.Element("DocumentKind")?.Value?.Trim();
            if (!string.IsNullOrEmpty(kindId))
              contract.DocumentKind = docKinds.FirstOrDefault(k => k.ExternalIdlitiko == kindId);

            // Группа документов
            var grpId = docXml.Element("DocumentGroup")?.Value?.Trim();
            if (!string.IsNullOrEmpty(grpId))
              contract.DocumentGroup = docGroups.FirstOrDefault(g => g.ExternalIdlitiko == grpId);

            // Валюта
            var curCode = docXml.Element("Currency")?.Value?.Trim();
            if (!string.IsNullOrEmpty(curCode))
              contract.Currency = currencies.FirstOrDefault(c => c.AlphaCode == curCode || c.NumericCode == curCode);
            
            var curOpCode = docXml.Element("OperationCurrency")?.Value?.Trim();
            if (!string.IsNullOrEmpty(curOpCode))
              contract.CurrencyOperationlitiko = currencies.FirstOrDefault(c => c.AlphaCode == curOpCode || c.NumericCode == curOpCode);

            // Подразделение
            var depCode = docXml.Element("Department")?.Value?.Trim();
            if (!string.IsNullOrEmpty(depCode))
              contract.Department = departments.FirstOrDefault(d => d.ExternalCodelitiko == depCode);

            // Сотрудники
            var respId = docXml.Element("ResponsibleEmployee")?.Value?.Trim();
            if (!string.IsNullOrEmpty(respId))
              contract.ResponsibleEmployee = employees.FirstOrDefault(e => e.ExternalId == respId);

            var authId = docXml.Element("Author")?.Value?.Trim();
            if (!string.IsNullOrEmpty(authId))
              contract.Author = employees.FirstOrDefault(e => e.ExternalId == authId);

            // Подписант
            var signId = docXml.Element("CounterpartySignatory")?.Value?.Trim();
            if (!string.IsNullOrEmpty(signId))
              contract.CounterpartySignatory = contacts.FirstOrDefault(c => c.ExternalIdlitiko == signId);
              
            // Регионы и Периодичность
            var payRegId = docXml.Element("PaymentRegion")?.Value?.Trim();
            if (!string.IsNullOrEmpty(payRegId))
              contract.PaymentRegionlitiko = paymentRegions.FirstOrDefault(r => r.ExternalId == payRegId || r.Code == payRegId);
              
            var rentRegId = docXml.Element("PaymentTaxRegion")?.Value?.Trim();
            if (!string.IsNullOrEmpty(rentRegId))
              contract.RegionOfRentallitiko = rentRegions.FirstOrDefault(r => r.ExternalId == rentRegId || r.Code == rentRegId);
              
            var freqName = docXml.Element("PaymentFrequency")?.Value?.Trim();
            if (!string.IsNullOrEmpty(freqName))
              contract.FrequencyOfPaymentlitiko = frequencies.FirstOrDefault(f => f.Name == freqName);

            // --- Простые типы ---
            contract.Subject = docXml.Element("Subject")?.Value?.Trim();
            contract.RBOlitiko = docXml.Element("RBO")?.Value;
            contract.ReasonForChangelitiko = docXml.Element("ChangeReason")?.Value;
            contract.AccDebtCreditlitiko = docXml.Element("AccountDebtCredt")?.Value;
            contract.AccFutureExpenselitiko = docXml.Element("AccountFutureExpense")?.Value;
            contract.RegistrationNumber = docXml.Element("RegistrationNumber")?.Value;
            contract.Note = docXml.Element("Note")?.Value;

            // Числа и Булевы
            contract.TotalAmountlitiko = ParseDoubleSafe(docXml.Element("TotalAmount")?.Value);
            contract.VatRatelitiko = ParseDoubleSafe(docXml.Element("VATRate")?.Value);
            contract.VatAmount = ParseDoubleSafe(docXml.Element("VATAmount")?.Value);
            contract.IncomeTaxRatelitiko = ParseDoubleSafe(docXml.Element("IncomeTaxRate")?.Value);
            contract.AmountForPeriodlitiko = ParseDoubleSafe(docXml.Element("AmountForPeriod")?.Value);
            
            contract.IsVATlitiko = ParseBoolSafe(docXml.Element("VATApplicable")?.Value);
            contract.IsPartialPaymentlitiko = ParseBoolSafe(docXml.Element("IsPartialPayment")?.Value);
            contract.IsEqualPaymentlitiko = ParseBoolSafe(docXml.Element("IsEqualPayment")?.Value);

            // Даты
            contract.ValidFrom = TryParseDate(docXml.Element("ValidFrom")?.Value);
            contract.ValidTill = TryParseDate(docXml.Element("ValidTill")?.Value);
            contract.RegistrationDate = TryParseDate(docXml.Element("RegistrationDate")?.Value);

            // Enum
            var payMethod = docXml.Element("PaymentMethod")?.Value?.Trim();
            if (payMethod == "Предоплата") contract.PaymentMethodlitiko = litiko.Eskhata.Contract.PaymentMethodlitiko.Prepayment;
            else if (payMethod == "Постоплата") contract.PaymentMethodlitiko = litiko.Eskhata.Contract.PaymentMethodlitiko.Postpay;

            // Сохраняем
            contract.Save();
          }); // Конец транзакции

          imported++;
          // Обновляем локальный кеш ID, чтобы внутри одного файла не создать дубликаты
          existingContractsIds.Add(xmlExtId);

          // Логируем прогресс каждые 100 записей, чтобы не спамить
          if (imported % 100 == 0)
            Logger.Debug($"AsyncHandler: Imported {imported}/{total} contracts...");
        }
        catch (Exception ex)
        {
          errorsCount++;
          // Сохраняем первые 50 ошибок, чтобы не переполнить текст задачи
          if (errorMessages.Count < 50)
            errorMessages.Add($"ID {xmlExtId}: {ex.Message}");
            
          Logger.Error($"AsyncHandler Error on ID {xmlExtId}: {ex.Message}");
          // Не делаем throw - продолжаем со следующим договором
        }
      }

      Logger.Debug("AsyncHandler: Finished processing.");

      // ======================================================================================
      // ЭТАП 3: УВЕДОМЛЕНИЕ ПОЛЬЗОВАТЕЛЯ
      // ======================================================================================
      var resultText = $"Импорт завершен.\n" +
                       $"Всего в файле: {total}\n" +
                       $"Создано: {imported}\n" +
                       $"Пропущено (дубли): {skipped}\n" +
                       $"Ошибок: {errorsCount}";

      if (errorMessages.Any())
      {
        resultText += "\n\nПримеры ошибок:\n" + string.Join("\n", errorMessages);
        if (errorsCount > errorMessages.Count)
          resultText += $"\n... и еще {errorsCount - errorMessages.Count} ошибок.";
      }

      SendNotice("Импорт договоров завершен", resultText, user);
    }

    // --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ---

    public virtual void SendNotice(string subject, string text, Sungero.CoreEntities.IUser user)
    {
      if (user == null) return;
      
      // Создаем простую задачу-уведомление
      var task = Sungero.Workflow.SimpleTasks.CreateWithNotices(subject, user);
      task.ActiveText = text;
      task.Save();
      task.Start();
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
      double r;
      if (string.IsNullOrWhiteSpace(value)) return 0.0;
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
  }
}