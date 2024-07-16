using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.DocflowEskhata
{
  partial class EnvelopeB4ReportServerHandlers
  {

    public override void AfterExecute(Sungero.Reporting.Server.AfterExecuteEventArgs e)
    {
      // Удалить временные таблицы.
      Sungero.Docflow.PublicFunctions.Module.DeleteReportData(Constants.EnvelopeB4Report.EnvelopesTableName, EnvelopeB4Report.ReportSessionId);
    }

    public override void BeforeExecute(Sungero.Reporting.Server.BeforeExecuteEventArgs e)
    {
      EnvelopeB4Report.ReportSessionId = Guid.NewGuid().ToString();
      Sungero.Docflow.PublicFunctions.Module.DeleteReportData(Constants.EnvelopeB4Report.EnvelopesTableName, EnvelopeB4Report.ReportSessionId);
      Functions.Module.FillEnvelopeTable(EnvelopeB4Report.ReportSessionId,
                                         EnvelopeB4Report.OutgoingDocuments.ToList(),
                                         EnvelopeB4Report.ContractualDocuments.ToList(),
                                         EnvelopeB4Report.AccountingDocuments.ToList());
    }

  }
}