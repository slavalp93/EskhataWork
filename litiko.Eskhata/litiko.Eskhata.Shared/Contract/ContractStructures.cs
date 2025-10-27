using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.Xml.Serialization;

namespace litiko.Eskhata.Structures.Contracts.Contract
{

  /// <summary>
  /// 
  /// </summary>
  partial class ImportResult
  {
    public int TotalProcessed {get;set;}
    public int Successful {get;set;}
    public int Failed {get;set;}
    public List<string> Errors {get;set;}
  }
}