using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.Eskhata.Module.ContractsUI.Server
{
  partial class MigratedContractslitikoFolderHandlers
  {
    public virtual IQueryable<litiko.Eskhata.IContract> MigratedContractslitikoDataQuery(IQueryable<litiko.Eskhata.IContract> query)
    {
      query = query.Where(c=>c.IsMigratedlitiko == true);
      
      if(_filter == null)
        return query;
  
      return query;
    }
  }
}