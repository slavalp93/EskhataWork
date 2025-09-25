using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Minutes;

namespace litiko.Eskhata.Client
{
  partial class MinutesActions
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

    public override void CreateActionItems(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.CreateActionItems(e);
    }

    public override bool CanCreateActionItems(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return false; //base.CanCreateActionItems(e);
    }

    public override void CreateFromTemplate(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      try
      {
        if (_obj.State.IsInserted)
          _obj.Save();
          
        litiko.CollegiateAgencies.PublicFunctions.Module.Remote.CreateMinutesBody(_obj, false);
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