using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.RegulatoryDocuments.RegulatoryDocument;

namespace litiko.RegulatoryDocuments
{
  partial class RegulatoryDocumentFilteringServerHandler<T>
  {

    public override IQueryable<T> Filtering(IQueryable<T> query, Sungero.Domain.FilteringEventArgs e)
    {      
      if (_filter == null)
        return query;
      
      // Фильтр по состоянию жизненного цикла.
      if ((_filter.Draft || _filter.Active || _filter.Obsolete) && !(_filter.Draft && _filter.Active && _filter.Obsolete))
        query = query.Where(x => (_filter.Draft && x.LifeCycleState == Sungero.Docflow.OfficialDocument.LifeCycleState.Draft) || 
                            (_filter.Active && x.LifeCycleState == Sungero.Docflow.OfficialDocument.LifeCycleState.Active) ||
                            (_filter.Obsolete && x.LifeCycleState == Sungero.Docflow.OfficialDocument.LifeCycleState.Obsolete)
                           );      
            
      // Фильтр по виду документа.
      if (_filter.DocumentKind != null)
        query = query.Where(d => Equals(d.DocumentKind, _filter.DocumentKind));            
            
      #region Фильтр по Дате пересмотра      
      var periodBegin = Calendar.SqlMinValue;
      var periodEnd = Calendar.SqlMaxValue;
      
      if (_filter.RevisionOverdue)
      {        
        periodEnd = Calendar.UserToday.AddDays(-1).EndOfDay();
      }
      
      if (_filter.RevisionUntilEndOfMonth)
      {
        periodBegin = Calendar.UserToday.BeginningOfDay();
        periodEnd = Calendar.UserToday.EndOfMonth();
      }
      
      if (_filter.RevisionNoLaterThanThreeMonths)
      {
        periodBegin = Calendar.UserToday.BeginningOfDay();
        periodEnd = Calendar.UserToday.AddMonths(3);
      }
      
      if (_filter.RevisionNoLaterThanSixMonths)
      {
        periodBegin = Calendar.UserToday.BeginningOfDay();
        periodEnd = Calendar.UserToday.AddMonths(6);
      }
      
      if (_filter.RevisionNoLaterThanNineMonths)
      {
        periodBegin = Calendar.UserToday.BeginningOfDay();
        periodEnd = Calendar.UserToday.AddMonths(9);
      }
      
      if (_filter.RevisionManualPeriod)
      {
        periodBegin = _filter.RevisionDateRangeFrom ?? Calendar.SqlMinValue;
        periodEnd = _filter.RevisionDateRangeTo ?? Calendar.SqlMaxValue;
      }
      
      var serverPeriodBegin = Equals(Calendar.SqlMinValue, periodBegin) ? periodBegin : Sungero.Docflow.PublicFunctions.Module.Remote.GetTenantDateTimeFromUserDay(periodBegin);
      var serverPeriodEnd = Equals(Calendar.SqlMaxValue, periodEnd) ? periodEnd : periodEnd.EndOfDay().FromUserTime();
      var clientPeriodEnd = !Equals(Calendar.SqlMaxValue, periodEnd) ? periodEnd.AddDays(1) : Calendar.SqlMaxValue;
      query = query.Where(j => (j.DateRevision.Between(serverPeriodBegin, serverPeriodEnd) ||
                                j.DateRevision == periodBegin) && j.DateRevision != clientPeriodEnd);      
      
      #endregion
      
      #region Фильтр по Дате актуализации      
      var periodBegin2 = Calendar.SqlMinValue;
      var periodEnd2 = Calendar.SqlMaxValue;      
      
      if (_filter.UpdateOverdue)
      {        
        periodEnd = Calendar.UserToday.AddDays(-1).EndOfDay();
      }

      if (_filter.UpdateUntilEndOfMonth)
      {
        periodBegin2 = Calendar.UserToday.BeginningOfDay();
        periodEnd2 = Calendar.UserToday.EndOfMonth();
      }
      
      if (_filter.UpdateNoLaterThanThreeMonths)
      {
        periodBegin2 = Calendar.UserToday.BeginningOfDay();
        periodEnd2 = Calendar.UserToday.AddMonths(3);
      }
      
      if (_filter.UpdateNoLaterThanSixMonths)
      {
        periodBegin2 = Calendar.UserToday.BeginningOfDay();
        periodEnd2 = Calendar.UserToday.AddMonths(6);
      }
      
      if (_filter.UpdateNoLaterThanNineMonths)
      {
        periodBegin2 = Calendar.UserToday.BeginningOfDay();
        periodEnd2 = Calendar.UserToday.AddMonths(9);
      }
      
      if (_filter.UpdateManualPeriod)
      {
        periodBegin2 = _filter.UpdateDateRangeFrom ?? Calendar.SqlMinValue;
        periodEnd2 = _filter.UpdateDateRangeTo ?? Calendar.SqlMaxValue;
      }
      
      var serverPeriodBegin2 = Equals(Calendar.SqlMinValue, periodBegin2) ? periodBegin2 : Sungero.Docflow.PublicFunctions.Module.Remote.GetTenantDateTimeFromUserDay(periodBegin2);
      var serverPeriodEnd2 = Equals(Calendar.SqlMaxValue, periodEnd2) ? periodEnd2 : periodEnd2.EndOfDay().FromUserTime();
      var clientPeriodEnd2 = !Equals(Calendar.SqlMaxValue, periodEnd2) ? periodEnd2.AddDays(1) : Calendar.SqlMaxValue;
      query = query.Where(j => (j.DateUpdate.Between(serverPeriodBegin2, serverPeriodEnd2) ||
                                j.DateUpdate == periodBegin2) && j.DateUpdate != clientPeriodEnd2);      
      
      #endregion      
      
      return query;
    }
  }

  partial class RegulatoryDocumentLegalActPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> LegalActFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return query.Where(d => litiko.Eskhata.Orders.Is(d) || litiko.Eskhata.Minuteses.Is(d));
    }
  }

  partial class RegulatoryDocumentServerHandlers
  {

    public override void Saving(Sungero.Domain.SavingEventArgs e)
    {
      base.Saving(e);
      
      #region Выдать права Изменение Руководителю процесса      
      if (_obj.AccessRights.StrictMode == AccessRightsStrictMode.Enhanced)
        return;
      
      var processManager = _obj.ProcessManager;
      if (processManager != null && !_obj.AccessRights.IsGrantedDirectly(DefaultAccessRightsTypes.Change, processManager))
        _obj.AccessRights.Grant(processManager, DefaultAccessRightsTypes.Change);      
      #endregion
    }

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

}