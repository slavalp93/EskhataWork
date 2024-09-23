using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.CollegiateAgencies.Projectsolution;

namespace litiko.CollegiateAgencies
{
  partial class ProjectsolutionFilteringServerHandler<T>
  {

    public override IQueryable<T> Filtering(IQueryable<T> query, Sungero.Domain.FilteringEventArgs e)
    {
      query = base.Filtering(query, e);
      
      if (_filter == null)
        return query;      
      
      if (_filter.Included)
        query = query.Where(d => d.IncludedInAgenda.GetValueOrDefault());
      
      if (_filter.NotIncluded)
        query = query.Where(d => !d.IncludedInAgenda.GetValueOrDefault());
      
      if (_filter.MeetingCategory != null)
        query = query.Where(d => Equals(d.MeetingCategory, _filter.MeetingCategory));
      
      if (_filter.Speaker != null)
        query = query.Where(d => Equals(d.Speaker, _filter.Speaker));
      
      if (_filter.OurSignatory != null)
        query = query.Where(d => Equals(d.OurSignatory, _filter.OurSignatory));       
      
      return query;
    }
  }

  partial class ProjectsolutionVotingMemberPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> VotingMemberFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      if (_root.Meeting != null)
      {
        var members = _root.Meeting.Members.Where(x => x.Member != null).Select(x => x.Member).ToList();
        if (_root.Meeting.President != null && !members.Contains(_root.Meeting.President))
          members.Add(_root.Meeting.President);
        
        query = query.Where(x => members.Contains(x));
      }        
      
      if (_root.Voting.Any())
      {
        var alreadySelected = _root.Voting.Where(x => x.Member != null).Select(x => x.Member).ToList();
        query = query.Where(x => !alreadySelected.Contains(x));
      }        
      
      return query;
    }
  }

  partial class ProjectsolutionLeadingDocumentPropertyFilteringServerHandler<T>
  {

    public override IQueryable<T> LeadingDocumentFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      query = base.LeadingDocumentFiltering(query, e);
      
      var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(litiko.CollegiateAgencies.PublicConstants.Module.DocumentKindGuids.ExplanatoryNote);
      if (docKind != null)
        query = query.Where(d => Equals(Sungero.Docflow.OfficialDocuments.As(d).DocumentKind, docKind));
      
      return query;
    }
  }

  partial class ProjectsolutionServerHandlers
  {

    public override void BeforeSaveHistory(Sungero.Content.DocumentHistoryEventArgs e)
    {
      base.BeforeSaveHistory(e);
            
      var isUpdateAction = e.Action == Sungero.CoreEntities.History.Action.Update;
      if (!isUpdateAction)
    	  return;
      
      var operation = new Enumeration("SDChange");
      var changeList = litiko.CollegiateAgencies.PublicFunctions.Module.ChangeRequisites(_obj);
      foreach (var comment in changeList)
      {
        e.Write(operation, null, comment);
      }
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      _obj.IncludedInAgenda = false;
    }
  }

  partial class ProjectsolutionSpeakerPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> SpeakerFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      var roleDepartmentManagers = Roles.GetAll().Where(x => x.Sid == Sungero.Docflow.PublicConstants.Module.RoleGuid.DepartmentManagersRole).FirstOrDefault();
      if (roleDepartmentManagers != null)
      {
        List<Sungero.Company.IEmployee> employees = new List<Sungero.Company.IEmployee>();
        foreach (var user in Roles.GetAllUsersInGroup(roleDepartmentManagers))
        {
          employees.Add(Sungero.Company.Employees.As(user));
        }
        query = query.Where(c => employees.Contains(c));
      }
      return query;
    }
  }

}