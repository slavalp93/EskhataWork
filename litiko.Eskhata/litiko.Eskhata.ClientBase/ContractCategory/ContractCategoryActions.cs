using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ContractCategory;

namespace litiko.Eskhata.Client
{
  partial class ContractCategoryCollectionBulkActions
  {
    public virtual void AddToMappinglitiko(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var result = NSI.PublicFunctions.Module.Remote.AddToMapping(NSI.Mapping.EntityType.ContrCategory.Value, _objIds.ToList());
      Dialogs.ShowMessage(result);      
    }
  }

}