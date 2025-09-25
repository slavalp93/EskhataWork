using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.CollegiateAgencies.Projectsolution;

namespace litiko.CollegiateAgencies
{
  partial class ProjectsolutionCreatingFromServerHandler
  {

    public override void CreatingFrom(Sungero.Domain.CreatingFromEventArgs e)
    {
      //base.CreatingFrom(e);
      
      // Область регистрации.
      e.Without(_info.Properties.RegistrationNumber);
      e.Without(_info.Properties.RegistrationDate);
      e.Without(_info.Properties.DocumentRegister);
      e.Without(_info.Properties.DeliveryMethod);
      e.Without(_info.Properties.CaseFile);
      e.Without(_info.Properties.PlacedToCaseFileDate);
      e.Without(_info.Properties.Tracking);
      
      // Область хранения.
      e.Without(_info.Properties.PaperCount);
      e.Without(_info.Properties.AddendaPaperCount);
      e.Without(_info.Properties.StoredIn);

      // Статусы жизненного цикла.
      e.Without(_info.Properties.LifeCycleState);
      e.Without(_info.Properties.RegistrationState);
      e.Without(_info.Properties.VerificationState);
      e.Without(_info.Properties.InternalApprovalState);
      e.Without(_info.Properties.ExternalApprovalState);
      e.Without(_info.Properties.ExecutionState);
      e.Without(_info.Properties.ControlExecutionState);
      e.Without(_info.Properties.LocationState);
      e.Without(_info.Properties.ExchangeState);
      
      // Свойства "Подписал" и "Основание".
      //e.Without(_info.Properties.OurSignatory);
      //e.Without(_info.Properties.OurSigningReason);
      
      // Свойство "Исполнитель".
      e.Without(_info.Properties.Assignee);
      
      e.Without(_info.Properties.LeadingDocument);
      e.Without(_info.Properties.Meeting);
      e.Without(_info.Properties.IncludedInAgenda);
      
      // Вкладка "Протокол"
      e.Without(_info.Properties.ListenedRUMinutes);
      e.Without(_info.Properties.ListenedTJMinutes);
      e.Without(_info.Properties.ListenedENMinutes);
      e.Without(_info.Properties.DecidedMinutes);

      // Вкладка "Голосование"
      e.Without(_info.Properties.Voting);
      
      e.Params.AddOrUpdate(Sungero.Docflow.PublicConstants.OfficialDocument.GrantAccessRightsToDocumentAsync, true);
    }
  }

  partial class ProjectsolutionFilteringServerHandler<T>
  {

    public override IQueryable<T> Filtering(IQueryable<T> query, Sungero.Domain.FilteringEventArgs e)
    {
      query = base.Filtering(query, e);
      
      if (_filter == null)
        return query;      
      
      if (_filter.Included)
        query = query.Where(d => d.IncludedInAgenda.GetValueOrDefault());
      
      if (_filter.NotIncluded)
        query = query.Where(d => !d.IncludedInAgenda.GetValueOrDefault());
      
      if (_filter.MeetingCategory != null)
        query = query.Where(d => Equals(d.MeetingCategory, _filter.MeetingCategory));
      
      if (_filter.Speaker != null)
        query = query.Where(d => Equals(d.Speaker, _filter.Speaker));
      
      if (_filter.OurSignatory != null)
        query = query.Where(d => Equals(d.OurSignatory, _filter.OurSignatory));       
      
      return query;
    }
  }

  partial class ProjectsolutionVotingMemberPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> VotingMemberFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      if (_root.Meeting != null)
      {
        var members = _root.Meeting.Members.Where(x => x.Member != null).Select(x => x.Member).ToList();
        if (_root.Meeting.President != null && !members.Contains(_root.Meeting.President))
          members.Add(_root.Meeting.President);
        
        query = query.Where(x => members.Contains(x));
      }        
      
      if (_root.Voting.Any())
      {
        var alreadySelected = _root.Voting.Where(x => x.Member != null).Select(x => x.Member).ToList();
        query = query.Where(x => !alreadySelected.Contains(x));
      }        
      
