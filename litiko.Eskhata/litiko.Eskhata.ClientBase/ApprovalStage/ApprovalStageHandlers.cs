using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ApprovalStage;

namespace litiko.Eskhata
{
  partial class ApprovalStageClientHandlers
  {

    public virtual IEnumerable<Enumeration> CustomStageTypelitikoFiltering(IEnumerable<Enumeration> query)
    {
      if (_obj.StageType != StageType.SimpleAgr || _obj.AllowSendToRework.GetValueOrDefault())
        query = query.Where(q => !Equals(q, CustomStageTypelitiko.Voting));
      
      if (_obj.StageType != StageType.SimpleAgr)
        query = query.Where(q => !Equals(q, CustomStageTypelitiko.IncludeInMeet));
      
      return query;
    }
  }

}