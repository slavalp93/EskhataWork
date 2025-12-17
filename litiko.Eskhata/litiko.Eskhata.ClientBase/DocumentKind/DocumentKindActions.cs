using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.DocumentKind;

namespace litiko.Eskhata.Client
{
  partial class DocumentKindCollectionBulkActions
  {

    public virtual void AddToMappinglitiko(Sungero.Domain.Client.ExecuteActionArgs e)
    {      
      var result = NSI.PublicFunctions.Module.Remote.AddToMapping(NSI.Mapping.EntityType.DocumentKind.Value, _objIds.ToList());
      Dialogs.ShowMessage(result);
    }
  }

}