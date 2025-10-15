using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.Xml.Serialization;

namespace litiko.Eskhata.Structures.Contracts.Contract
{
/*
[XmlRoot("root")]
public class ContractImportData
{
    [XmlElement("head")]
    public Head Head { get; set; }
    
    [XmlElement("request")]
    public Request Request { get; set; }
}

public class Head
{
    [XmlElement("session_id")]
    public string SessionId { get; set; }
    
    [XmlElement("protocol")]
    public string Protocol { get; set; }
    
    [XmlElement("application_key")]
    public string ApplicationKey { get; set; }
}

public class Request
{
    [XmlElement("protocol-version")]
    public string ProtocolVersion { get; set; }
    
    [XmlElement("request-type")]
    public string RequestType { get; set; }
    
    [XmlElement("dictionary")]
    public string Dictionary { get; set; }
    
    [XmlElement("lastId")]
    public int LastId { get; set; }
    
    [XmlElement("Data")]
    public Data Data { get; set; }
}

public class Data
{
    [XmlElement("Document")]
    public Document[] Documents { get; set; }
    
    [XmlElement("Counterparty")]
    public Counterparty[] Counterparties { get; set; }
}

public class Document
{
    [XmlElement("ExternalD")]
    public string ExternalId { get; set; }
    
    [XmlElement("DocumentKind")]
    public int DocumentKind { get; set; }
    
    [XmlElement("DocumentGroup")]
    public int DocumentGroup { get; set; }
    
    [XmlElement("Subject")]
    public string Subject { get; set; }
    
    [XmlElement("Name")]
    public string Name { get; set; }
    
    [XmlElement("CounterpartySignatory")]
    public string CounterpartySignatory { get; set; }
    
    [XmlElement("Department")]
    public string Department { get; set; }
    
    [XmlElement("ResponsibleEmployee")]
    public string ResponsibleEmployee { get; set; }
    
    [XmlElement("Author")]
    public string Author { get; set; }
    
    [XmlElement("ResponsibleAccountant")]
    public string ResponsibleAccountant { get; set; }
    
    [XmlElement("ResponsibleDepartment")]
    public string ResponsibleDepartment { get; set; }
    
    [XmlElement("RBO")]
    public string RBO { get; set; }
    
    [XmlElement("ValidFrom")]
    public string ValidFrom { get; set; }
    
    [XmlElement("ValidTill")]
    public string ValidTill { get; set; }
    
    [XmlElement("СhangeReason")]
    public string ChangeReason { get; set; }
    
    [XmlElement("AccountDebtCredt")]
    public string AccountDebtCredit { get; set; }
    
    [XmlElement("AccountFutureExpense")]
    public string AccountFutureExpense { get; set; }
    
    [XmlElement("InternalAcc")]
    public string InternalAcc { get; set; }
    
    [XmlElement("TotalAmount")]
    public decimal TotalAmount { get; set; }
    
    [XmlElement("Currency")]
    public string Currency { get; set; }
    
    [XmlElement("OperationCurrency")]
    public string OperationCurrency { get; set; }
    
    [XmlElement("VATApplicable")]
    public bool VATApplicable { get; set; }
    
    [XmlElement("VATRate")]
    public decimal VATRate { get; set; }
    
    [XmlElement("VATAmount")]
    public decimal VATAmount { get; set; }
    
    [XmlElement("IncomeTaxRate")]
    public decimal IncomeTaxRate { get; set; }
    
    [XmlElement("PaymentRegion")]
    public string PaymentRegion { get; set; }
    
    [XmlElement("PaymentTaxRegion")]
    public string PaymentTaxRegion { get; set; }
    
    [XmlElement("BatchProcessing")]
    public bool BatchProcessing { get; set; }
    
    [XmlElement("PaymentMethod")]
    public string PaymentMethod { get; set; }
    
    [XmlElement("PaymentFrequency")]
    public string PaymentFrequency { get; set; }
    
    [XmlElement("PaymentBasis")]
    public PaymentBasis PaymentBasis { get; set; }
    
    [XmlElement("PaymentClosureBasis")]
    public PaymentClosureBasis PaymentClosureBasis { get; set; }
    
    [XmlElement("IsPartialPayment")]
    public bool IsPartialPayment { get; set; }
    
    [XmlElement("IsEqualPayment")]
    public bool IsEqualPayment { get; set; }
    
    [XmlElement("AmountForPeriod")]
    public decimal AmountForPeriod { get; set; }
    
    [XmlElement("Note")]
    public string Note { get; set; }
    
    [XmlElement("RegistrationNumber")]
    public string RegistrationNumber { get; set; }
    
    [XmlElement("RegistrationDate")]
    public string RegistrationDate { get; set; }
}

public class PaymentBasis
{
    [XmlElement("IsPaymentContract")]
    public bool IsPaymentContract { get; set; }
    
    [XmlElement("IsPaymentInvoice")]
    public bool IsPaymentInvoice { get; set; }
    
    [XmlElement("IsPaymentTaxInvoice")]
    public bool IsPaymentTaxInvoice { get; set; }
    
    [XmlElement("IsPaymentAct")]
    public bool IsPaymentAct { get; set; }
    
    [XmlElement("IsPaymentOrder")]
    public bool IsPaymentOrder { get; set; }
}

public class PaymentClosureBasis
{
    [XmlElement("IsPaymentContract")]
    public bool IsPaymentContract { get; set; }
    
    [XmlElement("IsPaymentInvoice")]
    public bool IsPaymentInvoice { get; set; }
    
    [XmlElement("IsPaymentTaxInvoice")]
    public bool IsPaymentTaxInvoice { get; set; }
    
    [XmlElement("IsPaymentAct")]
    public bool IsPaymentAct { get; set; }
    
    [XmlElement("IsPaymentOrder")]
    public bool IsPaymentOrder { get; set; }
    
    [XmlElement("IsPaymentWaybill")]
    public bool IsPaymentWaybill { get; set; }
}

public class Counterparty
{
    [XmlElement("Person")]
    public Person Person { get; set; }
}

public class Person
{
    [XmlElement("ExternalID")]
    public string ExternalId { get; set; }
    
    [XmlElement("ID")]
    public string Id { get; set; }
    
    [XmlElement("LastName")]
    public string LastName { get; set; }
    
    [XmlElement("FirstName")]
    public string FirstName { get; set; }
    
    [XmlElement("MiddleName")]
    public string MiddleName { get; set; }
    
    [XmlElement("REZIDENT")]
    public bool Resident { get; set; }
    
    [XmlElement("NU_REZIDENT")]
    public bool NuResident { get; set; }
    
    [XmlElement("DATE_PERS")]
    public string BirthDate { get; set; }
    
    [XmlElement("SEX")]
    public string Sex { get; set; }
    
    [XmlElement("MARIGE_ST")]
    public string MaritalStatus { get; set; }
    
    [XmlElement("INN")]
    public string INN { get; set; }
    
    [XmlElement("CODE_OKONH")]
    public CodeList CodeOkonh { get; set; }
    
    [XmlElement("CODE_OKVED")]
    public CodeList CodeOkved { get; set; }
    
    [XmlElement("IIN")]
    public string IIN { get; set; }
    
    [XmlElement("COUNTRY")]
    public string Country { get; set; }
    
    [XmlElement("City")]
    public string City { get; set; }
    
    [XmlElement("IdentityDocument")]
    public IdentityDocument IdentityDocument { get; set; }
}

public class CodeList
{
    [XmlElement("element")]
    public string[] Elements { get; set; }
}

public class IdentityDocument
{
    [XmlElement("TYPE")]
    public string Type { get; set; }
    
    [XmlElement("DATE_BEGIN")]
    public string DateBegin { get; set; }
    
    [XmlElement("DATE_END")]
    public string DateEnd { get; set; }
    
    [XmlElement("NUM")]
    public string Number { get; set; }
    
    [XmlElement("SER")]
    public string Series { get; set; }
    
    [XmlElement("WHO")]
    public string IssuedBy { get; set; }
}*/

}