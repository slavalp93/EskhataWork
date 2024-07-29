using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Order;

namespace litiko.Eskhata.Client
{
  partial class OrderActions
  {
    public virtual void CreateArchiveSchedulelitiko(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Archive.PublicConstants.Module.DocumentKindGuids.ArchiveShedule);
      if (docKind != null && docKind.AccessRights.CanSelectInDocument())
      {               
        var doc = Functions.Order.Remote.CreateSimpleDocument();
        doc.DocumentKind = docKind;
        doc.LeadingDocument = _obj;
        var docId = doc.Id;
        doc.Show();

        // Связь при сохранении SimpleDocument
      }
      else
        e.AddInformation(litiko.Eskhata.Orders.Resources.NotAccessToExecute);
    }

    public virtual bool CanCreateArchiveSchedulelitiko(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.DocumentKind != null && Equals(_obj.DocumentKind, Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Archive.PublicConstants.Module.DocumentKindGuids.OrderArchive));         
    }

  }



}