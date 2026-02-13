using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;
using litiko.Eskhata.Module.Contracts.Structures.Module;
using Sungero.Domain.Shared;
using Sungero.Domain;

namespace litiko.Eskhata.Module.Contracts.Server
{
  partial class ModuleAsyncHandlers
  {

    //    public virtual void ExportFromBuslitiko(litiko.Eskhata.Module.Contracts.Server.AsyncHandlerInvokeArgs.ExportFromBuslitikoInvokeArgs args)
    //    {
    //      var userId = args.UserId;
//
    //      var ids = args.ExternalIds.Split(new[] { ','}, StringSplitOptions.RemoveEmptyEntries).ToList();
//
    //      if(!ids.Any())
    //      {
    //        Logger.Debug("Список ID пуст");
    //        return;
    //      }
//
    //      Logger.DebugFormat($"Start ExportFromBusHandler for {ids.Count} items");
//
    //      var errorList = new StringBuilder();
//
    //      byte[] zipContent = null;
//
    //      try
    //      {
    //        zipContent = GenerateZipArchive(ids, errorList);
    //      }
    //      catch (Exception ex)
    //      {
    //        Logger.Error("Critical error in ExportFromBusHandler", ex);
    //        SendNotification(userId, "Произошла критическая ошибка при формировании архива: " + ex.Message, null);
    //        return;
    //      }
//
    //      var documentName = $"Экспорт из Шины ({ids.Count} шт.) от {Calendar.Now:dd.MM.yyyy HH:mm}";
//
    //      var document = CreateSimpleDocument(documentName, zipContent);
//
    //      var message = "Экспорт завершен. Файл во вложении.";
//
    //      if (errorList.Length > 0)
    //        message += $"\n\nНе удалось загрузить следующие ID:\n{errorList}";
//
    //      SendNotification(userId, message, document);
    //    }

    //    public virtual void UploadHozDoglitiko(litiko.Eskhata.Module.Contracts.Server.AsyncHandlerInvokeArgs.UploadHozDoglitikoInvokeArgs args)
    //    {
    //      List<long> Ids = new List<long>
    //      {
    //        57227376326, 76733521958, 76075666319, 82317544435, 69857910867, 81559622395, 69130476865, 77802499802, 35733172640, 63167682556, 61907589585,
    //        60449322840, 77797991516, 1840886148, 9697688878, 12969032762, 27051363972, 39291663179, 4695340469, 12487526163, 10846060607, 1840876789,
    //        1840878612, 1848633541, 1848634362, 39488424213, 61719849499, 63997493763, 59466099513, 26090430823, 73024456932, 72704382656, 75537394341,
    //        69855836419, 41602534099, 78500824380, 82356433507
    //      };
    //    }
    
