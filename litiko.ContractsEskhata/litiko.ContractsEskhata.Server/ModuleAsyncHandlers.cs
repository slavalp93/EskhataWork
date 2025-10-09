using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Parties;
using Sungero.Docflow;
using Sungero.Contracts;
using Sungero.Company;
using Sungero.Commons;

namespace litiko.ContractsEskhata.Server
{
  public class ModuleAsyncHandlers
  {

    public virtual void ContractsBatchImportHandler(litiko.ContractsEskhata.Server.AsyncHandlerInvokeArgs.ContractsBatchImportHandlerInvokeArgs args)
    {
      string filePath = "C:\\RxData\\git_repository\\Contracts.xml";
      
      Logger.DebugFormat("Starting async batch import from file");
      
      if(!File.Exists(filePath))
      {
        Logger.ErrorFormat("File for batch import not found: {0}", filePath);
        return;
      }
      
      int successCount = 0;
      int errorCount = 0;
      var errorDetails = new StringBuilder();
      
      try
      {
        XDocument xmlDoc = XDocument.Load(filePath);
        XElement dataElement = xmlDoc.Descendants("Data").FirstOrDefault();
        
        if (dataElement == null)
        {
          Logger.ErrorFormat("XML structure error: <Data> tag not found in file {0}.", filePath);
          return;
        }
        
        var allDocumentElements = dataElement.Elements("Document").ToList();
        Logger.DebugFormat("Found {0} <Document> elements to process.", allDocumentElements.Count);

        foreach (var docElement in allDocumentElements)
        {
          string externalId = docElement.Element("ExternalId")?.Value ?? "N/A";


          var counterpartyElement = docElement.ElementsAfterSelf("Counterparty").FirstOrDefault();
          if (counterpartyElement == null)
            throw new InvalidDataException(
              "Data integrity error: <Counterparty> element is missing after its corresponding <Document> element.");

          Transactions.Execute(() =>
          {
            // <Document>
            var isExternalId = docElement.Element("ExternalD")?.Value;
            var isDocumentKind = docElement.Element("DocumentKind")?.Value;
            var isDocumentGroup = docElement.Element("DocumentGroup")?.Value;
            var isSubject = docElement.Element("Subject")?.Value;
            var isName = docElement.Element("Name")?.Value;
            var isCounterpartySignatory = docElement.Element("CounterpartySignatory")?.Value;
            var isDepartment = docElement.Element("Department")?.Value;
            var isAuthor = docElement.Element("Author")?.Value;
            var isResponsibleAccountant = docElement.Element("ResponsibleAccountant")?.Value;
            var isResponsibleDepartment = docElement.Element("ResponsibleDepartment")?.Value;
            var isRBO = docElement.Element("ResponsibleEmployee")?.Value;
            var isValidFrom = docElement.Element("ValidFrom")?.Value;
            var isValidTill = docElement.Element("ValidTill")?.Value;
            var isСhangeReason = docElement.Element("СhangeReason")?.Value;
            var isAccountDebtCredit = docElement.Element("AccountDebtCredit")?.Value;
            var isAccountFutureExpense = docElement.Element("AccountFutureExpense")?.Value;
            var isInternalAcc = docElement.Element("InternalAcc")?.Value;
            var isTotalAmount = docElement.Element("TotalAmount")?.Value;
            var isCurrency = docElement.Element("Currency")?.Value;
            var isOperationCurrency = docElement.Element("OperationCurrency")?.Value;
            var isVATApplicable = docElement.Element("VATApplicable")?.Value;
            var isVATRate = docElement.Element("VATRate")?.Value;
            var isVATAmount = docElement.Element("VATAmount")?.Value;
            var isIncomeTaxRate = docElement.Element("IncomeTaxRate")?.Value;
            var isPaymentRegion = docElement.Element("PaymentRegion")?.Value;
            var isPaymentTaxRegion = docElement.Element("PaymentTaxRegion")?.Value;
            var isBatchProcessing = docElement.Element("BatchProcessing")?.Value;
            var isPaymentMethod = docElement.Element("PaymentMethod")?.Value;
            var isPaymentFrequency = docElement.Element("PaymentFrequency")?.Value;
            var isPaymentBasis = docElement.Element("PaymentBasis").Elements("IsPaymentContract")
              .Elements("IsPaymentInvoice").Elements("IsPaymentTaxInvoice").Elements("IsPaymentAct")
              .Elements("IsPaymentOrder");
            var isPaymentClosureBasis = docElement.Element("PaymentClosureBasis").Elements("IsPaymentContract")
              .Elements("IsPaymentInvoice").Elements("IsPaymentTaxInvoice").Elements("IsPaymentAct")
              .Elements("IsPaymentOrder").Elements("IsPaymentWaybill");
            var isPartialPayment = docElement.Element("PartialPayment")?.Value;
            var isIsEqualPayment = docElement.Element("IsEqualPayment")?.Value;
            var isAmountForPeriod = docElement.Element("AmountForPeriod")?.Value;
            var isNote = docElement.Element("Note")?.Value;
            var isRegistrationNumber = docElement.Element("RegistrationNumber")?.Value;
            var isRegistrationDate = docElement.Element("RegistrationDate")?.Value;

            
            
           /* var personElement = counterpartyElement.ElementsAfterSelf("Person").FirstOrDefault();
            if (personElement == null)
              throw new InvalidDataException("<Person> element is missing inside <Counterparty>.");
            
            string inn = personElement.Element("INN")?.Value;
            if (string.IsNullOrWhiteSpace(inn))
              throw new InvalidDataException("Counterparty INN is missing or empty.");

            ICounterparty counterparty = Counterparties.GetAll(c => c.TIN == inn).FirstOrDefault();*/
           
           
          });
        }
      }
      catch (Exception ex)
      {
        
        throw;
      }
    }

  }
}