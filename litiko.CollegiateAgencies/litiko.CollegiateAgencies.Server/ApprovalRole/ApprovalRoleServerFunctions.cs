using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.CollegiateAgencies.ApprovalRole;

namespace litiko.CollegiateAgencies.Server
{
  partial class ApprovalRoleFunctions
  {
    public override Sungero.Company.IEmployee GetRolePerformer(Sungero.Docflow.IApprovalTask task)
    {
        if (_obj.Type == litiko.CollegiateAgencies.ApprovalRole.Type.Speaker)
        {
          var document = task.DocumentGroup.OfficialDocuments.FirstOrDefault();
          var projectSolutuionDoc = Projectsolutions.As(document);
          if (projectSolutuionDoc != null && projectSolutuionDoc.Speaker != null)
            return projectSolutuionDoc.Speaker;
            
          return null;
        }        
        
        if (_obj.Type == litiko.CollegiateAgencies.ApprovalRole.Type.SecretaryByCat)
        {
          var document = task.DocumentGroup.OfficialDocuments.FirstOrDefault();
          var projectSolutuionDoc = Projectsolutions.As(document);
          if (projectSolutuionDoc != null && projectSolutuionDoc.MeetingCategory != null)
            return projectSolutuionDoc.MeetingCategory.Secretary;
            
          return null;
        }
        
        return base.GetRolePerformer(task);
    }
  }
}