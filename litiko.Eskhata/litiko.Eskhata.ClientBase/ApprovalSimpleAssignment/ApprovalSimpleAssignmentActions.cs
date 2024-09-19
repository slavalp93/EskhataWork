using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ApprovalSimpleAssignment;

namespace litiko.Eskhata.Client
{
  partial class ApprovalSimpleAssignmentAnyChildEntityActions
  {
    public override void AddChildEntity(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      base.AddChildEntity(e);
    }

    public override bool CanAddChildEntity(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      var assignment = litiko.Eskhata.ApprovalSimpleAssignments.As(e.RootEntity);
      var isVoiting = assignment != null ? assignment.CustomStageTypelitiko == litiko.Eskhata.ApprovalSimpleAssignment.CustomStageTypelitiko.Voting : false;
      return base.CanAddChildEntity(e) && !isVoiting;
    }

    public override void CopyChildEntity(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      base.CopyChildEntity(e);
    }

    public override bool CanCopyChildEntity(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      var assignment = litiko.Eskhata.ApprovalSimpleAssignments.As(e.RootEntity);
      var isVoiting = assignment != null ? assignment.CustomStageTypelitiko == litiko.Eskhata.ApprovalSimpleAssignment.CustomStageTypelitiko.Voting : false;
      return base.CanCopyChildEntity(e) && !isVoiting;
    }

  }

  partial class ApprovalSimpleAssignmentAnyChildEntityCollectionActions
  {
    public override void DeleteChildEntity(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      base.DeleteChildEntity(e);
    }

    public override bool CanDeleteChildEntity(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {      
      var assignment = litiko.Eskhata.ApprovalSimpleAssignments.As(e.RootEntity);
      var isVoiting = assignment != null ? assignment.CustomStageTypelitiko == litiko.Eskhata.ApprovalSimpleAssignment.CustomStageTypelitiko.Voting : false;
      return base.CanDeleteChildEntity(e) && !isVoiting;      
    }

  }

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