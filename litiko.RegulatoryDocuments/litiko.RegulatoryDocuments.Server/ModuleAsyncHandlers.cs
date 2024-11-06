using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.RegulatoryDocuments.Server
{
  public class ModuleAsyncHandlers
  {
    /// <summary>
    /// Установить состояние документа в устаревший
    /// </summary>
    /// <param name="args"></param>
    public virtual void SetObsoleteLifeCircleStage(litiko.RegulatoryDocuments.Server.AsyncHandlerInvokeArgs.SetObsoleteLifeCircleStageInvokeArgs args)
    {
        var logPostfix = string.Format("DocId = '{0}'", args.DocumentID);
        Logger.DebugFormat("SetObsoleteLifeCircleStage. Start. {0}", logPostfix);
        
        var document = Sungero.Docflow.OfficialDocuments.GetAll(x => x.Id == args.DocumentID).FirstOrDefault();        
        if (document == null)
        {
          Logger.ErrorFormat("SetObsoleteLifeCircleStage. Document with id = {0} not found.", args.DocumentID);
          return;
        }
                  
        if (!Locks.TryLock(document))
        {            
          Logger.DebugFormat("SetObsoleteLifeCircleStage. Document with id = {0} is locked. Sent to retry", document.Id);
          args.Retry = true;
          return;
        }
              
        try
        {
          if (!Equals(document.LifeCycleState, Sungero.Docflow.OfficialDocument.LifeCycleState.Obsolete))
          {
            document.LifeCycleState = Sungero.Docflow.OfficialDocument.LifeCycleState.Obsolete;          
            document.Save();          
            Logger.DebugFormat("SetObsoleteLifeCircleStage. Document with id = {0} updated.", document.Id);
          }
          else
            Logger.DebugFormat("SetObsoleteLifeCircleStage. Document with id = {0} is already in Obsolete LifeCircleStage.", document.Id);
        }
        catch (Exception ex)
        {
          Logger.ErrorFormat("SetObsoleteLifeCircleStage. Error: {0}", ex.Message);
        }
        finally
        {
          Locks.Unlock(document);
        }     
       
        // Логируем завершение работы обработчика.
        Logger.DebugFormat("SetObsoleteLifeCircleStage. Finish. {0}", logPostfix);      
    }

  }
}