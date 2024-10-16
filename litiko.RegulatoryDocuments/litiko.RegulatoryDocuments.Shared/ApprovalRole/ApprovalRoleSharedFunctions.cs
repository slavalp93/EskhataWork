using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.RegulatoryDocuments.ApprovalRole;

namespace litiko.RegulatoryDocuments.Shared
{
  partial class ApprovalRoleFunctions
  {
    public override List<Sungero.Docflow.IDocumentKind> Filter(List<Sungero.Docflow.IDocumentKind> kinds)
    {
      var query = base.Filter(kinds);
      
      #region Руководитель процесса
      if (_obj.Type == litiko.RegulatoryDocuments.ApprovalRole.Type.ProcessManager)
        query = query.Where(k => k.DocumentType.DocumentTypeGuid == PublicConstants.Module.DocumentTypeGuids.RegulatoryDocument.ToString()).ToList();
      #endregion           
      
      return query;
    }
  }
}