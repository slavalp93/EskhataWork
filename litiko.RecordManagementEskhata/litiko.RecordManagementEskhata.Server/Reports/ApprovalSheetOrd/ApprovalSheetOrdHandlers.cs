using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.RecordManagementEskhata
{
  partial class ApprovalSheetOrdServerHandlers
  {

    public override void AfterExecute(Sungero.Reporting.Server.AfterExecuteEventArgs e)
    {
      Sungero.Docflow.PublicFunctions.Module.DeleteReportData(Constants.ApprovalSheetOrd.SourceTableName, ApprovalSheetOrd.ReportSessionId);
    }

    public override void BeforeExecute(Sungero.Reporting.Server.BeforeExecuteEventArgs e)
    {
      if (ApprovalSheetOrd.Document == null)
        return;
      var rep = ApprovalSheetOrd;
      rep.ReportSessionId = Guid.NewGuid().ToString();
      var rows = RecordManagementEskhata.PublicFunctions.Module.GetApprovalSheetOrdReportTable(rep.Document, rep.ReportSessionId);
      Sungero.Docflow.PublicFunctions.Module.WriteStructuresToTable(Constants.ApprovalSheetOrd.SourceTableName, rows);
    }

  }
}