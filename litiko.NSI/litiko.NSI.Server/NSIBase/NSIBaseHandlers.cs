using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.NSI.NSIBase;

namespace litiko.NSI
{
  partial class NSIBaseParentEntryPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> ParentEntryFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      if (OKFSes.Is(_obj))
        query = query.Where(x => OKFSes.Is(x));
      
      if (OKONHs.Is(_obj))
        query = query.Where(x => OKONHs.Is(x));
      
      if (OKOPFs.Is(_obj))
        query = query.Where(x => OKOPFs.Is(x));
      
      if (OKVEDs.Is(_obj))
        query = query.Where(x => OKVEDs.Is(x));
      
      if (EnterpriseTypes.Is(_obj))
        query = query.Where(x => EnterpriseTypes.Is(x));
      
      if (EnvironmentalRisks.Is(_obj))
        query = query.Where(x => EnvironmentalRisks.Is(x));
      
      if (FamilyStatuses.Is(_obj))
        query = query.Where(x => FamilyStatuses.Is(x));
      
      if (IDTypes.Is(_obj))
        query = query.Where(x => IDTypes.Is(x));            
      
      return query;
    }
  }

}