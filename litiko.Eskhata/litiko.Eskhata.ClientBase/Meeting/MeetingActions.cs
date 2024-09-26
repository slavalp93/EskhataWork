using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Meeting;

namespace litiko.Eskhata.Client
{
  partial class MeetingAnyChildEntityActions
  {
    public override void CopyChildEntity(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      base.CopyChildEntity(e);
    }

    public override bool CanCopyChildEntity(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      return false;
    }

  }

  partial class MeetingActions
  {
    public virtual void CreateActionItemslitiko(Sungero.Domain.Client.ExecuteActionArgs e)
    {            
      litiko.Eskhata.PublicFunctions.Meeting.CreateActionItemsFromMeetingDialog(_obj, e);
    }

    public virtual bool CanCreateActionItemslitiko(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !_obj.State.IsInserted && _obj.ProjectSolutionslitiko.Any(x => x.ProjectSolution != null);
    }

  }

}