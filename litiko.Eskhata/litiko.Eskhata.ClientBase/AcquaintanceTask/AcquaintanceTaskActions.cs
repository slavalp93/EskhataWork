using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.AcquaintanceTask;

namespace litiko.Eskhata.Client
{
  partial class AcquaintanceTaskActions
  {
    public override void ShowAcquaintanceReport(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      //base.ShowAcquaintanceReport(e);
      
      if (!_obj.DocumentGroup.OfficialDocuments.Any())
      {
        e.AddError(AcquaintanceTasks.Resources.DocumentCantBeEmpty);
        return;
      }           
      
      var document = _obj.DocumentGroup.OfficialDocuments.FirstOrDefault();
      if (document != null)
      {
        var report = litiko.RecordManagementEskhata.Reports.GetAcquaintanceApprovalSheet();
        report.Document = document;
        report.Task = _obj;
        report.Open();
      }      
    }

    public override bool CanShowAcquaintanceReport(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanShowAcquaintanceReport(e);
    }

  }

}