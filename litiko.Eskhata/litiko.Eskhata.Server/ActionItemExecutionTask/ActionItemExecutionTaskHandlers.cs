using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ActionItemExecutionTask;

namespace litiko.Eskhata
{
  partial class ActionItemExecutionTaskActionItemPartsAssigneePropertyFilteringServerHandler<T>
  {

    public override IQueryable<T> ActionItemPartsAssigneeFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      query = base.ActionItemPartsAssigneeFiltering(query, e);
      
      var document = _obj.ActionItemExecutionTask.DocumentsGroup.OfficialDocuments.FirstOrDefault();
      if (document?.DocumentKind == Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(RecordManagementEskhata.PublicConstants.Module.DocumentKindGuids.BranchOrder))
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

  partial class ActionItemExecutionTaskCoAssigneesAssigneePropertyFilteringServerHandler<T>
  {

    public override IQueryable<T> CoAssigneesAssigneeFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      query = base.CoAssigneesAssigneeFiltering(query, e);
      var document = _root?.DocumentsGroup?.OfficialDocuments?.FirstOrDefault();
      if (document?.DocumentKind == Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(RecordManagementEskhata.PublicConstants.Module.DocumentKindGuids.BranchOrder) ||
          document?.DocumentKind == Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(RecordManagementEskhata.PublicConstants.Module.DocumentKindGuids.BranchOrderEds))
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

  partial class ActionItemExecutionTaskAssigneePropertyFilteringServerHandler<T>
  {

    public override IQueryable<T> AssigneeFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      query = base.AssigneeFiltering(query, e);
      var document = _obj.DocumentsGroup.OfficialDocuments.FirstOrDefault();
      if (document?.DocumentKind == Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(RecordManagementEskhata.PublicConstants.Module.DocumentKindGuids.BranchOrder) ||
          document?.DocumentKind == Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(RecordManagementEskhata.PublicConstants.Module.DocumentKindGuids.BranchOrderEds))
      {
        string extCode = Eskhata.Departments.As(Sungero.Company.Employees.Current?.Department)?.ExternalCodelitiko;
        if (!string.IsNullOrEmpty(extCode) && extCode.Length > 6)
        {
          extCode = extCode.Substring(0,7);
        }
        if (string.IsNullOrEmpty(extCode))
        {
          return query;
        }
        
        query = query.Where(x => Eskhata.Departments.Is(x.Department) &&
                            Eskhata.Departments.As(x.Department).ExternalCodelitiko.StartsWith(extCode));
      }
      
      return query;
    }
  }

}