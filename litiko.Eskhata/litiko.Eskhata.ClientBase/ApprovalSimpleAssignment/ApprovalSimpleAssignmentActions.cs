using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ApprovalSimpleAssignment;

namespace litiko.Eskhata.Client
{
  partial class ApprovalSimpleAssignmentActions
  {
    public override void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      base.Complete(e);
      
      var doc = _obj.DocumentGroup.OfficialDocuments.FirstOrDefault();
      if (doc != null && litiko.Archive.ArchiveLists.Is(doc) && Equals(litiko.Archive.ArchiveLists.As(doc).Archive?.Archivist, _obj.Performer))
      {
        if (!litiko.Archive.PublicFunctions.ArchiveList.Remote.IsAllTransferedToArchive(litiko.Archive.ArchiveLists.As(doc)))
        {
          e.AddError(litiko.Archive.Resources.DocumentsAreNotTransferedToArchive);
          return;
        } 
      }                  
    }

    public override bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return base.CanComplete(e);
    }

  }

}