using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.RegulatoryDocuments
{
  partial class ControlApprovingIRDServerHandlers
  {

    public override void AfterExecute(Sungero.Reporting.Server.AfterExecuteEventArgs e)
    {
      Sungero.Docflow.PublicFunctions.Module.DeleteReportData(Constants.Module.SourceTableName, ControlApprovingIRD.ReportSessionId);
    }

    public override void BeforeExecute(Sungero.Reporting.Server.BeforeExecuteEventArgs e)
    {
      ControlApprovingIRD.ReportSessionId = Guid.NewGuid().ToString();      
      var documentIds = new List<long>();
      
      if (ControlApprovingIRD.DocumentId.Value == 0)
        documentIds.AddRange(litiko.RegulatoryDocuments.RegulatoryDocuments.GetAll()
          .Where(d => d.Created.HasValue && d.Created.Between(ControlApprovingIRD.DateBegin.Value, ControlApprovingIRD.DateEnd.Value))
          .Select(d => d.Id)
          .ToList());
      else
        documentIds.Add(ControlApprovingIRD.DocumentId.Value);
      
      var dataList = PublicFunctions.Module.GetApprovalSheetIRDData(documentIds, ControlApprovingIRD.ReportSessionId);
      Sungero.Docflow.PublicFunctions.Module.WriteStructuresToTable(Constants.Module.SourceTableName, dataList);
    }

  }
}