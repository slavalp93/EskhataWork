using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ContractCondition;

namespace litiko.Eskhata
{
  partial class ContractConditionDocumentGroupslitikoDocumentGroupPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> DocumentGroupslitikoDocumentGroupFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {            
      query = query.Where(g => Sungero.Contracts.ContractCategories.Is(g));
      return query;      
    }
  }

}