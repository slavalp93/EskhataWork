using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Counterparty;

namespace litiko.Eskhata.Client
{
  partial class CounterpartyActions
  {
    public virtual void SendToVerificationlitiko(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var documents = Sungero.Docflow.PublicFunctions.Module.Remote.GetCounterpartyDocuments(_obj);
      if (!documents.Any())
      {
        Dialogs.ShowMessage(Counterparties.Resources.InformationAboutCounterpartyIsRequired, MessageType.Warning);
        throw new OperationCanceledException();
      }
      
      var lastDocument = documents.OrderByDescending(d => d.Created).FirstOrDefault();
      // Если по документу ранее были запущены задачи, то вывести соответствующий диалог.
      if (!Sungero.Docflow.PublicFunctions.OfficialDocument.NeedCreateApprovalTask(lastDocument))
        return;
      
      var availableApprovalRules = Sungero.Docflow.PublicFunctions.OfficialDocument.Remote.GetApprovalRules(lastDocument).ToList();
      if (availableApprovalRules.Any())
      {
        var approvalTask = Sungero.Docflow.PublicFunctions.Module.Remote.CreateApprovalTask(lastDocument);
        if (!approvalTask.OtherGroup.All.Contains(_obj))
          approvalTask.OtherGroup.All.Add(_obj);
        
        approvalTask.Show();
        e.CloseFormAfterAction = true;
      }
      else
      {
        // Если по документу нет регламента, вывести сообщение.
        Dialogs.ShowMessage(OfficialDocuments.Resources.NoApprovalRuleWarning, MessageType.Warning);
        throw new OperationCanceledException();
      }      
    }

    public virtual bool CanSendToVerificationlitiko(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }


}