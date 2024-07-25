using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.RecordManagementEskhata.Server
{
  public class ModuleAsyncHandlers
  {

    public virtual void ConvertAcquaintedDocToPDF(litiko.RecordManagementEskhata.Server.AsyncHandlerInvokeArgs.ConvertAcquaintedDocToPDFInvokeArgs args)
    {
      long documentId = args.DocumentId;
      long versionId = args.VersionId;
      long taskId = args.TaskId;
      
      Logger.DebugFormat("ConvertDocumentToPdf: start convert document to pdf. Document id - {0}.", documentId);
      
      var document = Sungero.Docflow.OfficialDocuments.GetAll(x => x.Id == documentId).FirstOrDefault();
      if (document == null)
      {
        Logger.DebugFormat("ConvertDocumentToPdf: not found document with id {0}.", documentId);
        return;
      }
      
      var version = document.Versions.SingleOrDefault(v => v.Id == versionId);
      if (version == null)
      {
        Logger.DebugFormat("ConvertDocumentToPdf: not found version. Document id - {0}, version number - {1}.", documentId, versionId);
        return;
      }
      
      var task = Sungero.RecordManagement.AcquaintanceTasks.GetAll(x => x.Id == taskId).FirstOrDefault();
      if (task == null)
      {
        Logger.DebugFormat("ConvertDocumentToPdf: not found task with id {0}.", taskId);
        return;
      }
      
      if (!Locks.TryLock(version.Body))
      {
        Logger.DebugFormat("ConvertDocumentToPdf: version is locked. Document id - {0}, version number - {1}.", documentId, versionId);
        args.Retry = true;
        return;
      }
      var result = Functions.Module.ConvertAcquaintedDocToPDF(document, version.Id, task, string.Empty, true, 0, 0);
      Locks.Unlock(version.Body);

      if (result.HasErrors)
      {
        Logger.DebugFormat("ConvertDocumentToPdf: {0}", result.ErrorMessage);
        if (result.HasLockError)
        {
          args.Retry = true;
          return;
        }
        else
        {
          var operation = new Enumeration("ConvertToPdf");
          document.History.Write(operation, operation, string.Empty, version.Number);
          document.Save();
          
          args.Retry = false;
          var exceptionText = string.Format("ConvertDocumentToPdf: {0}", Sungero.Docflow.OfficialDocuments.Resources.ConvertionErrorTitleBase);
          throw AppliedCodeException.Create(exceptionText);
        }
      }

      Logger.DebugFormat("ConvertDocumentToPdf: convert document {0} to pdf successfully.", documentId);
    }

  }
}