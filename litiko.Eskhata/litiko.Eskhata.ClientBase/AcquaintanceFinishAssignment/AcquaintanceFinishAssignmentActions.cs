using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.AcquaintanceFinishAssignment;

namespace litiko.Eskhata.Client
{
  partial class AcquaintanceFinishAssignmentActions
  {
    public virtual void ConvertToPDFWithAcqList(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var report = RecordManagementEskhata.Reports.GetAcquaintanceApprovalSheet();
      report.Document = _obj.DocumentGroup.OfficialDocuments.FirstOrDefault();
      report.Task = AcquaintanceTasks.As(_obj.Task);
      report.Open();
    }

    public virtual bool CanConvertToPDFWithAcqList(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

}