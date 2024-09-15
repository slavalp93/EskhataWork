using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Addendum;

namespace litiko.Eskhata
{
  partial class AddendumLeadingDocumentPropertyFilteringServerHandler<T>
  {

    public override IQueryable<T> LeadingDocumentFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      query = base.LeadingDocumentFiltering(query, e);
            
      var docKindProjectSolution = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(litiko.CollegiateAgencies.PublicConstants.Module.DocumentKindGuids.ProjectSolution);
      var docKindExtractProtocol = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(litiko.CollegiateAgencies.PublicConstants.Module.DocumentKindGuids.ExtractProtocol);
      var docKindExplanatoryNote = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(litiko.CollegiateAgencies.PublicConstants.Module.DocumentKindGuids.ExplanatoryNote);
      var docKindResolution = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(litiko.CollegiateAgencies.PublicConstants.Module.DocumentKindGuids.Resolution);
      
      // Для Выписки из протокола - доступны только Проекты решений
      if (docKindExtractProtocol != null && docKindProjectSolution != null && Equals(_obj.DocumentKind, docKindExtractProtocol))
        query = query.Where(d => Equals(Sungero.Docflow.OfficialDocuments.As(d).DocumentKind, docKindProjectSolution));
      
      // Для Пояснительной записки - доступны только Проекты решений
      if (docKindExplanatoryNote != null && docKindProjectSolution != null && Equals(_obj.DocumentKind, docKindExplanatoryNote))
        query = query.Where(d => Equals(Sungero.Docflow.OfficialDocuments.As(d).DocumentKind, docKindProjectSolution));

      // Для Постановления - доступны только Проекты решений
      if (docKindResolution != null && docKindProjectSolution != null && Equals(_obj.DocumentKind, docKindResolution))
        query = query.Where(d => Equals(Sungero.Docflow.OfficialDocuments.As(d).DocumentKind, docKindProjectSolution));
      
      return query;
    }
  }

}