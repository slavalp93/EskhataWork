using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.ContractsEskhata.ApprovalRole;

namespace litiko.ContractsEskhata.Shared
{
  partial class ApprovalRoleFunctions
  {
    public override List<Sungero.Docflow.IDocumentKind> Filter(List<Sungero.Docflow.IDocumentKind> kinds)
    {
      var query = base.Filter(kinds);      
      
      #region Ответственный юрист
      if (_obj.Type == litiko.ContractsEskhata.ApprovalRole.Type.RespLawyer)
      {
        query = query.Where(k => k.DocumentType.DocumentTypeGuid == Sungero.Contracts.PublicConstants.Module.ContractGuid.ToString()).ToList();
      }
      #endregion
      
      #region Ответственный бухгалтер
      if (_obj.Type == litiko.ContractsEskhata.ApprovalRole.Type.RespAccountant)
      {
        query = query.Where(k => k.DocumentType.DocumentTypeGuid == Sungero.Contracts.PublicConstants.Module.ContractGuid.ToString() ||
                                 k.DocumentType.DocumentTypeGuid == Sungero.Contracts.PublicConstants.Module.SupAgreementGuid.ToString() ||
                                 k.DocumentType.DocumentTypeGuid == Constants.Module.DocumentTypeGuids.IncomingInvoice.ToString() ||
                                 k.DocumentType.DocumentTypeGuid == Constants.Module.DocumentTypeGuids.ContractStatement.ToString()
                           ).ToList();
      }
      #endregion

      #region Ответственный сотрудник АХД
      if (_obj.Type == litiko.ContractsEskhata.ApprovalRole.Type.RespAHD)
      {
        query = query.Where(k => k.DocumentType.DocumentTypeGuid == Sungero.Contracts.PublicConstants.Module.ContractGuid.ToString()).ToList();
      }
      #endregion      
      
      return query;
    }
  }
}