    //    private byte[] GenerateZipArchive(List<string> ids, StringBuilder errors)
    //    {
    //      using (var httpClient = new HttpClient())
    //      {
    //        httpClient.Timeout = TimeSpan.FromMinutes(10); // Увеличиваем тайм-аут
//
    //        // Создаем корневые элементы для общих файлов
    //        // Мы будем складывать все полученные пакеты внутрь этих тегов
    //        var contractsRoot = new XElement("Data");
    //        var clientsRoot = new XElement("Counterparty");
//
    //        foreach (var id in ids)
    //        {
    //          try
    //          {
    //            // --- ШАГ 1: ЗАПРОС ДОГОВОРА ---
    //            var contractReq = BuildXmlRequest("R_DR_GET_CONTRACT", "ExternalID", id);
//
    //            // Получаем ответ и чистим его от Java-ошибок
    //            var contractResp = GetCleanXmlFromBus(httpClient, contractReq);
//
    //            // Проверки на пустоту и корректность XML
    //            if (string.IsNullOrEmpty(contractResp))
    //            {
    //              errors.AppendLine($"{id}: Пустой ответ.");
    //              continue;
    //            }
    //            if (!contractResp.StartsWith("<"))
    //            {
    //              var snippet = contractResp.Length > 100 ? contractResp.Substring(0, 100) : contractResp;
    //              errors.AppendLine($"{id}: Ответ не XML. Начало: {snippet}");
    //              continue;
    //            }
//
    //            // Парсим XML
    //            var contractDoc = XDocument.Parse(contractResp);
//
    //            // Ищем тег <Document> в ответе
    //            var docNode = contractDoc.Descendants().FirstOrDefault(x => x.Name.LocalName == "Document");
//
    //            if (docNode != null)
    //            {
    //              // Добавляем этот "пакет" в общий список
    //              contractsRoot.Add(docNode);
    //            }
    //            else
    //            {
    //              var errMsg = contractDoc.Descendants("stateMsg").FirstOrDefault()?.Value
    //                ?? contractDoc.Descendants("message").FirstOrDefault()?.Value
    //                ?? "нет описания";
    //              errors.AppendLine($"{id}: нет данных (<Document>). Шина: {errMsg}");
    //              continue;
    //            }
//
    //            // --- ШАГ 2: ЗАПРОС КЛИЕНТА ---
    //            // Ищем ID клиента внутри полученного договора
    //            var cpIdValue = contractDoc.Descendants("CounterpartyExternalId").FirstOrDefault()?.Value;
//
    //            if (!string.IsNullOrEmpty(cpIdValue))
    //            {
    //              System.Threading.Thread.Sleep(200); // Пауза, чтобы не дудосить шину
//
    //              var clientReq = BuildXmlRequestForClient(id, cpIdValue);
    //              var clientResp = GetCleanXmlFromBus(httpClient, clientReq);
//
    //              if (!string.IsNullOrEmpty(clientResp) && clientResp.StartsWith("<"))
    //              {
    //                try
    //                {
    //                  var clientDoc = XDocument.Parse(clientResp);
    //                  var cpNode = clientDoc.Descendants().FirstOrDefault(x => x.Name.LocalName == "Counterparty");
//
    //                  if (cpNode != null)
    //                  {
    //                    // Добавляем содержимое (Company или Person) в общий список
    //                    clientsRoot.Add(cpNode.Elements());
    //                  }
    //                }
    //                catch { /* Ошибки клиента не критичны */ }
    //              }
    //            }
//
    //            System.Threading.Thread.Sleep(200);
    //          }
    //          catch (Exception ex)
    //          {
    //            Logger.Error($"Error processing ID {id}", ex);
    //            errors.AppendLine($"{id}: Ошибка C# - {ex.Message}");
    //          }
    //        }
//
    //        // --- ШАГ 3: ФОРМИРОВАНИЕ ZIP ---
    //        // Создаем итоговые XML документы
    //        var finalContracts = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), contractsRoot);
    //        var finalClients = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), clientsRoot);
//
    //        using (var memoryStream = new MemoryStream())
    //        {
    //          using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
    //          {
    //            AddFileToZip(archive, "contracts.xml", finalContracts);
    //            AddFileToZip(archive, "clients.xml", finalClients);
    //          }
    //          return memoryStream.ToArray();
    //        }
    //      }
    //    }
//
    //    private Sungero.Docflow.ISimpleDocument CreateSimpleDocument(string name, byte[] content)
    //    {
    //      var doc = Sungero.Docflow.SimpleDocuments.Create();
//
    //      doc.Name = name;
//
    //      using (var stream = new MemoryStream(content))
    //      {
    //        doc.CreateVersionFrom(stream, "zip");
    //      }
    //      doc.Save();
//
    //      return doc;
    //    }
//
    //    private void SendNotification(long userId, string messageBody, Sungero.Domain.Shared.IEntity attachment)
    //    {
    //      var author = Employees.GetAll(e => e.Id == userId).FirstOrDefault();
//
    //      var notice = Sungero.Workflow.SimpleTasks.CreateWithNotices("Результат выгрузки", author);
//
    //      notice.Subject = "Выгрузка из Шины (XML)";
//
    //      notice.ActiveText = messageBody;
//
    //      if(attachment != null)
    //      {
    //        notice.Attachments.Add(attachment);
    //      }
//
    //      notice.Start();
    //    }
//
    //    private string PostXml(HttpClient client, string xml)
    //    {
    //      var forwardUrl = Constants.Module.forwardUrl;
    //      try
    //      {
    //        var content = new StringContent(xml, Encoding.UTF8, "application/xml");
//
    //        var response = client.PostAsync(forwardUrl, content).Result;
//
    //        if (response.IsSuccessStatusCode)
    //          return response.Content.ReadAsStringAsync().Result;
    //      }
    //      catch (Exception ex)
    //      {
    //        Logger.Error("HTTP Error", ex);
    //      }
    //      return null;
    //    }
//
    //    private void AddFileToZip(ZipArchive archive, string name, XDocument xmlDoc)
    //    {
    //      var entry = archive.CreateEntry(name);
//
    //      using (var stream = entry.Open())
    //      {
    //        using (var writer = new StreamWriter(stream, Encoding.UTF8))
    //        {
    //          xmlDoc.Save(writer);
    //        }
    //      }
//
    //    }
//
    //    private string BuildXmlRequest(string dictionary, string keyName, string keyValue)
    //    {
    //      return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
    //                    <root>
    //                        <head>
    //                            <session_id>{Guid.NewGuid()}</session_id>
    //                            <application_key>192.168.206.23/Integration/odata/Integration/ProcessResponseFromIS##</application_key>
    //                        </head>
    //                        <request>
    //                            <protocol-version>1.00</protocol-version>
    //                            <request-type>R_DR_GET_DATA</request-type>
    //                            <dictionary>{dictionary}</dictionary>
    //                            <{keyName}>{keyValue}</{keyName}>
    //                        </request>
    //                    </root>";
    //    }
//
    //    private string BuildXmlRequestForClient(string dogId, string clientId)
    //    {
    //      return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
    //                    <root>
    //                        <head>
    //                            <session_id>{Guid.NewGuid()}</session_id>
    //                            <application_key>192.168.206.23/Integration/odata/Integration/ProcessResponseFromIS##</application_key>
    //                        </head>
    //                        <request>
    //                            <protocol-version>1.00</protocol-version>
    //                            <request-type>R_DR_GET_DATA</request-type>
    //                            <dictionary>R_DR_GET_COUNTERPARTY</dictionary>
    //                            <ExternalID>{dogId}</ExternalID>
    //                            <CounterpartyId>{clientId}</CounterpartyId>
    //                        </request>
    //                    </root>";
    //    }
//
    //   private string GetCleanXmlFromBus(HttpClient client, string requestXml)
    //    {
    //      var forwardUrl = Constants.Module.forwardUrl; // Или используйте константу ForwardUrl
    //      try
    //      {
    //        var content = new StringContent(requestXml, Encoding.UTF8, "application/xml");
//
    //        // 1. Отправляем запрос
    //        var response = client.PostAsync(forwardUrl, content).Result;
//
    //        // 2. ВАЖНО: Читаем тело ответа НЕЗАВИСИМО от статус-кода (200, 400 или 500)
    //        // Раньше тут могла быть проверка if (response.IsSuccessStatusCode), она всё портила.
    //        var rawResponse = response.Content.ReadAsStringAsync().Result;
//
    //        if (string.IsNullOrWhiteSpace(rawResponse)) return null;
//
    //        // 3. ОЧИСТКА: Ищем начало XML
    //        // Шина присылает: "java.io.FileNotFoundException... <?xml version..."
    //        var xmlStartIndex = rawResponse.IndexOf("<?xml");
//
    //        // На случай если <?xml нет, ищем <root
    //        if (xmlStartIndex == -1) xmlStartIndex = rawResponse.IndexOf("<root");
//
    //        // 4. Если нашли XML, отрезаем всё лишнее слева (ошибку Java)
    //        if (xmlStartIndex >= 0)
    //        {
    //          return rawResponse.Substring(xmlStartIndex).Trim();
    //        }
//
    //        // Если XML не нашли, возвращаем как есть (чтобы увидеть это в логах)
    //        return rawResponse.Trim();
    //      }
    //      catch (Exception ex)
    //      {
    //        Logger.Error("HTTP Request Error", ex);
    //        return null;
    //      }
    //    }

