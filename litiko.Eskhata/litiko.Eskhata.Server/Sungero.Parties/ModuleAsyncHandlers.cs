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

        // КЕШИРОВАНИЕ
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
                                         Logger.Debug($"Created Company with: " +
                                                      $"ID: {company.Id}| " +
                                                      $"ExternalID: {company.ExternalId}| " +
                                                      $"Name: {company.Name}| " +
                                                      $"LegalName: {company.LegalName}| " +
                                                      $"INN: {company.TIN}| " +
                                                      $"KPP: {company.TRRC}| " +
                                                      $"OKPO: {company.NCEO}| " +
                                                      $"IName: {company.Inamelitiko}| " +
                                                      $"Nonresident: {company.Nonresident}| " +
                                                      $"NuRezident: {company.NUNonrezidentlitiko}| " +
                                                      $"OKOPF: {(company.OKOPFlitiko != null ? company.OKOPFlitiko.ExternalId : "null")}| " +
                                                      $"OKFS: {(company.OKFSlitiko != null ? company.OKFSlitiko.ExternalId : "null")}| " +
                                                      $"OKONH: {(company.OKONHlitiko != null ? company.OKONHlitiko.ExternalId : "null")}| " +
                                                      $"OKVED: {(company.OKVEDlitiko != null ? company.OKVEDlitiko.ExternalId : "null")}| " +
                                                      $"RegNum: {company.RegNumlitiko}| " +
                                                      $"Numbers: {company.Numberslitiko}| " +
                                                      $"Business: {company.Businesslitiko}| " +
                                                      $"EnterpriseType: {(company.EnterpriseTypelitiko != null ? company.EnterpriseTypelitiko.Name : "null")}| " +
                                                      $"Country: {(company.Countrylitiko != null ? company.Countrylitiko.Name : "null")}| " +
                                                      $"City: {(company.City != null ? company.City.Name : "null")}| " +
                                                      $"AddressType: {company.AddressTypelitiko}| " +
                                                      $"PostAddress: {company.PostalAddress}| " +
                                                      $"LegalAddress: {company.LegalAddress}| " +
                                                      $"Street: {company.Streetlitiko}| " +
                                                      $"BuildingNumber: {company.HouseNumberlitiko}| " +
                                                      $"Phone: {company.Phones}| " +
                                                      $"Email: {company.Email}| " +
                                                      $"Bank: {company.Bank}| " +
                                                      $"WebSite: {company.Homepage}| " +
                                                      $"VatPayer: {company.VATPayerlitiko}| " +
                                                      $"Reliability: {company.Reliabilitylitiko}| " +
                                                      $"CorrAcc: {company.Account}| " +
                                                      $"InternalAcc: {company.AccountEskhatalitiko}|");
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
                                         Logger.Debug($"Created Person with: " +
                                                      $"Id={person.Id}| " +
                                                      $"ExternalID={person.ExternalId}| " +
                                                      $"LastName={person.LastName}| " +
                                                      $"FirstName={person.FirstName}| " +
                                                      $"MiddleName={person.MiddleName}| " +
                                                      $"Nonresident={person.Nonresident}| " +
                                                      $"NuRezident={person.NUNonrezidentlitiko}| " +
                                                      $"IName={person.Inamelitiko}| " +
                                                      $"DatePers={person.DateOfBirth:dd.MM.yyyy}| " +
                                                      $"Sex={person.Sex}| " +
                                                      $"MariageSt={(person.FamilyStatuslitiko != null ? person.FamilyStatuslitiko.Name : "null")}| " +
                                                      $"INN={person.TIN}| " +
                                                      $"IIN={person.SINlitiko}| " +
                                                      $"Country={(person.Citizenship != null ? person.Citizenship.Name : "null")}| " +
                                                      $"DocBirthPlace={person.BirthPlace}| " +
                                                      $"PostAddress={person.PostalAddress}| " +
                                                      $"Email={person.Email}| " +
                                                      $"Bank={person.Bank}|" +
                                                      $"Phone={person.Phones}| " +
                                                      $"City={(person.City != null ? person.City.Name : "null")}| " +
                                                      $"AddressType: {person.AddressTypelitiko}| " +
                                                      $"Street={person.Streetlitiko}| " +
                                                      $"BuildingNumber={person.HouseNumberlitiko}| " +
                                                      $"WebSite={person.Homepage}| " +
                                                      $"TaxNonResident={person.NUNonrezidentlitiko}| " +
                                                      $"VatPayer={person.VATPayerlitiko}| " +
                                                      $"Reliability={person.Reliabilitylitiko}| " +
                                                      $"CorrAcc={person.Account}| " +
                                                      $"InternalAcc={person.AccountEskhatalitiko}| " +
                                                      $"Identity -> " +
                                                      $"Kind={(person.IdentityKind != null ? person.IdentityKind.Name : "null")}| " +
                                                      $"Num={person.IdentityNumber}| " +
                                                      $"Ser={person.IdentitySeries}| " +
                                                      $"Who={person.IdentityAuthority}| " +
                                                      $"DateBegin={person.IdentityDateOfIssue:dd.MM.yyyy}| " +
                                                      $"DateEnd={person.IdentityExpirationDate:dd.MM.yyyy}|");
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
      var isExternalID = companyElement.Element("ExternalID")?.Value.Trim();
      var isINN = companyElement.Element("INN")?.Value.Trim();
      var isOKOPF = companyElement.Element("FORMA")?.Value.Trim();
      var isOKFS = companyElement.Element("OWNERSHIP")?.Value.Trim();
      var isCodeOKONHelements = companyElement.Element("CODE_OKONH")?.Elements("element");
      var isCodeOKVEDelements = companyElement.Element("CODE_OKVED")?.Elements("element");
      var isNumbers = companyElement.Element("NUMBERS")?.Value.Trim();
      var isPS_REF = companyElement.Element("PS_REF")?.Value;
      var isCountry = companyElement.Element("COUNTRY")?.Value;
      var isCity = companyElement.Element("City")?.Value;
      var isAddressType = companyElement.Element("AddressType")?.Value;
      var isBank = companyElement.Element("Bank")?.Value;
      var isTaxNonResident = companyElement.Element("TaxNonResident")?.Value;
      var isReliability = companyElement.Element("Reliability")?.Value;
      
      
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
      
      litiko.NSI.IOKOPF foundOkopf = null;
      
      if (!string.IsNullOrEmpty(isOKOPF) && okopfDict.TryGetValue(isOKOPF, out foundOkopf))
        company.OKOPFlitiko = foundOkopf;
      
      litiko.NSI.IOKFS foundOkfs = null;
      if (!string.IsNullOrEmpty(isOKFS) && okfsDict.TryGetValue(isOKFS, out foundOkfs))
        company.OKFSlitiko = foundOkfs;
      
      if (isCodeOKONHelements != null)
      {
        litiko.NSI.IOKONH foundOkonh = null;
        
        var firstVal = isCodeOKONHelements.FirstOrDefault(n => !string.IsNullOrWhiteSpace(n?.Value))?.Value?.Trim();
        
        if (!string.IsNullOrEmpty(firstVal) && okonhDict.TryGetValue(firstVal, out foundOkonh))
        {
          if (!object.Equals(company.OKONHlitiko, foundOkonh))
            company.OKONHlitiko = foundOkonh;
        }
      }
      if (isCodeOKVEDelements != null)
      {
        litiko.NSI.IOKVED foundOkved = null;
        
        var firstVal = isCodeOKVEDelements.FirstOrDefault(n => !string.IsNullOrWhiteSpace(n?.Value))?.Value?.Trim();
        
        if (!string.IsNullOrEmpty(firstVal) && okvedDict.TryGetValue(firstVal, out foundOkved))
        {
          if (!object.Equals(company.OKVEDlitiko, foundOkved))
            company.OKVEDlitiko = foundOkved;
        }
      }
      
      company.EINlitiko = companyElement.Element("IIN")?.Value.Trim(); // у юрлица вместо иин идет РЯМ
      
      company.RegNumlitiko = companyElement.Element("REGIST_NUM")?.Value;
      
      if (!string.IsNullOrEmpty(isNumbers))
        company.Numberslitiko = string.IsNullOrEmpty(isNumbers) ? (int?)null : int.Parse(isNumbers);
      
      company.Businesslitiko = companyElement.Element("BUSINESS")?.Value;
      
      if (!string.IsNullOrEmpty(isPS_REF))
      {
        var enterpriseType = litiko.NSI.EnterpriseTypes.GetAll().FirstOrDefault(x => x.ExternalId == isPS_REF);
        company.EnterpriseTypelitiko = enterpriseType;
      }
      
      ICountry foundCountry = null;
      if (!string.IsNullOrEmpty(isCountry) && countryDict.TryGetValue(isCountry, out foundCountry))
        company.Countrylitiko = foundCountry;
      
      company.PostalAddress = companyElement.Element("PostAdress")?.Value;
      
      company.LegalAddress = companyElement.Element("LegalAdress")?.Value;
      
      company.Phones = companyElement.Element("Phone")?.Value;
      
      ICity foundCity = null;
      if (!string.IsNullOrEmpty(isCity) && cityDict.TryGetValue(isCity, out foundCity))
        company.City = foundCity;
      
      litiko.NSI.IAddressType foundAddressType = null;
      if (!string.IsNullOrEmpty(isAddressType) && addressTypeDict.TryGetValue(isAddressType, out foundAddressType))
        company.AddressTypelitiko = foundAddressType;
      
      company.Streetlitiko = companyElement.Element("Street")?.Value;
      
      company.HouseNumberlitiko = companyElement.Element("BuildingNumber")?.Value;
      
      company.Email = companyElement.Element("Email")?.Value;
      
      company.Homepage = companyElement.Element("WebSite")?.Value;
      
      if (!string.IsNullOrEmpty(isTaxNonResident))
        company.NUNonrezidentlitiko = ParseBoolSafe(isTaxNonResident);
      
      company.VATPayerlitiko = ParseBoolSafe(companyElement.Element("VATPayer")?.Value);
      
      company.Account = companyElement.Element("CorrAcc")?.Value;
      
      company.AccountEskhatalitiko = companyElement.Element("InternalAcc")?.Value;
      
      if (!string.IsNullOrEmpty(isBank))
      {
        var bank = Sungero.Parties.Banks.GetAll().FirstOrDefault(x => x.ExternalId == isBank);
        company.Bank = bank;
      }

      if (!string.IsNullOrWhiteSpace(isReliability))
      {
        Sungero.Core.Enumeration? reliabilityEnum = null;
        var relTrim = isReliability.Trim();
        if (relTrim.Equals("Надежный", StringComparison.OrdinalIgnoreCase) || relTrim.Equals("Высокий", StringComparison.OrdinalIgnoreCase) || relTrim.Equals("НИЗКИЙ", StringComparison.OrdinalIgnoreCase) || relTrim.Equals("ВЫСОКИЙ", StringComparison.OrdinalIgnoreCase))
          reliabilityEnum = litiko.Eskhata.Company.Reliabilitylitiko.Reliable;
        else if (relTrim.Equals("Не надежный", StringComparison.OrdinalIgnoreCase) || relTrim.Equals("Низкая", StringComparison.OrdinalIgnoreCase) || relTrim.Equals("НИЗКИЙ", StringComparison.OrdinalIgnoreCase) || relTrim.Equals("ВЫСОКИЙ", StringComparison.OrdinalIgnoreCase))
          reliabilityEnum = litiko.Eskhata.Company.Reliabilitylitiko.NotReliable;
        
        if (reliabilityEnum.HasValue) company.Reliabilitylitiko = reliabilityEnum;
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
      var isCountry = personElement.Element("COUNTRY")?.Value;
      var isDateOfBirth = personElement.Element("DATE_PERS")?.Value;
      var isFamilyStatus = personElement.Element("MARIGE_ST")?.Value;
      var isDocBirthPlace = personElement.Element("DOC_BIRTH_PLACE")?.Value;
      var isPostAdress = personElement.Element("PostAdress")?.Value;
      var isWebSite = personElement.Element("WebSite")?.Value;
      var isCity = personElement.Element("City")?.Value;
      var isAddressType = personElement.Element("AddressType")?.Value;
      var isStreet = personElement.Element("Street")?.Value;
      var isBuildingNumber = personElement.Element("BuildingNumber")?.Value;
      var isTaxNonResident = personElement.Element("TaxNonResident")?.Value;
      var isVatPayer = personElement.Element("VATPayer")?.Value;
      var isReliability = personElement.Element("Reliability")?.Value;
      var isCorrAcc = personElement.Element("CorrAcc")?.Value;
      var isInternalAcc = personElement.Element("InternalAcc")?.Value;
      var isBank = personElement.Element("Bank")?.Value;
      
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
      
      person.NUNonrezidentlitiko = ParseBoolSafe(personElement.Element("NU_REZIDENT")?.Value);
      
      person.Inamelitiko = personElement.Element("I_NAME")?.Value?.Trim();
      
      var parsedDate = TryParseDate(isDateOfBirth);
      if (parsedDate.HasValue)
        person.DateOfBirth = parsedDate.Value;
      
      var sex = personElement.Element("SEX")?.Value;
      if (sex == "М") person.Sex = Eskhata.Person.Sex.Male;
      else if (sex == "Ж") person.Sex = Eskhata.Person.Sex.Female;
      
      if (!string.IsNullOrEmpty(isFamilyStatus))
      {
        var familyStatus = litiko.NSI.FamilyStatuses.GetAll().FirstOrDefault(x => x.ExternalId == isFamilyStatus);
        if (familyStatus != null) person.FamilyStatuslitiko = familyStatus;
      }
      
      if (!string.IsNullOrEmpty(isINN))
        person.TIN = isINN;
      
      ICountry foundCountry = null;
      if (!string.IsNullOrEmpty(isCountry) && countryDict.TryGetValue(isCountry, out foundCountry))
        person.Citizenship = foundCountry;
      
      if (!string.IsNullOrEmpty(isDocBirthPlace))
        person.BirthPlace = isDocBirthPlace.Trim();
      
      if (!string.IsNullOrEmpty(isPostAdress))
        person.PostalAddress = isPostAdress.Trim();
      
      person.Email = personElement.Element("Email")?.Value;
      
      person.Phones = personElement.Element("Phone")?.Value;
      
      if (!string.IsNullOrEmpty(isWebSite))
        person.Homepage = isWebSite.Trim();
      
      ICity foundCity = null;
      if (!string.IsNullOrEmpty(isCity) && cityDict.TryGetValue(isCity, out foundCity))
        person.City = foundCity;
      
      litiko.NSI.IAddressType foundAddressType = null;
      if (!string.IsNullOrEmpty(isAddressType) && addressTypeDict.TryGetValue(isAddressType, out foundAddressType))
        person.AddressTypelitiko = foundAddressType;
      
      if (!string.IsNullOrEmpty(isStreet))
        person.Streetlitiko = isStreet.Trim();
      
      if (!string.IsNullOrEmpty(isBuildingNumber))
        person.HouseNumberlitiko = isBuildingNumber.Trim();
      
      if (!string.IsNullOrEmpty(isTaxNonResident))
        person.NUNonrezidentlitiko = ParseBoolSafe(isTaxNonResident);
      
      if (!string.IsNullOrEmpty(isVatPayer))
        person.VATPayerlitiko = ParseBoolSafe(isVatPayer);
      
      if (!string.IsNullOrEmpty(isReliability))
      {
        Sungero.Core.Enumeration? reliabilityEnum = null;
        var relTrim = isReliability.Trim();
        if (relTrim == "Надежный") reliabilityEnum = litiko.Eskhata.Person.Reliabilitylitiko.Reliable;
        else if (relTrim == "Не надежный") reliabilityEnum = litiko.Eskhata.Person.Reliabilitylitiko.NotReliable;
        
        if (reliabilityEnum.HasValue) person.Reliabilitylitiko = reliabilityEnum;
      }
      
      if (!string.IsNullOrEmpty(isCorrAcc))
        person.Account = isCorrAcc.Trim();
      
      if (!string.IsNullOrEmpty(isInternalAcc))
        person.AccountEskhatalitiko = isInternalAcc.Trim();

      if (!string.IsNullOrEmpty(isBank))
      {
        var bank = Sungero.Parties.Banks.GetAll().FirstOrDefault(x => x.ExternalId == isBank);
        person.Bank = bank;
      }
      
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
    public virtual void DeleteMigratedPartiesAsynclitiko(litiko.Eskhata.Module.Parties.Server.AsyncHandlerInvokeArgs.DeleteMigratedPartiesAsynclitikoInvokeArgs args)
    {
      args.Retry = false;
      int deletedCount = 0;
      int errorCount = 0;
      var errorDetails = new List<string>();

      var companyIds = Eskhata.Companies.GetAll(c => c.IsMigratedlitiko == true).Select(c => c.Id).ToList();
      var personIds = Eskhata.People.GetAll(p => p.IsMigratedlitiko == true).Select(p => p.Id).ToList();
      
      Logger.DebugFormat("[MIGR_DELETE] Начинаю удаление: Компаний - {0}, Персон - {1}", companyIds.Count, personIds.Count);

      Action<long, bool> safeDelete = (id, isCompany) =>
      {
        try
        {
          Transactions.Execute(() =>
                               {
                                 IEntity obj = isCompany ? (IEntity)Eskhata.Companies.Get(id) : (IEntity)Eskhata.People.Get(id);
                                 if (obj != null)
                                 {
                                   if (Locks.GetLockInfo(obj).IsLocked) Locks.Unlock(obj);
                                   
                                   if (isCompany)
                                     Eskhata.Companies.Delete((litiko.Eskhata.ICompany)obj);
                                   else
                                     Eskhata.People.Delete((litiko.Eskhata.IPerson)obj);
                                   
                                   deletedCount++;
                                 }
                               });
        }
        catch (Exception ex)
        {
          errorCount++;
          
          if (errorDetails.Any())
            errorDetails.Add(ex.Message);
          
          Logger.DebugFormat("[MIGR_DELETE] Не удалось удалить {0} ID {1}: {2}", isCompany ? "Компанию" : "Персону", id, ex.Message);
        }
      };

      foreach (var id in companyIds) safeDelete(id, true);
      foreach (var id in personIds) safeDelete(id, false);

      var author = Employees.GetAll(e => e.Id == args.AuthorId).FirstOrDefault();
      if (author != null)
      {
        var notice = Sungero.Workflow.SimpleTasks.CreateWithNotices("Результат очистки контрагентов", author);
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Очистка завершена.");
        sb.AppendLine(string.Format("✅ Удалено: {0}", deletedCount));
        sb.AppendLine(string.Format("❌ Не удалено (есть ссылки или блокировки): {0}", errorCount));
        
        if (errorDetails.Any())
        {
          sb.AppendLine("\nПричины ошибок:");
          foreach (var err in errorDetails) sb.AppendLine("- " + err);
        }
        
        notice.ActiveText = sb.ToString();
        notice.Start();
      }
    }

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

//    // Вспомогательный метод для пакетного удаления
//    private int DeleteEntitiesBatch(List<long> ids, Type type, ref int errors)
//    {
//      int deletedCount = 0;
//      int batchSize = 50;
//      for (int i = 0; i < ids.Count; i += batchSize)
//      {
//        var batch = ids.Skip(i).Take(batchSize).ToList();
//        Transactions.Execute(() =>
//                             {
//                               foreach (var id in batch)
//                               {
//                                 try
//                                 {
//                                   var entity = (type == typeof(litiko.Eskhata.ICompany))
//                                     ? (IEntity)Eskhata.Companies.Get(id)
//                                     : (IEntity)Eskhata.People.Get(id);
//                                   
//                                   if (entity != null)
//                                   {
//                                     if (Locks.GetLockInfo(entity).IsLocked) Locks.Unlock(entity);
//                                     if (type == typeof(litiko.Eskhata.ICompany)) Eskhata.Companies.Delete((litiko.Eskhata.ICompany)entity);
//                                     else Eskhata.People.Delete((litiko.Eskhata.IPerson)entity);
//                                     deletedCount++;
//                                   }
//                                 }
//                                 catch {  }
//                               }
//                             });
//      }
//      return deletedCount;
//    }
//
//    private void NotifyAuthor(int authorId, string title, litiko.Eskhata.Module.Parties.Structures.Module.IResultImportCounterpartyXml res)
//    {
//      var author = Employees.GetAll(e => e.Id == authorId).FirstOrDefault();
//      if (author == null) return;
//
//      var notice = Sungero.Workflow.SimpleTasks.CreateWithNotices(title, author);
//      var sb = new System.Text.StringBuilder();
//      sb.AppendLine("📊 Результаты миграции:");
//      sb.AppendLine(string.Format("• Компании: {0} (Новых: {1}, Дублей: {2})", res.TotalCompanies, res.ImportedCompanies, res.DuplicateCompanies));
//      sb.AppendLine(string.Format("• Персоны: {0} (Новых: {1}, Дублей: {2})", res.TotalPersons, res.ImportedPersons, res.DuplicatePersons));
//      sb.AppendLine(string.Format("❌ Ошибок: {0}", res.Errors.Count));
//      
//      if (res.Errors.Any())
//      {
//        sb.AppendLine("\n⚠️ Ошибки:");
//        foreach (var err in res.Errors) sb.AppendLine(err);
//      }
//      
//      notice.ActiveText = sb.ToString();
//      notice.Start();
//    }
  }
}