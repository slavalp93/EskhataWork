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
        
        var counterparty = xDoc.Element("Counterparty");
        
        if (counterparty == null)
        {
          result.Errors.Add("Корневой элемент <Counterparty> не найден.");
          return result;
        }

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
                    
                    result.TotalCompanies++;

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


                    Logger.DebugFormat($"Created new Company with: " +
                                       $" Id={company.Id}, " +
                                       $" Name={company.Name}, " +
                                       $" ExternalId={company.ExternalId}," +
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
                                       $" Businesslitiko={company.Businesslitiko}," +
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
                    
                    result.TotalPersons++;

                    person.Save();

                    if (isNew)
                    {
                      result.ImportedPersons++;
                      result.ImportedCount++;
                    }
                    else
                    {
                      result.DuplicatePersons++; // Найден дубль (обновлен)
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

      Logger.DebugFormat("Import finished. Total={0}, New={1}, Errors={2}",result.TotalCount,result.ImportedCount,result.Errors.Count);

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
      var isCodeOKONHelements = companyElement.Element("CODE_OKONH").Elements("element") ?? Enumerable.Empty<XElement>();
      var isCodeOKVEDelements = companyElement.Element("CODE_OKVED").Elements("element") ?? Enumerable.Empty<XElement>();
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

      var company = litiko.Eskhata.Companies.GetAll().FirstOrDefault(x =>(!string.IsNullOrEmpty(isExternalD) && x.ExternalId == isExternalD)|| (!string.IsNullOrEmpty(isINN) && x.TIN == isINN));

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

      var allOkonhs = litiko.NSI.OKONHs.GetAll().ToList();
      var allOkveds = litiko.NSI.OKVEDs.GetAll().ToList();

      if (isCodeOKONHelements != null)
      {
        var firstOkonhElement = isCodeOKONHelements.FirstOrDefault(n => !string.IsNullOrEmpty(n?.Value));

        if (firstOkonhElement != null)
        {
          var code = firstOkonhElement.Value;

          var okonh = allOkonhs.FirstOrDefault(x => x.ExternalId == code);

          if (okonh != null && !object.Equals(company.OKONHlitiko, okonh))
          {
            company.OKONHlitiko = okonh;
          }
        }
      }

      if (isCodeOKVEDelements != null)
      {
        var firstOkvedElement = isCodeOKVEDelements
          .FirstOrDefault(n => !string.IsNullOrEmpty(n?.Value));

        if (firstOkvedElement != null)
        {
          var code = firstOkvedElement.Value;
          var okved = allOkveds.FirstOrDefault(x => x.ExternalId == code);

          if (okved != null && !object.Equals(company.OKVEDlitiko, okved))
          {
            company.OKVEDlitiko = okved;
          }
        }
      }

      company.RegNumlitiko = isRegistnum;
      company.Numberslitiko = string.IsNullOrEmpty(isNumbers) ? (int?)null : int.Parse(isNumbers);
      company.Businesslitiko = isBusiness;

      if (!string.IsNullOrEmpty(isPS_REF))
      {
        var enterpriseType = litiko.NSI.EnterpriseTypes.GetAll().FirstOrDefault(x => x.ExternalId == isPS_REF);
        company.EnterpriseTypelitiko = enterpriseType;
      }

      if (!string.IsNullOrEmpty(isCountry))
      {
        var country = litiko.Eskhata.Countries.GetAll().FirstOrDefault(x => x.ExternalIdlitiko == isCountry);
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

      var person = Eskhata.People.GetAll().FirstOrDefault(x => (!string.IsNullOrEmpty(isExternalD) && x.ExternalId == isExternalD) || (!string.IsNullOrEmpty(isINN) && x.TIN == isINN));
      
      if (person != null)
      {
        Logger.DebugFormat("Person found: External={0} Id={1} Name={2}",isExternalD,person.Id,person.Name);
        return person;
      }

      bool isNew = person == null;

      if (isNew)
      {
        person = Eskhata.People.Create();
        person.ExternalId = isExternalD;
      }

      Logger.DebugFormat("Create new Person with ExternalId:{0}. Id:{1}",isExternalD,person.Id);

      if (!string.IsNullOrEmpty(isLastName))
        person.LastName = isLastName.Trim();
      else
        Logger.DebugFormat("No LastName found for Person with ExternalId:{0}", isExternalD);

      if (!string.IsNullOrEmpty(isFirstName))
        person.FirstName = isFirstName.Trim();
      else
        Logger.DebugFormat("No FirstName found for Person with ExternalId:{0}",isExternalD);

      if (!string.IsNullOrEmpty(isMiddleName))
        person.MiddleName = isMiddleName.Trim();
      else
        Logger.DebugFormat("No MiddleName found for Person with ExternalId:{0}",isExternalD);

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
        Logger.DebugFormat("No valid DateOfBirth found for Person with ExternalId:{0}",isExternalD);

      if (isSex == "М")
        person.Sex = Eskhata.Person.Sex.Male;
      else if (isSex == "Ж")
        person.Sex = Eskhata.Person.Sex.Female;

      if (!string.IsNullOrEmpty(isFamilyStatus))
      {
        var familyStatus = litiko.NSI.FamilyStatuses.GetAll().FirstOrDefault(x => x.ExternalId == isFamilyStatus);
        if (familyStatus != null)
          person.FamilyStatuslitiko = familyStatus;
      }

      if (!string.IsNullOrEmpty(isINN))
        person.TIN = isINN;

      if (!string.IsNullOrEmpty(isIIN))
        person.SINlitiko = isIIN;

      if (!string.IsNullOrEmpty(isCountry))
      {
        var country = litiko.Eskhata.Countries.GetAll().FirstOrDefault(x => x.ExternalIdlitiko == isCountry);
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

      var identityElement = personElement.Element("IdentityDocument") ?? personElement.Element("IdentityDocuments")?.Element("element");
      if (identityElement != null)
      {
        var idSid = identityElement.Element("TYPE")?.Value ?? identityElement.Element("ID")?.Value;
        if (!string.IsNullOrEmpty(idSid))
        {
          var identityKind = Sungero.Parties.IdentityDocumentKinds.GetAll().FirstOrDefault(x => x.SID == idSid);
          if (identityKind != null)
            person.IdentityKind = identityKind;
          person.IdentityNumber = identityElement.Element("NUM")?.Value;
          person.IdentitySeries = identityElement.Element("SER")?.Value;
          DateTime tmp;
          if (Calendar.TryParseDate(identityElement.Element("DATE_BEGIN")?.Value, out tmp))
            person.IdentityDateOfIssue = tmp;
          if (Calendar.TryParseDate(identityElement.Element("DATE_END")?.Value, out tmp))
            person.IdentityExpirationDate = tmp;
        }
      }

      if (isNew)
        Logger.DebugFormat("Prepared to create new Person: Name='{0}', ExternalId='{1}'",person.Name,isExternalD);
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
