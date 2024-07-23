using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ApprovalStage;

namespace litiko.Eskhata.Shared
{
  partial class ApprovalStageFunctions
  {
    public override List<Enumeration?> GetPossibleRoles()
    {
      var baseRoles = base.GetPossibleRoles();
      baseRoles.Add(DocflowEskhata.UnitManagerApprovalRole.Type.UnitManager);
      
      if (_obj.StageType == Sungero.Docflow.ApprovalStage.StageType.Approvers)
        baseRoles.Add(DocflowEskhata.UnitManagerApprovalRole.Type.Signatory);
      
      return baseRoles;
    }
  }
}