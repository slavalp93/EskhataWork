using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Meeting;

namespace litiko.Eskhata
{
  partial class MeetingProjectSolutionslitikoClientHandlers
  {

    public virtual void ProjectSolutionslitikoNoValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      _obj.Accepted = _obj.Yes > e.NewValue.GetValueOrDefault() ? true : false;
    }

    public virtual void ProjectSolutionslitikoYesValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      _obj.Accepted = e.NewValue.GetValueOrDefault() > _obj.No ? true : false;
    }

    public virtual void ProjectSolutionslitikoProjectSolutionValueInput(litiko.Eskhata.Client.MeetingProjectSolutionslitikoProjectSolutionValueInputEventArgs e)
    {
      var projectSolution = e.NewValue;
      if (projectSolution != null)
      {
        if (!projectSolution.AccessRights.CanUpdate())
        {          
          e.AddError(litiko.Eskhata.Meetings.Resources.NoRightsForUpdateDocumentFormat(projectSolution.Name));
          return;
        }
        
        _obj.Yes = projectSolution.Voting.Count(x => x.Yes.Value);
        _obj.No = projectSolution.Voting.Count(x => x.No.Value);
        _obj.Abstained = projectSolution.Voting.Count(x => x.Abstained.Value);
        
        _obj.Accepted = _obj.Yes > _obj.No ? true : false;
        
        litiko.CollegiateAgencies.PublicFunctions.Projectsolution.ProcessIncludingInMeeting(projectSolution, _obj.Meeting, true);
      }
      else
      {
        _obj.Yes = null;
        _obj.No = null;
        _obj.Abstained = null;
        _obj.Accepted = null;
      }      
    }
  }

  partial class MeetingClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);
      
      _obj.State.Properties.MeetingCategorylitiko.IsRequired = true;
      _obj.State.Properties.Votinglitiko.IsRequired = true;
      _obj.State.Properties.Members.IsRequired = true;
      
      _obj.State.Properties.Secretary.IsEnabled = _obj.MeetingCategorylitiko != null ? true : false;
      _obj.State.Properties.President.IsEnabled = _obj.MeetingCategorylitiko != null ? true : false;
      _obj.State.Properties.Members.IsEnabled = _obj.MeetingCategorylitiko != null ? true : false;
      _obj.State.Properties.Presentlitiko.IsEnabled = _obj.MeetingCategorylitiko != null ? true : false;
    }

  }
}