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
    public virtual void ConfirmTransferToArchive(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var asyncHandler = litiko.Archive.AsyncHandlers.TransferToArchive.Create();
      asyncHandler.docId = _obj.Id;
      asyncHandler.dateTransfer = Calendar.Now;
      asyncHandler.ExecuteAsync(litiko.Archive.Resources.AsyncHandlerStartNotification,
                                litiko.Archive.Resources.AsyncHandlerFinishNotification,
                                litiko.Archive.Resources.NoticeSubjectForErrorTask,
                                Users.Current);
      
      //e.CloseFormAfterAction = true;
    }

    public virtual bool CanConfirmTransferToArchive(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !_obj.State.IsInserted && !_obj.State.IsChanged && _obj.Archive != null && Equals(_obj.Archive.Archivist, Users.Current)
        && _obj.CaseFiles.Where(x => x.CaseFile != null).Any(x => x.CaseFile.Archivelitiko == null);
    }

    public override void CreateFromTemplate(Sungero.Domain.Client.ExecuteActionArgs e)
    {     
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