    #region Миграция
    public virtual void DeleteMigratedContractAsynclitiko(litiko.Eskhata.Module.Contracts.Server.AsyncHandlerInvokeArgs.DeleteMigratedContractAsynclitikoInvokeArgs args)
    {
      
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
      Logger.Debug("Start Migrating Contracts");
      
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
          var batch = documentElements.Skip(i).Take(batchSize).ToList();

          Transactions.Execute(() =>
                               {
                                 foreach (var docXml in batch)
                                 {
                                   var externalId = docXml.Element("ExternalID")?.Value.Trim();
                                   
                                   try
                                   {
                                     var contract = ParseContractOptimized(docXml, result, currencies, docKinds, docGroups, departments,
                                                                           employees, contacts, payRegions, rentRegions, frequencies, counterparties, banks);
                                     
                                     if (contract != null)
                                       result.ImportedCount++;
                                   }
                                   catch (Exception ex)
                                   {
                                     result.Errors.Add(string.Format("Критический сбой договора с ИД - {0}: {1}", externalId, ex.Message));
                                   }
                                 }
                               });
        }
      }
      finally
      {
        if (migrationDoc != null) ContractsEskhata.MigrationDocuments.Delete(migrationDoc);

        var author = Employees.GetAll(e => e.Id == args.AuthorId).FirstOrDefault();
        if (author != null)
        {
          var notice = Sungero.Workflow.SimpleTasks.CreateWithNotices("Результат миграции договоров", author);
          var sb = new System.Text.StringBuilder();
          sb.AppendLine("Миграция завершена.");
          sb.AppendLine(string.Format("📊 Всего в файле: {0}", result.TotalCount));
          sb.AppendLine(string.Format("✅ Успешно создано: {0}", result.ImportedCount));
          sb.AppendLine(string.Format("🔄 Пропущено (дубли): {0}", result.DuplicateCount));
          sb.AppendLine(string.Format("❌ Ошибки: {0}", result.Errors.Count));
          
          if (result.Errors.Any())
          {
            sb.AppendLine("\n⚠️ Список ошибок:");
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
          result.Errors.Add($"Договор {externalId}: Вид документа '{documentKindId}' не найден.");
          return null;
        }
      }
      else
      {
        result.Errors.Add($"Договор {externalId}: Не указан Вид документа.");
        return null;
      }


      var contract = Eskhata.Contracts.Create();
      contract.ExternalId = externalId;
      contract.Name = !string.IsNullOrEmpty(name) ? name : "Без имени";

      contract.Counterparty = foundCounterparty;
      contract.DocumentKind = foundDocumentKind;
      
      var documentGroupId = docXml.Element("DocumentGroup")?.Value?.Trim();
      if(!string.IsNullOrEmpty(documentGroupId))
        contract.DocumentGroup = docGroups.FirstOrDefault(g => g.ExternalIdlitiko == documentGroupId);

      var currency = docXml.Element("Currency")?.Value?.Trim();
      if (!string.IsNullOrEmpty(currency))
        contract.Currency = currencies.FirstOrDefault(c => c.AlphaCode == currency || c.NumericCode == currency);

      var curOpCode = docXml.Element("OperationCurrency")?.Value?.Trim(); // если пусто, то будет TJS логика реализована в АБС при выгрузке
      if (!string.IsNullOrEmpty(curOpCode))
        contract.CurrencyOperationlitiko = currencies.FirstOrDefault(c => c.AlphaCode == curOpCode || c.NumericCode == curOpCode);

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
    #endregion
  }
}
