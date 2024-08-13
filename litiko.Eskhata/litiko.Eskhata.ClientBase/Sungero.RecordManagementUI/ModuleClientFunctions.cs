using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.Eskhata.Module.RecordManagementUI.Client
{
  partial class ModuleFunctions
  {
    public override void ShowAllReports()
    {
      var reports = Sungero.RecordManagement.Reports.GetAll()
        .Where(r => !(r is Sungero.RecordManagement.IAcquaintanceReport))
        .ToList();
      reports.Add(Sungero.Docflow.Reports.GetSkippedNumbersReport());
      reports.Add(RecordManagementEskhata.Reports.GetDocflowReport());
      reports.AsEnumerable().Show(Sungero.RecordManagement.Resources.AllReportsTitle);
    }
  }
}