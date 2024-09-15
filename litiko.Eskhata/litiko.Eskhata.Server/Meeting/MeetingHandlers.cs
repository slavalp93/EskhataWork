using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Meeting;

namespace litiko.Eskhata
{
  partial class MeetingProjectSolutionslitikoProjectSolutionPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> ProjectSolutionslitikoProjectSolutionFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      var meetingCategory = _root.MeetingCategorylitiko;
      if (meetingCategory != null)
        query = query.Where(x => Equals(x.MeetingCategory, meetingCategory) && !x.IncludedInAgenda.Value && x.InternalApprovalState == Sungero.Docflow.OfficialDocument.InternalApprovalState.Signed);
      
      return query;
    }
  }

  partial class MeetingServerHandlers
  {

    public override void Saving(Sungero.Domain.SavingEventArgs e)
    {
      base.Saving(e);
      
      if (_obj.State.Properties.ProjectSolutionslitiko.IsChanged && !e.Params.Contains(litiko.CollegiateAgencies.PublicConstants.Module.ParamNames.DontUpdateProjectSolution))
      {
        foreach (var element in _obj.ProjectSolutionslitiko.Where(x => x.ProjectSolution != null))
        {
          var doc = element.ProjectSolution;
          if (!Equals(doc.Meeting, _obj))
            doc.Meeting = _obj;
        }
      }
    }
  }

  partial class MeetingAbsentlitikoEmployeePropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> AbsentlitikoEmployeeFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      var aviabledMemberIDs = _root.Members.Where(x => x.Member != null).Select(m => m.Member.Id).ToList();
      if (_root.Secretary != null && !aviabledMemberIDs.Contains(_root.Secretary.Id))
        aviabledMemberIDs.Add(_root.Secretary.Id);
      if (_root.President != null && !aviabledMemberIDs.Contains(_root.President.Id))
        aviabledMemberIDs.Add(_root.President.Id);

      //aviabledMemberIDs = aviabledMemberIDs.Union(_root.InvitedEmployeeslitiko.Where(x => x.Employee != null).Select(m => m.Employee.Id)).ToList();
      foreach (var element in _root.InvitedEmployeeslitiko.Where(x => x.Employee != null))
      {
        if (!aviabledMemberIDs.Contains(element.Employee.Id))
          aviabledMemberIDs.Add(element.Employee.Id);
      }
        
      query = query.Where(x => aviabledMemberIDs.Contains(x.Id));      
      return query;
    }
  }

  partial class MeetingMembersMemberPropertyFilteringServerHandler<T>
  {

    public override IQueryable<T> MembersMemberFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      query = base.MembersMemberFiltering(query, e);
      query = query.Where(x => Sungero.Company.Employees.Is(x));
      return query;
    }
  }

  partial class MeetingPresentlitikoEmployeePropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> PresentlitikoEmployeeFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {      
      var aviabledMemberIDs = _root.Members.Where(x => x.Member != null).Select(m => m.Member.Id).ToList();
      if (_root.Secretary != null && !aviabledMemberIDs.Contains(_root.Secretary.Id))
        aviabledMemberIDs.Add(_root.Secretary.Id);
      if (_root.President != null && !aviabledMemberIDs.Contains(_root.President.Id))
        aviabledMemberIDs.Add(_root.President.Id);

      //aviabledMemberIDs = aviabledMemberIDs.Union(_root.InvitedEmployeeslitiko.Where(x => x.Employee != null).Select(m => m.Employee.Id)).ToList();
      foreach (var element in _root.InvitedEmployeeslitiko.Where(x => x.Employee != null))
      {
        if (!aviabledMemberIDs.Contains(element.Employee.Id))
          aviabledMemberIDs.Add(element.Employee.Id);
      }
        
      query = query.Where(x => aviabledMemberIDs.Contains(x.Id));      
      return query;
    }
  }

}