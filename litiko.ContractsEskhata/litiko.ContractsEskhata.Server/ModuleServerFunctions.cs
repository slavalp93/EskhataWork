using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Xml;
using litiko.Integration;
using Sungero.Parties;

namespace litiko.ContractsEskhata.Server
{
  public class ModuleFunctions
  {

//    /// <summary>
//    /// Reads contract and counterparty data from a single XML file and creates the corresponding entities within a single transaction.
//    /// </summary>
//    /// <returns>A string indicating the result of the operation.</returns>
//    public string ProcessContractsFromXML()
//    {
//      // Best practice: Avoid hardcoding paths. Consider moving this to a configuration setting.
//      var filePath = "C:\\RxData\\git_repository\\Contracts.xml";
//
//      if (!File.Exists(filePath))
//      {
//        Logger.ErrorFormat("File not found at the specified path: {0}.", filePath);
//        // Throwing an exception is fine, or you can return an error message.
//        return string.Format("Error: File not found at '{0}'.", filePath);
//      }
//
//      try
//      {
//        XDocument xmlDoc = XDocument.Load(filePath);
//        XElement dataElement = xmlDoc.Descendants("Data").FirstOrDefault();
//
//        if (dataElement == null)
//        {
//          var errorMessage = string.Format("The <Data> tag was not found in the file: {0}.", filePath);
//          Logger.Error(errorMessage);
//          return errorMessage;
//        }
//
//        // Get the main data blocks. No loop is needed for a single document/counterparty structure.
//        XElement documentElement = dataElement.Element("Document");
//        XElement personElement = dataElement.Element("Counterparty")?.Element("Person");
//
//        if (documentElement == null)
//        {
//          var errorMessage = "XML parsing error: <Document> element is missing inside <Data>.";
//          Logger.Error(errorMessage);
//          return errorMessage;
//        }
//
//        if (personElement == null)
//        {
//          var errorMessage = "XML parsing error: <Person> element is missing inside <Counterparty>.";
//          Logger.Error(errorMessage);
//          return errorMessage;
//        }
//
//
//        foreach (var element in documentElement)
//        {
//          
//          Transactions.Execute(() =>
//                               {
//                                 Logger.Debug("Extracting Document data from XML");
//                                 // <Document>
//                                 var isExternalId = element.Element("ExternalD")?.Value;
//                                 var isDocumentKind = element.Element("DocumentKind")?.Value;
//                                 var isDocumentGroup = element.Element("DocumentGroup")?.Value;
//                                 var isSubject = element.Element("Subject")?.Value;
//                                 var isName = element.Element("Name")?.Value;
//                                 var isCounterpartySignatory = element.Element("CounterpartySignatory")?.Value;
//                                 var isDepartment = element.Element("Department")?.Value;
//                                 var isAuthor = element.Element("Author")?.Value;
//                                 var isResponsibleAccountant = element.Element("ResponsibleAccountant")?.Value;
//                                 var isResponsibleDepartment = element.Element("ResponsibleDepartment")?.Value;
//                                 var isRBO = element.Element("ResponsibleEmployee")?.Value;
//                                 var isValidFrom = element.Element("ValidFrom")?.Value;
//                                 var isValidTill = element.Element("ValidTill")?.Value;
//                                 var isСhangeReason = element.Element("СhangeReason")?.Value;
//                                 var isAccountDebtCredt = element.Element("AccountDebtCredt")?.Value;
//                                 var isAccountFutureExpense = element.Element("AccountFutureExpense")?.Value;
//                                 var isInternalAcc = element.Element("InternalAcc")?.Value;
//                                 var isTotalAmount = element.Element("TotalAmount")?.Value;
//                                 var isCurrency = element.Element("Currency")?.Value;
//                                 var isOperationCurrency = element.Element("OperationCurrency")?.Value;
//                                 var isVATApplicable = element.Element("VATApplicable")?.Value;
//                                 var isVATRate = element.Element("VATRate")?.Value;
//                                 var isVATAmount = element.Element("VATAmount")?.Value;
//                                 var isIncomeTaxRate = element.Element("IncomeTaxRate")?.Value;
//                                 var isPaymentRegion = element.Element("PaymentRegion")?.Value;
//                                 var isPaymentTaxRegion = element.Element("PaymentTaxRegion")?.Value;
//                                 var isBatchProcessing = element.Element("BatchProcessing")?.Value;
//                                 var isPaymentMethod = element.Element("PaymentMethod")?.Value;
//                                 var isPaymentFrequency = element.Element("PaymentFrequency")?.Value;
//
//                                 var isPaymentBasis = element.Element("PaymentBasis").Elements("IsPaymentContract").Elements("IsPaymentInvoice").Elements("IsPaymentTaxInvoice").Elements("IsPaymentAct").Elements("IsPaymentOrder");
//                                 var isPaymentClosureBasis = element.Element("PaymentClosureBasis").Elements("IsPaymentContract").Elements("IsPaymentInvoice").Elements("IsPaymentTaxInvoice").Elements("IsPaymentAct").Elements("IsPaymentOrder").Elements("IsPaymentWaybill");
//
//                                 var isPartialPayment = element.Element("PartialPayment")?.Value;
//                                 var isIsEqualPayment = element.Element("IsEqualPayment")?.Value;
//                                 var isAmountForPeriod = element.Element("AmountForPeriod")?.Value;
//                                 var isNote = element.Element("Note")?.Value;
//                                 var isRegistrationNumber = element.Element("RegistrationNumber")?.Value;
//                                 var isRegistrationDate = element.Element("RegistrationDate")?.Value;
//                                 // </Document>
//                                 // <Counterparty>
//                                 // <Person>
//                                 var counterparty = dataElements.Descendants("Counterpaty").Elements("Person");
//                                 var isExternalIdCounterpartyPerson = element.Element("ExternalID");
//                                 var isId = element.Element("ID").Value;
//                                 var isLastName = element.Element("LastName").Value;
//                                 var isFirstName = element.Element("FirstName").Value;
//                                 var isMiddleName = element.Element("MiddleName").Value;
//                                 var isREZIDENT = element.Element("REZIDENT").Value;
//                                 var isNU_REZIDENT = element.Element("NU_REZIDENT").Value;
//                                 var isDATE_PERS = element.Element("DATE_PERS").Value;
//                                 var isSEX = element.Element("SEX").Value;
//                                 var isINN = element.Element("INN").Value;
//                                 var isCODE_OKONH = element.Element("CODE_OKONH").Elements("element");
//                                 var isCODE_OKVED = element.Element("CODE_OKVED").Elements("element");
//                                 var isIIN = element.Element("IIN").Value;
//                                 var isCountry = element.Element("COUNTRY").Value;
//                                 var isCity = element.Element("City").Value;
//                                 var isIdentityDocument = element.Element("IdentityDocument")
//                                   .Element("Type")
//                                   .Element("DATE_BEGIN")
//                                   .Element("DATE_END")
//                                   .Element("NUM")
//                                   .Element("SER")
//                                   .Element("WHO");
//
//                                 // <Counterparty>
//                                 
//                                 // ========== Step 1: Extract Counterparty Data ==========
//                                 Logger.Debug("Extracting counterparty data from XML.");
//                                 string personInn = personElement.Element("INN")?.Value;
//                                 string personLastName = personElement.Element("LastName")?.Value;
//                                 string personFirstName = personElement.Element("FirstName")?.Value;
//                                 // ... extract other person fields as needed ...
//
//                                 // TODO: Find or create the counterparty.
//                                 // This is a simplified example. You might need more complex logic
//                                 // (e.g., checking for duplicates by INN, then by name).
//                                 var counterparty = Counterparties.GetAll(c => c.TIN == personInn).FirstOrDefault();
//                                 if (counterparty == null)
//                                 {
//                                   Logger.DebugFormat("Counterparty with INN '{0}' not found. Creating a new one.", personInn);
//                                   counterparty = Counterparties.Create();
//                                   counterparty.TIN = personInn;
//                                   counterparty.Name = string.Format("{0} {1}", personLastName, personFirstName);
//                                   // ... set other counterparty properties ...
//                                   counterparty.Save();
//                                 }
//                                 else
//                                 {
//                                   Logger.DebugFormat("Found existing counterparty with INN '{0}'.", personInn);
//                                 }
//
//                                 // ========== Step 2: Extract Document Data ==========
//                                 Logger.Debug("Extracting document data from XML.");
//                                 string subject = documentElement.Element("Subject")?.Value;
//                                 string registrationNumber = documentElement.Element("RegistrationNumber")?.Value;
//                                 // DateTime registrationDate = ... convert element value to DateTime ...
//                                 string totalAmountStr = documentElement.Element("TotalAmount")?.Value;
//                                 // ... extract all other document fields ...
//
//                                 // ========== Step 3: Create and Fill the Contract ==========
//                                 // TODO: Replace with the specific type of your contract, e.g., SupAgreements.Create();
//                                 var contract = Sungero.Contracts.ContractualDocuments.Create();
//
//                                 contract.Subject = subject;
//                                 contract.RegistrationNumber = registrationNumber;
//                                 contract.Counterparty = counterparty; // Link the counterparty found/created in Step 1.
//
//                                 if (decimal.TryParse(totalAmountStr, out decimal totalAmount))
//                                   contract.TotalAmount = totalAmount;
//
//                                 // ... set all other contract properties using the extracted variables ...
//
//                                 contract.Save();
//                                 Logger.DebugFormat("Successfully created contract '{0}' with counterparty '{1}'.", contract.Name,
//                                                    counterparty.Name);
//
//                                 // Return a success message if the transaction completes.
//                                 return "Operation successful: The contract and counterparty have been processed.";
//                               });
//        }
//        catch (Exception ex)
//        {
//          // This will catch errors during file loading, XML parsing, or transaction execution.
//          Logger.Error("A critical error occurred during XML file processing.", ex);
//          return string.Format("Error: {0}", ex.Message);
//        }
//      }
    }
  }