      return query;
    }
  }

  partial class ProjectsolutionLeadingDocumentPropertyFilteringServerHandler<T>
  {

    public override IQueryable<T> LeadingDocumentFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      query = base.LeadingDocumentFiltering(query, e);
      
      var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(litiko.CollegiateAgencies.PublicConstants.Module.DocumentKindGuids.ExplanatoryNote);
      if (docKind != null)
        query = query.Where(d => Equals(Sungero.Docflow.OfficialDocuments.As(d).DocumentKind, docKind));
      
      return query;
    }
  }

  partial class ProjectsolutionServerHandlers
  {

    public override void Saving(Sungero.Domain.SavingEventArgs e)
    {
      base.Saving(e);      
      
      #region Выдать права
      if (_obj.AccessRights.StrictMode == Sungero.Core.AccessRightsStrictMode.None)
      {
        // Ответственный за подготовку - Изменение
        var ourSignatory = _obj.OurSignatory;
        if (ourSignatory != null && !_obj.AccessRights.IsGrantedDirectly(DefaultAccessRightsTypes.Change, ourSignatory))
          _obj.AccessRights.Grant(ourSignatory, DefaultAccessRightsTypes.Change);
        
        // Докладчик - Изменение
        var speaker = _obj.Speaker;
        if (speaker != null && !_obj.AccessRights.IsGrantedDirectly(DefaultAccessRightsTypes.Change, speaker))
          _obj.AccessRights.Grant(speaker, DefaultAccessRightsTypes.Change);
  
        // Приглашенные сотрудники - Просмотр      
        foreach (var element in _obj.InvitedEmployees.Where(x => x.Employee != null))
        {
          var invitedEmployee = element.Employee;
          if (!_obj.AccessRights.IsGrantedDirectly(DefaultAccessRightsTypes.Read, invitedEmployee))
          _obj.AccessRights.Grant(invitedEmployee, DefaultAccessRightsTypes.Read);
        }

        // Переводчик
        var roleTranslator = Roles.GetAll(r => r.Sid == Constants.Module.RoleGuid.Translator).FirstOrDefault();
        if (roleTranslator != null)
        {
          foreach (var element in roleTranslator.RecipientLinks)
          {
            if (!_obj.AccessRights.IsGrantedDirectly(DefaultAccessRightsTypes.Change, element.Member))
              _obj.AccessRights.Grant(element.Member, DefaultAccessRightsTypes.Change);
          }
        }
        
        // Члены категория заседания - Изменение
        if (_obj.MeetingCategory != null)
        {
          var categoryMembers = new List<Sungero.Company.IEmployee>();
          if (_obj.MeetingCategory.President != null)
            categoryMembers.Add(_obj.MeetingCategory.President);
          if (_obj.MeetingCategory.Secretary != null)
          {
            categoryMembers.Add(_obj.MeetingCategory.Secretary);
            
            var substituters = Sungero.CoreEntities.Substitutions.ActiveUsersWhoSubstitute(_obj.MeetingCategory.Secretary);
            foreach (var user in substituters)
              categoryMembers.Add(Sungero.Company.Employees.As(user));
          }
          foreach (Sungero.Company.IEmployee member in _obj.MeetingCategory.Members.Where(x => x.Member != null).Select(x => x.Member))
            categoryMembers.Add(member);
          
          foreach (var employee in categoryMembers)
          {
            if (!_obj.AccessRights.IsGrantedDirectly(DefaultAccessRightsTypes.Change, employee))
              _obj.AccessRights.Grant(employee, DefaultAccessRightsTypes.Change);                        
          }                   
        }
        
        // "Дополнительные члены Правления"
        if (_obj.MeetingCategory?.Name == "Заседание Правления")
        {
          var roleAdditionalBoardMembers = Roles.GetAll(r => r.Sid == PublicConstants.Module.RoleGuid.AdditionalBoardMembers).FirstOrDefault();
          if (roleAdditionalBoardMembers != null)
          {
            if (!_obj.AccessRights.IsGrantedDirectly(DefaultAccessRightsTypes.Change, roleAdditionalBoardMembers))
              _obj.AccessRights.Grant(roleAdditionalBoardMembers, DefaultAccessRightsTypes.Change);             
          }        
        }
      }            
      #endregion      
      
    }

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      base.BeforeSave(e);
            
      object paramValue;
      if (((Sungero.Domain.Shared.IExtendedEntity)_obj).Params.TryGetValue(litiko.RegulatoryDocuments.PublicConstants.Module.CreatedFromIRD_ID, out paramValue))
      {
        var docId = (long)paramValue;
        var document = Sungero.Docflow.OfficialDocuments.Get(docId);
        if (document != null && litiko.RegulatoryDocuments.RegulatoryDocuments.Is(document))
        {
          if (!_obj.Relations.GetRelated(Sungero.Docflow.PublicConstants.Module.SimpleRelationName).Contains(document))
            _obj.Relations.Add(Sungero.Docflow.PublicConstants.Module.SimpleRelationName, document);          
        }
        
        if (document != null && Sungero.Docflow.SimpleDocuments.Is(document))
        {
          if (!_obj.Relations.GetRelated(Sungero.Docflow.PublicConstants.Module.AddendumRelationName).Contains(document))
            _obj.Relations.Add(Sungero.Docflow.PublicConstants.Module.AddendumRelationName, document);          
        }        
      }
    }

    public override void BeforeSaveHistory(Sungero.Content.DocumentHistoryEventArgs e)
    {
      base.BeforeSaveHistory(e);
            
      var isUpdateAction = e.Action == Sungero.CoreEntities.History.Action.Update;
      if (!isUpdateAction)
    	  return;
      
      var operation = new Enumeration("SDChange");
      var changeList = litiko.CollegiateAgencies.PublicFunctions.Module.ChangeRequisites(_obj);
      foreach (var comment in changeList)
      {
        e.Write(operation, null, comment);
      }
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      _obj.IncludedInAgenda = false;
    }
  }

  partial class ProjectsolutionSpeakerPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> SpeakerFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      /* Deleted 07.01.2025
      var roleDepartmentManagers = Roles.GetAll().Where(x => x.Sid == Sungero.Docflow.PublicConstants.Module.RoleGuid.DepartmentManagersRole).FirstOrDefault();
      if (roleDepartmentManagers != null)
      {
        List<Sungero.Company.IEmployee> employees = new List<Sungero.Company.IEmployee>();
        foreach (var user in Roles.GetAllUsersInGroup(roleDepartmentManagers))
        {
          employees.Add(Sungero.Company.Employees.As(user));
        }
        query = query.Where(c => employees.Contains(c));
      }
      */
      return query;
    }
  }

}