using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Sungero.Core;
using litiko.Eskhata.Module.Contracts.Structures.Module;
//using litiko.Eskhata.Structures.Contracts.Contract;
using Sungero.CoreEntities;
using Sungero.Domain.Shared;
using Sungero.Workflow.TaskSchemeValidators;

namespace litiko.Eskhata.Module.Contracts.Server
{
  partial class ModuleFunctions
  {
    [Remote, Public]
    public IResultImportXmlUI ImportContractsFromXmlUI()
    {
      //var result = Eskhata.Structures.Contracts.Contract.ResultImportXml.Create();
      var result = litiko.Eskhata.Module.Contracts.Structures.Module.ResultImportXmlUI.Create();
      result.Errors = new List<string>();
      result.ImportedCount = 0;
      result.TotalCount = 0;
      
      Logger.Debug("Import contracts from XML - Start");
      
      var xmlPathFile = "Contracts.xml";
      
      if(!System.IO.File.Exists(xmlPathFile))
      {
        result.Errors.Add($"Файл '{xmlPathFile}' не найден");
        Logger.Error($"XML File {xmlPathFile} is not found");
        return result;
      }
      
      try
      {
        XDocument xDoc = XDocument.Load(xmlPathFile);

        var dataElements = xDoc.Element("Data");

        if (dataElements == null)
        {
          result.Errors.Add("Корневой элемент <Data> отсутствует в XML.");
          Logger.Error("No <Data> root element found in XML.");
          return result;
        }

        var documentElements = dataElements.Elements("Document").ToList();
        
        var counterpartyElements = dataElements.Elements("Counterparty").ToList();
        
        if(!documentElements.Any())
        {
          result.Errors.Add("В файле нет элементов <Document> для импорта");
          Logger.Error("No <Document> elements found");
          return result;
        }

        for (var i = 0; i < documentElements.Count; i++)
        {
          try
          {
            var documentElement = documentElements[i];
            
            if(documentElements[i] == null)
            {
              result.Errors.Add($"Элемент <Document> №{i + 1} пустой, пропущен");
              continue;
            }
            
            var counterpartyElement = counterpartyElements.ElementAtOrDefault(i);
            var counterparty = ParseCounterparty(counterpartyElement);
            var contract = ParseContract(documentElement, counterparty, result);
            
            result.TotalCount++;
            result.ImportedCount++;
            
            Logger.Debug($"Импортирован договор: {contract.Name}");
          }
          catch (Exception ex)
          {
            result.Errors.Add($"Ошибка при импорте документа №{i + 1}: {ex.Message}");
          }
        }
      }
      catch (Exception ex)
      {
        result.Errors.Add($"Общая ошибка импорта: {ex.Message}");
      }
      return result;
    }

    private ICounterparty ParseCounterparty(XElement counterpartyElement)
    {
      var _person = counterpartyElement.Element("Person");
      var _company = counterpartyElement.Element("Company");

      var externalId = counterpartyElement.Element("ExternalD")?.Value;
      var inn = counterpartyElement.Element("INN")?.Value;
      var counterparty = litiko.Eskhata.Counterparties.GetAll()
        .FirstOrDefault(x => x.ExternalId == externalId || x.TIN == inn);
      
      if(counterparty != null)
      {
        return ParseCounterpartyCompany(_company); 
      }

      if (_person != null)
      {
        return ParseCounterpartyPerson(_person);
      }
      return null;
    }
    
