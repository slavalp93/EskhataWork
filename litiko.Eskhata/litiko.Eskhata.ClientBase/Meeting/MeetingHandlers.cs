using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Meeting;

namespace litiko.Eskhata
{
  partial class MeetingClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);
      
      _obj.State.Properties.MeetingCategorylitiko.IsRequired = true;
      _obj.State.Properties.Votinglitiko.IsRequired = true;
      _obj.State.Properties.Members.IsRequired = true;
      
      _obj.State.Properties.Presentlitiko.IsEnabled = _obj.MeetingCategorylitiko != null ? true : false;
    }

  }
}