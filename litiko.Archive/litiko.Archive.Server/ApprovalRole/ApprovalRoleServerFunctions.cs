using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Archive.ApprovalRole;

namespace litiko.Archive.Server
{
  partial class ApprovalRoleFunctions
  {
    public override Sungero.Company.IEmployee GetRolePerformer(Sungero.Docflow.IApprovalTask task)
    {
        if (_obj.Type == litiko.Archive.ApprovalRole.Type.Archivist)
        {
          var document = task.DocumentGroup.OfficialDocuments.FirstOrDefault();
          var archiveListDoc = ArchiveLists.As(document);
          if (archiveListDoc != null && archiveListDoc.Archive != null)
            return archiveListDoc.Archive.Archivist;
            
          return null;
        }
          
        return base.GetRolePerformer(task);
    }
  }
}