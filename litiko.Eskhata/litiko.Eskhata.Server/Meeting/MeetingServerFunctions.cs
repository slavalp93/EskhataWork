using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Meeting;

using System.IO;
using System.Reflection;
using CommonLibrary;
using Sungero.Company;
using Sungero.Content;
using Sungero.CoreEntities.Server;
using Sungero.Docflow.ApprovalStage;
using Sungero.Docflow.OfficialDocument;
using Sungero.Domain.Shared;
using Sungero.Metadata;
using Sungero.Parties;
using Sungero.Workflow;

namespace litiko.Eskhata.Server
{
  partial class MeetingFunctions
  {
    /// <summary>
    /// Получить список участников категории совещания.
    /// </summary>
    /// <param name="onlyMembers">Признак отображения только списка участников.</param>
    /// <param name="withJobTitle">Признак отображения должности участников.</param>
    /// <returns>Список участников категории совещания.</returns>
    [Public]
    public string GetMeetingCategoryMembers(bool onlyMembers, bool withJobTitle)
    {
      if (_obj.MeetingCategorylitiko == null)
        return string.Empty;
      
      var employees = _obj.MeetingCategorylitiko.Members
        .OrderBy(x => x.Member.Name)
        .Select(x => x.Member).ToList();

      if (_obj.Secretary != null)
        employees.Insert(0, _obj.Secretary);
      if (_obj.President != null)
        employees.Insert(0, _obj.President);

      if (onlyMembers)
        employees = employees.Where(x => !Equals(x, _obj.President))
          .Where(x => !Equals(x, _obj.Secretary))
          .ToList();
      
      var employeesList = new List<string>();
      foreach (Sungero.Company.IEmployee employee in employees)
      {
        string fio = string.Empty;
        if (withJobTitle)
          fio = string.Format("{0} ({1})", employee.Name, employee.JobTitle?.Name);
        else
          fio = employee.Name;
        
        if (!string.IsNullOrEmpty(fio))
          employeesList.Add(fio);
      }
      
      return string.Join("\r\n", employeesList);
    } 

    
    /// <summary>
    /// Получить нумерованный список сотрудников.
    /// </summary>
    /// <param name="employees">Список сотрудников.</param>
    /// <param name="withJobTitle">Признак отображения должности сотрудников.</param>    
    /// <param name="withShortName">Признак формирования короткого ФИО.</param>    
    /// <returns>Строка с нумерованным списком сотрудников.</returns>
    [Public]
    public static string GetEmployeesNumberedList(List<litiko.Eskhata.IEmployee> employees, bool withJobTitle, bool withShortName, bool inTJ)
    {
        if (employees == null || !employees.Any())
            return null;
    
        // Сортировка по имени (в зависимости от языка)
        var sortedEmployees = inTJ
            ? employees.OrderBy(e => litiko.Eskhata.PublicFunctions.Person.GetNameTJ(People.As(e.Person))).ToList()
            : employees.OrderBy(e => e.Name).ToList();
    
        var employeesNumberedList = new List<string>();
    
        for (int i = 0; i < sortedEmployees.Count; i++)
        {
            var employee = sortedEmployees[i];
            string name;
    
            if (!inTJ)
            {
                name = withShortName
                    ? Sungero.Company.PublicFunctions.Employee.GetShortName(Sungero.Company.Employees.As(employee), true)
                    : employee.Name;
    
                var employeeNumberedName = $"{i + 1}. {name}";
                if (withJobTitle && employee.JobTitle != null && !string.IsNullOrWhiteSpace(employee.JobTitle.Name))
                    employeeNumberedName += $" – {employee.JobTitle.Name}";
    
                employeesNumberedList.Add(employeeNumberedName);
            }
            else
            {
                name = withShortName
                    ? litiko.Eskhata.PublicFunctions.Person.GetShortNameTJ(People.As(employee.Person))
                    : litiko.Eskhata.PublicFunctions.Person.GetNameTJ(People.As(employee.Person));
    
                var employeeNumberedName = $"{i + 1}. {name}";
                if (withJobTitle && employee.JobTitle != null && !string.IsNullOrWhiteSpace(JobTitles.As(employee.JobTitle).NameTGlitiko))
                    employeeNumberedName += $" – {JobTitles.As(employee.JobTitle).NameTGlitiko}";
    
                employeesNumberedList.Add(employeeNumberedName);
            }
        }
    
        return string.Join("\r\n", employeesNumberedList);
    }       
    
