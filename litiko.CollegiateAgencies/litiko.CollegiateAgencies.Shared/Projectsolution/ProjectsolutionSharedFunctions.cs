using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.CollegiateAgencies.Projectsolution;

namespace litiko.CollegiateAgencies.Shared
{
  partial class ProjectsolutionFunctions
  {

    /// <summary>
    /// Определить подписанта по умолчанию
    /// </summary>       
    public void DefineOurSignatory()
    {
      if (_obj.MeetingCategory != null)
      {
        if (_obj.MeetingCategory.Name == "Заседание Тендерной комиссии")
        {
          // Ответственный сотрудник АХД
          var role = Roles.GetAll(r => r.Sid == Constants.Module.RoleGuid.ResponsibleEmployeeAHD).FirstOrDefault();
          if (role != null)
          {
            var roleMember = Roles.GetAllUsersInGroup(role).FirstOrDefault();
            if (roleMember != null && !Equals(_obj.OurSignatory, Sungero.Company.Employees.As(roleMember)))
              _obj.OurSignatory = Sungero.Company.Employees.As(roleMember);
          }
        }
        else
        {
          // Глава подразделения при прямом подчинении Председателю, являющегося куратором/руководителем подразделения Автора
          var preparedBy = _obj.PreparedBy;
          if (preparedBy != null)
          {
            var department = preparedBy.Department;
            int hierarchyLevels = 0;
            while (department.HeadOffice?.HeadOffice != null && hierarchyLevels < 30)
            {
              department = department.HeadOffice;
              hierarchyLevels++;
            }
            
            if (hierarchyLevels < 30 && department?.Manager != null && !Equals(_obj.OurSignatory, department.Manager))
              _obj.OurSignatory = department.Manager;            
          }
        }
      }      
    }
    /// <summary>
    /// Обновить карточку документа.
    /// </summary>
    public override void RefreshDocumentForm()
    {
      base.RefreshDocumentForm();
      var properties = _obj.State.Properties;
      bool isEnabled = _obj.Meeting != null && litiko.Eskhata.PublicFunctions.Meeting.CurrentUserHasAccess(_obj.Meeting);
      
      properties.ListenedTJ.IsRequired = true;      
      
      
      #region Вкладка голосование      
      bool votingTableAviabled = true;
      if (_obj.Meeting != null && 
          (_obj.Meeting.Votinglitiko.GetValueOrDefault() == litiko.Eskhata.Meeting.Votinglitiko.extramural)
         )
        votingTableAviabled = false;
            
      _obj.State.Properties.Voting.IsEnabled = isEnabled && votingTableAviabled;
      #endregion
      
      #region Контроль бюджета
      if (_obj.MeetingCategory?.Name != "Заседание Тендерной комиссии")
      {
        _obj.State.Properties.Budget.IsVisible = false;
        _obj.State.Properties.BudgetRemaining.IsVisible = false;
      }
      else
      {
        _obj.State.Properties.Budget.IsVisible = true;
        _obj.State.Properties.BudgetRemaining.IsVisible = true;
      }
      #endregion
      
      #region Вкладка протокол
      bool isСommitteeMember = _obj.Meeting != null && _obj.Meeting.MeetingCategorylitiko != null && (
        _obj.Meeting.MeetingCategorylitiko.Members.Any(x => Equals(Sungero.Company.Employees.As(Users.Current), x.Member)) ||
        Equals(_obj.Meeting.MeetingCategorylitiko.President, Sungero.Company.Employees.As(Users.Current))
       );
      
      // Роль переводчик
      var isTranslator = false;
      var roleTranslator = Roles.GetAll(r => r.Sid == Constants.Module.RoleGuid.Translator).FirstOrDefault();
      if (roleTranslator != null && Users.Current.IncludedIn(roleTranslator))
        isTranslator = true;
      
      properties.ListenedRUMinutes.IsEnabled = isEnabled || isСommitteeMember || isTranslator;
      properties.ListenedENMinutes.IsEnabled = isEnabled || isСommitteeMember || isTranslator;
      properties.ListenedTJMinutes.IsEnabled = isEnabled || isСommitteeMember || isTranslator;
      properties.DecidedMinutes.IsEnabled = isEnabled || isСommitteeMember || isTranslator;
      
      #endregion
    }

    /// <summary>
    /// Обработка включения в совещание
    /// </summary>
    [Public]
    public void ProcessIncludingInMeeting(litiko.Eskhata.IMeeting meeting, bool isFromMeeting)
    {
      if (meeting != null)
      {
        if (_obj.MeetingCategory != null && !Equals(_obj.MeetingCategory, meeting.MeetingCategorylitiko))
          meeting.MeetingCategorylitiko = _obj.MeetingCategory;
        
        if (!isFromMeeting && !meeting.ProjectSolutionslitiko.Any(x => Equals(x.ProjectSolution, _obj)))
          meeting.ProjectSolutionslitiko.AddNew().ProjectSolution = _obj;
        
        if (_obj.Speaker != null && !meeting.Members.Any(x => Equals(Sungero.Company.Employees.As(x.Member), _obj.Speaker)))
          meeting.Members.AddNew().Member = _obj.Speaker;
        
        if (_obj.InvitedEmployees.Any())
        {
          foreach (Sungero.Company.IEmployee employee in _obj.InvitedEmployees.Where(x => x.Employee != null).Select(x => x.Employee))
          {
            if (!meeting.InvitedEmployeeslitiko.Any(x => Equals(x.Employee, employee)))
              meeting.InvitedEmployeeslitiko.AddNew().Employee = employee;
          }
        }

        if (_obj.InvitedExternal.Any())
        {
          foreach (Sungero.Parties.IContact contact in _obj.InvitedExternal.Where(x => x.Contact != null).Select(x => x.Contact))
          {
            if (!meeting.InvitedExternallitiko.Any(x => Equals(x.Contact, contact)))
              meeting.InvitedExternallitiko.AddNew().Contact = contact;
          }
        }        
      }    
    }
    
    /// <summary>
    /// Обработать добавление документа как основного вложения в задачу.
    /// </summary>
    /// <param name="task">Задача.</param>
    /// <remarks>Только для задач, создаваемых пользователем вручную.</remarks>
    [Public]
    public override void DocumentAttachedInMainGroup(Sungero.Workflow.ITask task)
    {      
      var approvalTask = Sungero.Docflow.ApprovalTasks.As(task);
      if (approvalTask != null)
      {
        var relatedIRDs = _obj.Relations.GetRelated(Sungero.Docflow.PublicConstants.Module.SimpleRelationName)
          .Where(d => litiko.RegulatoryDocuments.RegulatoryDocuments.Is(d));
        foreach (var document in relatedIRDs)
          if (!approvalTask.OtherGroup.All.Contains(document))
            approvalTask.OtherGroup.All.Add(document);
      }

    }    
  }
}