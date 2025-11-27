using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.Eskhata.Module.Parties.Structures.Module
{
  [Public]
  partial class ResultImportCounterpartyXml
  {
    public List<string> Errors { get; set; }
    public int TotalCompanies { get ; set; }
    public int ImportedCompanies{ get ; set; }
    public int TotalPersons{ get ; set; }
    public int ImportedPersons{ get ; set; }
    public int ImportedCount{ get ; set; }
    public int TotalCount{ get ; set; }
    public int DuplicateCompanies {get; set;}
    public int DuplicatePersons {get; set;}
    
    public List<string> SkippedCompanies{ get ; set; }
    public List<string> SkippedPersons{ get ; set; }
    public List<string> SkippedEntities{ get ; set; }

  }
}