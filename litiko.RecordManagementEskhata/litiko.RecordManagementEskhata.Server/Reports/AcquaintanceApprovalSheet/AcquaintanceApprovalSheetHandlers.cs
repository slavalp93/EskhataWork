using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.RecordManagementEskhata
{
  partial class AcquaintanceApprovalSheetServerHandlers
  {

    public override void AfterExecute(Sungero.Reporting.Server.AfterExecuteEventArgs e)
    {
      Sungero.Docflow.PublicFunctions.Module.DeleteReportData(Constants.AcquaintanceApprovalSheet.SourceTableName, AcquaintanceApprovalSheet.ReportSessionId);
    }

    public override void BeforeExecute(Sungero.Reporting.Server.BeforeExecuteEventArgs e)
    {
      var sourceDocument = AcquaintanceApprovalSheet.Document;
      var sourceTask = AcquaintanceApprovalSheet.Task;
      
      AcquaintanceApprovalSheet.DocName = sourceDocument.Name;
      AcquaintanceApprovalSheet.Department = sourceDocument.Department;
      AcquaintanceApprovalSheet.DepartmentName = AcquaintanceApprovalSheet.Department.Name;
      var calledFromDocument = sourceDocument != null;
      var selectedVersionNumber = AcquaintanceApprovalSheet.DocumentVersion;
      
      var versionNumber = sourceTask.AcquaintanceVersions.First(v => v.IsMainDocument == true).Number.Value;
      
      var tasks = new List<Sungero.RecordManagement.IAcquaintanceTask>();
      
      if (calledFromDocument)
      {
        // Получить задачи на ознакомление по документу.
        tasks = Sungero.Docflow.PublicFunctions.OfficialDocument.Remote.GetAcquaintanceTasks(sourceDocument);
        
        // Фильтр по номеру версии.
        tasks = tasks
          .Where(t => t.AcquaintanceVersions.First(v => v.IsMainDocument == true).Number == versionNumber)
          .ToList();
      }
      else
      {
        tasks.Add(sourceTask);
        versionNumber = GetDocumentVersion(sourceTask);
        sourceDocument = sourceTask.DocumentGroup.OfficialDocuments.First();
      }
      
      // Провалидировать подписи версии.
      Sungero.Domain.Shared.IEntity version = null;
      if (versionNumber > 0 && sourceDocument.Versions.Any(v => v.Number == versionNumber))
        version = sourceDocument.Versions.First(v => v.Number == versionNumber).ElectronicDocument;
      var validationMessages = Sungero.RecordManagement.PublicFunctions.Module.GetDocumentSignatureValidationErrors(version, true);
      if (validationMessages.Any())
      {
        validationMessages.Insert(0, Sungero.RecordManagement.Resources.SignatureValidationErrorMessage);
        AcquaintanceApprovalSheet.SignValidationErrors = string.Join(System.Environment.NewLine, validationMessages);
      }
      
      // Шапка.
      var nonBreakingSpace = Convert.ToChar(160);
      AcquaintanceApprovalSheet.DocumentHyperlink = Hyperlinks.Get(sourceDocument);
      AcquaintanceApprovalSheet.DocumentName = Sungero.Docflow.PublicFunctions.Module.FormatDocumentNameForReport(sourceDocument, versionNumber, true);
      
      // Приложения.
      var documentAddenda = GetAcquintanceTaskAddendas(tasks);
      if (documentAddenda.Any())
      {
        AcquaintanceApprovalSheet.AddendaDescription = Reports.Resources.AcquaintanceApprovalSheet.Addendas;
        foreach (var addendum in documentAddenda)
        {
          var addendumInfo = string.Format("\n - {0} ({1}:{2}{3}).", addendum.DisplayValue.Trim(),
                                           Sungero.Docflow.Resources.Id, nonBreakingSpace, addendum.Id);
          AcquaintanceApprovalSheet.AddendaDescription += addendumInfo;
        }
      }
      
      // Данные.
      var reportSessionId = System.Guid.NewGuid().ToString();
      AcquaintanceApprovalSheet.ReportSessionId = reportSessionId;
      var dataTable = new List<Structures.AcquaintanceApprovalSheet.TableLine>();
      var department = AcquaintanceApprovalSheet.Department;
      
      foreach (var task in tasks)
      {
        AcquaintanceApprovalSheet.Author = task.Author?.Name;
        if (Sungero.Company.Employees.Is(task.Author))
         AcquaintanceApprovalSheet.AuthorJobTitle = Sungero.Company.Employees.As(task.Author).JobTitle?.Name; 
        var createdDate = Sungero.Docflow.PublicFunctions.Module.ToShortDateShortTime(task.Created.Value.ToUserTime());
        var taskId = task.Id;
        var taskHyperlink = Hyperlinks.Get(task);
        var isElectronicAcquaintance = task.IsElectronicAcquaintance == true;
        var taskDisplayName = isElectronicAcquaintance
          ? Reports.Resources.AcquaintanceApprovalSheet.ElectronicAcquaintanceTaskDisplayNameFormat(createdDate)
          : Reports.Resources.AcquaintanceApprovalSheet.SelfSignAcquaintanceTaskDisplayNameFormat(createdDate);
        
        // Фильтрация сотрудников по подразделениям.
        var acquainters = GetEmployeesFromParticipants(task);
        if (AcquaintanceApprovalSheet.Department != null)
          acquainters = AcquaintanceApprovalSheet.IncludeSubDepartments == true
            ? acquainters.Where(x => x.IncludedIn(AcquaintanceApprovalSheet.Department))
            : acquainters.Where(x => Equals(x.Department, AcquaintanceApprovalSheet.Department));
        
        foreach (var employee in acquainters)
        {
          // Задание.
          var assignment = Sungero.RecordManagement.AcquaintanceAssignments.GetAll()
            .Where(a => Equals(a.Task, task) &&
                   Equals(a.Performer, employee) &&
                   a.Created >= task.Started)
            .FirstOrDefault();
          
          // Не включать сотрудника в отчёт, если его задание было снято.
          var isAborted = assignment == null ? false : assignment.Status == Sungero.Workflow.Assignment.Status.Aborted;
          if (isAborted)
            continue;
          
          var newLine = Structures.AcquaintanceApprovalSheet.TableLine.Create();
          newLine.RowNumber = 0;
          newLine.ReportSessionId = reportSessionId;
          
          // Задача.
          newLine.TaskDisplayName = taskDisplayName;
          newLine.TaskId = taskId;
          newLine.TaskHyperlink = taskHyperlink;
          
          // Сотрудник.
          newLine.ShortName = employee.Person.ShortName;
          newLine.LastName = employee.Person.LastName;
          if (employee.JobTitle != null)
            newLine.JobTitle = employee.JobTitle.DisplayValue;
          newLine.Department = employee.Department.DisplayValue;
          
          if (task.Status != Sungero.Workflow.Task.Status.InProcess &&
              task.Status != Sungero.Workflow.Task.Status.Suspended &&
              task.Status != Sungero.Workflow.Task.Status.Completed)
          {
            if (employee.Status != Sungero.Company.Employee.Status.Closed)
              dataTable.Add(newLine);
            continue;
          }
          
          if (assignment == null)
          {
            if (employee.Status != Sungero.Company.Employee.Status.Closed)
              dataTable.Add(newLine);
            continue;
          }
          
          newLine.AssignmentId = assignment.Id.ToString();
          if (task.IsElectronicAcquaintance == true)
            newLine.AssignmentHyperlink = string.Format("{0};{1}", newLine.ShortName, Hyperlinks.Get(sourceDocument));
          
          var isCompleted = assignment.Status == Sungero.Workflow.Task.Status.Completed;
          if (isCompleted)
          {
            // Дата ознакомления.
            var completed = Calendar.ToUserTime(assignment.Completed.Value);
            newLine.AcquaintanceDate = Sungero.Docflow.PublicFunctions.Module.ToShortDateShortTime(completed);
            
            // Примечание.
            if (!Equals(assignment.CompletedBy, assignment.Performer))
            {
              var completedByShortName = Sungero.Company.Employees.Is(assignment.CompletedBy)
                ? Sungero.Company.Employees.As(assignment.CompletedBy).Person.ShortName
                : assignment.CompletedBy.Name;
              newLine.Note += string.Format("{0}\n", completedByShortName);
              newLine.Note += string.Format("\"{0}\"", assignment.ActiveText);
            }
            else if (!Equals(assignment.ActiveText, Sungero.RecordManagement.Reports.Resources.AcquaintanceReport.AcquaintedDefaultResult.ToString()))
            {
              newLine.Note += string.Format("\"{0}\"", assignment.ActiveText);
            }
          }
          
          // Статус.
          newLine.State = GetAcquaintanceAssignmentState(assignment, isElectronicAcquaintance, isCompleted);
          
          dataTable.Add(newLine);
        }
      }
      
      Sungero.Docflow.PublicFunctions.Module.WriteStructuresToTable(Constants.AcquaintanceApprovalSheet.SourceTableName, dataTable);
      
      // Подвал.
      var currentUser = Users.Current;
      var printedByName = Sungero.Company.Employees.Is(currentUser)
        ? Sungero.Company.Employees.As(currentUser).Person.ShortName
        : currentUser.Name;
      AcquaintanceApprovalSheet.Printed = Reports.Resources.AcquaintanceApprovalSheet.PrintedByFormat(printedByName, Calendar.UserNow);
    }
    #region private
    private int GetDocumentVersion(Sungero.RecordManagement.IAcquaintanceTask task)
    {
      // Вернуть номер версии только если у документа есть версии, и статус задачи не "Черновик", иначе - 0.
      var acquaintanceVersion = task.AcquaintanceVersions.FirstOrDefault(v => v.IsMainDocument == true);
      if (acquaintanceVersion != null &&
          (task.Status == Sungero.Workflow.Task.Status.InProcess ||
           task.Status == Sungero.Workflow.Task.Status.Suspended ||
           task.Status == Sungero.Workflow.Task.Status.Completed ||
           task.Status == Sungero.Workflow.Task.Status.Aborted))
        return acquaintanceVersion.Number.Value;
      
      var document = task.DocumentGroup.OfficialDocuments.First();
      return document.HasVersions ? document.LastVersion.Number.Value : 0;
    }
    private List<Sungero.Content.IElectronicDocument> GetAcquintanceTaskAddendas(List<Sungero.RecordManagement.IAcquaintanceTask> tasks)
    {
      var addenda = new List<Sungero.Content.IElectronicDocument>();
      var addendaIds = tasks.SelectMany(x => x.AcquaintanceVersions)
        .Where(x => x.IsMainDocument != true)
        .Select(x => x.DocumentId);
      
      var documentAddenda = tasks.SelectMany(x => x.AddendaGroup.OfficialDocuments)
        .Where(x => addendaIds.Contains(x.Id))
        .Distinct()
        .ToList();
      addenda.AddRange(documentAddenda);

      return addenda;
    }
    private static IEnumerable<Sungero.Company.IEmployee> GetEmployeesFromParticipants(Sungero.RecordManagement.IAcquaintanceTask task)
    {
      // Заполнение AcquaintanceTaskParticipants происходит в схеме.
      // От старта задачи до начала обработки схемы там ничего не будет - взять из исполнителей задачи.
      var storedParticipants = Sungero.RecordManagement.AcquaintanceTaskParticipants.GetAll().FirstOrDefault(x => x.TaskId == task.Id);
      if (storedParticipants != null)
        return storedParticipants.Employees.Select(p => p.Employee).ToList();
      
      return Sungero.RecordManagement.PublicFunctions.AcquaintanceTask.Remote.GetParticipants(task);
    }
    private string GetAcquaintanceAssignmentState(Sungero.RecordManagement.IAcquaintanceAssignment assignment,
                                                         bool isElectronicAcquaintance,
                                                         bool isCompleted)
    {
      if (!isCompleted)
        return string.Empty;
      
      if (Equals(assignment.CompletedBy, assignment.Performer) || !isElectronicAcquaintance)
        return Sungero.RecordManagement.Reports.Resources.AcquaintanceReport.AcquaintedState;

      return Sungero.RecordManagement.Reports.Resources.AcquaintanceReport.CompletedState;
    }
    #endregion
  }
}