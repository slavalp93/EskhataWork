using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.RecordManagementEskhata.Structures.DocflowReport
{
  partial class DocflowReportLine
  {
    public string Responsible { get; set; }
    
    public string Department { get; set; }
    
    public int Total { get; set; }
    public int Nbt { get; set; }
    public int Filial { get; set; }
    public int Letters { get; set; }
    public int Others { get; set; }
  }
}