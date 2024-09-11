using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.Eskhata
{
  partial class MinutesTemplateServerHandlers
  {

    public override void BeforeExecute(Sungero.Reporting.Server.BeforeExecuteEventArgs e)
    {
      //Sungero.Company.PublicFunctions.Employee.GetShortName(employee, true)      
    }

    public virtual IQueryable<litiko.Eskhata.IMeeting> GetMeeting()
    {
      return litiko.Eskhata.Meetings.GetAll().Where(x => x.Id == MinutesTemplate.Entity.Meeting.Id);
    }

    public virtual IQueryable<litiko.Eskhata.IAgenda> GetDoc()
    {
      return litiko.Eskhata.Agendas.GetAll().Where(x => x.Id == MinutesTemplate.Entity.Id);
    }

  }
}