using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using litiko.Eskhata.Module.Parties.Structures.Module;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Shared;


namespace litiko.Eskhata.Module.Parties.Server
{
  partial class ModuleAsyncHandlers
  {
    public virtual void ImportPartiesAsyncHandlerlitiko(litiko.Eskhata.Module.Parties.Server.AsyncHandlerInvokeArgs.ImportPartiesAsyncHandlerlitikoInvokeArgs args)
    {
      args.Retry = false;
      var result = litiko.Eskhata.Module.Parties.Structures.Module.ResultImportCounterpartyXml.Create();
      result.DuplicateCompanies = 0;
      result.DuplicatePersons = 0;
      result.ImportedCompanies = 0;
      result.ImportedPersons = 0;
      result.SkippedCompanies = new List<string>();
      result.SkippedPersons = new List<string>();
      result.TotalCompanies = 0;
      result.TotalPersons = 0;
      result.TotalCount = 0;
      result.Errors = new List<string>();
      
      var migrationParties = ContractsEskhata.MigrationDocuments.GetAll(d => d.Id == args.MigrationDocumentId).FirstOrDefault();
      
      if (migrationParties == null) return;

      try
      {
        XDocument xDoc;
        using (var stream = migrationParties.LastVersion.Body.Read())
        {
          xDoc = XDocument.Load(stream);
        }

        var nodes = xDoc.Element("Counterparty")?.Elements().ToList();
        //var nodes = xDoc.Root.Elements().ToList();

        if (!nodes.Any())
        {
          result.Errors.Add("Файл пуст: не найдено дочерних узлов в <Counterparty>.");
          return;
        }

        // КЕШИРОВАНИЕ (Ваша логика загрузки словарей)
        var okonhDict = litiko.NSI.OKONHs.GetAll().Where(x => x.ExternalId != null && x.ExternalId != "").ToDictionary(x => x.ExternalId);
        var okvedDict = litiko.NSI.OKVEDs.GetAll().Where(x => x.ExternalId != null && x.ExternalId != "").ToDictionary(x => x.ExternalId);
        var okopfDict = litiko.NSI.OKOPFs.GetAll().Where(x => x.ExternalId != null && x.ExternalId != "").ToDictionary(x => x.ExternalId);
        var okfsDict = litiko.NSI.OKFSes.GetAll().Where(x => x.ExternalId != null && x.ExternalId != "").ToDictionary(x => x.ExternalId);
        var countryDict = litiko.Eskhata.Countries.GetAll().Where(x => x.ExternalIdlitiko != null && x.ExternalIdlitiko != "").ToDictionary(x => x.ExternalIdlitiko);
        var cityDict = litiko.Eskhata.Cities.GetAll().Where(x => x.ExternalIdlitiko != null && x.ExternalIdlitiko != "").ToDictionary(x => x.ExternalIdlitiko);
        var addressTypeDict = litiko.NSI.AddressTypes.GetAll().Where(x => x.ExternalId != null && x.ExternalId != "").ToDictionary(x => x.ExternalId);

        int batchSize = 50;
        for (int i = 0; i < nodes.Count; i += batchSize)
        {
          var batch = nodes.Skip(i).Take(batchSize).ToList();
          Transactions.Execute(() =>
                               {
                                 foreach (var node in batch)
                                 {
                                   try
                                   {
                                     if (node.Name.LocalName == "Company")
                                     {
                                       result.TotalCompanies++;
                                       var company = ParseCompany(node, okonhDict, okvedDict, okopfDict, okfsDict, countryDict, cityDict, addressTypeDict);
                                       if (company != null)
                                       {
                                         company.IsMigratedlitiko = true;
                                         if (company.State.IsInserted)
                                         {
                                           result.ImportedCompanies++;
                                           result.ImportedCount++;
                                         }
                                         else result.DuplicateCompanies++;
                                         company.Save();
                                       }
                                     }
                                     else if (node.Name.LocalName == "Person")
                                     {
                                       result.TotalPersons++;
                                       var person = ParsePerson(node, countryDict, cityDict, addressTypeDict);
                                       if (person != null)
                                       {
                                         person.IsMigratedlitiko = true;
                                         if (person.State.IsInserted)
                                         {
                                           result.ImportedPersons++;
                                           result.ImportedCount++;
                                         }
                                         else result.DuplicatePersons++;
                                         person.Save();
                                       }
                                     }
                                   }
                                   catch (Exception ex)
                                   {
                                     result.Errors.Add(string.Format("Ошибка в узле {0}: {1}", node.Name.LocalName, ex.Message));
                                   }
                                 }
                               });
        }
      }
      catch (Exception ex)
      {
        Logger.Error("Critical Import Parties Error", ex);
      }
      finally
      {
        try
        {
          SendNotice(args.AuthorId, result);
        }
        catch (Exception ex)
        {
          Logger.Error("Не удалось отправить уведомление", ex);
        }
        
        if (migrationParties != null)
        {
          try
          {
            if (Locks.GetLockInfo(migrationParties).IsLocked)
              Locks.Unlock(migrationParties);
            
            ContractsEskhata.MigrationDocuments.Delete(migrationParties);
          }
          catch (Exception ex) {
            Logger.Error("Не удалось удалить тех. док", ex);
          }
        }
      }
    }

