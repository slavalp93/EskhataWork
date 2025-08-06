using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.CollegiateAgencies.Projectsolution;

namespace litiko.CollegiateAgencies
{
  partial class ProjectsolutionVotingSharedHandlers
  {

    public virtual void VotingAbstainedChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue.Value)
      {
        _obj.Yes = false;
        _obj.No = false;
      }      
    }

    public virtual void VotingNoChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue.Value)
      {
        _obj.Yes = false;
        _obj.Abstained = false;
      }      
    }

    public virtual void VotingYesChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue.Value)
      {
        _obj.No = false;
        _obj.Abstained = false;
      }
    }
  }

  partial class ProjectsolutionDecidedMinutesSharedCollectionHandlers
  {

    public virtual void DecidedMinutesDeleted(Sungero.Domain.Shared.CollectionPropertyDeletedEventArgs e)
    {
      int minNumber = 1;
      foreach (var decision in _obj.DecidedMinutes)
        decision.Number = minNumber++;      
    }

    public virtual void DecidedMinutesAdded(Sungero.Domain.Shared.CollectionPropertyAddedEventArgs e)
    {
      _added.Number = (_obj.DecidedMinutes.Max(a => a.Number) ?? 0) + 1;
    }
  }

  partial class ProjectsolutionDecidedSharedCollectionHandlers
  {

    public virtual void DecidedDeleted(Sungero.Domain.Shared.CollectionPropertyDeletedEventArgs e)
    {
      int minNumber = 1;
      foreach (var decision in _obj.Decided)
        decision.Number = minNumber++;      
    }

    public virtual void DecidedAdded(Sungero.Domain.Shared.CollectionPropertyAddedEventArgs e)
    {
      _added.Number = (_obj.Decided.Max(a => a.Number) ?? 0) + 1;
    }
  }

  partial class ProjectsolutionSharedHandlers
  {

    public virtual void MeetingCategoryChanged(litiko.CollegiateAgencies.Shared.ProjectsolutionMeetingCategoryChangedEventArgs e)
    {
      Functions.Projectsolution.DefineOurSignatory(_obj);
    }

    public virtual void MeetingChanged(litiko.CollegiateAgencies.Shared.ProjectsolutionMeetingChangedEventArgs e)
    {
      if (e.NewValue != null && !Equals(e.NewValue, e.OldValue))
        _obj.IncludedInAgenda = true;
    }

    public override void PreparedByChanged(Sungero.Docflow.Shared.OfficialDocumentPreparedByChangedEventArgs e)
    {
      base.PreparedByChanged(e);
      
      Functions.Projectsolution.DefineOurSignatory(_obj);
    }

  }
}