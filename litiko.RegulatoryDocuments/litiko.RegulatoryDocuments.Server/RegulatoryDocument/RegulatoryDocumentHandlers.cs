using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.RegulatoryDocuments.RegulatoryDocument;

namespace litiko.RegulatoryDocuments
{
  partial class RegulatoryDocumentLegalActPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> LegalActFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return query.Where(d => litiko.Eskhata.Orders.Is(d) || litiko.Eskhata.Minuteses.Is(d));
    }
  }

  partial class RegulatoryDocumentServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      base.BeforeSave(e);
      
      // Обновить связь с Правовым актом
      if (_obj.LegalAct != null && _obj.LegalAct.AccessRights.CanRead() &&
          !_obj.Relations.GetRelated(Sungero.Docflow.PublicConstants.Module.SimpleRelationName).Contains(_obj.LegalAct))
        _obj.Relations.AddOrUpdate(Sungero.Docflow.PublicConstants.Module.SimpleRelationName, _obj.State.Properties.LegalAct.OriginalValue, _obj.LegalAct);
      
      // Обновить связь с Основным ВНД
      if (_obj.LeadingDocument != null && _obj.LeadingDocument.AccessRights.CanRead() &&
          !_obj.Relations.GetRelatedFrom(Sungero.Docflow.PublicConstants.Module.BasisRelationName).Contains(_obj.LeadingDocument))
        _obj.Relations.AddFromOrUpdate(Sungero.Docflow.PublicConstants.Module.BasisRelationName, _obj.State.Properties.LeadingDocument.OriginalValue, _obj.LeadingDocument);      
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      _obj.IsRequirements = false;
      _obj.IsRecommendations = false;
      _obj.IsRelatedToStructure = false;
    }
  }

  partial class RegulatoryDocumentLeadingDocumentPropertyFilteringServerHandler<T>
  {

    public override IQueryable<T> LeadingDocumentFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      query = base.LeadingDocumentFiltering(query, e);
      return query.Where(d => RegulatoryDocuments.Is(d));
    }
  }

}