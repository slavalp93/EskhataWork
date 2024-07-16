using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.DocflowEskhata.Structures.Module
{
  /// <summary>
  /// Строчка отчета.
  /// </summary>
  partial class EnvelopeReportTableLine
  {
    public string ReportSessionId { get; set; }
    
    public int Id { get; set; }
    
    public string ToName { get; set; }
    
    public string FromName { get; set; }
    
    public string ToZipCode { get; set; }

    public string FromZipCode { get; set; }
    
    public string ToPlace { get; set; }
    
    public string FromPlace { get; set; }
  }
  
  /// <summary>
  /// Получатель и отправитель для конвертов.
  /// </summary>
  partial class AddresseeAndSender
  {
    public Sungero.Parties.ICounterparty Addresse { get; set; }
    
    public Sungero.Company.IBusinessUnit Sender { get; set; }
  }
  
  /// <summary>
  /// Индекс и адрес без индекса.
  /// </summary>
  partial class ZipCodeAndAddress
  {
    public string ZipCode { get; set; }
    
    public string Address { get; set; }
  }
}