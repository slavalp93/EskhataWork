using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Order;

namespace litiko.Eskhata
{
  partial class OrderAssigneePropertyFilteringServerHandler<T>
  {

    public override IQueryable<T> AssigneeFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      query = base.AssigneeFiltering(query, e);
      if (_obj.DocumentKind == Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(RecordManagementEskhata.PublicConstants.Module.DocumentKindGuids.BranchOrder))
      {
        string extCode = Eskhata.Departments.As(Sungero.Company.Employees.Current?.Department)?.ExternalCodelitiko;
        if (!string.IsNullOrEmpty(extCode) && extCode.Length > 6)
          extCode = extCode.Substring(0,7);
        if (string.IsNullOrEmpty(extCode))
          return query;
        
        query = query.Where(x => Eskhata.Departments.Is(x.Department) &&
                            Eskhata.Departments.As(x.Department).ExternalCodelitiko.StartsWith(extCode));
      }
      return query;
    }
  }

  partial class OrderServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.ChiefAccountantApproving = false;
      base.Created(e);
    }
  }

}