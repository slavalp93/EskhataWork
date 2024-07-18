using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.IncomingLetter;

namespace litiko.Eskhata
{
  partial class IncomingLetterCategoryPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> CategoryFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return query.Where(x => x.DocumentKinds.Any(k => Equals(k.DocumentKind,  _obj.DocumentKind)));
    }
  }

  partial class IncomingLetterServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      var currentDepartment = Sungero.Company.Employees.Current?.Department;
      if (currentDepartment != null && Eskhata.Departments.Is(currentDepartment))
      {
        _obj.ReceivedTo = Eskhata.Departments.As(currentDepartment);
      _obj.RegistrationDepartment = Eskhata.Departments.As(currentDepartment);
      }
    }
  }

}