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
      
      #region Согласование
      if (_obj.StageType == Sungero.Docflow.ApprovalStage.StageType.Approvers)
      {
        baseRoles.Add(DocflowEskhata.UnitManagerApprovalRole.Type.Signatory);
        baseRoles.Add(Archive.ApprovalRole.Type.Archivist);
        baseRoles.Add(CollegiateAgencies.ApprovalRole.Type.Speaker);
        baseRoles.Add(CollegiateAgencies.ApprovalRole.Type.SecretaryByCat);
      }
      #endregion
      
      #region Задание
      if (_obj.StageType == Sungero.Docflow.ApprovalStage.StageType.SimpleAgr)
      {
        baseRoles.Add(Archive.ApprovalRole.Type.Archivist);
        baseRoles.Add(CollegiateAgencies.ApprovalRole.Type.SecretaryByCat);
      }
      #endregion
      
      return baseRoles;
    }
  }
}