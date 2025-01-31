using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.OfficialDocument;

namespace litiko.Eskhata.Client
{
  partial class OfficialDocumentCollectionActions
  {
    public override void Sign(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (_objs.Any(d => d.LastVersionApproved.GetValueOrDefault()))
      {
        Dialogs.NotifyMessage(litiko.Eskhata.Resources.LastVersionApproved);
        return;
      }      
      
      base.Sign(e);
    }

    public override bool CanSign(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanSign(e);
    }

  }


  partial class OfficialDocumentActions
  {
    public override void ShowAcquaintanceReport(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      //base.ShowAcquaintanceReport(e);
      
      if (!Sungero.Docflow.PublicFunctions.OfficialDocument.Remote.GetAcquaintanceTasks(_obj).Any())
      {
        Dialogs.NotifyMessage(OfficialDocuments.Resources.NoAcquaintanceTasks);
        return;
      }          
      
      RecordManagementEskhata.PublicFunctions.Module.GetAcquaintanceReport(_obj).Open();
    }

    public override bool CanShowAcquaintanceReport(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanShowAcquaintanceReport(e);
    }

  }

}