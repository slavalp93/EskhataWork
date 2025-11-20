using System;
using System.Collections.Generic;
using System.Linq;
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
    /*[Public, Remote]
    public IResultImportCounterpartyXml ImportCounterpartyFromXml()
    {
      var result =
        litiko.Eskhata.Module.Parties.Structures.Module.ResultImportCounterpartyXml.Create();
      result.Errors = new List<string>();
      // SkippedEntities больше не нужен в этой логике, так как мы либо создаем, либо обновляем
      result.ImportedCount = 0; // Будет считать только НОВЫХ
      result.TotalCompanies = 0; // Всего компаний в XML
      result.ImportedCompanies = 0; // Новых компаний
      result.TotalPersons = 0; // Всего персон в XML
      result.ImportedPersons = 0; // Новых персон

      Logger.Debug("Import counterparties from XML - Start");

      var xmlPathFile = "Counterparty.xml";

      if (!System.IO.File.Exists(xmlPathFile))
      {
        result.Errors.Add($"Файл '{xmlPathFile}' не найден");
        Logger.Error($"XML File {xmlPathFile} is not found");
        return result;
      }

      try
      {
        XDocument xDoc = XDocument.Load(xmlPathFile);
        var counterpartyElements = xDoc.Descendants("Counterparty").ToList();

        if (!counterpartyElements.Any())
        {
          result.Errors.Add("В файле нет элементов <Counterparty> для импорта");
          Logger.Error("No <Counterparty> elements found");
          return result;
        }

        Logger.DebugFormat(
          "Found {0} <Counterparty> nodes in XML.",
          counterpartyElements.Count
         );

        for (int i = 0; i < counterpartyElements.Count; i++)
        {
          var counterpartyNode = counterpartyElements[i];
          result.TotalCount++;

          try
          {
            var companies = counterpartyNode.Elements("Company").ToList();
            var persons = counterpartyNode.Elements("Person").ToList();

            foreach (var companyElement in companies)
            {
              var company = ParseCompany(companyElement);
            }

            foreach (var personElement in persons)
            {
              var person = ParsePerson(personElement);
            }

          }
          catch (Exception ex)
          {
            result.Errors.Add($"Ошибка при обработке Counterparty №{i + 1}: {ex.Message}");
          }
        }
      }
      catch (Exception ex)
      {
        // Если произошла любая ошибка, откатываем все сделанные изменения
        Logger.Error("General error during Counterparty import, transaction rolled back.",ex);
        result.Errors.Add($"Критическая ошибка импорта, все изменения отменены: {ex.Message}");
      }
      result.TotalCount = result.TotalCompanies + result.TotalPersons;
      Logger.DebugFormat("Import counterparties finished. Total processed: {0}, Created: {1}, Errors: {2}",result.TotalCount,result.ImportedCount,result.Errors.Count);

      return result;
    }*/

    /*private List<object> ParseCounterparty(XElement counterpartyRoot)
    {
      var result = new List<object>();

      if (counterpartyRoot == null)
      {
        Logger.Debug("ParseCounterparty: <Counterparty> not found");
        return result;
      }

      // Берём ВСЕ дочерние элементы, сохраняя порядок в XML
      var nodes = counterpartyRoot.Elements().ToList();

      foreach (var node in nodes)
      {
        try
        {
          switch (node.Name.LocalName)
          {
            case "Company":
              var company = ParseCompany(node);
              if (company != null)
                result.Add(company);
              break;

            case "Person":
              var person = ParsePerson(node);
              if (person != null)
                result.Add(person);
              break;

            default:
              Logger.Debug($"ParseCounterparty: Unknown element <{node.Name}>");
              break;
          }
        }
        catch (Exception ex)
        {
          Logger.Error($"ParseCounterparty: error processing <{node.Name}>: {ex.Message}", ex);
        }
      }

      return result;
    }*/