    // =====================================================================
    // ПАРСИНГ КОМПАНИИ (Все ваши поля)
    // =====================================================================
    private litiko.Eskhata.ICompany ParseCompany(XElement companyElement,
                                                 Dictionary<string, litiko.NSI.IOKONH> okonhDict,
                                                 Dictionary<string, litiko.NSI.IOKVED> okvedDict,
                                                 Dictionary<string, litiko.NSI.IOKOPF> okopfDict,
                                                 Dictionary<string, litiko.NSI.IOKFS> okfsDict,
                                                 Dictionary<string, litiko.Eskhata.ICountry> countryDict,
                                                 Dictionary<string, litiko.Eskhata.ICity> cityDict,
                                                 Dictionary<string, litiko.NSI.IAddressType> addressTypeDict)
    {
      var isExternalID = companyElement.Element("ExternalID")?.Value;
      var isINN = companyElement.Element("INN")?.Value;

      var company = litiko.Eskhata.Companies.GetAll()
        .FirstOrDefault(x => (!string.IsNullOrEmpty(isExternalID) && x.ExternalId == isExternalID) ||
                        (!string.IsNullOrEmpty(isINN) && x.TIN == isINN));

      if (company == null)
      {
        company = litiko.Eskhata.Companies.Create();
        company.ExternalId = isExternalID;
        company.TIN = isINN;
      }

      company.Name = companyElement.Element("Name")?.Value.Trim() ?? "Без имени";
      company.LegalName = companyElement.Element("LONG_NAME")?.Value.Trim();
      company.Inamelitiko = companyElement.Element("I_NAME")?.Value.Trim();
      company.Nonresident = ParseBoolSafe(companyElement.Element("REZIDENT")?.Value);
      company.NUNonrezidentlitiko = ParseBoolSafe(companyElement.Element("NU_REZIDENT")?.Value);
      company.TRRC = companyElement.Element("KPP")?.Value;
      company.NCEO = companyElement.Element("KOD_OKPO")?.Value;
      company.RegNumlitiko = companyElement.Element("REGIST_NUM")?.Value;
      company.Businesslitiko = companyElement.Element("BUSINESS")?.Value;
      company.PostalAddress = companyElement.Element("PostAdress")?.Value;
      company.LegalAddress = companyElement.Element("LegalAdress")?.Value;
      company.Phones = companyElement.Element("Phone")?.Value;
      company.Email = companyElement.Element("Email")?.Value;
      company.Homepage = companyElement.Element("WebSite")?.Value;
      company.VATPayerlitiko = ParseBoolSafe(companyElement.Element("VATPayer")?.Value);
      company.AccountEskhatalitiko = companyElement.Element("InternalAcc")?.Value;
      company.Account = companyElement.Element("CorrAcc")?.Value;
      company.Streetlitiko = companyElement.Element("Street")?.Value;
      company.HouseNumberlitiko = companyElement.Element("BuildingNumber")?.Value;

      // Словари
      var isOKOPF = companyElement.Element("FORMA")?.Value;
      if (!string.IsNullOrEmpty(isOKOPF) && okopfDict.ContainsKey(isOKOPF)) company.OKOPFlitiko = okopfDict[isOKOPF];

      var isOKFS = companyElement.Element("OWNERSHIP")?.Value;
      if (!string.IsNullOrEmpty(isOKFS) && okfsDict.ContainsKey(isOKFS)) company.OKFSlitiko = okfsDict[isOKFS];

      var isCountry = companyElement.Element("COUNTRY")?.Value;
      if (!string.IsNullOrEmpty(isCountry) && countryDict.ContainsKey(isCountry)) company.Countrylitiko = countryDict[isCountry];

      var isCity = companyElement.Element("City")?.Value;
      if (!string.IsNullOrEmpty(isCity) && cityDict.ContainsKey(isCity)) company.City = cityDict[isCity];

      var isAddressType = companyElement.Element("AddressType")?.Value;
      if (!string.IsNullOrEmpty(isAddressType) && addressTypeDict.ContainsKey(isAddressType)) company.AddressTypelitiko = addressTypeDict[isAddressType];

      // OKONH / OKVED (Elements)
      var okonhEl = companyElement.Element("CODE_OKONH")?.Elements("element").FirstOrDefault()?.Value?.Trim();
      if (!string.IsNullOrEmpty(okonhEl) && okonhDict.ContainsKey(okonhEl)) company.OKONHlitiko = okonhDict[okonhEl];

      var okvedEl = companyElement.Element("CODE_OKVED")?.Elements("element").FirstOrDefault()?.Value?.Trim();
      if (!string.IsNullOrEmpty(okvedEl) && okvedDict.ContainsKey(okvedEl)) company.OKVEDlitiko = okvedDict[okvedEl];

      // Числа
      var isNumbers = companyElement.Element("NUMBERS")?.Value;
      if (!string.IsNullOrEmpty(isNumbers)) company.Numberslitiko = int.Parse(isNumbers);

      // Надежность (Enum)
      var isRel = companyElement.Element("Reliability")?.Value?.Trim();
      if (!string.IsNullOrEmpty(isRel))
      {
        if (isRel.Equals("Надежный", StringComparison.OrdinalIgnoreCase)) company.Reliabilitylitiko = litiko.Eskhata.Company.Reliabilitylitiko.Reliable;
        else company.Reliabilitylitiko = litiko.Eskhata.Company.Reliabilitylitiko.NotReliable;
      }

      return company;
    }