    /// <summary>
    /// Получить нумерованный список присутствующих совещания.
    /// </summary>
    /// <param name="withJobTitle">Признак отображения должности.</param>
    /// <returns>Нумерованный список присутствующих совещания.</returns>
    [Public]
    public string GetMeetingPresentNumberedList(bool withJobTitle, bool inTJ = false, bool withShortName = false)
    {                  
      var employees = _obj.Presentlitiko.Select(x => litiko.Eskhata.Employees.As(x.Employee)).ToList();
      if (!employees.Any())
        return string.Empty;
      
      return GetEmployeesNumberedList(employees, withJobTitle, withShortName, inTJ);      
    }

    /// <summary>
    /// Получить нумерованный список отсутствующих совещания.
    /// </summary>
    /// <param name="withJobTitle">Признак отображения должности.</param>
    /// <param name="withReason">Признак отображения причины.</param>
    /// <returns>Нумерованный список отсутствующих совещания.</returns>
    [Public]
    public string GetMeetingAbsentNumberedList(bool withJobTitle, bool withReason = true, bool inTJ = false, bool withShortName = false)
    {                        
      if (!_obj.Absentlitiko.Any())
        return string.Empty;            
      
      var employeesNumberedList = new List<string>();
      int number = 1;
      foreach (var element in _obj.Absentlitiko.Where(x => x.Employee != null))
      {
        var employee = element.Employee;
        var reason = element.AbsentReason?.Name;
        string name = string.Empty;
        if (!inTJ)          
          name = withShortName ? Sungero.Company.PublicFunctions.Employee.GetShortName(Sungero.Company.Employees.As(employee), true) : employee.Name;
        else
          name = withShortName ? litiko.Eskhata.PublicFunctions.Person.GetShortNameTJ(People.As(employee.Person)) : litiko.Eskhata.PublicFunctions.Person.GetNameTJ(People.As(employee.Person));
        var employeeNumberedName = string.Format("{0}. {1}", number, name);
        if (withJobTitle && employee.JobTitle != null)
        {
          if (!inTJ && !string.IsNullOrWhiteSpace(employee.JobTitle.Name))
            employeeNumberedName = string.Format("{0} – {1}", employeeNumberedName, employee.JobTitle.Name);
          if (inTJ && !string.IsNullOrWhiteSpace(JobTitles.As(employee.JobTitle).NameTGlitiko))
            employeeNumberedName = string.Format("{0} – {1}", employeeNumberedName, JobTitles.As(employee.JobTitle).NameTGlitiko);
        }        
        
        if (withReason && !string.IsNullOrWhiteSpace(reason))
          employeeNumberedName = string.Format("{0} ({1})", employeeNumberedName, reason);
        employeesNumberedList.Add(employeeNumberedName);
        number++;
      }
      
      return string.Join("\r\n", employeesNumberedList);                 
    }

    /// <summary>
    /// Получить нумерованный список приглашенных совещания.
    /// </summary>
    /// <param name="withJobTitle">Признак отображения должности.</param>
    /// <returns>Нумерованный список приглашенных совещания.</returns>
    [Public]
    public string GetMeetingInvitedNumberedList(bool withJobTitle, bool inTJ = false, bool withShortName = false)
    {                  
      var employees = _obj.InvitedEmployeeslitiko.Select(x => litiko.Eskhata.Employees.As(x.Employee)).ToList();
      if (!employees.Any())
        return string.Empty;
      
      return GetEmployeesNumberedList(employees, withJobTitle, withShortName, inTJ);      
    }
    
    /// <summary>
    /// Получить нумерованный список заголовков проектов решений совещания.
    /// </summary>
    /// <param name="withJobTitle">Признак отображения должности.</param>
    /// <returns>Нумерованный список заголовков проектов решений совещания.</returns>
    [Public]
    public string GetMeetingProjectSolutionsNumberedList()
    {                        
      if (!_obj.ProjectSolutionslitiko.Any())
        return string.Empty;

      return string.Join(
        "\r\n", 
        _obj.ProjectSolutionslitiko
            .OrderBy(element => element.Number)
            .Select(element => $"{element.Number}. {element.ProjectSolution.Subject}.")
           );
    }
    
