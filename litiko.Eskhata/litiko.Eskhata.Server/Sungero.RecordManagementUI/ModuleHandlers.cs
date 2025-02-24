using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.Eskhata.Module.RecordManagementUI.Server
{
  partial class OrdersAndCompanyDirectiveslitikoFolderHandlers
  {

    public virtual IQueryable<Sungero.Workflow.IAssignmentBase> OrdersAndCompanyDirectiveslitikoDataQuery(IQueryable<Sungero.Workflow.IAssignmentBase> query)
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
      
      result.Where(x => x.AttachmentDetails.Any(a => a.GroupId == Guid.Parse("08e1ef90-521f-41a1-a13f-c6f175007e54") &&
                                                    (a.AttachmentTypeGuid ==  Guid.Parse("9570e517-7ab7-4f23-a959-3652715efad3") || a.AttachmentTypeGuid ==  Guid.Parse("264ada4e-b272-4ecc-a115-1246c9556bfa"))));
      
      return result;
    }
  }

  partial class RecordManagementUIHandlers
  {
  }
}