    private ICompany ParseCounterpartyCompany(XElement companyElements)
    {
        var companyElement = companyElements.Element("Company");
        
        var isId = companyElement.Element("ID")?.Value;
        var isExternalD = companyElement.Element("ExternalD").Value;
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
        var isCodeOKONHelements = companyElement.Element("CODE_OKONH").Elements("element") ?? Enumerable.Empty<XElement>();
        var isCodeOKVEDelements = companyElement.Element("CODE_OKVED").Elements("element") ?? Enumerable.Empty<XElement>();
        var isRegistnum = companyElement.Element("REGIST_NUM")?.Value;
        var isNumbers = companyElement.Element("NUMBERS")?.Value;
        var isBusiness = companyElement.Element("BUSINESS")?.Value;
        var isPS_REF = companyElement.Element("PS_REF")?.Value;
        var isCountry = companyElement.Element("COUNTRY")?.Value;
        var isPostAdress = companyElement.Element("PostAdress")?.Value;
        var isLegalAdress = companyElement.Element("LegalAdress")?.Value;
        var isPhone = companyElement.Element("Phone")?.Value;
        var isEmail = companyElement.Element("Email")?.Value;
        var isWebSite = companyElement.Element("WebSite")?.Value;
        var isTaxNonResident = companyElement.Element("TaxNonResident")?.Value;
        var isVatPayer = companyElement.Element("VATPayer")?.Value;
        var isReliability = companyElement.Element("Reliability")?.Value;
        var isCorrAcc = companyElement.Element("CorrAcc")?.Value;
        var isInernalAcc = companyElement.Element("InternalAcc")?.Value;

        try
        {
          var result = litiko.Eskhata.Module.Contracts.Structures.Module.ResultImportXmlUI.Create();
          result.Errors = new List<string>();

          var company = litiko.Eskhata.Companies.GetAll().FirstOrDefault(x => x.ExternalId == isExternalD);
          if (company == null)
            company = litiko.Eskhata.Companies.Create();

          else
          {

            company.ExternalId = isExternalD;
            company.Name = isName;
            company.TIN = isINN;
            company.LegalName = isLongName;
            company.Inamelitiko = isIName;
            company.NUNonrezidentlitiko = ParseBoolSafe(isNunRezident);
            company.Nonresident = ParseBoolSafe(isRezident);
            company.TRRC = isKPP;
            company.NCEO = isOKPO;

            var okopf = litiko.NSI.OKOPFs.GetAll().Where(x => x.ExternalId == isOKOPF).FirstOrDefault();
            if (okopf != null && !Equals(company.OKOPFlitiko, okopf))
            {
              Logger.DebugFormat("Change OKOPFlitiko: current:{0}, new:{1}", company.OKOPFlitiko?.Name, okopf.Name);
              company.OKOPFlitiko = okopf;
            }

            if (!string.IsNullOrEmpty(isOKFS))
            {
              var okfs = litiko.NSI.OKFSes.GetAll().Where(x => x.ExternalId == isOKFS).FirstOrDefault();
              if (okfs != null && !Equals(company.OKFSlitiko, okfs))
              {
                Logger.DebugFormat("Change OKFSlitiko: current:{0}, new:{1}", company.OKOPFlitiko?.Name, okfs.Name);
                company.OKFSlitiko = okfs;
              }
            }

            if (isCodeOKONHelements.Any())
            {
              var elementValues = isCodeOKONHelements.Select(x => x.Value).ToList();
              if (company.OKONHlitiko.Select(x => x.OKONH.ExternalId).Any(x => !elementValues.Contains(x)))
              {
                company.OKONHlitiko.Clear();
                Logger.DebugFormat("Change OKONHlitiko: Clear");
              }

              foreach (var isCodeOKONH in isCodeOKONHelements)
              {
                var okonh = litiko.NSI.OKONHs.GetAll().Where(x => x.ExternalId == isCodeOKONH.Value).FirstOrDefault();
                if (okonh != null && !company.OKONHlitiko.Any(x => Equals(x.OKONH, okonh)))
                {
                  var newRecord = company.OKONHlitiko.AddNew();
                  newRecord.OKONH = okonh;
                  Logger.DebugFormat("Change OKONHlitiko: added:{0}", okonh.Name);
                }
              }
            }

            if (isCodeOKVEDelements.Any())
            {
              var elementValues = isCodeOKVEDelements.Select(x => x.Value).ToList();
              if (company.OKVEDlitiko.Select(x => x.OKVED.ExternalId).Any(x => !elementValues.Contains(x)))
              {
                company.OKVEDlitiko.Clear();
                Logger.DebugFormat("Change OKVEDlitiko: Clear");
              }

              foreach (var isCodeOKVED in isCodeOKVEDelements)
              {
                var okved = litiko.NSI.OKVEDs.GetAll().Where(x => x.ExternalId == isCodeOKVED.Value).FirstOrDefault();
                if (okved != null && !company.OKVEDlitiko.Any(x => Equals(x.OKVED, okved)))
                {
                  var newRecord = company.OKVEDlitiko.AddNew();
                  newRecord.OKVED = okved;
                  Logger.DebugFormat("Change OKVEDlitiko: added:{0}", okved.Name);
                }
              }
            }

            if (!string.IsNullOrEmpty(isRegistnum) && company.RegNumlitiko != isRegistnum)
            {
              Logger.DebugFormat("Change RegNumlitiko: current:{0}, new:{1}", company.RegNumlitiko, isRegistnum);
              company.RegNumlitiko = isRegistnum;
            }

            if (!string.IsNullOrEmpty(isNumbers) && company.Numberslitiko != int.Parse(isNumbers))
            {
              Logger.DebugFormat("Change Numberslitiko: current:{0}, new:{1}", company.Numberslitiko, isNumbers);
              company.Numberslitiko = int.Parse(isNumbers);
            }

            if (!string.IsNullOrEmpty(isBusiness) && company.Businesslitiko != isBusiness)
            {
              Logger.DebugFormat("Change Businesslitiko: current:{0}, new:{1}", company.Businesslitiko, isBusiness);
              company.Businesslitiko = isBusiness;
            }

            if (!string.IsNullOrEmpty(isPS_REF))
            {
              var enterpriseType = litiko.NSI.EnterpriseTypes.GetAll().Where(x => x.ExternalId == isPS_REF)
                .FirstOrDefault();
              if (enterpriseType != null && !Equals(company.EnterpriseTypelitiko, enterpriseType))
              {
                Logger.DebugFormat("Change EnterpriseTypelitiko: current:{0}, new:{1}",
                  company.EnterpriseTypelitiko?.Name, enterpriseType.Name);
                company.EnterpriseTypelitiko = enterpriseType;
              }
            }

            if (!string.IsNullOrEmpty(isCountry))
            {
              var country = litiko.Eskhata.Countries.GetAll().Where(x => x.ExternalIdlitiko == isCountry)
                .FirstOrDefault();
              if (country != null && !Equals(company.Countrylitiko, country))
              {
                Logger.DebugFormat("Change Countrylitiko: current:{0}, new:{1}", company.Countrylitiko?.Name,
                  country.Name);
                company.Countrylitiko = country;
              }
            }

            if (!string.IsNullOrEmpty(isPostAdress) && company.PostalAddress != isPostAdress)
            {
              Logger.DebugFormat("Change PostalAddress: current:{0}, new:{1}", company.PostalAddress, isPostAdress);
              company.PostalAddress = isPostAdress;
            }

            if (!string.IsNullOrEmpty(isLegalAdress) && company.LegalAddress != isLegalAdress)
            {
              Logger.DebugFormat("Change LegalAddress: current:{0}, new:{1}", company.LegalAddress, isLegalAdress);
              company.LegalAddress = isLegalAdress;
            }

            if (!string.IsNullOrEmpty(isPhone) && company.Phones != isPhone)
            {
              Logger.DebugFormat("Change Phones: current:{0}, new:{1}", company.Phones, isPhone);
              company.Phones = isPhone;
            }

            if (!string.IsNullOrEmpty(isEmail) && company.Email != isEmail)
            {
              Logger.DebugFormat("Change Email: current:{0}, new:{1}", company.Email, isEmail);
              company.Email = isEmail;
            }

            if (!string.IsNullOrEmpty(isWebSite) && company.Homepage != isWebSite)
            {
              Logger.DebugFormat("Change Homepage: current:{0}, new:{1}", company.Homepage, isWebSite);
              company.Homepage = isWebSite;
            }

            company.VATPayerlitiko = ParseBoolSafe(isVatPayer);
            //company. = ParseBoolSafe(isTaxNonResident);

            // PaymentMethod (enum) — сопоставь текст с вариантами
            if (!string.IsNullOrWhiteSpace(isReliability))
            {
              Sungero.Core.Enumeration? reliabilityEnum = null;
              switch (isReliability.Trim())
              {
                case "Надежный":
                  reliabilityEnum = litiko.Eskhata.Company.Reliabilitylitiko.Reliable;
                  break;
                case "Не надежный":
                  reliabilityEnum = litiko.Eskhata.Company.Reliabilitylitiko.NotReliable;
                  break;
                default:
                  Logger.DebugFormat("Unknown Reliability: {0}", isReliability);
                  break;
              }

              if (reliabilityEnum.HasValue)
                company.Reliabilitylitiko = reliabilityEnum;
            }

            company.AccountEskhatalitiko = isInernalAcc;
            company.Account = isCorrAcc;

            company.Save();
          }
        }
        catch (Exception ex)
        {
          Logger.ErrorFormat("Error parsing Company with ExternalId:{0}. Exception:{1}", isExternalD, ex.Message);
        }

        return null;
    }

