using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Archive.ArchiveList;

namespace litiko.Archive
{
  partial class ArchiveListCaseFilesCaseFilePropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> CaseFilesCaseFileFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      var caseFileIds = _obj.ArchiveList.CaseFiles.Where(x => x.CaseFile != null).Select(f => f.CaseFile.Id).ToList();
      return query.Where(x => !caseFileIds.Contains(x.Id) && x.Archivelitiko == null && !x.TransferredToArchivelitiko.HasValue);
    }
  }

  partial class ArchiveListServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      // Архив, в карточке которого указано текущее подразделение
      if (_obj.Department != null)
      {
        var archive = Archives.GetAll().Where(x => x.Departments.Select(d => d.Department).Contains(_obj.Department)).FirstOrDefault();
        if (archive != null)
          _obj.Archive = archive;
      }
    }
  }

  partial class ArchiveListLeadingDocumentPropertyFilteringServerHandler<T>
  {

    public override IQueryable<T> LeadingDocumentFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      query = base.LeadingDocumentFiltering(query, e);
      
      var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(litiko.Archive.PublicConstants.Module.DocumentKindGuids.OrderArchive);
      if (docKind != null)
        query = query.Where(d => Sungero.RecordManagement.Orders.Is(d) && Equals(Sungero.RecordManagement.Orders.As(d).DocumentKind, docKind));
      
      return query;
    }
  }



}