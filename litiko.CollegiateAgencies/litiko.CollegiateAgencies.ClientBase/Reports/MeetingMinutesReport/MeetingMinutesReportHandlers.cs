using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.CollegiateAgencies
{
  partial class MeetingMinutesReportClientHandlers
  {

    public override void BeforeExecute(Sungero.Reporting.Client.BeforeExecuteEventArgs e)
    {
      var dialog = Dialogs.CreateInputDialog(litiko.CollegiateAgencies.Reports.Resources.MeetingMinutesReport.MeetingMinutesReportTitle);
      
      var currentDate = Sungero.Core.Calendar.Now;
      var beginningofMonth = Sungero.Core.Calendar.BeginningOfMonth(currentDate);
      
      var meetingMinutesDateFrom = dialog.AddDate("С", true, beginningofMonth);
      var meetingMinutesDateTo = dialog.AddDate("По", true, currentDate);
      var category = dialog.AddSelect("Категория заседания", true, CollegiateAgencies.MeetingCategories.Null);
      
      if (dialog.Show() == DialogButtons.Ok)
      {
        MeetingMinutesReport.MeetingMinutesDateFrom = meetingMinutesDateFrom.Value;
        MeetingMinutesReport.MeetingMinutesDateTo = meetingMinutesDateTo.Value;
        MeetingMinutesReport.MeetingCategoryId = category.Value.Id;
      }
      else
      {
        e.Cancel = true;
      }
    }

  }
}