    // =====================================================================
    // ПАРСИНГ ПЕРСОНЫ (Все ваши поля)
    // =====================================================================
    private litiko.Eskhata.IPerson ParsePerson(XElement personElement,
                                               Dictionary<string, litiko.Eskhata.ICountry> countryDict,
                                               Dictionary<string, litiko.Eskhata.ICity> cityDict,
                                               Dictionary<string, litiko.NSI.IAddressType> addressTypeDict)
    {
      var isExternalID = personElement.Element("ExternalID")?.Value;
      var isINN = personElement.Element("INN")?.Value;

      var person = Eskhata.People.GetAll()
        .FirstOrDefault(x => (!string.IsNullOrEmpty(isExternalID) && x.ExternalId == isExternalID) || (!string.IsNullOrEmpty(isINN) && x.TIN == isINN));

      if (person == null)
      {
        person = Eskhata.People.Create();
        person.ExternalId = isExternalID;
      }

      person.LastName = personElement.Element("LastName")?.Value?.Trim();
      person.FirstName = personElement.Element("FirstName")?.Value?.Trim();
      person.MiddleName = personElement.Element("MiddleName")?.Value?.Trim();
      person.Nonresident = ParseBoolSafe(personElement.Element("REZIDENT")?.Value);
      person.Inamelitiko = personElement.Element("I_NAME")?.Value?.Trim();
      person.TIN = isINN;
      person.SINlitiko = personElement.Element("I_IIN")?.Value;
      person.BirthPlace = personElement.Element("DOC_BIRTH_PLACE")?.Value;
      person.PostalAddress = personElement.Element("PostAdress")?.Value;
      person.Email = personElement.Element("Email")?.Value;
      person.Phones = personElement.Element("Phone")?.Value;
      person.Streetlitiko = personElement.Element("Street")?.Value;
      person.HouseNumberlitiko = personElement.Element("BuildingNumber")?.Value;
      person.Account = personElement.Element("CorrAcc")?.Value;
      person.AccountEskhatalitiko = personElement.Element("InternalAcc")?.Value;

      var bDate = TryParseDate(personElement.Element("DATE_PERS")?.Value);
      if (bDate.HasValue) person.DateOfBirth = bDate;

      var sex = personElement.Element("SEX")?.Value;
      if (sex == "М") person.Sex = Eskhata.Person.Sex.Male;
      else if (sex == "Ж") person.Sex = Eskhata.Person.Sex.Female;

      var isCountry = personElement.Element("COUNTRY")?.Value;
      if (!string.IsNullOrEmpty(isCountry) && countryDict.ContainsKey(isCountry)) person.Citizenship = countryDict[isCountry];

      var isCity = personElement.Element("City")?.Value;
      if (!string.IsNullOrEmpty(isCity) && cityDict.ContainsKey(isCity)) person.City = cityDict[isCity];

      var isAddressType = personElement.Element("AddressType")?.Value;
      if (!string.IsNullOrEmpty(isAddressType) && addressTypeDict.ContainsKey(isAddressType)) person.AddressTypelitiko = addressTypeDict[isAddressType];

      // Паспортные данные
      var identity = personElement.Element("IdentityDocument");
      if (identity != null)
      {
        var xmlType = identity.Element("TYPE")?.Value;
        var kind = Sungero.Parties.IdentityDocumentKinds.GetAll().FirstOrDefault(x => x.SID == xmlType);
        if (kind != null) person.IdentityKind = kind;
        
        person.IdentityNumber = identity.Element("NUM")?.Value;
        person.IdentitySeries = identity.Element("SER")?.Value;
        person.IdentityAuthority = identity.Element("WHO")?.Value;
        person.IdentityDateOfIssue = TryParseDate(identity.Element("DATE_BEGIN")?.Value);
        person.IdentityExpirationDate = TryParseDate(identity.Element("DATE_END")?.Value);
      }

      return person;
    }

