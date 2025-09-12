using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Integration.ExchangeDocument;

namespace litiko.Integration.Client
{
  partial class ExchangeDocumentActions
  {
    public virtual void ExchangeQueue(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var exchQueue = Functions.Module.Remote.GetExchangeQueueByDoc(_obj);
      exchQueue.Show();
    }

    public virtual bool CanExchangeQueue(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void StartProcessingXML(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (Locks.GetLockInfo(_obj).IsLockedByMe)
        Locks.Unlock(_obj);
      
      var asyncHandler = Integration.AsyncHandlers.ImportData.Create();
      asyncHandler.ExchangeDocId = _obj.Id;
      asyncHandler.ExecuteAsync("ImportData started","ImportData finished", "ImportData was errors", Users.Current);     
    }

    public virtual bool CanStartProcessingXML(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

}