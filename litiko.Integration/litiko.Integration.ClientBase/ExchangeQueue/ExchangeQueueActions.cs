using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Integration.ExchangeQueue;
using System.IO;

namespace litiko.Integration.Client
{
  partial class ExchangeQueueActions
  {
    public virtual void OpenXML(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (_obj.Xml == null)
        Dialogs.NotifyMessage("XML отсутствует");      
      else
      {			  
			  string fileName = string.Format("{0}.xml", _obj.Name.ToString());			  
			  Sungero.Core.EntityDataPropertyExtensions.Open(_obj.Xml, fileName);
      }      
    }

    public virtual bool CanOpenXML(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

}