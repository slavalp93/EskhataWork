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
    public override void CreateFromTemplate(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      try
      {
        litiko.Eskhata.Functions.Agenda.Remote.FillAgendaTemplate(_obj);
        e.AddInformation(litiko.Eskhata.Resources.VersionCreatedSuccessfully);
      }
      catch (Exception ex)
      {        
        throw;
      }      
    }

    public override bool CanCreateFromTemplate(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanCreateFromTemplate(e) && !_obj.State.IsInserted;
    }

  }

}