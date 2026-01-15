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

namespace litiko.Eskhata.Module.Parties.Server
{
  partial class ModuleFunctions
  {
    [Public, Remote]
    public IResultImportCounterpartyXml ImportCounterpartyFromXml(string fileBase64, string fileName)
    {
      var result = litiko.Eskhata.Module.Parties.Structures.Module.ResultImportCounterpartyXml.Create();
      result.Errors = new List<string>();
      result.ImportedCount = 0;
      result.ImportedCompanies = 0;
      result.ImportedPersons = 0;
      result.TotalCompanies = 0;
      result.TotalPersons = 0;
      result.DuplicateCompanies = 0;
      result.DuplicatePersons = 0;

      Logger.Debug("Start import counterparty from file: {0}", fileName);

      byte[] fileBytes;
      
      try
      {
        if(string.IsNullOrEmpty(fileBase64))
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
        
        using(var stream = new MemoryStream(fileBytes))
        {
          xDoc = XDocument.Load(stream);
        }
        
        var counterpartyRoot = xDoc.Element("Counterparty");
        if (counterpartyRoot == null)
        {
          result.Errors.Add("Корневой элемент <Counterparty> не найден.");
          return result;
        }

        var nodes = counterpartyRoot.Elements().ToList();
        Logger.Debug($"Found {nodes.Count} child nodes under <Counterparty>");

        Logger.Debug("Loading dictionaries to cache...");
        
        var okonhDict = litiko.NSI.OKONHs.GetAll()
          .Where(x => x.ExternalId != null && x.ExternalId != "")
          .ToDictionary(x => x.ExternalId);

        var okvedDict = litiko.NSI.OKVEDs.GetAll()
          .Where(x => x.ExternalId != null && x.ExternalId != "")
          .ToDictionary(x => x.ExternalId);

        var okopfDict = litiko.NSI.OKOPFs.GetAll()
          .Where(x => x.ExternalId != null && x.ExternalId != "")
          .ToDictionary(x => x.ExternalId);
        
        var okfsDict = litiko.NSI.OKFSes.GetAll()
          .Where(x => x.ExternalId != null && x.ExternalId != "")
          .ToDictionary(x => x.ExternalId);

        var countryDict = litiko.Eskhata.Countries.GetAll()
          .Where(x => x.ExternalIdlitiko != null && x.ExternalIdlitiko != "")
          .ToDictionary(x => x.ExternalIdlitiko);

        var cityDict = litiko.Eskhata.Cities.GetAll()
          .Where(x => x.ExternalIdlitiko != null && x.ExternalIdlitiko != "")
          .ToDictionary(x => x.ExternalIdlitiko);

        var addressTypeDict = litiko.NSI.AddressTypes.GetAll()
          .Where(x => x.ExternalId != null && x.ExternalId != "")
          .ToDictionary(x=>x.ExternalId);

        int nodeIndex = 0;

        foreach (var node in nodes)
        {
          nodeIndex++;
          string recordInfo = $"Запись №{nodeIndex} ({node.Name.LocalName})";
          
          try
          {
            var tmpName = node.Name.LocalName == "Company" ? node.Element("Name")?.Value : node.Element("LastName")?.Value;
            var tmpInn = node.Element("INN")?.Value;
            if (!string.IsNullOrEmpty(tmpName)) recordInfo += $" '{tmpName}'";
            if (!string.IsNullOrEmpty(tmpInn)) recordInfo += $" (ИНН: {tmpInn})";

            switch (node.Name.LocalName)
            {
              case "Company":
                {
                  var company = ParseCompany(node, okonhDict, okvedDict, okopfDict, okfsDict, countryDict, cityDict, addressTypeDict);
                  
                  if (company != null)
                  {
                    bool isNew = company.State.IsInserted;
                    
                    result.TotalCompanies++;

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

                    if (isNew)
                    {
                      result.ImportedCompanies++;
                      result.ImportedCount++;
                    }
                    else
                    {
                      result.DuplicateCompanies++;
                    }
                  }
                  break;
                }

              case "Person":
                {
                  var person = ParsePerson(node, countryDict, cityDict, addressTypeDict);
                  if (person != null)
                  {
                    bool isNew = person.State.IsInserted;
                    
                    result.TotalPersons++;

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
                    
                    if (isNew)
                    {
                      result.ImportedPersons++;
                      result.ImportedCount++;
                    }
                    else
                    {
                      result.DuplicatePersons++;
                    }
                  }
                  break;
                }

              default:
                Logger.Debug($"Unknown tag <{node.Name.LocalName}> inside <Counterparty>");
                break;
            }
          }
          catch (Exception ex)
          {
            var msg = $"❌ Ошибка в {recordInfo}: {ex.Message}";
            if (ex.InnerException != null) msg += $" -> {ex.InnerException.Message}";
            
            result.Errors.Add(msg);
            Logger.Error(msg, ex);
          }
        }

        result.TotalCount = result.TotalCompanies + result.TotalPersons;
      }
      catch (Exception ex)
      {
        var error = $"Критическая ошибка импорта: {ex.Message}";
        result.Errors.Add(error);
        Logger.Error(error, ex);
      }
      
      Logger.Debug("End import counterparty from file: {0}", fileName);
      return result;
    }