    // =====================================================================
    // УДАЛЕНИЕ МИГРИРОВАННЫХ
    // =====================================================================
    public virtual void DeleteMigratedPartiesAsync(litiko.Eskhata.Module.Parties.Server.AsyncHandlerInvokeArgs.DeleteMigratedPartiesAsynclitikoInvokeArgs args)
    {
      int deleted = 0;
      var companyIds = Eskhata.Companies.GetAll(c => c.IsMigratedlitiko == true).Select(c => c.Id).ToList();
      var personIds = Eskhata.People.GetAll(p => p.IsMigratedlitiko == true).Select(p => p.Id).ToList();

      int batchSize = 50;
      
      // Компании
      for (int i = 0; i < companyIds.Count; i += batchSize) {
        var batch = companyIds.Skip(i).Take(batchSize).ToList();
        Transactions.Execute(() => {
                               foreach (var id in batch) {
                                 var obj = Eskhata.Companies.Get(id);
                                 if (Locks.GetLockInfo(obj).IsLocked) Locks.Unlock(obj);
                                 Eskhata.Companies.Delete(obj);
                                 deleted++;
                               }
                             });
      }

      // Персоны
      for (int i = 0; i < personIds.Count; i += batchSize) {
        var batch = personIds.Skip(i).Take(batchSize).ToList();
        Transactions.Execute(() =>
                             {
                               foreach (var id in batch) {
                                 var obj = Eskhata.People.Get(id);
                                 if (Locks.GetLockInfo(obj).IsLocked) Locks.Unlock(obj);
                                 Eskhata.People.Delete(obj);
                                 deleted++;
                               }
                             });
      }
    }

    // Вспомогательные методы (Ваши)
    private static DateTime? TryParseDate(string date) {
      DateTime r;
      if (DateTime.TryParseExact(date, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out r)) return r;
      return null;
    }

    private static bool ParseBoolSafe(string v) {
      if (string.IsNullOrWhiteSpace(v)) return false;
      var n = v.Trim().ToLowerInvariant();
      return n == "1" || n == "true" || n == "yes";
    }

    private void SendNotice(long authorId, litiko.Eskhata.Module.Parties.Structures.Module.IResultImportCounterpartyXml res)
    {
      var author = Employees.GetAll(e => e.Id == authorId).FirstOrDefault();
      if (author == null) return;

      var notice = Sungero.Workflow.SimpleTasks.CreateWithNotices("Импорт контрагентов завершен", author);
      
      var sb = new System.Text.StringBuilder();
      sb.AppendLine("📊 Результаты миграции:");
      sb.AppendLine(string.Format("• Успешно создано: {0}", res.ImportedCount));
      sb.AppendLine(string.Format("• Компаний: {0} (Дублей: {1})", res.ImportedCompanies, res.DuplicateCompanies));
      sb.AppendLine(string.Format("• Персон: {0} (Дублей: {1})", res.ImportedPersons, res.DuplicatePersons));
      sb.AppendLine(string.Format("❌ Ошибок: {0}", res.Errors.Count));
      
      if (res.Errors.Any())
      {
        sb.AppendLine("\n⚠️ Детали ошибок:");
        foreach (var err in res.Errors) sb.AppendLine("- " + err);
      }
      
      notice.ActiveText = sb.ToString();
      notice.Start();
    }
    
