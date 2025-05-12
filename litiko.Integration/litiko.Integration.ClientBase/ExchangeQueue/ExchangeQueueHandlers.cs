using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Integration.ExchangeQueue;

namespace litiko.Integration
{
  partial class ExchangeQueueClientHandlers
  {

    public override void Closing(Sungero.Presentation.FormClosingEventArgs e)
    {
      string nameFile = string.Format("XML_{0}", _obj.Name.ToString());
      var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Sungero.Docflow.PublicConstants.Module.Initialize.SimpleDocumentKind);
      var docs = Sungero.Docflow.SimpleDocuments.GetAll().Where(d => d.Name == nameFile && d.Note == "#TEMP#" && Equals(d.DocumentKind, docKind) && Equals(d.Author, Users.Current));
      foreach (var doc in docs)
      {
        Sungero.Docflow.SimpleDocuments.Delete(doc);
      }
    }

  }
}