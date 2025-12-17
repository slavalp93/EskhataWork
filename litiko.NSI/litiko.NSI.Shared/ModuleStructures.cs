using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.NSI.Structures.Module
{
  partial class MappingRecordInfo
  {    
    public string Type { get; set; }    
    public long Id { get; set; }
    public string Name { get; set; }    
    public string ExternalId { get; set; }
  }
  
  [Public]
  partial class ResultImportXml
  {    
    public int TotalCount { get; set; }
    public int ImportedCount { get; set; }
    public int ChangedCount {get; set;}
    public int SkippedCount {get; set;}
    public List<string> Errors { get; set; }
  }  
}