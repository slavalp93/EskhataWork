using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Meeting;

namespace litiko.Eskhata
{
  partial class MeetingPresentlitikoEmployeePropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> PresentlitikoEmployeeFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {      

      /*
      var meeting = _obj.Meeting;
      if (meeting != null && meeting.MeetingCategorylitiko != null)
      {
        var aviabledMemberIDs = meeting.MeetingCategorylitiko.Members.Where(x => x.Member != null).Select(m => m.Member.Id).ToList();
        query = query.Where(x => aviabledMemberIDs.Contains(x.Id));
      }        
      */
      return query;
    }
  }

}