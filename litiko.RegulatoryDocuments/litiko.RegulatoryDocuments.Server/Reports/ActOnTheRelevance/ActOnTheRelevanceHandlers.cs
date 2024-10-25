using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.RegulatoryDocuments
{
  partial class ActOnTheRelevanceServerHandlers
  {

    public override void AfterExecute(Sungero.Reporting.Server.AfterExecuteEventArgs e)
    {
      Sungero.Docflow.PublicFunctions.Module.DeleteReportData(Constants.Module.SourceTableName, ActOnTheRelevance.ReportSessionId);
    }

    public override void BeforeExecute(Sungero.Reporting.Server.BeforeExecuteEventArgs e)
    {
      ActOnTheRelevance.ReportSessionId = Guid.NewGuid().ToString();
      
      var documentIds = new List<long> { ActOnTheRelevance.Entity.Id };
      var dataList = PublicFunctions.Module.GetApprovalSheetIRDData(documentIds, ActOnTheRelevance.ReportSessionId);
      Sungero.Docflow.PublicFunctions.Module.WriteStructuresToTable(Constants.Module.SourceTableName, dataList);      
    }

    public virtual IQueryable<litiko.RegulatoryDocuments.IRegulatoryDocument> GetDoc()
    {
      return litiko.RegulatoryDocuments.RegulatoryDocuments.GetAll(x => x.Id == ActOnTheRelevance.Entity.Id);
    }

  }
}