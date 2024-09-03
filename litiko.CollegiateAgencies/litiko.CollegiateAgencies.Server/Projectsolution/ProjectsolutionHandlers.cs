using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.CollegiateAgencies.Projectsolution;

namespace litiko.CollegiateAgencies
{
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