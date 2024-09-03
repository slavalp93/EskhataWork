using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.CollegiateAgencies.ApprovalRole;

namespace litiko.CollegiateAgencies.Shared
{
  partial class ApprovalRoleFunctions
  {
    public override List<Sungero.Docflow.IDocumentKind> Filter(List<Sungero.Docflow.IDocumentKind> kinds)
    {
      var query = base.Filter(kinds);
      
      if (_obj.Type == litiko.CollegiateAgencies.ApprovalRole.Type.Speaker)
        query = query.Where(k => k.DocumentType.DocumentTypeGuid == PublicConstants.Module.DocumentTypeGuids.ProjectSolution.ToString()).ToList();
      
      if (_obj.Type == litiko.CollegiateAgencies.ApprovalRole.Type.SecretaryByCat)
        query = query.Where(k => k.DocumentType.DocumentTypeGuid == PublicConstants.Module.DocumentTypeGuids.ProjectSolution.ToString()).ToList();      
      
      return query;
    }
  }
}