using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Parties.PublicFunctions;

namespace litiko.Integration.Structures.Module
{

  /// <summary>
  /// Результат обработки должности
  /// </summary>
  partial class ProcessingJobTittleResult
  {
    /// <summary>
    /// Должность.
    /// </summary>
    public Eskhata.IJobTitle jobTittle { get; set; }
    
    /// <summary>
    /// Признак, было ли изменение или создание.
    /// </summary>
    public bool isCreatedOrUpdated { get; set; }
  }
  
  /// <summary>
  /// Результат обработки персоны
  /// </summary>
  partial class ProcessingPersonResult
  {
    /// <summary>
    /// Персона.
    /// </summary>
    public Eskhata.IPerson person { get; set; }
    
    /// <summary>
    /// Признак, было ли изменение или создание.
    /// </summary>
    public bool isCreatedOrUpdated { get; set; }
  }
  
  /// <summary>
  /// Информация о ФИО
  /// </summary>
  partial class FIOInfo
  {
    /// <summary>
    /// Фамилия.
    /// </summary>
    public string LastNameRU { get; set; }
    
    /// <summary>
    /// Имя.
    /// </summary>
    public string FirstNameRU { get; set; }
    
    /// <summary>
    /// Отчество.
    /// </summary>
    public string MiddleNameRU { get; set; }
    
    /// <summary>
    /// Фамилия тадж.
    /// </summary>
    public string LastNameTG { get; set; }
    
    /// <summary>
    /// Имя тадж.
    /// </summary>
    public string FirstNameTG { get; set; }
    
