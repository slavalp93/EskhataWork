using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Minutes;

namespace litiko.Eskhata
{
  partial class MinutesSharedHandlers
  {

    public override void MeetingChanged(Sungero.Meetings.Shared.MinutesMeetingChangedEventArgs e)
    {
      base.MeetingChanged(e);
      
      var meeting = e.NewValue;
      if (meeting != null)
      {                
        _obj.OurSignatory = litiko.Eskhata.Meetings.As(meeting)?.President;
      }      
    }

  }
}