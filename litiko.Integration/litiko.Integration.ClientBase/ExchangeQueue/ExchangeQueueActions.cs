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
			  string nameFile = string.Format("XML_{0}", _obj.Name.ToString());
			  
        using (var xmlStream = new MemoryStream(_obj.Xml))
        {
          var doc = Sungero.Docflow.SimpleDocuments.Create();
          doc.DocumentKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Sungero.Docflow.PublicConstants.Module.Initialize.SimpleDocumentKind);
          doc.Name = nameFile;
          doc.Subject = nameFile;
          doc.Note = "#TEMP#";
          doc.CreateVersionFrom(xmlStream, "xml");
          doc.Save();          
  
          bool b = doc.Export();
          
          //Sungero.Docflow.SimpleDocuments.Delete(doc);
        }		    
      }      
    }

    public virtual bool CanOpenXML(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

}