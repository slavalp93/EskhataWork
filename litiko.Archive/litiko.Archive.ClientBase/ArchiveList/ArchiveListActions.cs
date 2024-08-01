using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Archive.ArchiveList;

namespace litiko.Archive.Client
{
  partial class ArchiveListActions
  {
    public override void CreateFromTemplate(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      //base.CreateFromTemplate(e);
      
      try
      {
        litiko.Archive.Functions.ArchiveList.Remote.FillArchiveListTemplate(_obj);
        e.AddInformation(litiko.Eskhata.Resources.VersionCreatedSuccessfully);
      }
      catch (Exception ex)
      {        
        throw;
      }
      
      
    }

    public override bool CanCreateFromTemplate(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanCreateFromTemplate(e);
    }

  }

}