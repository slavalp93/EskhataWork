using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.RegulatoryDocuments.Structures.Module
{
  [Public]
  partial class ApprovalSheetLine
  {    
    public int Number { get; set; }
    public long DocId { get; set; }
    public long TaskId { get; set; }
    public long AssignmentId { get; set; }
    public string TaskAuthor { get; set; }
    public string Performer { get; set; }    
    public string Department { get; set; }
    public DateTime? Created { get; set; }
    public DateTime? Complated { get; set; }
    public string Result { get; set; }
    public string Comment { get; set; }
    public string ReportSessionId { get; set; }
  }
}