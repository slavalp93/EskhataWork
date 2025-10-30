using System.Collections.Generic;
using Sungero.Core;

namespace litiko.Eskhata.Structures.Contracts.Contract
{
  [Public]
  partial class ResultImportXml
  {
    public List<string> Errors { get; set; }
    public int ImportedCount { get; set; }
  }
  
}