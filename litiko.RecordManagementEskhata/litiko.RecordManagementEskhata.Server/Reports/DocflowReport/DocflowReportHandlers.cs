using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.RecordManagementEskhata
{
  partial class DocflowReportServerHandlers
  {

    public override void BeforeExecute(Sungero.Reporting.Server.BeforeExecuteEventArgs e)
    {
      DocflowReport.ReportSessionId = Guid.NewGuid().ToString();
      var rows = RecordManagementEskhata.PublicFunctions.Module.GetActionItemCompletionData(DocflowReport.BeginDate, DocflowReport.EndDate, DocflowReport.State, DocflowReport.ReportSessionId);
      Sungero.Docflow.PublicFunctions.Module.WriteStructuresToTable(Constants.DocflowReport.SourceTableName, rows);
    }

  }
}