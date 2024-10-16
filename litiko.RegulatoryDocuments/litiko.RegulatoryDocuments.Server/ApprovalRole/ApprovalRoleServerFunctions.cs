using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.RegulatoryDocuments.ApprovalRole;

namespace litiko.RegulatoryDocuments.Server
{
  partial class ApprovalRoleFunctions
  {
    public override Sungero.Company.IEmployee GetRolePerformer(Sungero.Docflow.IApprovalTask task)
    {
      var document = task.DocumentGroup.OfficialDocuments.FirstOrDefault();
      if (document == null)
        return null;
      
      #region Руководитель процесса 
      if (_obj.Type == litiko.RegulatoryDocuments.ApprovalRole.Type.ProcessManager)
      {        
        var regulatoryDocument = RegulatoryDocuments.As(document);
        if (regulatoryDocument != null)
          return regulatoryDocument.ProcessManager;
            
        return null;
      }        
      #endregion
        
      return base.GetRolePerformer(task);
    }
  }
}