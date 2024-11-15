using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.Integration.Server
{
  partial class ModuleJobsFolderHandlers
  {

    public virtual IQueryable<Sungero.CoreEntities.IJob> ModuleJobsDataQuery(IQueryable<Sungero.CoreEntities.IJob> query)
    {
      return query.Where(x => x.Name.StartsWith("Интеграция."));
    }
  }

  partial class IntegrationHandlers
  {
  }
}