    /// <summary>
    /// Отчество тадж.
    /// </summary>
    public string MiddleNameTG { get; set; }
    
  }
//
//  [XmlRoot]
//  public class DocumentInfo
//  {
//    [XmlElement("ExternalD")]
//    public string ExternalD { get; set; }
//
//    [XmlElement("DocumentKind")]
//    public string DocumentKind { get; set; }
//
//    [XmlElement("DocumentGroup")]
//    public string DocumentGroup { get; set; }
//
//    [XmlElement("Subject")]
//    public string Subject { get; set; }
//
//    [XmlElement("Name")]
//    public string Name { get; set; }
//
//    [XmlElement("CounterpartySignatory")]
//    public string CounterpartySignatory { get; set; }
//
//    [XmlElement("Department")]
//    public string Department { get; set; }
//
//    [XmlElement("ResponsibleEmployee")]
//    public string ResponsibleEmployee { get; set; }
//
//    [XmlElement("Author")]
//    public string Author { get; set; }
//
//    [XmlElement("ResponsibleAccountant")]
//    public string ResponsibleAccountant { get; set; }
//
//    [XmlElement("ResponsibleDepartment")]
//    public string ResponsibleDepartment { get; set; }
//
//    [XmlElement("RBO")]
//    public string RBO { get; set; }
//
//    [XmlElement("ValidFrom")]
//    public string ValidFrom { get; set; } 
//
//    [XmlElement("ValidTill")]
//    public string ValidTill { get; set; } 
//
//    [XmlElement("ChangeReason")] 
//    public string ChangeReason { get; set; }
//
//    [XmlElement("AccountDebtCredt")]
//    public string AccountDebtCredt { get; set; }
//
//    [XmlElement("AccountFutureExpense")]
//    public string AccountFutureExpense { get; set; }
//
//    [XmlElement("InternalAcc")]
//    public string InternalAcc { get; set; }
//
//    [XmlElement("TotalAmount")]
//    public decimal TotalAmount { get; set; }
//
//    [XmlElement("Currency")]
//    public string Currency { get; set; }
//
//    [XmlElement("OperationCurrency")]
//    public string OperationCurrency { get; set; }
//
//    [XmlElement("VATApplicable")]
//    public bool VATApplicable { get; set; }
//
//    [XmlElement("VATRate")]
//    public int VATRate { get; set; }
//
//    [XmlElement("VATAmount")]
//    public decimal VATAmount { get; set; }
//
//    [XmlElement("IncomeTaxRate")]
//    public int IncomeTaxRate { get; set; }
//
//    [XmlElement("PaymentRegion")]
//    public string PaymentRegion { get; set; }
//
//    [XmlElement("PaymentTaxRegion")]
//    public string PaymentTaxRegion { get; set; }
//
//    [XmlElement("BatchProcessing")]
//    public bool BatchProcessing { get; set; }
//
//    [XmlElement("PaymentMethod")]
//    public string PaymentMethod { get; set; }
//
//    [XmlElement("PaymentFrequency")]
//    public string PaymentFrequency { get; set; }
//
//    [XmlElement("PaymentBasis")]
//    public PaymentBasisInfo PaymentBasis { get; set; }
//
//    [XmlElement("PaymentClosureBasis")]
//    public PaymentClosureBasis PaymentClosureBasis { get; set; }
//
//    [XmlElement("IsPartialPayment")]
//    public bool IsPartialPayment { get; set; }
//
//    [XmlElement("IsEqualPayment")]
//    public bool IsEqualPayment { get; set; }
//
//    [XmlElement("AmountForPeriod")]
//    public decimal AmountForPeriod { get; set; }
//
//    [XmlElement("Note")]
//    public string Note { get; set; }
//
//    [XmlElement("RegistrationNumber")]
//    public string RegistrationNumber { get; set; }
//
//    [XmlElement("RegistrationDate")]
//    public string RegistrationDate { get; set; }
//    
//    [XmlElement("Counterparty")]
//    public CounterpartyInfo CounterpartyInfo { get; set; }
//    
//  }
//  
//  partial class PaymentBasisInfo
//  {
//    [XmlElement("IsPaymentContract")]
//    public bool IsPaymentContract { get; set; }
//
//    [XmlElement("IsPaymentInvoice")]
//    public bool IsPaymentInvoice { get; set; }
//
//    [XmlElement("IsPaymentTaxInvoice")]
//    public bool IsPaymentTaxInvoice { get; set; }
//
//    [XmlElement("IsPaymentAct")]
//    public bool IsPaymentAct { get; set; }
//
//    [XmlElement("IsPaymentOrder")]
//    public bool IsPaymentOrder { get; set; }
//    
//  }
//  
//  public class PaymentClosureBasisInfo
//  {
//    [XmlElement("IsPaymentContract")]
//    public bool IsPaymentContract { get; set; }
//
//    [XmlElement("IsPaymentInvoice")]
//    public bool IsPaymentInvoice { get; set; }
//
//    [XmlElement("IsPaymentTaxInvoice")]
//    public bool IsPaymentTaxInvoice { get; set; }
//
//    [XmlElement("IsPaymentAct")]
//    public bool IsPaymentAct { get; set; }
//
//    [XmlElement("IsPaymentOrder")]
//    public bool IsPaymentOrder { get; set; }
//
//    [XmlElement("IsPaymentWaybill")]
//    public bool IsPaymentWaybill { get; set; }
//  }
//
//  public class CounterpartyInfo
//  {
//    [XmlElement("Person")]
//    public PersonInfo Person { get; set; }
//  }
//
//  public class PersonInfo
//  {
//    [XmlElement("ExternalID")]
//    public string ExternalID { get; set; }
//
//    [XmlElement("ID")]
//    public string ID { get; set; }
//
//    [XmlElement("LastName")]
//    public string LastName { get; set; }
//
//    [XmlElement("FirstName")]
//    public string FirstName { get; set; }
//
//    [XmlElement("MiddleName")]
//    public string MiddleName { get; set; }
//
//    [XmlElement("REZIDENT")]
//    public bool REZIDENT { get; set; }
//
//    [XmlElement("NU_REZIDENT")]
//    public bool NU_REZIDENT { get; set; }
//
//    [XmlElement("DATE_PERS")]
//    public string DATE_PERS { get; set; }
//
//    [XmlElement("SEX")]
//    public string SEX { get; set; }
//
//    [XmlElement("MARIGE_ST")]
//    public string MARIGE_ST { get; set; }
//
//    [XmlElement("INN")]
//    public string INN { get; set; }
//    
//    // Для списков элементов, как <CODE_OKONH>
//    [XmlArray("CODE_OKONH")]
//    [XmlArrayItem("element")]
//    public List<string> CodeOkonh { get; set; }
//
//    [XmlArray("CODE_OKVED")]
//    [XmlArrayItem("element")]
//    public List<string> CodeOkved { get; set; }
//
//    [XmlElement("IIN")]
//    public string IIN { get; set; }
//
//    [XmlElement("COUNTRY")]
//    public string COUNTRY { get; set; }
//
//    [XmlElement("City")]
//    public string City { get; set; }
//
//    [XmlElement("IdentityDocument")]
//    public IdentityDocumentInfo IdentityDocument { get; set; }
//  }
//  public class IdentityDocumentInfo
//  {
//    [XmlElement("TYPE")]
//    public string TYPE { get; set; }
//
//    [XmlElement("DATE_BEGIN")]
//    public string DATE_BEGIN { get; set; }
//
//    [XmlElement("DATE_END")]
//    public string DATE_END { get; set; }
//
//    [XmlElement("NUM")]
//    public string NUM { get; set; }
//
//    [XmlElement("SER")]
//    public string SER { get; set; }
//
//    [XmlElement("WHO")]
//    public string WHO { get; set; }
//  }
}