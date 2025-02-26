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
      var result = query;
      result = Functions.Module.GetSpecificAssignmentsWithCollapsed(result, Sungero.Docflow.ApprovalReviewAssignmentCollapsedStagesTypesRe.StageType.Register);
      
      // Запрос непрочитанных без фильтра.
      if (_filter == null)
        return Sungero.RecordManagement.PublicFunctions.Module.ApplyCommonSubfolderFilters(result);
      
      // Фильтры по статусу, замещению и периоду.
      result = Sungero.RecordManagement.PublicFunctions.Module.ApplyCommonSubfolderFilters(result, _filter.InProcesslitiko,
                                                                                   _filter.Last30Dayslitiko, _filter.Last90Dayslitiko, _filter.Last180Dayslitiko, false);
      // Фильтр по типу вложенного документа в задание.
      result = result.Where(x => x.AttachmentDetails.Any(a => a.GroupId == Guid.Parse("08e1ef90-521f-41a1-a13f-c6f175007e54") && a.AttachmentTypeGuid == Guid.Parse("9151081e-29d5-4c68-9204-68f04ff4d7e5")));
      
      return result;
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
      var result = query;
      result = Functions.Module.GetSpecificAssignmentsWithCollapsed(result, Sungero.Docflow.ApprovalReviewAssignmentCollapsedStagesTypesRe.StageType.Register);
      
      // Запрос непрочитанных без фильтра.
      if (_filter == null)
        return Sungero.RecordManagement.PublicFunctions.Module.ApplyCommonSubfolderFilters(result);
      
      // Фильтры по статусу, замещению и периоду.
      result = Sungero.RecordManagement.PublicFunctions.Module.ApplyCommonSubfolderFilters(result, _filter.InProcesslitiko,
                                                                                   _filter.Last30Dayslitiko, _filter.Last90Dayslitiko, _filter.Last180Dayslitiko, false);
      // Фильтр по типу вложенного документа в задание.
      result = result.Where(x => x.AttachmentDetails.Any(a => a.GroupId == Guid.Parse("08e1ef90-521f-41a1-a13f-c6f175007e54") && a.AttachmentTypeGuid == Sungero.RecordManagement.Server.OutgoingLetter.ClassTypeGuid ));
      
      return result;
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
      var result = query;
      result = Functions.Module.GetSpecificAssignmentsWithCollapsed(result, Sungero.Docflow.ApprovalReviewAssignmentCollapsedStagesTypesRe.StageType.Register);
      
      // Запрос непрочитанных без фильтра.
      if (_filter == null)
        return Sungero.RecordManagement.PublicFunctions.Module.ApplyCommonSubfolderFilters(result);
      
      // Фильтры по статусу, замещению и периоду.
      result = Sungero.RecordManagement.PublicFunctions.Module.ApplyCommonSubfolderFilters(result, _filter.InProcesslitiko,
                                                                                   _filter.Last30Dayslitiko, _filter.Last90Dayslitiko, _filter.Last180Dayslitiko, false);
      // Фильтр по типу вложенного документа в задание.
      result = result.Where(x => x.AttachmentDetails.Any(a => a.GroupId == Guid.Parse("08e1ef90-521f-41a1-a13f-c6f175007e54") && a.AttachmentTypeGuid == Sungero.RecordManagement.Server.IncomingLetter.ClassTypeGuid ));
      
      return result;
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
      var result = query;
      result = Functions.Module.GetSpecificAssignmentsWithCollapsed(result, Sungero.Docflow.ApprovalReviewAssignmentCollapsedStagesTypesRe.StageType.Register);
      
      // Запрос непрочитанных без фильтра.
      if (_filter == null)
        return Sungero.RecordManagement.PublicFunctions.Module.ApplyCommonSubfolderFilters(result);
      
      // Фильтры по статусу, замещению и периоду.
      result = Sungero.RecordManagement.PublicFunctions.Module.ApplyCommonSubfolderFilters(result, _filter.InProcesslitiko,
                                                                                   _filter.Last30Dayslitiko, _filter.Last90Dayslitiko, _filter.Last180Dayslitiko, false);
      // Фильтр по типу вложенного документа в задание.
      result = result.Where(x => x.AttachmentDetails.Any(a => a.GroupId == Guid.Parse("08e1ef90-521f-41a1-a13f-c6f175007e54") &&
                                                    (a.AttachmentTypeGuid ==  Sungero.RecordManagement.Server.Order.ClassTypeGuid || a.AttachmentTypeGuid ==  Sungero.RecordManagement.Server.CompanyDirective.ClassTypeGuid)));
      
      return result;
    }
  }

  partial class RecordManagementUIHandlers
  {
  }
}