using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.OfficialDocument;

namespace litiko.Eskhata.Client
{


  partial class OfficialDocumentActions
  {
    public virtual void ExportToABSlitiko(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      string errorMessage = litiko.Integration.PublicFunctions.Module.IntegrationClientAction(_obj);
      if (!string.IsNullOrEmpty(errorMessage))
      {
        e.AddError(errorMessage);
        return;
      }
      else
      {
        e.AddInformation(litiko.Integration.Resources.DataUpdatedSuccessfully);        
      }      
    }

    public virtual bool CanExportToABSlitiko(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Доступность определяется в наследниках
      return false;
    }

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