    public virtual void DeleteMigratedPartiesAsynclitiko(litiko.Eskhata.Module.Parties.Server.AsyncHandlerInvokeArgs.DeleteMigratedPartiesAsynclitikoInvokeArgs args)
    {
      args.Retry = false;
      int deleted = 0;
      int errors = 0;

      // Получаем список всех мигрированных (Компании + Персоны)
      var companyIds = Eskhata.Companies.GetAll(c => c.IsMigratedlitiko == true).Select(c => c.Id).ToList();
      var personIds = Eskhata.People.GetAll(p => p.IsMigratedlitiko == true).Select(p => p.Id).ToList();

      // Удаляем Компании
      deleted += DeleteEntitiesBatch(companyIds, typeof(litiko.Eskhata.ICompany), ref errors);
      // Удаляем Персон
      deleted += DeleteEntitiesBatch(personIds, typeof(litiko.Eskhata.IPerson), ref errors);

      // Уведомление
      var author = Employees.GetAll(e => e.Id == args.AuthorId).FirstOrDefault();
      if (author != null)
      {
        var notice = Sungero.Workflow.SimpleTasks.CreateWithNotices("Очистка контрагентов завершена", author);
        notice.ActiveText = string.Format("Удалено: {0}\nОшибок (связи/блокировки): {1}", deleted, errors);
        notice.Start();
      }
    }

    // Вспомогательный метод для пакетного удаления
    private int DeleteEntitiesBatch(List<long> ids, Type type, ref int errors)
    {
      int deletedCount = 0;
      int batchSize = 50;
      for (int i = 0; i < ids.Count; i += batchSize)
      {
        var batch = ids.Skip(i).Take(batchSize).ToList();
        Transactions.Execute(() =>
                             {
                               foreach (var id in batch)
                               {
                                 try
                                 {
                                   var entity = (type == typeof(litiko.Eskhata.ICompany))
                                     ? (IEntity)Eskhata.Companies.Get(id)
                                     : (IEntity)Eskhata.People.Get(id);
                                   
                                   if (entity != null)
                                   {
                                     if (Locks.GetLockInfo(entity).IsLocked) Locks.Unlock(entity);
                                     if (type == typeof(litiko.Eskhata.ICompany)) Eskhata.Companies.Delete((litiko.Eskhata.ICompany)entity);
                                     else Eskhata.People.Delete((litiko.Eskhata.IPerson)entity);
                                     deletedCount++;
                                   }
                                 }
                                 catch {  }
                               }
                             });
      }
      return deletedCount;
    }

    private void NotifyAuthor(int authorId, string title, litiko.Eskhata.Module.Parties.Structures.Module.IResultImportCounterpartyXml res)
    {
      var author = Employees.GetAll(e => e.Id == authorId).FirstOrDefault();
      if (author == null) return;

      var notice = Sungero.Workflow.SimpleTasks.CreateWithNotices(title, author);
      var sb = new System.Text.StringBuilder();
      sb.AppendLine("📊 Результаты миграции:");
      sb.AppendLine(string.Format("• Компании: {0} (Новых: {1}, Дублей: {2})", res.TotalCompanies, res.ImportedCompanies, res.DuplicateCompanies));
      sb.AppendLine(string.Format("• Персоны: {0} (Новых: {1}, Дублей: {2})", res.TotalPersons, res.ImportedPersons, res.DuplicatePersons));
      sb.AppendLine(string.Format("❌ Ошибок: {0}", res.Errors.Count));
      
      if (res.Errors.Any())
      {
        sb.AppendLine("\n⚠️ Ошибки:");
        foreach (var err in res.Errors) sb.AppendLine(err);
      }
      
      notice.ActiveText = sb.ToString();
      notice.Start();
    }
  }
}