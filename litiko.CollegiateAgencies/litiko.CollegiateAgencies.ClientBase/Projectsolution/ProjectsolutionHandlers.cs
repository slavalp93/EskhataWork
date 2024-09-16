using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.CollegiateAgencies.Projectsolution;

namespace litiko.CollegiateAgencies
{
  partial class ProjectsolutionClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);
      
      bool votingTableAviabled = true;
      if (_obj.Meeting != null && _obj.Meeting.Votinglitiko.GetValueOrDefault() == litiko.Eskhata.Meeting.Votinglitiko.extramural)
        votingTableAviabled = false;
      
      _obj.State.Properties.Voting.IsEnabled = votingTableAviabled;
    }
  }

  partial class ProjectsolutionVotingClientHandlers
  {

    public virtual void VotingYesValueInput(Sungero.Presentation.BooleanValueInputEventArgs e)
    {
      if (e.NewValue != null && e.NewValue.GetValueOrDefault())
        _obj.State.Properties.Comment.IsRequired = false;
    }

    public virtual void VotingAbstainedValueInput(Sungero.Presentation.BooleanValueInputEventArgs e)
    {
      if (e.NewValue != null && e.NewValue.GetValueOrDefault())
        _obj.State.Properties.Comment.IsRequired = true;      
    }

    public virtual void VotingNoValueInput(Sungero.Presentation.BooleanValueInputEventArgs e)
    {
      if (e.NewValue != null && e.NewValue.GetValueOrDefault())
        _obj.State.Properties.Comment.IsRequired = true;
    }
  }

}