using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Archive.ApprovalRole;

namespace litiko.Archive.Shared
{
  partial class ApprovalRoleFunctions
  {
    public override List<Sungero.Docflow.IDocumentKind> Filter(List<Sungero.Docflow.IDocumentKind> kinds)
    {
      var query = base.Filter(kinds);
      
      if (_obj.Type == litiko.Archive.ApprovalRole.Type.Archivist)
        query = query.Where(k => k.DocumentType.DocumentTypeGuid == PublicConstants.Module.DocumentTypeGuids.ArchiveList.ToString()).ToList();
      
      return query;
    }
  }
}