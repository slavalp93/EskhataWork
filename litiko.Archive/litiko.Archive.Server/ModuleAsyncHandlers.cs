using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.Archive.Server
{
  public class ModuleAsyncHandlers
  {

    public virtual void TransferToArchive(litiko.Archive.Server.AsyncHandlerInvokeArgs.TransferToArchiveInvokeArgs args)
    {
        var logPostfix = string.Format("DocId = '{0}'", args.docId);
        Logger.DebugFormat("TransferToArchive. Start. {0}", logPostfix);
        
        var doc = ArchiveLists.GetAll(x => x.Id == args.docId).FirstOrDefault();
        
        if (doc == null)
        {
          Logger.ErrorFormat("TransferToArchive. Document with id = {0} not found.", args.docId);
          return;
        }
        
        foreach (litiko.Eskhata.ICaseFile caseFile in doc.CaseFiles.Where(x => x.CaseFile != null && x.CaseFile.Archivelitiko == null).Select(x => x.CaseFile))
        {
          if (!Locks.TryLock(caseFile))
          {            
            Logger.DebugFormat("TransferToArchive. CaseFile with id = {0} is locked. Sent to retry", caseFile.Id);
            args.Retry = true;
            return;
          }
          
          try
          {
            var docsInCaseFile = litiko.Eskhata.OfficialDocuments.GetAll().Where(d => Equals(d.CaseFile, caseFile) && d.Archivelitiko == null);
            foreach (var document in docsInCaseFile)
            {
              if (!Locks.TryLock(document))
              {            
                Logger.DebugFormat("TransferToArchive. Document with id = {0} is locked. Sent to retry", document.Id);
                args.Retry = true;
                return;
              }
              
              try
              {
                document.Archivelitiko = doc.Archive;
                document.TransferredToArchivelitiko = args.dateTransfer;                
                // Местонахождение и Вкладка Выдача заполняются при сохранении док-та                
                
                document.Save();
                Logger.DebugFormat("TransferToArchive. Document with id = {0} updated.", document.Id);
              }
              finally
              {
                Locks.Unlock(document);
              }              
            }
            
            caseFile.Archivelitiko = doc.Archive;
            caseFile.TransferredToArchivelitiko = args.dateTransfer;
            caseFile.Save();
            Logger.DebugFormat("TransferToArchive. CaseFile with id = {0} updated.", caseFile.Id);  
          }
          catch (Exception ex)
          {            
            Logger.ErrorFormat("TransferToArchive. An error occured when processing documents in caseFile – {0}, ErrorMessage: {1}", ex, caseFile.Id, ex.Message);
            
            // Отправляем уведомление Архивариусу
            var newTask = Sungero.Workflow.SimpleTasks.CreateWithNotices(Resources.NoticeSubjectForErrorTask, doc.Archive.Archivist);
            newTask.NeedsReview = false;
            newTask.ActiveText = ex.Message + Environment.NewLine + ex.StackTrace;
            newTask.Attachments.Add(doc);
            newTask.Start();
            Logger.DebugFormat("Notice with Id '{0}' has been started.", newTask.Id);

            // args.Retry = true;
            // throw;
          }
          finally
          {
            Locks.Unlock(caseFile);
          }
        }      
       
        // Логируем завершение работы обработчика.
        Logger.DebugFormat("TransferToArchive. Finish. {0}", logPostfix);
    }

  }
}