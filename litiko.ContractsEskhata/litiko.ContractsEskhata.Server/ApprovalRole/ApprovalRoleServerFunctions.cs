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
      
      var contract = litiko.Eskhata.Contracts.Null;
      if (litiko.Eskhata.Contracts.Is(document))
        contract = litiko.Eskhata.Contracts.As(document);
      else if (litiko.Eskhata.SupAgreements.Is(document))
        contract = litiko.Eskhata.Contracts.As(document.LeadingDocument);
      else if (litiko.Eskhata.IncomingInvoices.Is(document))
      {
        var incomingInvoice = litiko.Eskhata.IncomingInvoices.As(document);        
        if (litiko.Eskhata.Contracts.Is(incomingInvoice.Contract))
          contract = litiko.Eskhata.Contracts.As(incomingInvoice.Contract);
        else if (litiko.Eskhata.SupAgreements.Is(incomingInvoice.Contract))
          contract = litiko.Eskhata.Contracts.As(incomingInvoice.Contract);
      }
      else if (Sungero.FinancialArchive.ContractStatements.Is(document))
      {
        var contractStatement = Sungero.FinancialArchive.ContractStatements.As(document);        
        if (litiko.Eskhata.Contracts.Is(contractStatement.LeadingDocument))
          contract = litiko.Eskhata.Contracts.As(contractStatement.LeadingDocument);
        else if (litiko.Eskhata.SupAgreements.Is(contractStatement.LeadingDocument))
          contract = litiko.Eskhata.Contracts.As(contractStatement.LeadingDocument);
      }
      
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