    /// <summary>
    /// Создать задачу согласования по регламенту для голосования.
    /// </summary>
    /// <param name="projectSolutions">Список проектов решений.</param>    
    [Remote(PackResultEntityEagerly = true)]
    public Sungero.Docflow.IApprovalTask CreateTaskForVoting(List<litiko.CollegiateAgencies.IProjectsolution> projectSolutions)
    {                        
      if (!projectSolutions.Any())
        return null;
      
      var agenda = Agendas.GetAll().Where(d => Equals(d.Meeting, _obj)).FirstOrDefault();
      if (agenda == null)
        return null;            
      
      //var votingApprovalRule = Sungero.Docflow.ApprovalRules.GetAll().Where(r => r.Name == litiko.CollegiateAgencies.PublicConstants.Module.VotingApprovalRuleName).FirstOrDefault();
      var availableApprovalRules = Sungero.Docflow.PublicFunctions.OfficialDocument.Remote.GetApprovalRules(agenda);
      var votingApprovalRule = availableApprovalRules.Where(r => r.Name == litiko.CollegiateAgencies.PublicConstants.Module.VotingApprovalRuleName).FirstOrDefault();
      if (votingApprovalRule == null)
        return null;
      
      var task = Sungero.Docflow.ApprovalTasks.Create();
      task.DocumentGroup.All.Add(agenda);            
      foreach (var element in task.AddendaGroup.All)
      {        
        if (!projectSolutions.Select(x => x.Id).Contains(element.Id))
          task.AddendaGroup.All.Remove(element);              
      }      
      foreach (var document in projectSolutions)
      {
        if (!task.AddendaGroup.All.Contains(document))
          task.AddendaGroup.All.Add(document);
      }

      if (!task.OtherGroup.All.Contains(_obj))
        task.OtherGroup.All.Add(_obj);
      
      task.Subject = "Голосование: " + _obj.Name;
      task.ActiveText = "Прошу проголосовать.";
      task.ApprovalRule = votingApprovalRule;      
      
      return task;
    }

    /// <summary>
    /// Получить протокол.
    /// </summary>
    /// <returns>Протокол совещания.</returns>
    [Remote, Public]
    public virtual litiko.Eskhata.IMinutes GetMinutes()
    {
      return litiko.Eskhata.Minuteses.GetAll(d => Equals(d.Meeting, _obj)).FirstOrDefault();
    }

    /// <summary>
    /// Получить обновленный список поручений.
    /// </summary>
    /// <param name="ids">Список Id поручений.</param>
    /// <returns>Обновленный список поручений.</returns>
    [Remote]
    public static List<Sungero.RecordManagement.IActionItemExecutionTask> GetActionItemsExecutionTasks(List<long> ids)
    {
      return Sungero.RecordManagement.ActionItemExecutionTasks.GetAll(t => ids.Contains(t.Id)).ToList();
    }    

    /// <summary>
    /// Удаление поручения, созданного по документу.
    /// </summary>
    /// <param name="actionItemId">ИД задачи, которую необходимо удалить.</param>
    /// <returns>True, если удаление прошло успешно.</returns>
    [Remote]
    public static bool TryDeleteActionItemTask(long actionItemId)
    {
      try
      {
        var task = Sungero.RecordManagement.ActionItemExecutionTasks.Get(actionItemId);
        if (task.AccessRights.CanDelete())
          Sungero.RecordManagement.ActionItemExecutionTasks.Delete(task);
        else
          return false;
      }
      catch
      {
        return false;
      }

      return true;
    }
    
