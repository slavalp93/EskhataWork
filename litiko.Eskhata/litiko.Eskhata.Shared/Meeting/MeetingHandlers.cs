using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Meeting;

namespace litiko.Eskhata
{
  partial class MeetingSharedHandlers
  {

    public virtual void MeetingCategorylitikoChanged(litiko.Eskhata.Shared.MeetingMeetingCategorylitikoChangedEventArgs e)
    {
var selectedCategory = e.NewValue;
      if (selectedCategory != null && !Equals(selectedCategory, e.OldValue))
      {
        if (selectedCategory.Members.Any())
        {
          foreach (var element in selectedCategory.Members.Where(x => x.Member != null).Select(x => x.Member))
          {
            var member = Recipients.As(element);
            if (member != null && !_obj.Members.Any(x => Equals(x.Member, member)))
            {
              var newMember = _obj.Members.AddNew();
              newMember.Member = member;
            }
            
            if (!_obj.Presentlitiko.Any(x => Equals(x.Employee, element)))
            {
              var newMember = _obj.Presentlitiko.AddNew();
              newMember.Employee = element;
            }            
          }        
        }
        
        if (selectedCategory.President != null && !Equals(selectedCategory.President, _obj.President))
          _obj.President = selectedCategory.President;
        
        if (selectedCategory.Secretary != null && !Equals(selectedCategory.Secretary, _obj.Secretary))
          _obj.Secretary = selectedCategory.Secretary;

        Functions.Meeting.SetQuorum(_obj, selectedCategory);
      }      
    }
  }

  partial class MeetingProjectSolutionslitikoSharedHandlers
  {

    public virtual void ProjectSolutionslitikoProjectSolutionChanged(litiko.Eskhata.Shared.MeetingProjectSolutionslitikoProjectSolutionChangedEventArgs e)
    {
      var projectSolution = e.NewValue;
      if (projectSolution != null)
      {
        _obj.Yes = projectSolution.Voting.Count(x => x.Yes.Value);
        _obj.No = projectSolution.Voting.Count(x => x.No.Value);
        _obj.Abstained = projectSolution.Voting.Count(x => x.Abstained.Value);
        
        _obj.Accepted = _obj.Yes > _obj.No ? true : false;
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

  partial class MeetingProjectSolutionslitikoSharedCollectionHandlers
  {

    public virtual void ProjectSolutionslitikoDeleted(Sungero.Domain.Shared.CollectionPropertyDeletedEventArgs e)
    {
      var doc = _deleted.ProjectSolution;
      if (doc != null && doc.AccessRights.CanUpdate())
      {
        doc.Meeting = null;
        doc.IncludedInAgenda = false;
        doc.Save();
      }
    }

    public virtual void ProjectSolutionslitikoAdded(Sungero.Domain.Shared.CollectionPropertyAddedEventArgs e)
    {
      _added.Number = _obj.ProjectSolutionslitiko.Count;
    }
  }

  partial class MeetingPresentlitikoSharedCollectionHandlers
  {

    public virtual void PresentlitikoDeleted(Sungero.Domain.Shared.CollectionPropertyDeletedEventArgs e)
    {
      Functions.Meeting.SetQuorum(_obj, _obj.MeetingCategorylitiko);
    }

    public virtual void PresentlitikoAdded(Sungero.Domain.Shared.CollectionPropertyAddedEventArgs e)
    {
      Functions.Meeting.SetQuorum(_obj, _obj.MeetingCategorylitiko);
    }
  }


  partial class MeetingPresentlitikoSharedHandlers
  {

    public virtual void PresentlitikoEmployeeChanged(litiko.Eskhata.Shared.MeetingPresentlitikoEmployeeChangedEventArgs e)
    {
      var selectedEmployee = e.NewValue;
      if (selectedEmployee != null && selectedEmployee != e.OldValue && _obj.Meeting.Absentlitiko.Any(x => Equals(selectedEmployee, x.Employee)))
      {
        foreach (var element in _obj.Meeting.Absentlitiko.Where(x => Equals(selectedEmployee, x.Employee)).ToList())
        {
          _obj.Meeting.Absentlitiko.Remove(element);
        }
      }      
    }
  }

  partial class MeetingAbsentlitikoSharedHandlers
  {

    public virtual void AbsentlitikoEmployeeChanged(litiko.Eskhata.Shared.MeetingAbsentlitikoEmployeeChangedEventArgs e)
    {
      var selectedEmployee = e.NewValue;
      if (selectedEmployee != null && selectedEmployee != e.OldValue && _obj.Meeting.Presentlitiko.Any(x => Equals(selectedEmployee, x.Employee)))
      {
        foreach (var element in _obj.Meeting.Presentlitiko.Where(x => Equals(selectedEmployee, x.Employee)).ToList())
        {
          _obj.Meeting.Presentlitiko.Remove(element);
        }
      }
    }
  }
}