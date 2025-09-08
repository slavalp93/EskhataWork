using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.ContractsEskhata.ApprovalRole;

namespace litiko.ContractsEskhata.Server
{
  partial class ApprovalRoleFunctions
  {
    public override Sungero.Company.IEmployee GetRolePerformer(Sungero.Docflow.IApprovalTask task)
    {
      var document = task.DocumentGroup.OfficialDocuments.FirstOrDefault();
      if (document == null)
        return null;
      
      var contract = litiko.Eskhata.Contracts.As(document);
      if (contract == null)
        return null;
      
      var matrix = litiko.NSI.PublicFunctions.Module.GetResponsibilityMatrix(contract);
      if (matrix == null)
        return null;
      
      #region Ответственный юрист
      if (_obj.Type == litiko.ContractsEskhata.ApprovalRole.Type.RespLawyer)     
        return Sungero.Company.Employees.As(matrix.ResponsibleLawyer);
      #endregion
      
      #region Ответственный бухгалтер
      if (_obj.Type == litiko.ContractsEskhata.ApprovalRole.Type.RespAccountant)     
        return Sungero.Company.Employees.As(matrix.ResponsibleAccountant);
      #endregion

      #region Ответственный сотрудник АХД
      if (_obj.Type == litiko.ContractsEskhata.ApprovalRole.Type.RespAHD)     
        return Sungero.Company.Employees.As(matrix.ResponsibleAHD);
      #endregion      
        
      return base.GetRolePerformer(task);
    }
  }
}