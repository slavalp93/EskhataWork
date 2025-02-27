using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.Eskhata.Module.RecordManagementUI.Server
{
  partial class OnRegisterRegulatoryDocumentslitikoFolderHandlers
  {

    public virtual bool IsOnRegisterRegulatoryDocumentslitikoVisible()
    {
      return Sungero.Docflow.PublicFunctions.Module.Remote.IncludedInClerksRole();
    }

    public virtual IQueryable<Sungero.Workflow.IAssignmentBase> OnRegisterRegulatoryDocumentslitikoDataQuery(IQueryable<Sungero.Workflow.IAssignmentBase> query)
    {
      // Фильтр по типу.
      query = Functions.Module.GetSpecificAssignmentsWithCollapsed(query, Sungero.Docflow.ApprovalReviewAssignmentCollapsedStagesTypesRe.StageType.Register);
      
      // Запрос непрочитанных без фильтра.
      if (_filter == null)
        query = Sungero.RecordManagement.PublicFunctions.Module.ApplyCommonSubfolderFilters(query);
      
      // Фильтры по статусу, замещению и периоду.
      if (_filter != null)
        query = Sungero.RecordManagement.PublicFunctions.Module.ApplyCommonSubfolderFilters(query, _filter.InProcesslitiko,
                                                                                   _filter.Last30Dayslitiko, _filter.Last90Dayslitiko, _filter.Last180Dayslitiko, false);
      // Фильтр по типу вложенного документа в задание.
      //query = query.Where(x => x.AttachmentDetails.Any(a => a.GroupId == Guid.Parse("08e1ef90-521f-41a1-a13f-c6f175007e54") && a.AttachmentTypeGuid == Guid.Parse("9151081e-29d5-4c68-9204-68f04ff4d7e5")));
      List<long> assignmentIDs = new List<long>();
      foreach (var x in query)
      {
        if ((Sungero.Docflow.ApprovalSigningAssignments.Is(x) && litiko.RegulatoryDocuments.RegulatoryDocuments.Is(Sungero.Docflow.ApprovalSigningAssignments.As(x).DocumentGroup.OfficialDocuments.FirstOrDefault())) ||
            (Sungero.Docflow.ApprovalRegistrationAssignments.Is(x) && litiko.RegulatoryDocuments.RegulatoryDocuments.Is(Sungero.Docflow.ApprovalRegistrationAssignments.As(x).DocumentGroup.OfficialDocuments.FirstOrDefault())) ||
            (Sungero.Docflow.ApprovalReviewAssignments.Is(x) && litiko.RegulatoryDocuments.RegulatoryDocuments.Is(Sungero.Docflow.ApprovalReviewAssignments.As(x).DocumentGroup.OfficialDocuments.FirstOrDefault())) ||
            (Sungero.Docflow.ApprovalExecutionAssignments.Is(x) && litiko.RegulatoryDocuments.RegulatoryDocuments.Is(Sungero.Docflow.ApprovalExecutionAssignments.As(x).DocumentGroup.OfficialDocuments.FirstOrDefault())))
            assignmentIDs.Add(x.Id);
      }
      query = assignmentIDs.Count > 0 ? query.Where(x => assignmentIDs.Contains(x.Id)) : query.Where(x => x.Id < 0);
      
      return query;
    }
  }

  partial class OnRegisterOutgoingLetterslitikoFolderHandlers
  {

    public virtual bool IsOnRegisterOutgoingLetterslitikoVisible()
    {
      return Sungero.Docflow.PublicFunctions.Module.Remote.IncludedInClerksRole();
    }

    public virtual IQueryable<Sungero.Workflow.IAssignmentBase> OnRegisterOutgoingLetterslitikoDataQuery(IQueryable<Sungero.Workflow.IAssignmentBase> query)
    {
      // Фильтр по типу.
      query = Functions.Module.GetSpecificAssignmentsWithCollapsed(query, Sungero.Docflow.ApprovalReviewAssignmentCollapsedStagesTypesRe.StageType.Register);
      
      // Запрос непрочитанных без фильтра.
      if (_filter == null)
        query = Sungero.RecordManagement.PublicFunctions.Module.ApplyCommonSubfolderFilters(query);
      
      // Фильтры по статусу, замещению и периоду.
      if (_filter != null)
        query = Sungero.RecordManagement.PublicFunctions.Module.ApplyCommonSubfolderFilters(query, _filter.InProcesslitiko,
                                                                                   _filter.Last30Dayslitiko, _filter.Last90Dayslitiko, _filter.Last180Dayslitiko, false);
      // Фильтр по типу вложенного документа в задание.
      //query = query.Where(x => x.AttachmentDetails.Any(a => a.GroupId == Guid.Parse("08e1ef90-521f-41a1-a13f-c6f175007e54") && a.AttachmentTypeGuid == Sungero.RecordManagement.Server.OutgoingLetter.ClassTypeGuid ));
      List<long> assignmentIDs = new List<long>();
      foreach (var x in query)
      {
        if ((Sungero.Docflow.ApprovalSigningAssignments.Is(x) && Sungero.RecordManagement.OutgoingLetters.Is(Sungero.Docflow.ApprovalSigningAssignments.As(x).DocumentGroup.OfficialDocuments.FirstOrDefault())) ||
            (Sungero.Docflow.ApprovalRegistrationAssignments.Is(x) && Sungero.RecordManagement.OutgoingLetters.Is(Sungero.Docflow.ApprovalRegistrationAssignments.As(x).DocumentGroup.OfficialDocuments.FirstOrDefault())) ||
            (Sungero.Docflow.ApprovalReviewAssignments.Is(x) && Sungero.RecordManagement.OutgoingLetters.Is(Sungero.Docflow.ApprovalReviewAssignments.As(x).DocumentGroup.OfficialDocuments.FirstOrDefault())) ||
            (Sungero.Docflow.ApprovalExecutionAssignments.Is(x) && Sungero.RecordManagement.OutgoingLetters.Is(Sungero.Docflow.ApprovalExecutionAssignments.As(x).DocumentGroup.OfficialDocuments.FirstOrDefault())))
            assignmentIDs.Add(x.Id);
      }
      query = assignmentIDs.Count > 0 ? query.Where(x => assignmentIDs.Contains(x.Id)) : query.Where(x => x.Id < 0);
      
      return query;
    }
  }

  partial class OnRegisterIncomingLetterslitikoFolderHandlers
  {

    public virtual bool IsOnRegisterIncomingLetterslitikoVisible()
    {
      return Sungero.Docflow.PublicFunctions.Module.Remote.IncludedInClerksRole();
    }

    public virtual IQueryable<Sungero.Workflow.IAssignmentBase> OnRegisterIncomingLetterslitikoDataQuery(IQueryable<Sungero.Workflow.IAssignmentBase> query)
    {
      // Фильтр по типу.
      query = Functions.Module.GetSpecificAssignmentsWithCollapsed(query, Sungero.Docflow.ApprovalReviewAssignmentCollapsedStagesTypesRe.StageType.Register);
      
      // Запрос непрочитанных без фильтра.
      if (_filter == null)
        query = Sungero.RecordManagement.PublicFunctions.Module.ApplyCommonSubfolderFilters(query);

      // Фильтры по статусу, замещению и периоду.
      if (_filter != null)
        query = Sungero.RecordManagement.PublicFunctions.Module.ApplyCommonSubfolderFilters(query, _filter.InProcesslitiko,
                                                                                   _filter.Last30Dayslitiko, _filter.Last90Dayslitiko, _filter.Last180Dayslitiko, false);
      
      // Фильтр по типу вложенного документа в задание.
      // query = query.Where(x => x.AttachmentDetails.Any(a => a.GroupId == Guid.Parse("08e1ef90-521f-41a1-a13f-c6f175007e54") && a.AttachmentTypeGuid == Sungero.RecordManagement.Server.IncomingLetter.ClassTypeGuid ));
      List<long> assignmentIDs = new List<long>();
      foreach (var x in query)
      {
        if ((Sungero.Docflow.ApprovalSigningAssignments.Is(x) && Sungero.RecordManagement.IncomingLetters.Is(Sungero.Docflow.ApprovalSigningAssignments.As(x).DocumentGroup.OfficialDocuments.FirstOrDefault())) ||
            (Sungero.Docflow.ApprovalRegistrationAssignments.Is(x) && Sungero.RecordManagement.IncomingLetters.Is(Sungero.Docflow.ApprovalRegistrationAssignments.As(x).DocumentGroup.OfficialDocuments.FirstOrDefault())) ||
            (Sungero.Docflow.ApprovalReviewAssignments.Is(x) && Sungero.RecordManagement.IncomingLetters.Is(Sungero.Docflow.ApprovalReviewAssignments.As(x).DocumentGroup.OfficialDocuments.FirstOrDefault())) ||
            (Sungero.Docflow.ApprovalExecutionAssignments.Is(x) && Sungero.RecordManagement.IncomingLetters.Is(Sungero.Docflow.ApprovalExecutionAssignments.As(x).DocumentGroup.OfficialDocuments.FirstOrDefault())))
            assignmentIDs.Add(x.Id);
      }
      query = assignmentIDs.Count > 0 ? query.Where(x => assignmentIDs.Contains(x.Id)) : query.Where(x => x.Id < 0);
      
      return query;
    }
  }

  partial class OnRegisterOrdersAndCompanyDirectiveslitikoFolderHandlers
  {

    public virtual bool IsOnRegisterOrdersAndCompanyDirectiveslitikoVisible()
    {
      return Sungero.Docflow.PublicFunctions.Module.Remote.IncludedInClerksRole();
    }

    public virtual IQueryable<Sungero.Workflow.IAssignmentBase> OnRegisterOrdersAndCompanyDirectiveslitikoDataQuery(IQueryable<Sungero.Workflow.IAssignmentBase> query)
    {
      // Фильтр по типу.
      query = Functions.Module.GetSpecificAssignmentsWithCollapsed(query, Sungero.Docflow.ApprovalReviewAssignmentCollapsedStagesTypesRe.StageType.Register);
      
      // Запрос непрочитанных без фильтра.
      if (_filter == null)
        query = Sungero.RecordManagement.PublicFunctions.Module.ApplyCommonSubfolderFilters(query);
      
      // Фильтры по статусу, замещению и периоду.
      if (_filter == null)
        query = Sungero.RecordManagement.PublicFunctions.Module.ApplyCommonSubfolderFilters(query, _filter.InProcesslitiko,
                                                                                   _filter.Last30Dayslitiko, _filter.Last90Dayslitiko, _filter.Last180Dayslitiko, false);
      
      // Фильтр по типу вложенного документа в задание.
      //query = query.Where(x => x.AttachmentDetails.Any(a => a.GroupId == Guid.Parse("08e1ef90-521f-41a1-a13f-c6f175007e54") && a.AttachmentTypeGuid == Sungero.RecordManagement.Server.Order.ClassTypeGuid ));
      List<long> assignmentIDs = new List<long>();
      foreach (var x in query)
      {
        //var attachmentDetails = x.AttachmentDetails;
        //var containsGroup = x.AttachmentDetails.Any(a => a.GroupId == Guid.Parse("08e1ef90-521f-41a1-a13f-c6f175007e54"));
        //var containsAttachmentType = x.AttachmentDetails.Any(a => a.AttachmentTypeGuid == Sungero.RecordManagement.Server.Order.ClassTypeGuid);
        //var singingAssignment = litiko.Eskhata.ApprovalSigningAssignments.Is(x);
        //var taskAttachment = litiko.Eskhata.ApprovalSigningAssignments.As(x).DocumentGroup.OfficialDocuments.FirstOrDefault();
        //var isOrder = litiko.Eskhata.Orders.Is(taskAttachment);
        
        if ((Sungero.Docflow.ApprovalSigningAssignments.Is(x) && Sungero.RecordManagement.Orders.Is(Sungero.Docflow.ApprovalSigningAssignments.As(x).DocumentGroup.OfficialDocuments.FirstOrDefault())) ||
            (Sungero.Docflow.ApprovalRegistrationAssignments.Is(x) && Sungero.RecordManagement.Orders.Is(Sungero.Docflow.ApprovalRegistrationAssignments.As(x).DocumentGroup.OfficialDocuments.FirstOrDefault())) ||
            (Sungero.Docflow.ApprovalReviewAssignments.Is(x) && Sungero.RecordManagement.Orders.Is(Sungero.Docflow.ApprovalReviewAssignments.As(x).DocumentGroup.OfficialDocuments.FirstOrDefault())) ||
            (Sungero.Docflow.ApprovalExecutionAssignments.Is(x) && Sungero.RecordManagement.Orders.Is(Sungero.Docflow.ApprovalExecutionAssignments.As(x).DocumentGroup.OfficialDocuments.FirstOrDefault())) ||
            (Sungero.Docflow.ApprovalSigningAssignments.Is(x) && Sungero.RecordManagement.CompanyDirectives.Is(Sungero.Docflow.ApprovalSigningAssignments.As(x).DocumentGroup.OfficialDocuments.FirstOrDefault())) ||
            (Sungero.Docflow.ApprovalRegistrationAssignments.Is(x) && Sungero.RecordManagement.CompanyDirectives.Is(Sungero.Docflow.ApprovalRegistrationAssignments.As(x).DocumentGroup.OfficialDocuments.FirstOrDefault())) ||
            (Sungero.Docflow.ApprovalReviewAssignments.Is(x) && Sungero.RecordManagement.CompanyDirectives.Is(Sungero.Docflow.ApprovalReviewAssignments.As(x).DocumentGroup.OfficialDocuments.FirstOrDefault())) ||
            (Sungero.Docflow.ApprovalExecutionAssignments.Is(x) && Sungero.RecordManagement.CompanyDirectives.Is(Sungero.Docflow.ApprovalExecutionAssignments.As(x).DocumentGroup.OfficialDocuments.FirstOrDefault())))
            assignmentIDs.Add(x.Id);
      }                                    
      query = assignmentIDs.Count > 0 ? query.Where(x => assignmentIDs.Contains(x.Id)) : query.Where(x => x.Id < 0);
      
      return query;
    }
  }

  partial class RecordManagementUIHandlers
  {
  }
}