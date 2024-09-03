using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Meeting;

namespace litiko.Eskhata.Shared
{
  partial class MeetingFunctions
  {

    /// <summary>
    /// Установить кворум
    /// </summary>       
    public void SetQuorum(litiko.CollegiateAgencies.IMeetingCategory category)
    {
      if (category != null)
      {
        if (!category.Quorum.HasValue)
          _obj.Quorumlitiko = null;        
        else
        {
          int quorumLimit = category.Quorum.Value;
          int presentCount = _obj.Presentlitiko.Count;
          if (presentCount >= quorumLimit)
            _obj.Quorumlitiko = litiko.Eskhata.Meeting.Quorumlitiko.Present;
          else
            _obj.Quorumlitiko = litiko.Eskhata.Meeting.Quorumlitiko.NotPresent;
        }
      }      
    }

  }
}