    public IPerson ParseCounterpartyPerson(XElement personData)
    {
      const string dateFormat = "dd.MM.yyyy";

      var isID = personData.Element("ID")?.Value;
      var isExternalD = personData.Element("isExternalD")?.Value;
      var isName = personData.Element("NAME")?.Value;
      var isSex = personData.Element("SEX")?.Value;

      var isIName = personData.Element("I_NAME")?.Value;
      var isRezident = personData.Element("REZIDENT")?.Value;
      var isNuRezident = personData.Element("NU_REZIDENT")?.Value;
      var isDateOfBirth = personData.Element("DATE_PERS")?.Value;
      var isFamilyStatus = personData.Element("MARIGE_ST")?.Value;
      var isINN = personData.Element("INN")?.Value;
      var isCodeOKONHelements = personData.Element("CODE_OKONH").Elements("element");
      var isCodeOKVEDelements = personData.Element("CODE_OKVED").Elements("element");
      var isCountry = personData.Element("COUNTRY")?.Value;
      var isLegalAdress = personData.Element("DOC_BIRTH_PLACE")?.Value;
      var isPostAdress = personData.Element("PostAdress")?.Value;
      var isPhone = personData.Element("Phone")?.Value;
      var isEmail = personData.Element("Email")?.Value;
      var isWebSite = personData.Element("WebSite")?.Value;

      var isVATApplicable = personData.Element("VATApplicable")?.Value;
      var isIIN = personData.Element("IIN")?.Value;
      var isCorrAcc = personData.Element("CorrAcc")?.Value;
      var isInternalAcc = personData.Element("InternalAcc")?.Value;

      var isIdentityDocument = personData.Element("IdentityDocuments")?.Element("element");

      string isLastNameRu = string.Empty, isFirstNameRu = string.Empty, isMiddleNameRU = string.Empty;
      string isLastNameTG = string.Empty, isFirstNameTG = string.Empty, isMiddleNameTG = string.Empty;

      var parsePersonresult = Structures.Module.ResultImportXmlUI.Create();
      var person = People.GetAll().FirstOrDefault(x => x.ExternalId == isExternalD);

      if (string.IsNullOrWhiteSpace(isExternalD))
      {
        parsePersonresult.Errors.Add($"У контрагента с именем {isName} пропущен ExternalId");
        Logger.Error("Missing ExternalId in XML File");
        throw new KeyNotFoundException("У контрагента отсутствует внешний идентификатор");
      }

      var isNew = person == null;
      if (isNew)
      {

        if (person == null)
        {
          person = Eskhata.People.Create();
          person.ExternalId = isExternalD;
          Logger.DebugFormat("Create new Person with ExternalId:{0}. Id:{1}", isExternalD, person.ExternalId);
        }
      }

      person.Name = isName;
      person.LastName = isLastNameRu;
      person.FirstName = isFirstNameRu;
      person.MiddleName = isMiddleNameRU;
      person.LastNameTGlitiko = isLastNameTG;
      person.FirstNameTGlitiko = isFirstNameTG;
      person.MiddleNameTGlitiko = isMiddleNameTG;

      if (isSex == "М" && !Equals(person.Sex, Eskhata.Person.Sex.Male))
      {
        Logger.DebugFormat("Change Sex: current:{0}, new:{1}", person.Info.Properties.Sex.GetLocalizedValue(person.Sex),
          person.Info.Properties.Sex.GetLocalizedValue(Eskhata.Person.Sex.Male));
        person.Sex = Eskhata.Person.Sex.Male;
      }
      else if (isSex == "Ж" && !Equals(person.Sex, Eskhata.Person.Sex.Female))
      {
        Logger.DebugFormat("Change Sex: current:{0}, new:{1}", person.Info.Properties.Sex.GetLocalizedValue(person.Sex),
          person.Info.Properties.Sex.GetLocalizedValue(Eskhata.Person.Sex.Female));
        person.Sex = Eskhata.Person.Sex.Female;
      }

      person.Inamelitiko = isIName;

      if (!string.IsNullOrEmpty(isNuRezident))
      {
        bool isNuRezidentBool = isNuRezident == "1" ? true : false;
        if (person.NUNonrezidentlitiko != !isNuRezidentBool)
        {
          Logger.DebugFormat("Change NUNonrezidentlitiko: current:{0}, new:{1}", person.NUNonrezidentlitiko,
            !isNuRezidentBool);
          person.NUNonrezidentlitiko = !isNuRezidentBool;
        }
      }

      if (!string.IsNullOrEmpty(isRezident))
      {
        bool isRezidentBool = isRezident == "1" ? true : false;
        if (person.Nonresident != !isRezidentBool)
        {
          Logger.DebugFormat("Change Nonresident: current:{0}, new:{1}", person.Nonresident, !isRezidentBool);
          person.Nonresident = !isRezidentBool;
        }
      }

      if (!string.IsNullOrEmpty(isDateOfBirth))
      {
        var dateOfBirth = DateTime.Parse(isDateOfBirth);
        if (!Equals(person.DateOfBirth, dateOfBirth))
        {
          var curDate = person.DateOfBirth.HasValue ? person.DateOfBirth.Value.ToString("dd.MM.yyyy") : string.Empty;
          Logger.DebugFormat("Change DateOfBirth: current:{0}, new:{1}", curDate, dateOfBirth.ToString("dd.MM.yyyy"));
          person.DateOfBirth = dateOfBirth;
        }
      }

      if (!string.IsNullOrEmpty(isFamilyStatus))
      {
        var familyStatus = NSI.FamilyStatuses.GetAll().Where(x => x.ExternalId == isFamilyStatus).FirstOrDefault();
        if (familyStatus != null && !Equals(person.FamilyStatuslitiko, familyStatus))
        {
          Logger.DebugFormat("Change FamilyStatuslitiko: current:{0}, new:{1}", person.FamilyStatuslitiko?.Name,
            familyStatus?.Name);
          person.FamilyStatuslitiko = familyStatus;
        }
      }

      person.TIN = isINN;

      if (isCodeOKONHelements != null && isCodeOKONHelements.Any())
      {
        var elementValues = isCodeOKONHelements.Select(x => x.Value).ToList();
        if (person.OKONHlitiko.Select(x => x.OKONH.ExternalId).Any(x => !elementValues.Contains(x)))
        {
          person.OKONHlitiko.Clear();
          Logger.DebugFormat("Change OKONHlitiko: Clear");
        }

        foreach (var isCodeOKONH in isCodeOKONHelements)
        {
          var okonh = litiko.NSI.OKONHs.GetAll().Where(x => x.ExternalId == isCodeOKONH.Value).FirstOrDefault();
          if (okonh != null && !person.OKONHlitiko.Any(x => Equals(x.OKONH, okonh)))
          {
            var newRecord = person.OKONHlitiko.AddNew();
            newRecord.OKONH = okonh;
            Logger.DebugFormat("Change OKONHlitiko: added:{0}", okonh.Name);
          }
        }
      }

      if (isCodeOKVEDelements.Any())
      {
        var elementValues = isCodeOKVEDelements.Select(x => x.Value).ToList();
        if (person.OKVEDlitiko.Select(x => x.OKVED.ExternalId).Any(x => !elementValues.Contains(x)))
        {
          person.OKVEDlitiko.Clear();
          Logger.DebugFormat("Change OKVEDlitiko: Clear");
        }

        foreach (var isCodeOKVED in isCodeOKVEDelements)
        {
          var okved = litiko.NSI.OKVEDs.GetAll().Where(x => x.ExternalId == isCodeOKVED.Value).FirstOrDefault();
          if (okved != null && !person.OKVEDlitiko.Any(x => Equals(x.OKVED, okved)))
          {
            var newRecord = person.OKVEDlitiko.AddNew();
            newRecord.OKVED = okved;
            Logger.DebugFormat("Change OKVEDlitiko: added:{0}", okved.Name);
          }
        }
      }

      if (!string.IsNullOrEmpty(isCountry))
      {
        var country = Eskhata.Countries.GetAll().Where(x => x.ExternalIdlitiko == isCountry).FirstOrDefault();
        if (country != null && !Equals(person.Citizenship, country))
        {
          Logger.DebugFormat("Change Citizenship: current:{0}, new:{1}", person.Citizenship?.Name, country?.Name);
          person.Citizenship = country;
        }
      }

      person.PostalAddress = isPostAdress;
      person.LegalAddress = isLegalAdress;
      person.Phones = isPhone;
      person.Email = isEmail;
      person.Homepage = isWebSite;

      if (!string.IsNullOrEmpty(isVATApplicable))
      {
        bool VATPayer = isVATApplicable == "1" ? true : false;
        if (person.VATPayerlitiko != VATPayer)
        {
          Logger.DebugFormat("Change VATPayerlitiko: current:{0}, new:{1}", person.VATPayerlitiko.GetValueOrDefault(),
            VATPayer);
          person.VATPayerlitiko = VATPayer;
        }
      }

      if (!string.IsNullOrEmpty(isIIN))
      {
        int untIIN;
        if (int.TryParse(isIIN, out untIIN) && person.SINlitiko != untIIN)
        {
          Logger.DebugFormat("Change SINlitiko: current:{0}, new:{1}", person.SINlitiko, untIIN);
          person.SINlitiko = untIIN;
        }
        else
          Logger.ErrorFormat("Can`t convert to int value of IIN:{0}", isIIN);
      }

      person.Account = isCorrAcc;
      person.AccountEskhatalitiko = isInternalAcc;


      /* !!! IdentityDocuments !!! */
      if (isIdentityDocument != null)
      {
        var id = isIdentityDocument?.Element("ID")?.Value;
        if (!string.IsNullOrEmpty(id))
        {
          var identityDocument =
            Sungero.Parties.IdentityDocumentKinds.GetAll().Where(x => x.SID == id).FirstOrDefault();
          if (identityDocument != null)
          {
            Logger.DebugFormat("IdentityDocument with SID:{0} was found. Id:{1}, Name:{2}", id, identityDocument.Id,
              identityDocument.Name);
            var isDateBegin = isIdentityDocument.Element("DATE_BEGIN")?.Value;
            var isDateEnd = isIdentityDocument.Element("DATE_END")?.Value;
            var isNum = isIdentityDocument.Element("NUM")?.Value;
            var isSer = isIdentityDocument.Element("SER")?.Value;
            var isWho = isIdentityDocument.Element("WHO")?.Value;

            person.IdentityKind = identityDocument;

            DateTime dateBegin;
            if (!string.IsNullOrEmpty(isDateBegin) && Calendar.TryParseDate(isDateBegin, out dateBegin) &&
                !Equals(person.IdentityDateOfIssue, dateBegin))
            {
              Logger.DebugFormat("Change IdentityDateOfIssue: current:{0}, new:{1}",
                person.IdentityDateOfIssue?.ToString(dateFormat), dateBegin.ToString(dateFormat));
              person.IdentityDateOfIssue = dateBegin;
            }

            DateTime dateEnd;
            if (!string.IsNullOrEmpty(isDateEnd) && Calendar.TryParseDate(isDateEnd, out dateEnd) &&
                !Equals(person.IdentityExpirationDate, dateEnd))
            {
              Logger.DebugFormat("Change IdentityExpirationDate: current:{0}, new:{1}",
                person.IdentityExpirationDate?.ToString(dateFormat), dateEnd.ToString(dateFormat));
              person.IdentityExpirationDate = dateEnd;
            }

            person.IdentityNumber = isNum;

            person.IdentitySeries = isSer;

            person.IdentityAuthority = isWho;
          }
        }

        var result = Structures.Module.ProcessingPersonResult.Create(person, false);
        if (person.State.IsChanged || person.State.IsInserted)
        {
          person.Save();
          Logger.DebugFormat("Person successfully saved. ExternalId:{0}, Id:{1}", isID, person.Id);
          result.isCreatedOrUpdated = true;
        }
        else
        {
          Logger.DebugFormat("There are no changes in Person. ExternalId:{0}, Id:{1}", isID, person.Id);
          result.isCreatedOrUpdated = false;
        }
      }
      return person;
    }

