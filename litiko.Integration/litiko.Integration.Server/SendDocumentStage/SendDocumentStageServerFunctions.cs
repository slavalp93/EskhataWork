using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Integration.SendDocumentStage;

namespace litiko.Integration.Server
{
  partial class SendDocumentStageFunctions
  {
    /// <summary>
    /// Выполнить сценарий.
    /// </summary>
    /// <param name="approvalTask">Задача на согласование по регламенту.</param>
    /// <returns>Результат выполнения сценария.</returns>
    public override Sungero.Docflow.Structures.ApprovalFunctionStageBase.ExecutionResult Execute(Sungero.Docflow.IApprovalTask approvalTask)
    {
      var logPrefix = "Integration. SendDocument stage. Execute";
      Logger.DebugFormat("{0}. Start. ApprovalTask Id: {1}", logPrefix, approvalTask.Id);
      
      var result = base.Execute(approvalTask);      
      var document = litiko.Eskhata.OfficialDocuments.As(approvalTask.DocumentGroup.OfficialDocuments.FirstOrDefault());
      if (document == null)
        return this.GetErrorResult(SendDocumentStages.Resources.DocumentNotFound);            

      string integrationMethodName = string.Empty;
                
      if (litiko.Eskhata.Contracts.Is(document))        
        integrationMethodName = Constants.Module.IntegrationMethods.R_DR_SET_CONTRACT;        
      else if (litiko.Eskhata.SupAgreements.Is(document) || litiko.Eskhata.IncomingInvoices.Is(document) || Sungero.FinancialArchive.ContractStatements.Is(document))        
        integrationMethodName = Constants.Module.IntegrationMethods.R_DR_SET_PAYMENT_DOCUMENT;        
      else
        return this.GetErrorResult(SendDocumentStages.Resources.UnsupportedDocumentType);
        
      var integrationMethod = IntegrationMethods.GetAll().Where(x => x.Name == integrationMethodName).FirstOrDefault();
      if (integrationMethod == null)
        return this.GetErrorResult(SendDocumentStages.Resources.IntegrationMethodNotFoundFormat(integrationMethodName));
      
      var exchDocId = litiko.Eskhata.ApprovalTasks.As(approvalTask).ExchangeDocIdlitiko;
      var exchDoc = Integration.ExchangeDocuments.Null;
      if (exchDocId != null)
        exchDoc = Integration.ExchangeDocuments.GetAll().Where(x => x.Id == exchDocId).FirstOrDefault();
      else
      {
        exchDoc = Integration.ExchangeDocuments.Create();
        exchDoc.IntegrationMethod = integrationMethod;
        exchDoc.EntityId = document.Id;        
        exchDoc.Save();
          
        litiko.Eskhata.ApprovalTasks.As(approvalTask).ExchangeDocIdlitiko = exchDoc.Id;
        litiko.Eskhata.ApprovalTasks.As(approvalTask).Save();
      }                
        
      if (exchDoc == null)
        return this.GetErrorResult(SendDocumentStages.Resources.ExchangeDocumentNotFound);      
      
      if (!Locks.TryLock(document))
        this.GetRetryResult(SendDocumentStages.Resources.DocumentIsLockedFormat(document.Id));
      
      try
      {                                 
        int lastId = 0;
        var errorMessage = Functions.Module.SendRequestToIS(exchDoc, lastId, document);
        if (!string.IsNullOrEmpty(errorMessage))
          result = this.GetRetryResult(errorMessage);        
        
        if (!Equals(document.IntegrationStatuslitiko, litiko.Eskhata.OfficialDocument.IntegrationStatuslitiko.Send))
        {
          document.IntegrationStatuslitiko = litiko.Eskhata.OfficialDocument.IntegrationStatuslitiko.Send;
          document.Save();
        }
        
        Logger.DebugFormat("{0}. Request to IS success. DocumentId: {1}. ExchangeDocumetId: {2}", logPrefix, document.Id, exchDoc.Id);
         
      }
      catch (Exception ex)
      {        
        Logger.ErrorFormat("{0}. Error: {1}. Stack trace: {2}", logPrefix, ex.Message, ex.StackTrace);                
        result = this.GetRetryResult(ex.Message);
      }
      finally
      {
        Locks.Unlock(document);
      }
      
      Logger.DebugFormat("{0}. Finish. ApprovalTask Id: {1}", logPrefix, approvalTask.Id);
      return result;      
    }
    
    /// <summary>
    /// Проверить состояние этапа выполнения сценария.
    /// </summary>
    /// <param name="approvalTask">Задача на согласование по регламенту.</param>
    /// <returns>Состояние этапа.</returns>    
    public override Sungero.Docflow.Structures.ApprovalFunctionStageBase.ExecutionResult CheckCompletionState(Sungero.Docflow.IApprovalTask approvalTask)
    {
      var logPrefix = "Integration. SendDocument stage. CheckCompletionState";
      Logger.DebugFormat("{0}. Start. ApprovalTask Id: {1}", logPrefix, approvalTask.Id);
      
      var exchDocId = litiko.Eskhata.ApprovalTasks.As(approvalTask).ExchangeDocIdlitiko;
      if (exchDocId == null)
        return this.GetErrorResult(SendDocumentStages.Resources.ExchangeDocumentNotFound);
          
      var document = litiko.Eskhata.OfficialDocuments.As(approvalTask.DocumentGroup.OfficialDocuments.FirstOrDefault());
      if (document == null)
        return this.GetErrorResult(SendDocumentStages.Resources.DocumentNotFound);       
      
      if (Equals(document.IntegrationStatuslitiko, litiko.Eskhata.OfficialDocument.IntegrationStatuslitiko.Error))
      {
        litiko.Eskhata.ApprovalTasks.As(approvalTask).ExchangeDocIdlitiko = null;
        litiko.Eskhata.ApprovalTasks.As(approvalTask).Save();        
        
        var errorInfo = string.Empty;
        var exchDoc = ExchangeDocuments.GetAll().FirstOrDefault(x => x.Id == exchDocId);
        if (exchDoc != null && !string.IsNullOrEmpty(exchDoc.RequestToRXInfo))
          errorInfo = SendDocumentStages.Resources.ErrorIntegrationFormat(exchDoc.RequestToRXInfo);
        else
          errorInfo = SendDocumentStages.Resources.ErrorIntegrationFormat(SendDocumentStages.Resources.UnknownError);
                
        Logger.DebugFormat("{0}. Error. ApprovalTask Id: {1}. Document Id:{2}. Error: {3}", logPrefix, approvalTask.Id, document.Id, errorInfo);
        return this.GetErrorResult(errorInfo);
      }
      else if (Equals(document.IntegrationStatuslitiko, litiko.Eskhata.OfficialDocument.IntegrationStatuslitiko.Success))
      {
        litiko.Eskhata.ApprovalTasks.As(approvalTask).ExchangeDocIdlitiko = null;
        litiko.Eskhata.ApprovalTasks.As(approvalTask).Save();
        Logger.DebugFormat("{0}. Success. ApprovalTask Id: {1}. Document Id:{2}", logPrefix, approvalTask.Id, document.Id);
        return this.GetSuccessResult();
      }
      
      Logger.DebugFormat("{0}. Finish. ApprovalTask Id: {1}", logPrefix, approvalTask.Id);
      return this.GetRetryResult(string.Empty);
    }    
  }
}