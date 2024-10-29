using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Shared;
using Sungero.Metadata;

namespace litiko.RegulatoryDocuments.Server
{
  public class ModuleFunctions
  {
    /// <summary>
    /// Получить данные для отчета по согласованию ВНД
    /// </summary>
    /// <param name="document">Документ</param>
    /// <param name="reportSessionId">ИД сессии</param>
    /// <returns>Список структур IApprovalSheetLine</returns>
    [Public]
    public List<Structures.Module.IApprovalSheetLine> GetApprovalSheetIRDData(List<long> documentIds, string reportSessionId)
    {
      var dataList = new List<Structures.Module.IApprovalSheetLine>();
            
      var docGuid = litiko.RegulatoryDocuments.PublicConstants.Module.DocumentTypeGuids.RegulatoryDocument;
      var approvalTaskDocumentGroupGuid = Sungero.Docflow.PublicConstants.Module.TaskMainGroup.ApprovalTask;
      var approvalTasks = Sungero.Docflow.ApprovalTasks.GetAll()        
        .Where(t => t.AttachmentDetails
               .Any(att => att.AttachmentId.HasValue && documentIds.Contains(att.AttachmentId.Value) && att.EntityTypeGuid == docGuid &&
                    att.GroupId == approvalTaskDocumentGroupGuid));
            
      var approvalAssignments = Sungero.Workflow.Assignments.GetAll()
        .Where(x => approvalTasks.Contains(x.Task))
        .Where(x => Sungero.Docflow.ApprovalAssignments.Is(x) || Sungero.Docflow.ApprovalManagerAssignments.Is(x))
        .OrderBy(x => x.Created);
      
      int number = 0;      
      foreach (var assignment in approvalAssignments)
      {
        number++;
        var reportLine = Structures.Module.ApprovalSheetLine.Create();
        reportLine.Number = number;
        var document = Sungero.Docflow.ApprovalTasks.As(assignment.Task).DocumentGroup.OfficialDocuments.FirstOrDefault();
        if (document != null)
          reportLine.DocId = document.Id;
        reportLine.TaskId = assignment.Task.Id;
        reportLine.AssignmentId = assignment.Id;
        reportLine.TaskAuthor = assignment.Task.Author.Name;
        reportLine.Performer = Equals(assignment.Performer, assignment.CompletedBy) ? assignment.Performer.Name : string.Format("{0} за {1}", assignment.CompletedBy.Name, assignment.Performer.Name);
        reportLine.Department = Equals(assignment.Performer, assignment.CompletedBy) ? Sungero.Company.Employees.As(assignment.Performer).Department.Name : Sungero.Company.Employees.As(assignment.CompletedBy).Department.Name;
        reportLine.Created = assignment.Created;
        reportLine.Complated = assignment.Completed;
        
        string performResult = string.Empty;
        if (litiko.Eskhata.ApprovalAssignments.Is(assignment))
        {
          var appAssignment = litiko.Eskhata.ApprovalAssignments.As(assignment);
          if (appAssignment.Result == litiko.Eskhata.ApprovalAssignment.Result.Approved)
          {
            if (appAssignment.IsNotAgreeResultlitiko.GetValueOrDefault())
              performResult = litiko.Eskhata.ApprovalAssignments.Resources.NotAgreeResult;
            else
              performResult = litiko.Eskhata.ApprovalTasks.Resources.StateViewEndorsed;
          }
          
          if (appAssignment.Result == litiko.Eskhata.ApprovalAssignment.Result.WithSuggestions)          
            performResult = litiko.Eskhata.ApprovalTasks.Resources.StateViewEndorsedWithSuggestions;
          
          if (appAssignment.Result == litiko.Eskhata.ApprovalAssignment.Result.ForRevision)          
            performResult = litiko.Eskhata.ApprovalTasks.Resources.InReportForRework;          
            
        }
        else if (Sungero.Docflow.ApprovalManagerAssignments.Is(assignment))
        {
          var appAssignment = Sungero.Docflow.ApprovalManagerAssignments.As(assignment);
          if (appAssignment.Result == Sungero.Docflow.ApprovalManagerAssignment.Result.Approved)
            performResult = litiko.Eskhata.ApprovalTasks.Resources.StateViewEndorsed;

          if (appAssignment.Result == Sungero.Docflow.ApprovalManagerAssignment.Result.WithSuggestions)
            performResult = litiko.Eskhata.ApprovalTasks.Resources.StateViewEndorsedWithSuggestions;
          
          if (appAssignment.Result == Sungero.Docflow.ApprovalManagerAssignment.Result.ForRevision)          
            performResult = litiko.Eskhata.ApprovalTasks.Resources.InReportForRework;           
        }
        
        reportLine.Result = performResult;
        reportLine.Comment = assignment.ActiveText == Sungero.Docflow.ApprovalTasks.Resources.Endorsed ? string.Empty : assignment.ActiveText;
        reportLine.ReportSessionId = reportSessionId;
        
        dataList.Add(reportLine);        
      }
      
      return dataList;
    }
  }
}