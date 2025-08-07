using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.NSI.ResponsibilityMatrix;

namespace litiko.NSI
{
  partial class ResponsibilityMatrixServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.ConclusionDKR = false;
      _obj.BatchProcessing = false;
    }
  }

  partial class ResponsibilityMatrixResponsibleAHDPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> ResponsibleAHDFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return ResponsibilityMatrixFilteringHelper.ApplyCommonPersonFiltering(query);
    }
  }
  
  partial class ResponsibilityMatrixResponsibleAccountantPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> ResponsibleAccountantFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return ResponsibilityMatrixFilteringHelper.ApplyCommonPersonFiltering(query);
    }
  }

  partial class ResponsibilityMatrixResponsibleLawyerPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> ResponsibleLawyerFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return ResponsibilityMatrixFilteringHelper.ApplyCommonPersonFiltering(query);
    }
  }

  partial class ResponsibilityMatrixDocumentKindPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> DocumentKindFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      var availableDocumentKinds = Sungero.Contracts.PublicFunctions.ContractCategory.GetAllowedDocumentKinds();      
      return query.Where(a => availableDocumentKinds.Contains(a));
    }
  }
  
  partial class ResponsibilityMatrixContractCategoriesCategoryPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> ContractCategoriesCategoryFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return query.Where(x => x.DocumentKinds.Any(k => Equals(k.DocumentKind, _root.DocumentKind)));     
    }
  }
  
  public static class ResponsibilityMatrixFilteringHelper
  {
    public static IQueryable<T> ApplyCommonPersonFiltering<T>(IQueryable<T> query)
      where T : Sungero.CoreEntities.IDatabookEntry
    {
      // Для выбора доступны только сотрудники и одиночные роли.
      return query
        .Where(q => q.Status == Sungero.CoreEntities.DatabookEntry.Status.Active)         
        .Where(q => Sungero.Company.Employees.Is(q) || 
                    (Roles.Is(q) && Roles.As(q).IsSingleUser == true));
    }
  }  

}