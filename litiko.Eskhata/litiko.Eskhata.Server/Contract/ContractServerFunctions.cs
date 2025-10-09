using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Text.RegularExpressions;

using System.Xml.Serialization;
using litiko.Eskhata.Contract;

namespace litiko.Eskhata.Server
{
  partial class ContractFunctions
  {
    /// <summary>
    /// Импорт договоров из файла экспортированных из АБС
    /// </summary>
    [Remote, Public]
    public List<string> ImportContractsFromXml()
    {
      var errorList = new List<string>();
      
      string filePath = "C:\\RxData\\git_repository\\Contracts.xml";

      if(!File.Exists(filePath))
      {
        Logger.Error("XML file is not exist");
        return errorList;
      }
      
      try
      {
       // var documentKind = litiko.Eskhata.Contract
       //var person = litiko.Eskhata.Contracts.As()
        XDocument xmlDoc = XDocument.Load(filePath);
        
        XElement dataElement = xmlDoc.Descendants("Data").FirstOrDefault();
        
        if(dataElement == null)
        {
          Logger.Error("<DATA> tag is not found.");
          return errorList;
        }
        
        foreach (var docElement in dataElement.Elements("Document"))
        {
          var contract = litiko.Eskhata.Contracts.Create();

          contract.ExternalId = docElement.Element("ExternalD")?.Value;
          
          var documentKind = docElement.Element("DocumentKind")?.Value;
          var docKind = Sungero.Docflow.DocumentKinds.GetAll().FirstOrDefault(k=>k.Code == documentKind);
          if(docKind != null)
            contract.DocumentKind = docKind;
          else
            Logger.Error("DocumentKind not found");
          
          var documentGroup = docElement.Element("DocumentGroup").Value;
          if(documentGroup != null)
            contract.DocumentGroup = documentGroup;
          else
            Logger.Error("Document group not found");
          
          contract.Subject = docElement.Element("Subject").Value;
          contract.Name = docElement.Element("Name").Value;
          
          /*var counterpartySignatory = (Sungero.Parties.IContact)(docElement.Element("CounterpartySignatory"));
          contract.CounterpartySignatory = counterpartySignatory;*/
         
          /*contract.Department = (Sungero.Company.IDepartment)docElement.Element("Department");
          contract.ResponsibleEmployee = (Sungero.Company.IEmployee)docElement.Element("ResponsibleEmployee");
          contract.Author = (IUser)docElement.Element("Author");/*
          contract.ResponsibleAccountantlitiko = docElement.Element("ResponsibleAccountant").Value; 
          contract.ResponsibleDepartmentlitiko = docElement.Element("ResponsibleDepartment").Value; 
          contract.RBOlitiko = docElement.Element("RBO").Value;
          contract.ValidFrom = docElement.Element("ValidFrom").Value;
          contract.ValidTill = docElement.Element("ValidTill").Value;
          contract.ReasonForChangelitiko = docElement.Element("СhangeReason").Value;
          contract.AccDebtCreditlitiko = docElement.Element("AccountDebtCredt").Value;
          contract.AccFutureExpenselitiko = docElement.Element("AccountFutureExpense").Value;
          contract.InternalAcclitikoternalAcc = docElement.Element("InternalAcc").Value; // create
          contract.TotalAmountlitiko = docElement.Element("TotalAmount").Value;
          contract.Currency = docElement.Element("Currency").Value;
          contract.CurrencyOperationlitiko = docElement.Element("OperationCurrency").Value;
          contract.IsVATlitiko = docElement.Element("VATApplicable").Value;
          contract.VatRatelitiko = docElement.Element("VATRate").Value;
          contract.VatAmount = docElement.Element("VATAmount").Value;
          contract.IncomeTaxRatelitiko = docElement.Element("IncomeTaxRate").Value;
          contract.PaymentRegionlitiko = docElement.Element("PaymentRegion").Value;
          contract.PaymentTaxRegionlitiko = docElement.Element("PaymentTaxRegion").Value;
          contract.BatchProcessinglitiko = docElement.Element("BatchProcessing").Value;
          contract.PaymentMethodlitiko = docElement.Element("PaymentMethod").Value;
          contract.FrequencyOfPaymentlitiko = docElement.Element("PaymentFrequency").Value;
          */
          
         /* var paymentBasisElement = docElement.Element("PaymentBasis");
          if(paymentBasisElement != null)
          {
            var item = contract.PaymentBasislitiko.AddNew();
            
            item.IsPaymentContract   = (bool)paymentBasisElement.Element("IsPaymentContract");
            item.IsPaymentInvoice    = (bool)paymentBasisElement.Element("IsPaymentInvoice");
            item.IsPaymentTaxInvoice = (bool)paymentBasisElement.Element("IsPaymentTaxInvoice");
            item.IsPaymentAct        = (bool)paymentBasisElement.Element("IsPaymentAct");
            item.IsPaymentOrder      = (bool)paymentBasisElement.Element("IsPaymentOrder");
          }

          var paymentClosureBasis = docElement.Element("PaymentClosureBasis");
          if (paymentClosureBasis != null)
          {
            var item = contract.PaymentClosureBasislitiko.AddNew();
            
            item.IsPaymentContract   = (bool)paymentClosureBasis.Element("IsPaymentContract");
            item.IsPaymentInvoice    = (bool)paymentClosureBasis.Element("IsPaymentInvoice");
            item.IsPaymentTaxInvoice = (bool)paymentClosureBasis.Element("IsPaymentTaxInvoice");
            item.IsPaymentAct        = (bool)paymentClosureBasis.Element("IsPaymentAct");
            item.IsPaymentOrder      = (bool)paymentClosureBasis.Element("IsPaymentOrder");
            item.IsPaymentWaybill    = (bool)paymentClosureBasis.Element("IsPaymentWaybill");
          }
          /*
          contract.IsPartialPaymentlitiko = docElement.Element("IsPartialPayment");
          contract.FrequencyOfPaymentlitiko = docElement.Element("PaymentFrequency").Value;
          contract.FrequencyOfPaymentlitiko = docElement.Element("PaymentFrequency").Value;
          contract.FrequencyOfPaymentlitiko = docElement.Element("PaymentFrequency").Value;
          contract.FrequencyOfPaymentlitiko = docElement.Element("PaymentFrequency").Value;
          contract.FrequencyOfPaymentlitiko = docElement.Element("PaymentFrequency").Value;
          contract.FrequencyOfPaymentlitiko = docElement.Element("PaymentFrequency").Value;
          contract.FrequencyOfPaymentlitiko = docElement.Element("PaymentFrequency").Value;
          contract.FrequencyOfPaymentlitiko = docElement.Element("PaymentFrequency").Value;
          */
          
          
         contract.Save();
          
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("{0}", ex);
     
      }

      return errorList;
    }
    
    /// <summary>
    /// Создать юрид. заключение.
    /// </summary>
    /// <returns>Юридическое заключение.</returns>
    [Remote, Public]
    public static Sungero.Docflow.IAddendum CreateLegalOpinion()
    {
      var aviabledDocumentKinds = Sungero.Docflow.PublicFunctions.DocumentKind.GetAvailableDocumentKinds(typeof(Sungero.Docflow.IAddendum));
      var docKind = aviabledDocumentKinds
        .Where(x => x.Status == Sungero.CoreEntities.DatabookEntry.Status.Active && x.Name == "Юридическое заключение")
        .FirstOrDefault();
      
      if (docKind == null)
        return null;
      
      var newDoc = Sungero.Docflow.Addendums.Create();
      newDoc.DocumentKind = docKind;
      return newDoc;
    }
    
    /// <summary>
    /// Построить сводку по документу.
    /// </summary>
    /// <returns>Сводка по документу.</returns>
    [Remote(IsPure = true)]
    public override StateView GetDocumentSummary()
    {
      var documentSummary = StateView.Create();
      var documentBlock = documentSummary.AddBlock();
      
      // Краткое имя документа.
      var documentName = _obj.DocumentKind.Name;
      if (!string.IsNullOrWhiteSpace(_obj.RegistrationNumber))
        documentName += Sungero.Docflow.OfficialDocuments.Resources.Number + _obj.RegistrationNumber;
      
      if (_obj.RegistrationDate != null)
        documentName += Sungero.Docflow.OfficialDocuments.Resources.DateFrom + _obj.RegistrationDate.Value.ToString("d");
      
      documentBlock.AddLabel(documentName);
      
      // Типовой/Не типовой, Рамочный.
      var isStandardLabel = _obj.IsStandard.Value ? Sungero.Contracts.ContractBases.Resources.isStandartContract : Sungero.Contracts.ContractBases.Resources.isNotStandartContract;
      var isframeworkContractLabel = _obj.IsFrameworkContract.Value ? _obj.Info.Properties.IsFrameworkContract.LocalizedName : string.Empty;
      
      if (string.IsNullOrEmpty(isframeworkContractLabel))
        documentBlock.AddLabel(string.Format("({0})", isStandardLabel));
      else
        documentBlock.AddLabel(string.Format("({0}, {1})", isStandardLabel, isframeworkContractLabel));
      documentBlock.AddLineBreak();
      documentBlock.AddLineBreak();
      
      // НОР.
      documentBlock.AddLabel(string.Format("{0}: ", _obj.Info.Properties.BusinessUnit.LocalizedName));
      if (_obj.BusinessUnit != null)
        documentBlock.AddLabel(Hyperlinks.Get(_obj.BusinessUnit));
      else
        documentBlock.AddLabel("-");
      
      documentBlock.AddLineBreak();
      
      // Контрагент.
      documentBlock.AddLabel(string.Format("{0}:", Sungero.Contracts.ContractBases.Resources.Counterparty));
      if (_obj.Counterparty != null)
      {
        documentBlock.AddLabel(Hyperlinks.Get(_obj.Counterparty));
        if (_obj.Counterparty.Nonresident == true)
          documentBlock.AddLabel(string.Format("({0})", _obj.Counterparty.Info.Properties.Nonresident.LocalizedName).ToLower());
      }
      else
      {
        documentBlock.AddLabel("-");
      }
      
      documentBlock.AddLineBreak();
      
      // Содержание.
      var subject = !string.IsNullOrEmpty(_obj.Subject) ? _obj.Subject : "-";
      documentBlock.AddLabel(string.Format("{0}: {1}", Sungero.Contracts.ContractBases.Resources.Subject, subject));
      documentBlock.AddLineBreak();
      
      // Сумма договора.
      var amount = this.GetTotalAmountDocumentSummary(_obj.TotalAmountlitiko);
      var amountText = string.Format("{0}: {1}", _obj.Info.Properties.TotalAmountlitiko.LocalizedName, amount);
      documentBlock.AddLabel(amountText);
      documentBlock.AddLineBreak();

      // Валюта.
      var currencyText = string.Format("{0}: {1}", _obj.Info.Properties.CurrencyContractlitiko.LocalizedName, _obj.CurrencyContractlitiko);
      documentBlock.AddLabel(currencyText);
      documentBlock.AddLineBreak();
      
      // Срок действия договора.
      var validity = "-";
      var validFrom = _obj.ValidFrom.HasValue ?
        string.Format("{0} {1} ", Sungero.Contracts.ContractBases.Resources.From, _obj.ValidFrom.Value.Date.ToShortDateString()) :
        string.Empty;
      
      var validTill = _obj.ValidTill.HasValue ?
        string.Format("{0} {1}", Sungero.Contracts.ContractBases.Resources.Till, _obj.ValidTill.Value.Date.ToShortDateString()) :
        string.Empty;
      
      var isAutomaticRenewal = _obj.IsAutomaticRenewal.Value &&  !string.IsNullOrEmpty(validTill) ?
        string.Format(", {0}", Sungero.Contracts.ContractBases.Resources.Renewal) :
        string.Empty;
      
      if (!string.IsNullOrEmpty(validFrom) || !string.IsNullOrEmpty(validTill))
        validity = string.Format("{0}{1}{2}", validFrom, validTill, isAutomaticRenewal);
      
      var validityText = string.Format("{0}:", Sungero.Contracts.ContractBases.Resources.Validity);
      documentBlock.AddLabel(validityText);
      documentBlock.AddLabel(validity);
      documentBlock.AddLineBreak();
      documentBlock.AddEmptyLine();
      
      // Примечание.
      var note = string.IsNullOrEmpty(_obj.Note) ? "-" : _obj.Note;
      var noteText = string.Format("{0}:", Sungero.Contracts.ContractBases.Resources.Note);
      documentBlock.AddLabel(noteText);
      documentBlock.AddLabel(note);
      
      return documentSummary;
    }
  }
}