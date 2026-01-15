using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.IO;
using System.Xml.Linq;
using litiko.Eskhata.Module.Contracts.Structures.Module;
using Sungero.Domain.Shared;
using Sungero.Domain;

namespace litiko.Eskhata.Module.Contracts.Server
{
  partial class ModuleAsyncHandlers
  {

    public virtual void DeleteMigratedContractAsynclitiko(litiko.Eskhata.Module.Contracts.Server.AsyncHandlerInvokeArgs.DeleteMigratedContractAsynclitikoInvokeArgs args)
    {
      
      // Получаем только ID, чтобы не грузить память
      var allIds = Eskhata.Contracts.GetAll(c => c.IsMigratedlitiko == true)
        .Select(c => c.Id).ToList();

      int deletedCount = 0;
      
      if (!allIds.Any()) return;

      int batchSize = 50;
      for (int i = 0; i < allIds.Count; i += batchSize)
      {
        var batch = allIds.Skip(i).Take(batchSize).ToList();
        foreach (var id in batch)
        {
          var contract = Eskhata.Contracts.GetAll(c => c.Id == id).FirstOrDefault();
          if (contract != null)
          {
            if (Locks.GetLockInfo(contract).IsLocked) Locks.Unlock(contract);
            Eskhata.Contracts.Delete(contract);
            deletedCount++;
          }
        }
      }
      
      var author = Employees.GetAll(e=>e.Id == args.AuthorId).FirstOrDefault();
      if(author != null)
      {
        var notice = Sungero.Workflow.SimpleTasks.CreateWithNotices("Очистка мигрированных договоров завершена.", author);
        notice.ActiveText = string.Format("Фоновое удаление успешно выполнено.\nУдалено {0} договоров", deletedCount);
        notice.Start();
      }
      Logger.Debug("Фоновое удаление мигрированных данных завершено.");
    }

    
    public virtual void ImportContractsAsyncHandlerlitiko(litiko.Eskhata.Module.Contracts.Server.AsyncHandlerInvokeArgs.ImportContractsAsyncHandlerlitikoInvokeArgs args)
    {
      args.Retry = false;
      
      var result = litiko.Eskhata.Module.Contracts.Structures.Module.ResultImportXmlUI.Create();
      result.Errors = new List<string>();
      result.ImportedCount = 0;
      result.DuplicateCount = 0;
      result.TotalCount = 0;
      
      var migrationDoc = ContractsEskhata.MigrationDocuments.GetAll(d => d.Id == args.MigrationDocumentId).FirstOrDefault();
      
      if (migrationDoc == null)
      {
        Logger.ErrorFormat("Async Import: Документ с Id {0} не найден в БД", args.MigrationDocumentId);
        return;
      }
      
      try
      {
        XDocument xDoc;
        using (var stream = migrationDoc.LastVersion.Body.Read())
          xDoc = XDocument.Load(stream);
        
        var documentElements = xDoc.Element("Data").Elements("Document").ToList();
        result.TotalCount = documentElements.Count;
        
        var xmlCounterpartyIds = documentElements
          .Select(x => x.Element("CounterpartyExternalId")?.Value?.Trim())
          .Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
        
        var currencies = litiko.Eskhata.Currencies.GetAll().ToList();
        var docKinds = litiko.Eskhata.DocumentKinds.GetAll().ToList();
        var docGroups = litiko.Eskhata.DocumentGroupBases.GetAll().ToList();
        var departments = litiko.Eskhata.Departments.GetAll().ToList();
        var employees = litiko.Eskhata.Employees.GetAll().ToList();
        var payRegions = litiko.NSI.PaymentRegions.GetAll().ToList();
        var rentRegions = litiko.NSI.RegionOfRentals.GetAll().ToList();
        var frequencies = litiko.NSI.FrequencyOfPayments.GetAll().ToList();
        var contacts = litiko.Eskhata.Contacts.GetAll().ToList();
        var counterparties = litiko.Eskhata.Counterparties.GetAll().Where(c => xmlCounterpartyIds.Contains(c.ExternalId)).ToList();
        var banks = litiko.Eskhata.Banks.GetAll().Where(b => xmlCounterpartyIds.Contains(b.ExternalId)).ToList();

        int batchSize = 100;
        for (int i = 0; i < documentElements.Count; i += batchSize)
        {
          // Берем следующую пачку из 100 элементов
          var batch = documentElements.Skip(i).Take(batchSize).ToList();

          Transactions.Execute(() =>
                               {
                                 foreach (var docXml in batch)
                                 {
                                   try
                                   {
                                     var contract = ParseContractOptimized(docXml, result, currencies, docKinds, docGroups, departments,
                                                                           employees, contacts, payRegions, rentRegions, frequencies, counterparties, banks);
                                     
                                     if (contract != null)
                                       result.ImportedCount++;
                                   }
                                   catch (Exception ex)
                                   {
                                     result.Errors.Add("Критический сбой строки: " + ex.Message);
                                   }
                                 }
                               });
        }
      }
      finally
      {
        if (migrationDoc != null) ContractsEskhata.MigrationDocuments.Delete(migrationDoc);

        // ОТПРАВКА УВЕДОМЛЕНИЯ на основе данных из result
        var author = Employees.GetAll(e => e.Id == args.AuthorId).FirstOrDefault();
        if (author != null)
        {
          var notice = Sungero.Workflow.SimpleTasks.CreateWithNotices("Результат импорта договоров", author);
          var sb = new System.Text.StringBuilder();
          sb.AppendLine("Импорт завершен.");
          sb.AppendLine(string.Format("📊 Всего в файле: {0}", result.TotalCount));
          sb.AppendLine(string.Format("✅ Успешно создано: {0}", result.ImportedCount));
          sb.AppendLine(string.Format("🔄 Пропущено (дубли): {0}", result.DuplicateCount));
          sb.AppendLine(string.Format("❌ Ошибки: {0}", result.Errors.Count));
          
          if (result.Errors.Any())
          {
            sb.AppendLine("\n⚠️ Список проблем:");
            foreach (var err in result.Errors)
              sb.AppendLine(err);
          }
          
          notice.ActiveText = sb.ToString();
          notice.Start();
        }
      }
    }

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
      var externalId = docXml.Element("ExternalID")?.Value?.Trim();
      
      if(string.IsNullOrEmpty(externalId))
      {
        result.Errors.Add($"У договора отсутствует ExternalId");
        return null;
      }
      var name = docXml.Element("Name")?.Value;
      
      // Проверка Контрагента
      litiko.Eskhata.ICounterparty foundCounterparty = null;
      var counterpartyExternalId = docXml.Element("CounterpartyExternalId")?.Value?.Trim();
      
      if (!string.IsNullOrEmpty(counterpartyExternalId))
      {
        foundCounterparty = counterparties.FirstOrDefault(c => c.ExternalId == counterpartyExternalId);

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
      
      contract.IsMigratedlitiko = true;

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
    
    [Remote, Public]
    public void RunAsyncDelete()
    {
      var args = litiko.Eskhata.Module.Contracts.AsyncHandlers.ImportContractsAsyncHandlerlitiko.Create();
      args.ExecuteAsync();
    }
  }
}
