using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.RegulatoryDocuments
{
  partial class ControlApprovingIRDClientHandlers
  {

    public override void BeforeExecute(Sungero.Reporting.Client.BeforeExecuteEventArgs e)
    {
      if (!ControlApprovingIRD.DocumentId.HasValue)
      {
        var dialog = Dialogs.CreateInputDialog(litiko.RegulatoryDocuments.Reports.Resources.ControlApprovingIRD.DialogTittle);
        var btnOk = dialog.Buttons.AddOk();
        var btnCancel = dialog.Buttons.AddCancel();
        var dateBegin = dialog.AddDate(litiko.RegulatoryDocuments.Reports.Resources.ControlApprovingIRD.DateBegin, true, Calendar.BeginningOfMonth(Calendar.Today));
        var dateEnd = dialog.AddDate(litiko.RegulatoryDocuments.Reports.Resources.ControlApprovingIRD.DateEnd, true, Calendar.Today);      
        
        var result = dialog.Show();
        if (result == btnOk)
        {
          ControlApprovingIRD.DateBegin = dateBegin.Value;
          ControlApprovingIRD.DateEnd = dateEnd.Value;
          ControlApprovingIRD.DocumentId = 0;
        }
        else      
          e.Cancel = true;      
      }
      else
      {
        ControlApprovingIRD.DateBegin = Calendar.GetDate(1900, 12, 31);
        ControlApprovingIRD.DateEnd = Calendar.GetDate(2500, 12, 31);
      }
    }

  }
}