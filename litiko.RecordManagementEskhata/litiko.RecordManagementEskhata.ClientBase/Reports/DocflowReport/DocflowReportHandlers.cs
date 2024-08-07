using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.RecordManagementEskhata
{
  partial class DocflowReportClientHandlers
  {

    public override void BeforeExecute(Sungero.Reporting.Client.BeforeExecuteEventArgs e)
    {
      var report = DocflowReport;
      
      var personalSettings = Sungero.Docflow.PublicFunctions.PersonalSetting.GetPersonalSettings(null);
      var dialog = Dialogs.CreateInputDialog("dialog title");

      var settingsStartDate = Sungero.Docflow.PublicFunctions.PersonalSetting.GetStartDate(personalSettings);
      var beginDate = dialog.AddDate("PeriodFrom", true, settingsStartDate ?? Calendar.UserToday);
      var settingsEndDate = Sungero.Docflow.PublicFunctions.PersonalSetting.GetEndDate(personalSettings);
      var endDate = dialog.AddDate("PeriodTo", true, settingsEndDate ?? Calendar.UserToday);
      var state = dialog.AddSelect("State", true, litiko.RecordManagementEskhata.Resources.All)
        .From(new string[]
              {
                litiko.RecordManagementEskhata.Resources.All,
                litiko.RecordManagementEskhata.Resources.Completed,
                litiko.RecordManagementEskhata.Resources.InWork,
                litiko.RecordManagementEskhata.Resources.Expired
              });
      
      dialog.SetOnButtonClick((args) =>
                              {
                                Sungero.Docflow.PublicFunctions.Module.CheckReportDialogPeriod(args, beginDate, endDate);
                              });
      
      dialog.Buttons.AddOkCancel();
      
      if (dialog.Show() == DialogButtons.Ok)
      {
        report.BeginDate = beginDate.Value.Value;
        report.ClientEndDate = endDate.Value.Value;
        report.EndDate = endDate.Value.Value.EndOfDay();
        report.State = state.Value;
      }
      else
      {
        e.Cancel = true;
      }
    }

  }
}