    /// <summary>
    /// Создать поручения по совещанию.
    /// </summary>
    /// <returns>Список созданных поручений.</returns>
    [Remote, Public]
    public virtual List<Sungero.RecordManagement.IActionItemExecutionTask> CreateActionItemsFromMeeting()
    {      
      var resultList = new List<Sungero.RecordManagement.IActionItemExecutionTask>();
      
      if (!_obj.ProjectSolutionslitiko.Any(x => x.ProjectSolution != null))      
        return resultList;

      var minutes = this.GetMinutes();
      if (minutes == null)
        return resultList;
      
      foreach (var element in _obj.ProjectSolutionslitiko.Where(x => x.ProjectSolution != null))
      {
        var projectSolution = element.ProjectSolution;
        foreach (var decision in projectSolution.DecidedMinutes.Where(x => !string.IsNullOrEmpty(x.DecisionRU) && x.Responsible != null)) //&& x.Date.HasValue
        {
          var actionItem = Sungero.RecordManagement.PublicFunctions.Module.Remote.CreateActionItemExecution(minutes);
          actionItem.AssignedBy = _obj.President;
          actionItem.IsUnderControl = true;
          actionItem.Supervisor = _obj.Secretary;
          actionItem.ActiveText = decision.DecisionRU;
          actionItem.Assignee = decision.Responsible;
          actionItem.Deadline = decision.Date;
          
          // Vals added 25.09.10 Добавляем проектное решение и приложения + выдаём права исполнителю
          AddProjectSolutionWithAddendaToActionItem(projectSolution, actionItem);

          foreach (var property in actionItem.State.Properties)
            property.IsRequired = false;          
          
          actionItem.Save();
          
          resultList.Add(actionItem);
        }
      }
      
      return resultList;
    }
    
    /// <summary>
    /// Старт задач на исполнение поручений по совещанию.
    /// </summary>
    /// <param name="actionItems">Список задач для старта.</param>
    [Public]
    public virtual void StartActionItemTasksFromDialog(List<Sungero.RecordManagement.IActionItemExecutionTask> actionItems)
    {
      var taskIds = actionItems.Select(t => t.Id).ToList();      
      
      var completedNotification = litiko.Eskhata.Meetings.Resources.ActionItemsSentSeccessfully;
      var startedNotification = litiko.Eskhata.Meetings.Resources.ActionItemCreateFromDialogNotification;
      var errorNotification = litiko.Eskhata.Meetings.Resources.StartActionItemExecutionTasksErrorSolutionFormat(Environment.NewLine);

      var startActionItemsAsyncHandler = Sungero.RecordManagement.AsyncHandlers.StartActionItemExecutionTasks.Create();
      startActionItemsAsyncHandler.TaskIds = string.Join(",", taskIds);
      if (Users.Current != null)
        startActionItemsAsyncHandler.StartedByUserId = Users.Current.Id;
      startActionItemsAsyncHandler.ExecuteAsync(startedNotification, completedNotification, errorNotification, Users.Current);
    }  

    #region Private Functions
    /// <summary>
    /// Добавляет проектное решение и его приложения в поручение и выдаёт права исполнителю.
    /// </summary>
    /// <param name="projectSolution">Документ проекта, который нужно добавить.</param>
    /// <param name="actionItem">Поручение, в которое добавляются вложения.</param>
    private void AddProjectSolutionWithAddendaToActionItem(Sungero.Docflow.IOfficialDocument projectSolution,
                                                                  Sungero.RecordManagement.IActionItemExecutionTask actionItem)
    {
        if (projectSolution == null || actionItem == null)
            return;
    
        // Приводим к IOfficialDocument
        var projectDoc = Sungero.Docflow.OfficialDocuments.As(projectSolution);
        if (projectDoc == null)
            return;

        // Добавляем проектное решение во вложения
        if (!actionItem.OtherGroup.All.Contains(projectSolution))
            actionItem.OtherGroup.All.Add(projectSolution);
    
        // Выдаём права исполнителю
        if (actionItem.Assignee != null)
        {
            projectSolution.AccessRights.Grant(actionItem.Assignee, DefaultAccessRightsTypes.Read);
            projectSolution.Save();

            // Получаем приложения документа (Addenda)
            var addenda = projectDoc.Relations.GetRelated(Sungero.Docflow.PublicConstants.Module.AddendumRelationName);
            
            foreach (var doc in addenda)
            {
                doc.AccessRights.Grant(actionItem.Assignee, DefaultAccessRightsTypes.Read);
                doc.Save();
            }
        }
    }    
    #endregion    
       
  }
}