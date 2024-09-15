using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Agenda;

namespace litiko.Eskhata
{
  partial class AgendaSharedHandlers
  {

    public override void MeetingChanged(Sungero.Meetings.Shared.AgendaMeetingChangedEventArgs e)
    {
      base.MeetingChanged(e);
      
      var meeting = e.NewValue;
      if (meeting != null)
      {
        if (string.IsNullOrEmpty(_obj.RegistrationNumber))
          _obj.RegistrationNumber = litiko.Eskhata.Meetings.As(meeting)?.Numberlitiko;
        if (!_obj.RegistrationDate.HasValue)
          _obj.RegistrationDate = Calendar.Now;
        
        _obj.OurSignatory = litiko.Eskhata.Meetings.As(meeting)?.Secretary;
        
      }
    }

  }
}