using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.CollegiateAgencies.Server
{
  partial class ExtractsProtocolFolderHandlers
  {

    public virtual IQueryable<litiko.Eskhata.IAddendum> ExtractsProtocolDataQuery(IQueryable<litiko.Eskhata.IAddendum> query)
    {
      var docKindExtractProtocol = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(litiko.CollegiateAgencies.PublicConstants.Module.DocumentKindGuids.ExtractProtocol);
      if (docKindExtractProtocol != null)
        return query.Where(d => Equals(d.DocumentKind, docKindExtractProtocol));
      
      return query;
    }
  }


  partial class CollegiateAgenciesHandlers
  {
  }
}