using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.RecordManagementEskhata.Structures.ConvertOrdToPdf
{
  [Public]
  partial class ApprovalRow
  {
    public int RowIndex {get;set;}
    public string JobTitle {get;set;}
    public string SignerName {get;set;}
    public string SignResult {get;set;}
    public string Comment {get;set;}
    public string Signature {get;set;}
    public string Date {get;set;}
    public string ReportSessionId {get;set;}
  }
}