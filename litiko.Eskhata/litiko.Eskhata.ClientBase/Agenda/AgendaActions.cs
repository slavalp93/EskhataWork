using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Agenda;

namespace litiko.Eskhata.Client
{
  partial class AgendaActions
  {
    public override void SendForApproval(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (!_obj.HasVersions || _obj.LastVersion.Body == null)
      {
        Dialogs.ShowMessage(litiko.CollegiateAgencies.Resources.NoVersionMessage, MessageType.Warning);
        throw new OperationCanceledException();
      }
      
      base.SendForApproval(e);
    }

    public override bool CanSendForApproval(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanSendForApproval(e);
    }

    public override void CreateFromTemplate(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (_obj.State.IsInserted)
      {
        //Dialogs.ShowMessage(litiko.CollegiateAgencies.Resources.SaveObjectMessage, MessageType.Warning);
        //throw new OperationCanceledException();                  
        _obj.Save();
      }      
      
      try
      {
        litiko.Eskhata.Functions.Agenda.Remote.FillAgendaTemplate(_obj);
        e.AddInformation(litiko.Eskhata.Resources.VersionCreatedSuccessfully);
      }
      catch (Exception ex)
      {
        e.AddError(ex.Message);
        throw;
      }      
    }

    public override bool CanCreateFromTemplate(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanCreateFromTemplate(e);
    }

  }

}