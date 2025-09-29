using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Integration.SendDocumentToIS;

namespace litiko.Integration.Server
{
  partial class SendDocumentToISFunctions
  {
    /// <summary>
    /// Выполнить сценарий.
    /// </summary>
    /// <param name="approvalTask">Задача на согласование по регламенту.</param>
    /// <returns>Результат выполнения сценария.</returns>
    public override Sungero.Docflow.Structures.ApprovalFunctionStageBase.ExecutionResult Execute(Sungero.Docflow.IApprovalTask approvalTask)
    {
      var logPrefix = "Integration. SendDocumentToIS stage";
      Logger.DebugFormat("{0}. Start. ApprovalTask Id: {1}", logPrefix, approvalTask.Id);
      
      var result = base.Execute(approvalTask);      
      var document = approvalTask.DocumentGroup.OfficialDocuments.FirstOrDefault();
      if (document == null)
        return this.GetErrorResult("Не найден документ.");            

      string integrationMethodName = string.Empty;
                
      if (litiko.Eskhata.Contracts.Is(document))        
        integrationMethodName = Constants.Module.IntegrationMethods.R_DR_SET_CONTRACT;        
      else if (litiko.Eskhata.SupAgreements.Is(document) || litiko.Eskhata.IncomingInvoices.Is(document) || Sungero.FinancialArchive.ContractStatements.Is(document))        
        integrationMethodName = Constants.Module.IntegrationMethods.R_DR_SET_PAYMENT_DOCUMENT;        
      else
        return this.GetErrorResult("Неподдерживаемый тип документа");
        
      var integrationMethod = IntegrationMethods.GetAll().Where(x => x.Name == integrationMethodName).FirstOrDefault();
      if (integrationMethod == null)
        return this.GetErrorResult($"Протокол интеграции {integrationMethodName} не найден"); 
      
      //if (!Locks.TryLock(document))
      //  this.GetRetryResult("Документ заблокирован");            
      
      try
      {                         
        var exchDoc = Integration.ExchangeDocuments.Create();
        exchDoc.IntegrationMethod = integrationMethod;      
        exchDoc.Save();
        
        int lastId = 0;
        var errorMessage = Functions.Module.SendRequestToIS(exchDoc, lastId, document);
        if (!string.IsNullOrEmpty(errorMessage))
          //throw new AppliedCodeException(errorMessage);
        
        Logger.DebugFormat("{0}. Request to IS success. DocumentId: {1}. ExchangeDocumetId: {2}", logPrefix, document.Id, exchDoc.Id);
         
      }
      catch (Exception ex)
      {        
        Logger.ErrorFormat("{0}. Error: {1}. Stack trace: {2}", logPrefix, ex.Message, ex.StackTrace);
        
        // !!! Количество повторов ? !!!
        result = this.GetRetryResult(ex.Message);
      }
      finally
      {
        // Locks.Unlock(document);
      }
      
      Logger.DebugFormat("{0}. Finish. ApprovalTask Id: {1}", logPrefix, approvalTask.Id);
      return result;      
    }
  }
}