[Public, Remote]
public IResultImportCounterpartyXml ImportCounterpartyFromXml()
{
    var result = litiko.Eskhata.Module.Parties.Structures.Module.ResultImportCounterpartyXml.Create();
    result.Errors = new List<string>();
    result.ImportedCount = 0;
    result.ImportedCompanies = 0;
    result.ImportedPersons = 0;
    result.TotalCompanies = 0;
    result.TotalPersons = 0;

    Logger.Debug("ImportCounterpartyFromXml: Start");

    var xmlPathFile = "Counterparty.xml";

    if (!System.IO.File.Exists(xmlPathFile))
    {
        result.Errors.Add($"Файл '{xmlPathFile}' не найден");
        return result;
    }

    try
    {
        var xDoc = XDocument.Load(xmlPathFile);

        var counterparty = xDoc.Element("Counterparty");
        if (counterparty == null)
        {
            result.Errors.Add("Корневой элемент <Counterparty> не найден.");
            return result;
        }

        // Берём ВСЕ дочерние элементы Company/Person в натуральном порядке
        var nodes = counterparty.Elements().ToList();
        Logger.Debug($"Found {nodes.Count} child nodes under <Counterparty>");

        foreach (var node in nodes)
        {
            try
            {
                switch (node.Name.LocalName)
                {
                    case "Company":
                    {
                        var company = ParseCompany(node);
                        if (company != null)
                        {
                            bool isNew = company.State.IsInserted;

                            company.Save();

                            if (isNew)
                            {
                                result.ImportedCompanies++;
                                result.ImportedCount++;
                            }

                            result.TotalCompanies++;

                            Logger.DebugFormat($"Created new Company with: " +
                                               $"Id={company.Id}, " +
                                               $"Name={company.Name}, " +
                                               $"ExternalId={company.ExternalId}," +
                                               $" INN={company.TIN}," +
                                               $" LegalName={company.LegalName}," +
                                               $" Inamelitiko={company.Inamelitiko}," +
                                               $" Nonresident={company.Nonresident}," +
                                               $" NUNonrezidentlitiko={company.NUNonrezidentlitiko}," +
                                               $" TRRC={company.TRRC}," +
                                               $" NCEO={company.NCEO}," +
                                               $" OKOPFlitiko={(company.OKOPFlitiko != null ? company.OKOPFlitiko.ExternalId : "null")}," +
                                               $" litiko={(company.OKFSlitiko != null ? company.OKFSlitiko.ExternalId : "null")}," +
                                               $" RegNumlitiko={company.RegNumlitiko}," +
                                               $" Numberslitiko={company.Numberslitiko}, " +
                                               $"Businesslitiko={company.Businesslitiko}," +
                                               $" EnterpriseTypelitiko={(company.EnterpriseTypelitiko != null ? company.EnterpriseTypelitiko.ExternalId : "null")}," +
                                               $" Countrylitiko={(company.Countrylitiko != null ? company.Countrylitiko.ExternalIdlitiko : "null")}," +
                                               $" PostalAddress={company.PostalAddress}," +
                                               $" LegalAddress={company.LegalAddress}," +
                                               $" Phones={company.Phones}," +
                                               $" Email={company.Email}," +
                                               $" Homepage={company.Homepage}," +
                                               $" VATPayerlitiko={company.VATPayerlitiko}," +
                                               $" AccountEskhatalitiko={company.AccountEskhatalitiko}," +
                                               $" Account={company.Account}, Reliabilitylitiko={company.Reliabilitylitiko}");
                            
                        }
                        break;
                    }

                    case "Person":
                    {
                        var person = ParsePerson(node);
                        if (person != null)
                        {
                            bool isNew = person.State.IsInserted;

                            person.Save();

                            if (isNew)
                            {
                                result.ImportedPersons++;
                                result.ImportedCount++;
                            }

                            result.TotalPersons++;
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
                var msg = $"Ошибка при обработке элемента <{node.Name}>: {ex.Message}";
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

    Logger.DebugFormat(
        "Import finished. Total={0}, New={1}, Errors={2}",
        result.TotalCount,
        result.ImportedCount,
        result.Errors.Count
    );

    return result;
}


    private ICompany ParseCompany(XElement companyElement)
    {
      if (companyElement == null)
        return null;

      var isId = companyElement.Element("ID")?.Value;
      var isExternalD = companyElement.Element("ExternalD")?.Value;
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
      var isCodeOKONHelements =
        companyElement.Element("CODE_OKONH").Elements("element")
        ?? Enumerable.Empty<XElement>();
      var isCodeOKVEDelements =
        companyElement.Element("CODE_OKVED").Elements("element")
        ?? Enumerable.Empty<XElement>();
      var isRegistnum = companyElement.Element("REGIST_NUM")?.Value;
      var isNumbers = companyElement.Element("NUMBERS")?.Value;
      var isBusiness = companyElement.Element("BUSINESS")?.Value;
      var isPS_REF = companyElement.Element("PS_REF")?.Value;
      var isCountry = companyElement.Element("COUNTRY")?.Value;
      var isPostAdress = companyElement.Element("PostAdress")?.Value;
      var isLegalAdress = companyElement.Element("LegalAdress")?.Value;
      var isPhone = companyElement.Element("Phone")?.Value;
      var isCity = companyElement.Element("City")?.Value; // EXternalId from City
      var isStreet = companyElement.Element("Street")?.Value;
      var isBuildingNumber = companyElement.Element("BuildingNumber")?.Value;
      var isEmail = companyElement.Element("Email")?.Value;
      var isWebSite = companyElement.Element("WebSite")?.Value;
      var isTaxNonResident = companyElement.Element("TaxNonResident")?.Value;
      var isVatPayer = companyElement.Element("VATPayer")?.Value;
      var isReliability = companyElement.Element("Reliability")?.Value;
      var isCorrAcc = companyElement.Element("CorrAcc")?.Value;
      var isInernalAcc = companyElement.Element("InternalAcc")?.Value;

      var company = litiko
        .Eskhata.Companies.GetAll()
        .FirstOrDefault(x =>
                        (!string.IsNullOrEmpty(isExternalD) && x.ExternalId == isExternalD)
                        || (!string.IsNullOrEmpty(isINN) && x.TIN == isINN)
                       );

      //var company = litiko.Eskhata.Companies.GetAll().FirstOrDefault(x => x.ExternalId == isExternalD || x.TIN == isINN);

      if (company != null)
      {
        Logger.DebugFormat("Company with ExternalId:{0} or INN:{1} was found. Id:{2}, Name:{3}", isExternalD, isINN, company.Id, company.Name);
        return company;
      }

      bool isNew = company == null;

      if (isNew)
      {
        company = litiko.Eskhata.Companies.Create();
        company.ExternalId = isExternalD;
        company.Name = isName;
        company.TIN = isINN;
      }

      company.LegalName = isLongName;
      company.Inamelitiko = isIName;
      company.Nonresident = ParseBoolSafe(isRezident);
      company.NUNonrezidentlitiko = ParseBoolSafe(isNunRezident); 
      company.TRRC = isKPP;
      company.NCEO = isOKPO;

      if (isOKOPF != null)
      {
        var okopf = litiko.NSI.OKOPFs.GetAll().FirstOrDefault(x => x.ExternalId == isOKOPF);
        company.OKOPFlitiko = okopf;
      }

      if (isOKFS != null)
      {
        var okfs = litiko.NSI.OKFSes.GetAll().FirstOrDefault(x => x.ExternalId == isOKFS);
        company.OKFSlitiko = okfs;
      }

      foreach (var n in isCodeOKONHelements)
      {
        var code = n?.Value;
        if (string.IsNullOrEmpty(code))
          continue;
        var okonh = litiko.NSI.OKONHs.GetAll().FirstOrDefault(x => x.ExternalId == code);
        if (okonh != null && !company.OKONHlitiko.Any(x => Equals(x.OKONH, okonh)))
        {
          var rec = company.OKONHlitiko.AddNew();
          rec.OKONH = okonh;
        }
      }
      foreach (var n in isCodeOKVEDelements)
      {
        var code = n?.Value;
        if (string.IsNullOrEmpty(code))
          continue;
        var okved = litiko.NSI.OKVEDs.GetAll().FirstOrDefault(x => x.ExternalId == code);
        if (okved != null && !company.OKVEDlitiko.Any(x => Equals(x.OKVED, okved)))
        {
          var rec = company.OKVEDlitiko.AddNew();
          rec.OKVED = okved;
        }
      }

      company.RegNumlitiko = isRegistnum;
      company.Numberslitiko = string.IsNullOrEmpty(isNumbers)
        ? (int?)null
        : int.Parse(isNumbers);
      company.Businesslitiko = isBusiness;

      if (!string.IsNullOrEmpty(isPS_REF))
      {
        var enterpriseType = litiko
          .NSI.EnterpriseTypes.GetAll()
          .FirstOrDefault(x => x.ExternalId == isPS_REF);
        company.EnterpriseTypelitiko = enterpriseType;
      }

      if (!string.IsNullOrEmpty(isCountry))
      {
        var country = litiko
          .Eskhata.Countries.GetAll()
          .FirstOrDefault(x => x.ExternalIdlitiko == isCountry);
        company.Countrylitiko = country;
      }

      company.PostalAddress = isPostAdress;
      company.LegalAddress = isLegalAdress;
      company.Phones = isPhone;
      
      if (!string.IsNullOrEmpty(isCity))
      {
        var city = litiko.Eskhata.Cities.GetAll().FirstOrDefault(x => x.ExternalIdlitiko == isCity);
        
        if(city != null)
          company.City = city;
      }
      
      if (!string.IsNullOrEmpty(isStreet))
        company.Streetlitiko = isStreet;
      
      if (!string.IsNullOrEmpty(isBuildingNumber))
        company.HouseNumberlitiko = isBuildingNumber;
      
      company.Email = isEmail;
      company.Homepage = isWebSite;
      company.NUNonrezidentlitiko = ParseBoolSafe(isTaxNonResident);
      company.VATPayerlitiko = ParseBoolSafe(isVatPayer);
      company.AccountEskhatalitiko = isInernalAcc;
      company.Account = isCorrAcc;

      if (!string.IsNullOrWhiteSpace(isReliability))
      {
        Sungero.Core.Enumeration? reliabilityEnum = null;
        switch (isReliability.Trim())
        {
          case "Надежный":
          case"Высокий":
            reliabilityEnum = litiko.Eskhata.Company.Reliabilitylitiko.Reliable;
            break;
          case "Не надежный":
          case "Низкая":
            reliabilityEnum = litiko.Eskhata.Company.Reliabilitylitiko.NotReliable;
            break;
          default:
            Logger.DebugFormat("Unknown Reliability: {0}", isReliability);
            break;
        }

        if (reliabilityEnum.HasValue)
          company.Reliabilitylitiko = reliabilityEnum;
      }
      if (isNew)
        Logger.DebugFormat("Prepare to create new Company: Name={0}, ExternalId={1}, INN={2}", company.Name, company.ExternalId, company.TIN);
      return company;
    }

    private IPerson ParsePerson(XElement personElement)
    {
      if (personElement == null)
        return null;

      var isExternalD = personElement.Element("ExternalID")?.Value;
      var isLastName = personElement.Element("LastName")?.Value;
      var isFirstName = personElement.Element("FirstName")?.Value;
      var isMiddleName = personElement.Element("MiddleName")?.Value;
      var isRezident = personElement.Element("REZIDENT")?.Value;
      var isNuRezident = personElement.Element("NU_REZIDENT")?.Value;
      var isIName = personElement.Element("I_NAME")?.Value;
      var isDateOfBirth = personElement.Element("DATE_PERS")?.Value;
      var isSex = personElement.Element("SEX")?.Value;
      var isFamilyStatus = personElement.Element("MARIGE_ST")?.Value; // ExternalId
      var isINN = personElement.Element("INN")?.Value;
      var isCodeOKONHelements = personElement.Element("CODE_OKONH").Elements("element");
      var isCodeOKVEDelements = personElement.Element("CODE_OKVED").Elements("element");
      var isIIN = personElement.Element("IIN")?.Value;
      var isCountry = personElement.Element("COUNTRY")?.Value;
      var isDocBirthPlace = personElement.Element("DOC_BIRTH_PLACE")?.Value;
      var isPostAdress = personElement.Element("PostAdress")?.Value;
      var isEmail = personElement.Element("Email")?.Value;
      var isPhone = personElement.Element("Phone")?.Value;
      var isCity = personElement.Element("City")?.Value; // EXternalId from City
      var isStreet = personElement.Element("Street")?.Value;
      var isBuildingNumber = personElement.Element("BuildingNumber")?.Value;
      var isWebSite = personElement.Element("WebSite")?.Value;
      var isTaxNonResident = personElement.Element("TaxNonResident")?.Value;
      var isVatPayer = personElement.Element("VATPayer")?.Value;
      var isReliability = personElement.Element("Reliability")?.Value;
      var isCorrAcc = personElement.Element("CorrAcc")?.Value;
      var isInternalAcc = personElement.Element("InternalAcc")?.Value;
      var isIdentityDocument = personElement.Element("IdentityDocuments")?.Element("element");

      var person = Eskhata.People.GetAll().FirstOrDefault(x => x.ExternalId == isExternalD);

      if (person != null)
      {
        Logger.DebugFormat(
          "Person found: External={0} Id={1} Name={2}",
          isExternalD,
          person.Id,
          person.Name
         );
        return person; // если есть — возвращаем найденного, не создаём дубликат
      }

      bool isNew = person == null;

      if (isNew)
      {
        person = Eskhata.People.Create();
        person.ExternalId = isExternalD;
      }

      Logger.DebugFormat(
        "Create new Person with ExternalId:{0}. Id:{1}",
        isExternalD,
        person.Id
       );

      if (!string.IsNullOrEmpty(isLastName))
        person.LastName = isLastName.Trim();
      else
        Logger.DebugFormat("No LastName found for Person with ExternalId:{0}", isExternalD);

      if (!string.IsNullOrEmpty(isFirstName))
        person.FirstName = isFirstName.Trim();
      else
        Logger.DebugFormat(
          "No FirstName found for Person with ExternalId:{0}",
          isExternalD
         );

      if (!string.IsNullOrEmpty(isMiddleName))
        person.MiddleName = isMiddleName.Trim();
      else
        Logger.DebugFormat(
          "No MiddleName found for Person with ExternalId:{0}",
          isExternalD
         );

      if (!string.IsNullOrEmpty(isRezident))
        person.Nonresident = ParseBoolSafe(isRezident);

      if (!string.IsNullOrEmpty(isNuRezident))
        person.NUNonrezidentlitiko = ParseBoolSafe(isNuRezident);

      if (!string.IsNullOrEmpty(isIName))
        person.Inamelitiko = isIName.Trim();

      var parsedDate = TryParseDate(isDateOfBirth);
      if (parsedDate.HasValue)
        person.DateOfBirth = parsedDate.Value;
      else
        Logger.DebugFormat(
          "No valid DateOfBirth found for Person with ExternalId:{0}",
          isExternalD
         );

      if (isSex == "М")
        person.Sex = Eskhata.Person.Sex.Male;
      else if (isSex == "Ж")
        person.Sex = Eskhata.Person.Sex.Female;

      if (!string.IsNullOrEmpty(isFamilyStatus))
      {
        var familyStatus = litiko
          .NSI.FamilyStatuses.GetAll()
          .FirstOrDefault(x => x.ExternalId == isFamilyStatus);
        if (familyStatus != null)
          person.FamilyStatuslitiko = familyStatus;
      }

      if (!string.IsNullOrEmpty(isINN))
        person.TIN = isINN;

      foreach (var n in isCodeOKONHelements)
      {
        var code = n?.Value;
        if (string.IsNullOrWhiteSpace(code))
          continue;

        // Ищем справочник OKONH
        var okonh = litiko.NSI.OKONHs.GetAll()
          .FirstOrDefault(x => x.ExternalId == code);

        if (okonh == null)
        {
          Logger.Debug($"OKONH with ExternalId={code} not found.");
          continue;
        }

        // Проверяем, есть ли уже такая запись в коллекции
        var exists = person.OKONHlitiko
          .Any(x => x.OKONH != null && x.OKONH.Id == okonh.Id);

        if (!exists)
        {
          var rec = person.OKONHlitiko.AddNew();
          rec.OKONH = okonh;
          Logger.Debug($"Added OKONH '{code}' to person '{person.Name}'.");
        }
        else
        {
          Logger.Debug($"OKONH '{code}' already exists for person '{person.Name}'.");
        }
      }


      foreach (var n in isCodeOKVEDelements)
      {
        var code = n?.Value;
        if (string.IsNullOrWhiteSpace(code))
          continue;

        // Ищем справочник OKVED
        var okved = litiko.NSI.OKVEDs.GetAll()
          .FirstOrDefault(x => x.ExternalId == code);

        if (okved == null)
        {
          Logger.Debug($"OKVED with ExternalId={code} not found.");
          continue;
        }

        // Проверяем, уже есть?
        var exists = person.OKVEDlitiko
          .Any(x => x.OKVED != null && x.OKVED.Id == okved.Id);

        if (!exists)
        {
          var rec = person.OKVEDlitiko.AddNew();
          rec.OKVED = okved;
          Logger.Debug($"Added OKVED '{code}' to person '{person.Name}'.");
        }
        else
        {
          Logger.Debug($"OKVED '{code}' already exists for person '{person.Name}'.");
        }
      }


      if (!string.IsNullOrEmpty(isIIN))
        person.SINlitiko = int.Parse(isIIN);

      if (!string.IsNullOrEmpty(isCountry))
      {
        var country = litiko
          .Eskhata.Countries.GetAll()
          .FirstOrDefault(x => x.ExternalIdlitiko == isCountry);
        if (country != null)
          person.Citizenship = country;
      }

      if (!string.IsNullOrEmpty(isDocBirthPlace))
        person.BirthPlace = isDocBirthPlace.Trim();

      if (!string.IsNullOrEmpty(isPostAdress))
        person.PostalAddress = isPostAdress.Trim();

      if (!string.IsNullOrEmpty(isEmail))
        person.Email = isEmail.Trim();

      if (!string.IsNullOrEmpty(isPhone))
        person.Phones = isPhone.Trim();

      if (!string.IsNullOrEmpty(isCity))
      {
        var city = litiko.Eskhata.Cities.GetAll().FirstOrDefault(x => x.ExternalIdlitiko == isCity);
        
        if(city != null)
          person.City = city;
      }

      if (!string.IsNullOrEmpty(isStreet))
        person.Streetlitiko = isStreet.Trim();

      if (!string.IsNullOrEmpty(isBuildingNumber))
        person.HouseNumberlitiko = isBuildingNumber.Trim();

      if (!string.IsNullOrEmpty(isWebSite))
        person.Homepage = isWebSite.Trim();

      if (!string.IsNullOrEmpty(isTaxNonResident))
        person.NUNonrezidentlitiko = ParseBoolSafe(isTaxNonResident);

      if (!string.IsNullOrEmpty(isVatPayer))
        person.VATPayerlitiko = ParseBoolSafe(isVatPayer);

      if (!string.IsNullOrEmpty(isReliability))
      {
        Sungero.Core.Enumeration? reliabilityEnum = null;
        switch (isReliability.Trim())
        {
          case "Надежный":
            reliabilityEnum = litiko.Eskhata.Person.Reliabilitylitiko.Reliable;
            break;
          case "Не надежный":
            reliabilityEnum = litiko.Eskhata.Person.Reliabilitylitiko.NotReliable;
            break;
          default:
            Logger.DebugFormat("Unknown Reliability: {0}", isReliability);
            break;
        }

        if (reliabilityEnum.HasValue)
          person.Reliabilitylitiko = reliabilityEnum;
      }
      
      if (!string.IsNullOrEmpty(isCorrAcc))
        person.Account = isCorrAcc.Trim();

      if (!string.IsNullOrEmpty(isInternalAcc))
        person.AccountEskhatalitiko = isInternalAcc.Trim();

      var identityElement =
        personElement.Element("IdentityDocument")
        ?? personElement.Element("IdentityDocuments")?.Element("element");
      if (identityElement != null)
      {
        var idSid =
          identityElement.Element("TYPE")?.Value ?? identityElement.Element("ID")?.Value;
        if (!string.IsNullOrEmpty(idSid))
        {
          var identityKind = Sungero
            .Parties.IdentityDocumentKinds.GetAll()
            .FirstOrDefault(x => x.SID == idSid);
          if (identityKind != null)
            person.IdentityKind = identityKind;
          person.IdentityNumber = identityElement.Element("NUM")?.Value;
          person.IdentitySeries = identityElement.Element("SER")?.Value;
          DateTime tmp;
          if (
            Calendar.TryParseDate(identityElement.Element("DATE_BEGIN")?.Value, out tmp)
           )
            person.IdentityDateOfIssue = tmp;
          if (Calendar.TryParseDate(identityElement.Element("DATE_END")?.Value, out tmp))
            person.IdentityExpirationDate = tmp;
        }
      }

      if (isNew)
        Logger.DebugFormat(
          "Prepared to create new Person: Name='{0}', ExternalId='{1}'",
          person.Name,
          isExternalD
         );
      return person;
    }

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
  }
}