    private IContract ParseContract(XElement documentElement, ICounterparty counterparty, IResultImportXmlUI result)
    {
      int addedCount = 0;
      int updatedCount = 0;

      if (documentElement == null)
      {
        Logger.Debug("Skipping <Data> without <Document>.");
      }

      var isExternalD = documentElement.Element("ExternalD")?.Value?.Trim();
      var isDocumentKind = documentElement.Element("DocumentKind")?.Value?.Trim();
      var isDocumentGroup = documentElement.Element("DocumentGroup")?.Value?.Trim();
      var isSubject = documentElement.Element("Subject")?.Value;
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
      var isPaymentFrequency = documentElement.Element("PaymentFrequency")?.Value?.Trim();
      var isPartialPayment = documentElement.Element("IsPartialPayment")?.Value?.Trim();
      var isEqualPayment = documentElement.Element("IsEqualPayment")?.Value?.Trim();
      var isAmountForPeriod = documentElement.Element("AmountForPeriod")?.Value?.Trim();
      var isRegistrationNumber = documentElement.Element("RegistrationNumber")?.Value?.Trim();
      var isRegistrationDate = documentElement.Element("RegistrationDate")?.Value?.Trim();

      // Найти контракт по externalId
      var contract = Eskhata.Contracts.GetAll().FirstOrDefault(x => x.ExternalId == isExternalD);
      
      var isNew = contract == null;
      if (isNew)
      {
        if(string.IsNullOrWhiteSpace(isExternalD))
        {
          result.Errors.Add($"У документа с именем {isName} пропущен ExternalId");
          Logger.Error("Missing ExternalId in XML File");
          throw new KeyNotFoundException("У документа отсутствует внешний идентификатор");
        }
        contract = Eskhata.Contracts.Create();
        
        contract.ExternalId = isExternalD;
      }

      contract.Counterparty = counterparty;
      // --- DocumentKind (назначаем только если нашли)
      if (!string.IsNullOrWhiteSpace(isDocumentKind))
      {
        var documentKind = litiko
          .Eskhata.DocumentKinds.GetAll()
          .FirstOrDefault(k => k.ExternalIdlitiko == isDocumentKind);
        if (documentKind != null)
          contract.DocumentKind = documentKind;
        else
          Logger.DebugFormat(
            "DocumentKind with ExternalId {0} not found, skipping assignment.",
            isDocumentKind
           );
      }

      // --- DocumentGroup
      if (!string.IsNullOrWhiteSpace(isDocumentGroup))
      {
        var documentGroup = litiko
          .Eskhata.DocumentGroupBases.GetAll()
          .FirstOrDefault(g => g.ExternalIdlitiko == isDocumentGroup);
        if (documentGroup != null)
          contract.DocumentGroup = documentGroup;
        else
          Logger.DebugFormat(
            "DocumentGroup with ExternalId {0} not found, skipping assignment.",
            isDocumentGroup
           );
      }

      // Простые поля
      contract.Subject = isSubject;
      contract.Name = isName;
      contract.RBOlitiko = isRBO;
      contract.ReasonForChangelitiko = isChangeReason;
      contract.AccDebtCreditlitiko = isAccountDebtCredt;
      contract.AccFutureExpenselitiko = isAccountFutureExpense;
      contract.RegistrationNumber = isRegistrationNumber;
      contract.FrequencyExpenseslitiko = contract.FrequencyOfPaymentlitiko;
      
      var counterpartySignatory = litiko.Eskhata.Contacts.GetAll()
        .FirstOrDefault(x => x.ExternalIdlitiko == isCounterpartySignatory);
      contract.CounterpartySignatory = counterpartySignatory;
      
      // Department
      if (!string.IsNullOrWhiteSpace(isDepartment))
      {
        var dept = litiko
          .Eskhata.Departments.GetAll()
          .FirstOrDefault(d => d.ExternalCodelitiko == isDepartment);
        if (dept != null)
          contract.Department = dept;
      }

      // ResponsibleEmployee
      if (!string.IsNullOrWhiteSpace(isResponsibleEmployee))
      {
        var emp = litiko
          .Eskhata.Employees.GetAll()
          .FirstOrDefault(e => e.ExternalId == isResponsibleEmployee);
        if (emp != null)
          contract.ResponsibleEmployee = emp;
      }

      // Author
      if (!string.IsNullOrWhiteSpace(isAuthor))
      {
        var author = litiko.Eskhata.Employees.GetAll()
          .FirstOrDefault(u => u.ExternalId == isAuthor);
        if (author != null)
          contract.Author = author;
      }

      // Dates — безопасно
      var parsedFrom = TryParseDate(isValidFrom);
      if (parsedFrom.HasValue)
        contract.ValidFrom = parsedFrom.Value;
      var parsedTill = TryParseDate(isValidTill);
      if (parsedTill.HasValue)
        contract.ValidTill = parsedTill.Value;

      // Money / numeric
      contract.TotalAmountlitiko = ParseDoubleSafe(isTotalAmount);

      
      if (!string.IsNullOrWhiteSpace(isCurrencyOperation))
      {
        var currencyOp = litiko.Eskhata.Currencies.GetAll()
          .FirstOrDefault(c => c.AlphaCode == isCurrencyOperation || c.NumericCode == isCurrencyOperation);
        
        if (currencyOp != null)
          contract.CurrencyOperationlitiko = currencyOp;
        else
          Logger.Debug($"Currency Operation is not found by code '{isCurrencyOperation}'");
      }
      
      // Currency
      if (!string.IsNullOrWhiteSpace(isCurrency))
      {
        var currency = litiko.Eskhata.Currencies.GetAll()
          .FirstOrDefault(c => c.AlphaCode == isCurrency || c.NumericCode == isCurrency);
        if (currency != null)
          contract.Currency = currency;
      }
      
      contract.VatRatelitiko = ParseDoubleSafe(isVATRate);
      
      contract.VatAmount = ParseDoubleSafe(isVATAmount);
      
      contract.IncomeTaxRatelitiko = ParseDoubleSafe(isIncomeTaxRate);

      contract.IsVATlitiko = ParseBoolSafe(isVATApplicable);

      // PaymentRegion
      if (!string.IsNullOrWhiteSpace(isPaymentRegion))
      {
        var paymentRegion = litiko
          .NSI.PaymentRegions.GetAll()
          .FirstOrDefault(r =>
                          r.ExternalId == isPaymentRegion || r.Code == isPaymentRegion
                         );
        if (paymentRegion != null)
          contract.PaymentRegionlitiko = paymentRegion;
      }

      // PaymentTaxRegion / RegionOfRental
      if (!string.IsNullOrWhiteSpace(isPaymentTaxRegion))
      {
        var region = litiko
          .NSI.RegionOfRentals.GetAll()
          .FirstOrDefault(r =>
                          r.ExternalId == isPaymentTaxRegion || r.Code == isPaymentTaxRegion
                         );
        if (region != null)
          contract.RegionOfRentallitiko = region;
      }

      // PaymentMethod (enum) — сопоставь текст с вариантами
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

      // Frequency
      if (!string.IsNullOrWhiteSpace(isPaymentFrequency))
      {
        var frequency = litiko
          .NSI.FrequencyOfPayments.GetAll()
          .FirstOrDefault(f =>
                          f.Name.Equals(isPaymentFrequency, StringComparison.OrdinalIgnoreCase)
                         );
        if (frequency != null)
          contract.FrequencyOfPaymentlitiko = frequency;
      }

      // Bool fields
      if (!string.IsNullOrWhiteSpace(isPartialPayment))
        contract.IsPartialPaymentlitiko = ParseBoolSafe(isPartialPayment);
      if (!string.IsNullOrWhiteSpace(isEqualPayment))
        contract.IsEqualPaymentlitiko = ParseBoolSafe(isEqualPayment);

      // AmountForPeriod
      contract.AmountForPeriodlitiko = ParseDoubleSafe(isAmountForPeriod);

      // Registration
      contract.Note = documentElement.Element("Note")?.Value;

      var regDate = TryParseDate(isRegistrationDate);
      if (regDate.HasValue)
        contract.RegistrationDate = regDate.Value;


      // Сохранить
      contract.Save();

      if (isNew)
        addedCount++;
      else
        updatedCount++;

      return contract;
    }

    // --- вспомогательные функции ---
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