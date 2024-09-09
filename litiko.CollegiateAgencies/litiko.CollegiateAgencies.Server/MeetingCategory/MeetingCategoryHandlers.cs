using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.CollegiateAgencies.MeetingCategory;

namespace litiko.CollegiateAgencies
{
  partial class MeetingCategoryServerHandlers
  {

    public override void Saving(Sungero.Domain.SavingEventArgs e)
    {
      Functions.MeetingCategory.SynchronizeSecretaryInRole(_obj);
      Functions.MeetingCategory.SynchronizePresidentInRole(_obj);
    }
  }

}