    private ICompany ParseCompany(XElement companyElement,
                                  Dictionary<string, litiko.NSI.IOKONH> okonhDict,
                                  Dictionary<string, litiko.NSI.IOKVED> okvedDict,
                                  Dictionary<string, litiko.NSI.IOKOPF> okopfDict,
                                  Dictionary<string, litiko.NSI.IOKFS> okfsDict,
                                  Dictionary<string, litiko.Eskhata.ICountry> countryDict,
                                  Dictionary<string, litiko.Eskhata.ICity> cityDict,
                                  Dictionary<string, litiko.NSI.IAddressType> addressType)
    {
      if (companyElement == null) return null;

      var isId = companyElement.Element("ID")?.Value;
      var isExternalID = companyElement.Element("ExternalID")?.Value;
      var isName = companyElement.Element("Name")?.Value.Trim();
      var isINN = companyElement.Element("INN")?.Value;
      var isLongName = companyElement.Element("LONG_NAME")?.Value.Trim();
      var isIName = companyElement.Element("I_NAME")?.Value.Trim();
      var isRezident = companyElement.Element("REZIDENT")?.Value;
      var isNunRezident = companyElement.Element("NU_REZIDENT")?.Value;
      var isKPP = companyElement.Element("KPP")?.Value;
      var isOKPO = companyElement.Element("KOD_OKPO")?.Value;
      var isOKOPF = companyElement.Element("FORMA")?.Value;
      var isOKFS = companyElement.Element("OWNERSHIP")?.Value;
      var isRegistnum = companyElement.Element("REGIST_NUM")?.Value;
      var isNumbers = companyElement.Element("NUMBERS")?.Value;
      var isBusiness = companyElement.Element("BUSINESS")?.Value;
      var isPS_REF = companyElement.Element("PS_REF")?.Value;
      var isCountry = companyElement.Element("COUNTRY")?.Value;
      var isPostAdress = companyElement.Element("PostAdress")?.Value;
      var isLegalAdress = companyElement.Element("LegalAdress")?.Value;
      var isPhone = companyElement.Element("Phone")?.Value;
      var isCity = companyElement.Element("City")?.Value;
      var isStreet = companyElement.Element("Street")?.Value;
      var isBuildingNumber = companyElement.Element("BuildingNumber")?.Value;
      var isEmail = companyElement.Element("Email")?.Value;
      var isBank = companyElement.Element("Bank")?.Value;
      var isWebSite = companyElement.Element("WebSite")?.Value;
      var isTaxNonResident = companyElement.Element("TaxNonResident")?.Value;
      var isVatPayer = companyElement.Element("VATPayer")?.Value;
      var isReliability = companyElement.Element("Reliability")?.Value;
      var isCorrAcc = companyElement.Element("CorrAcc")?.Value;
      var isInernalAcc = companyElement.Element("InternalAcc")?.Value;
      var isAddressType = companyElement.Element("AddressType")?.Value;

      var company = litiko.Eskhata.Companies.GetAll()
        .FirstOrDefault(x => (!string.IsNullOrEmpty(isExternalID) && x.ExternalId == isExternalID) ||
                        (!string.IsNullOrEmpty(isINN) && x.TIN == isINN));

      bool isNew = company == null;

      if (isNew)
      {
        company = litiko.Eskhata.Companies.Create();
        company.ExternalId = isExternalID;
        company.TIN = isINN;
      }
      
      if (!string.IsNullOrEmpty(isName)) company.Name = isName;
      if (!string.IsNullOrEmpty(isLongName)) company.LegalName = isLongName;
      
      if (!string.IsNullOrEmpty(isIName))company.Inamelitiko = isIName;
      if (!string.IsNullOrEmpty(isRezident))company.Nonresident = ParseBoolSafe(isRezident);
      if (!string.IsNullOrEmpty(isNunRezident))company.NUNonrezidentlitiko = ParseBoolSafe(isNunRezident);
      if (!string.IsNullOrEmpty(isKPP))company.TRRC = isKPP;
      if (!string.IsNullOrEmpty(isOKPO))company.NCEO = isOKPO;

      litiko.NSI.IOKOPF foundOkopf = null;
      if (!string.IsNullOrEmpty(isOKOPF) && okopfDict.TryGetValue(isOKOPF, out foundOkopf))
        company.OKOPFlitiko = foundOkopf;

      litiko.NSI.IOKFS foundOkfs = null;
      if (!string.IsNullOrEmpty(isOKFS) && okfsDict.TryGetValue(isOKFS, out foundOkfs))
        company.OKFSlitiko = foundOkfs;

      var isCodeOKONHelements = companyElement.Element("CODE_OKONH")?.Elements("element");
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

      var isCodeOKVEDelements = companyElement.Element("CODE_OKVED")?.Elements("element");
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

      if (!string.IsNullOrEmpty(isRegistnum))company.RegNumlitiko = isRegistnum;
      if (!string.IsNullOrEmpty(isNumbers))company.Numberslitiko = string.IsNullOrEmpty(isNumbers) ? (int?)null : int.Parse(isNumbers);
      if (!string.IsNullOrEmpty(isBusiness))company.Businesslitiko = isBusiness;

      if (!string.IsNullOrEmpty(isPS_REF))
      {
        var enterpriseType = litiko.NSI.EnterpriseTypes.GetAll().FirstOrDefault(x => x.ExternalId == isPS_REF);
        company.EnterpriseTypelitiko = enterpriseType;
      }

      ICountry foundCountry = null;
      if (!string.IsNullOrEmpty(isCountry) && countryDict.TryGetValue(isCountry, out foundCountry))
        company.Countrylitiko = foundCountry;

      if (!string.IsNullOrEmpty(isPostAdress))company.PostalAddress = isPostAdress;
      if (!string.IsNullOrEmpty(isLegalAdress))company.LegalAddress = isLegalAdress;
      if (!string.IsNullOrEmpty(isPhone))company.Phones = isPhone;
      
      litiko.NSI.IAddressType foundAddressType = null;
      if (!string.IsNullOrEmpty(isAddressType) && addressType.TryGetValue(isAddressType, out foundAddressType))
        company.AddressTypelitiko = foundAddressType; 
      
      ICity foundCity = null;
      if (!string.IsNullOrEmpty(isCity) && cityDict.TryGetValue(isCity, out foundCity))
        company.City = foundCity;
      
      if (!string.IsNullOrEmpty(isStreet)) company.Streetlitiko = isStreet;
      if (!string.IsNullOrEmpty(isBuildingNumber)) company.HouseNumberlitiko = isBuildingNumber;

      var bank = Sungero.Parties.Banks.GetAll().FirstOrDefault(x => x.ExternalId == isBank);
      company.Bank = bank;
      
      if (!string.IsNullOrEmpty(isEmail))company.Email = isEmail;
      if (!string.IsNullOrEmpty(isWebSite))company.Homepage = isWebSite;
      if (!string.IsNullOrEmpty(isTaxNonResident))company.NUNonrezidentlitiko = ParseBoolSafe(isTaxNonResident);
      if (!string.IsNullOrEmpty(isVatPayer))company.VATPayerlitiko = ParseBoolSafe(isVatPayer);
      if (!string.IsNullOrEmpty(isInernalAcc))company.AccountEskhatalitiko = isInernalAcc;
      if (!string.IsNullOrEmpty(isCorrAcc))company.Account = isCorrAcc;

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

    private IPerson ParsePerson(XElement personElement, Dictionary<string, litiko.Eskhata.ICountry> countryDict, Dictionary<string, litiko.Eskhata.ICity> cityDict, Dictionary<string, litiko.NSI.IAddressType> addressType)
    {
      if (personElement == null) return null;

      var isExternalID = personElement.Element("ExternalID")?.Value;
      var isLastName = personElement.Element("LastName")?.Value;
      var isFirstName = personElement.Element("FirstName")?.Value;
      var isMiddleName = personElement.Element("MiddleName")?.Value;
      var isRezident = personElement.Element("REZIDENT")?.Value;
      var isNuRezident = personElement.Element("NU_REZIDENT")?.Value;
      var isIName = personElement.Element("I_NAME")?.Value;
      var isDateOfBirth = personElement.Element("DATE_PERS")?.Value;
      var isSex = personElement.Element("SEX")?.Value;
      var isFamilyStatus = personElement.Element("MARIGE_ST")?.Value;
      var isINN = personElement.Element("INN")?.Value;
      var isIIN = personElement.Element("IIN")?.Value;
      var isCountry = personElement.Element("COUNTRY")?.Value;
      var isDocBirthPlace = personElement.Element("DOC_BIRTH_PLACE")?.Value;
      var isPostAdress = personElement.Element("PostAdress")?.Value;
      var isEmail = personElement.Element("Email")?.Value;
      var isPhone = personElement.Element("Phone")?.Value;
      var isCity = personElement.Element("City")?.Value;
      var isStreet = personElement.Element("Street")?.Value;
      var isBuildingNumber = personElement.Element("BuildingNumber")?.Value;
      var isWebSite = personElement.Element("WebSite")?.Value;
      var isBank = personElement.Element("Bank")?.Value;
      var isTaxNonResident = personElement.Element("TaxNonResident")?.Value;
      var isVatPayer = personElement.Element("VATPayer")?.Value;
      var isReliability = personElement.Element("Reliability")?.Value;
      var isCorrAcc = personElement.Element("CorrAcc")?.Value;
      var isInternalAcc = personElement.Element("InternalAcc")?.Value;
      var isAddressType = personElement.Element("AddressType")?.Value;
      
      var person = Eskhata.People.GetAll()
        .FirstOrDefault(x =>  (!string.IsNullOrEmpty(isExternalID) && x.ExternalId == isExternalID) || (!string.IsNullOrEmpty(isINN) && x.TIN == isINN));
      
      bool isNew = person == null;

      if (isNew)
      {
        person = Eskhata.People.Create();
        person.ExternalId = isExternalID;
      }

      if (!string.IsNullOrEmpty(isLastName)) person.LastName = isLastName.Trim();
      if (!string.IsNullOrEmpty(isFirstName)) person.FirstName = isFirstName.Trim();
      if (!string.IsNullOrEmpty(isMiddleName)) person.MiddleName = isMiddleName.Trim();

      if (!string.IsNullOrEmpty(isRezident)) person.Nonresident = ParseBoolSafe(isRezident);
      if (!string.IsNullOrEmpty(isNuRezident)) person.NUNonrezidentlitiko = ParseBoolSafe(isNuRezident);
      if (!string.IsNullOrEmpty(isIName)) person.Inamelitiko = isIName.Trim();

      var parsedDate = TryParseDate(isDateOfBirth);
      if (parsedDate.HasValue) person.DateOfBirth = parsedDate.Value;

      if (isSex == "М") person.Sex = Eskhata.Person.Sex.Male;
      else if (isSex == "Ж") person.Sex = Eskhata.Person.Sex.Female;

      if (!string.IsNullOrEmpty(isFamilyStatus))
      {
        var familyStatus = litiko.NSI.FamilyStatuses.GetAll().FirstOrDefault(x => x.ExternalId == isFamilyStatus);
        if (familyStatus != null) person.FamilyStatuslitiko = familyStatus;
      }

      if (!string.IsNullOrEmpty(isINN)) person.TIN = isINN;
      if (!string.IsNullOrEmpty(isIIN)) person.SINlitiko = isIIN;

      ICountry foundCountry = null;
      if (!string.IsNullOrEmpty(isCountry) && countryDict.TryGetValue(isCountry, out foundCountry))
        person.Citizenship = foundCountry;

      if (!string.IsNullOrEmpty(isDocBirthPlace)) person.BirthPlace = isDocBirthPlace.Trim();
      if (!string.IsNullOrEmpty(isPostAdress)) person.PostalAddress = isPostAdress.Trim();
      if (!string.IsNullOrEmpty(isEmail)) person.Email = isEmail.Trim();
      if (!string.IsNullOrEmpty(isPhone)) person.Phones = isPhone.Trim();

      litiko.NSI.IAddressType foundAddressType = null;
      if (!string.IsNullOrEmpty(isAddressType) && addressType.TryGetValue(isAddressType, out foundAddressType))
        person.AddressTypelitiko = foundAddressType; 
        
      ICity foundCity = null;
      if (!string.IsNullOrEmpty(isCity) && cityDict.TryGetValue(isCity, out foundCity))
        person.City = foundCity;

      if (!string.IsNullOrEmpty(isStreet)) person.Streetlitiko = isStreet.Trim();
      if (!string.IsNullOrEmpty(isBuildingNumber)) person.HouseNumberlitiko = isBuildingNumber.Trim();
      if (!string.IsNullOrEmpty(isWebSite)) person.Homepage = isWebSite.Trim();
      if (!string.IsNullOrEmpty(isBank))
      {
        var bank = Sungero.Parties.Banks.GetAll().FirstOrDefault(x => x.ExternalId == isBank);
        person.Bank = bank;
      }
      if (!string.IsNullOrEmpty(isTaxNonResident)) person.NUNonrezidentlitiko = ParseBoolSafe(isTaxNonResident);
      if (!string.IsNullOrEmpty(isVatPayer)) person.VATPayerlitiko = ParseBoolSafe(isVatPayer);
      
      if (!string.IsNullOrEmpty(isReliability))
      {
        Sungero.Core.Enumeration? reliabilityEnum = null;
        var relTrim = isReliability.Trim();
        if (relTrim == "Надежный") reliabilityEnum = litiko.Eskhata.Person.Reliabilitylitiko.Reliable;
        else if (relTrim == "Не надежный") reliabilityEnum = litiko.Eskhata.Person.Reliabilitylitiko.NotReliable;
        
        if (reliabilityEnum.HasValue) person.Reliabilitylitiko = reliabilityEnum;
      }
      
      if (!string.IsNullOrEmpty(isCorrAcc)) person.Account = isCorrAcc.Trim();
      if (!string.IsNullOrEmpty(isInternalAcc)) person.AccountEskhatalitiko = isInternalAcc.Trim();

      var identityElement = personElement.Element("IdentityDocument");
      
      if (identityElement != null)
      {
        var xmlType = identityElement.Element("TYPE")?.Value;
        var xmlName = identityElement.Element("NAME")?.Value;
        
        var identityKind = Sungero.Parties.IdentityDocumentKinds.GetAll()
          .FirstOrDefault(x => x.SID == xmlType || x.Name == xmlName);
        
        if (identityKind != null)
        {
          person.IdentityKind = identityKind;
        }
        else
        {
          Logger.Error("IdentityKind is not found! Identity data`s is not full.");
        }
        
        person.IdentityNumber = identityElement.Element("NUM")?.Value;
        person.IdentitySeries = identityElement.Element("SER")?.Value;
        
        person.IdentityAuthority = identityElement.Element("WHO")?.Value;
        
        person.IdentityDateOfIssue = TryParseDate(identityElement.Element("DATE_BEGIN")?.Value);
        person.IdentityExpirationDate = TryParseDate(identityElement.Element("DATE_END")?.Value);
      }

      return person;
    }
    private static DateTime? TryParseDate(string date)
    {
      var result = DateTime.MinValue;
      if (DateTime.TryParseExact(date,"dd.MM.yyyy",null,System.Globalization.DateTimeStyles.None,out result))
        return result;
      return null;
    }

    private static double ParseDoubleSafe(string value)
    {
      if (string.IsNullOrWhiteSpace(value))
        return 0.0;
      double r;
      if (double.TryParse(value.Trim().Replace(',', '.'),System.Globalization.NumberStyles.Any,System.Globalization.CultureInfo.InvariantCulture,out r))
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
      var norm = value.Trim().ToLowerInvariant();
      if (norm == "1" || norm == "true" || norm == "yes")
        return true;
      return false;
    }
  }
}
