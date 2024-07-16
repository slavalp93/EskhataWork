using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.DocflowEskhata.UnitManagerApprovalRole;

namespace litiko.DocflowEskhata.Server
{
  partial class UnitManagerApprovalRoleFunctions
  {
    public override Sungero.Company.IEmployee GetRolePerformer(Sungero.Docflow.IApprovalTask task)
    {
      if (_obj.Type != DocflowEskhata.UnitManagerApprovalRole.Type.UnitManager)
        return base.GetRolePerformer(task);
      
      var document = task.DocumentGroup.OfficialDocuments.FirstOrDefault();
      if (document?.Department?.HeadOffice == null)
        return null;
      
      var department = document.Department;
      
      while (department.HeadOffice.HeadOffice != null)
      {
        department = department.HeadOffice;
      }

      return department?.Manager;
    }
  }
}