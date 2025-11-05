using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using litiko.Eskhata.Module.Contracts.Structures.Module;
using Sungero.Core;
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
            var result =
                litiko.Eskhata.Module.Contracts.Structures.Module.ResultImportXmlUI.Create();
            result.Errors = new List<string>();
            result.ImportedCount = 0;
            result.TotalCount = 0;

            Logger.Debug("Import contracts from XML - Start");

            var xmlPathFile = "Contracts.xml";

            if (!System.IO.File.Exists(xmlPathFile))
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

                if (!documentElements.Any())
                {
                    result.Errors.Add("В файле нет элементов <Document> для импорта");
                    Logger.Error("No <Document> elements found");
                    return result;
                }

                for (var i = 0; i < documentElements.Count; i++)
                {
                    result.TotalCount++;
                    
                    try
                    {
                        var documentElement = documentElements[i];

                        if (documentElements[i] == null)
                        {
                            result.Errors.Add($"Элемент <Document> №{i + 1} пустой, пропущен");
                            continue;
                        }

                        var counterpartyElement = counterpartyElements.ElementAtOrDefault(i);
                        if (counterpartyElement == null)
                        {
                            result.Errors.Add($"Элемент <Counterparty> №{i + 1} отсутствует, пропущен");
                            Logger.DebugFormat($"No <Counterparty> element for document №{i + 1}, skipping.");
                            continue;
                        }
                        var counterparty = ParseCounterparty(counterpartyElement);
                        var contract = ParseContract(documentElement, counterparty, result);
                        if (contract == null)
                            continue;
                        result.ImportedCount++;

                        Logger.Debug($"Импортирован договор: {contract.Name}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Error due parsing Counterparty or Document №{i + 1}: {ex.Message}");
                        result.Errors.Add($"Ошибка при импорте документа №{i + 1}: {ex.Message}");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error due parsing Counterparty or Document: {ex.Message}");
                result.Errors.Add($"Общая ошибка импорта: {ex.Message}");
                throw;
            }
            return result;
        }

        private ICounterparty ParseCounterparty(XElement counterpartyElement)
        {
            if (counterpartyElement == null)
                return null;

            var personElement = counterpartyElement.Element("Person");
            var companyElement = counterpartyElement.Element("Company");

            if (companyElement != null)
            {
                return ParseCompany(companyElement);
            }
            Logger.DebugFormat("No <Company> element found in <Counterparty>, skipping.");
            return null;
        }

        private ICompany ParseCompany(XElement companyElement)
        {
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
            var isEmail = companyElement.Element("Email")?.Value;
            var isWebSite = companyElement.Element("WebSite")?.Value;
            var isTaxNonResident = companyElement.Element("TaxNonResident")?.Value;
            var isVatPayer = companyElement.Element("VATPayer")?.Value;
            var isReliability = companyElement.Element("Reliability")?.Value;
            var isCorrAcc = companyElement.Element("CorrAcc")?.Value;
            var isInernalAcc = companyElement.Element("InternalAcc")?.Value;

            var company = litiko.Eskhata.Companies.GetAll().FirstOrDefault(x => x.ExternalId == isExternalD || x.TIN == isINN);

            if (company != null)
            {
                return company;
            }

            company = litiko.Eskhata.Companies.Create();

            company.ExternalId = isExternalD;
            company.Name = isName;
            company.TIN = isINN;
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

            foreach (var isCodeOKONH in isCodeOKONHelements)
            {
                var okonh = litiko
                    .NSI.OKONHs.GetAll()
                    .Where(x => x.ExternalId == isCodeOKONH.Value)
                    .FirstOrDefault();
                if (okonh != null && !company.OKONHlitiko.Any(x => Equals(x.OKONH, okonh)))
                {
                    var newRecord = company.OKONHlitiko.AddNew();
                    newRecord.OKONH = okonh;
                }
            }

            foreach (var isCodeOKVED in isCodeOKVEDelements)
            {
                var okved = litiko
                    .NSI.OKVEDs.GetAll()
                    .Where(x => x.ExternalId == isCodeOKVED.Value)
                    .FirstOrDefault();
                if (okved != null && !company.OKVEDlitiko.Any(x => Equals(x.OKVED, okved)))
                {
                    var newRecord = company.OKVEDlitiko.AddNew();
                    newRecord.OKVED = okved;
                    Logger.DebugFormat("Change OKVEDlitiko: added:{0}", okved.Name);
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
            company.Email = isEmail;
            company.Homepage = isWebSite;
            company.VATPayerlitiko = ParseBoolSafe(isVatPayer);
            company.AccountEskhatalitiko = isInernalAcc;
            company.Account = isCorrAcc;

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
            company.Save();
            Logger.DebugFormat($"Create new Company: {company.Name}");
            return company;
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
            var isSubject = documentElement.Element("Subject")?.Value?.Trim();
            var isName = documentElement.Element("Name")?.Value;
            var isCounterpartySignatory = documentElement
                .Element("CounterpartySignatory")
                ?.Value?.Trim();
            var isDepartment = documentElement.Element("Department")?.Value?.Trim();
            var isResponsibleEmployee = documentElement
                .Element("ResponsibleEmployee")
                ?.Value?.Trim();
            var isAuthor = documentElement.Element("Author")?.Value?.Trim();
            var isRBO = documentElement.Element("RBO")?.Value ?? "";
            var isValidFrom = documentElement.Element("ValidFrom")?.Value?.Trim();
            var isValidTill = documentElement.Element("ValidTill")?.Value?.Trim();
            var isChangeReason = documentElement.Element("ChangeReason")?.Value.Trim();
            var isAccountDebtCredt = documentElement.Element("AccountDebtCredt")?.Value.Trim();
            var isAccountFutureExpense = documentElement
                .Element("AccountFutureExpense")
                ?.Value.Trim();
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
            var contract = Eskhata.Contracts.GetAll().FirstOrDefault(x => 
                (!string.IsNullOrEmpty(isExternalD) && x.ExternalId == isExternalD));

            if(contract != null)
            {
                Logger.DebugFormat($"Contract with ExternalId {isExternalD} found, continue.");
                return null;
            }
            
           
            if (string.IsNullOrWhiteSpace(isExternalD))
            {
                result.Errors.Add($"У документа с именем {isName} пропущен ExternalId");
                Logger.Error("Missing ExternalId in XML File");
                throw new KeyNotFoundException("У документа отсутствует внешний идентификатор");
            }
            contract = Eskhata.Contracts.Create();

            contract.ExternalId = isExternalD;
            

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

            var counterpartySignatory = litiko
                .Eskhata.Contacts.GetAll()
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
                var author = litiko
                    .Eskhata.Employees.GetAll()
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
                var currencyOp = litiko
                    .Eskhata.Currencies.GetAll()
                    .FirstOrDefault(c =>
                        c.AlphaCode == isCurrencyOperation || c.NumericCode == isCurrencyOperation
                    );

                if (currencyOp != null)
                    contract.CurrencyOperationlitiko = currencyOp;
                else
                    Logger.Debug(
                        $"Currency Operation is not found by code '{isCurrencyOperation}'"
                    );
            }

            // Currency
            if (!string.IsNullOrWhiteSpace(isCurrency))
            {
                var currency = litiko
                    .Eskhata.Currencies.GetAll()
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
