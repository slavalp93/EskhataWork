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

    public virtual IEnumerable<Enumeration> ProjectSolutionslitikoVotingTypeFiltering(IEnumerable<Enumeration> query)
    {      
      if (_obj.Meeting.Votinglitiko == litiko.Eskhata.Meeting.Votinglitiko.Intramural)
        query = query.Where(q => !Equals(q, litiko.Eskhata.MeetingProjectSolutionslitiko.VotingType.Extramural));
      if (_obj.Meeting.Votinglitiko == litiko.Eskhata.Meeting.Votinglitiko.extramural)
        query = query.Where(q => !Equals(q, litiko.Eskhata.MeetingProjectSolutionslitiko.VotingType.Intramural));
      if (_obj.Meeting.Votinglitiko == litiko.Eskhata.Meeting.Votinglitiko.NoVoting)
        query = query.Where(q => Equals(q, litiko.Eskhata.MeetingProjectSolutionslitiko.VotingType.NoVoting));
      
      return query;
    }

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
      if (e.OldValue != null && !Equals(e.NewValue, e.OldValue))
      {
        e.AddError(litiko.Eskhata.Meetings.Resources.YouMustDeleteEntireLine);
        return;        
      }
      
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

    public virtual void VotinglitikoValueInput(Sungero.Presentation.EnumerationValueInputEventArgs e)
    {
      var selectedType = e.NewValue;
      Nullable<Enumeration> typeForTable = null;
      if (selectedType != null)
      {
        if (selectedType == litiko.Eskhata.Meeting.Votinglitiko.extramural)
          typeForTable = litiko.Eskhata.MeetingProjectSolutionslitiko.VotingType.Extramural;
        if (selectedType == litiko.Eskhata.Meeting.Votinglitiko.Intramural)
          typeForTable = litiko.Eskhata.MeetingProjectSolutionslitiko.VotingType.Intramural;
        if (selectedType == litiko.Eskhata.Meeting.Votinglitiko.NoVoting)
          typeForTable = litiko.Eskhata.MeetingProjectSolutionslitiko.VotingType.NoVoting;
        if (selectedType == litiko.Eskhata.Meeting.Votinglitiko.IntExt)
          typeForTable = litiko.Eskhata.MeetingProjectSolutionslitiko.VotingType.Intramural;
      }
      
      foreach (var element in _obj.ProjectSolutionslitiko)
      {
        if (element.VotingType != typeForTable)
          element.VotingType = typeForTable;
      }

    }

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);
      
      _obj.State.Properties.MeetingCategorylitiko.IsRequired = true;
      _obj.State.Properties.MeetingCategorylitiko.IsEnabled = _obj.State.IsInserted || !_obj.ProjectSolutionslitiko.Any();
      _obj.State.Properties.Votinglitiko.IsRequired = true;
      _obj.State.Properties.Members.IsRequired = true;
      
      _obj.State.Properties.Secretary.IsEnabled = _obj.MeetingCategorylitiko != null ? true : false;
      _obj.State.Properties.President.IsEnabled = _obj.MeetingCategorylitiko != null ? true : false;
      _obj.State.Properties.Members.IsEnabled = _obj.MeetingCategorylitiko != null ? true : false;
      _obj.State.Properties.Presentlitiko.IsEnabled = _obj.MeetingCategorylitiko != null ? true : false;
    }

  }
}