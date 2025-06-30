using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.Eskhata
{
  partial class AgendaTemplateServerHandlers
  {

    public virtual IQueryable<litiko.CollegiateAgencies.IProjectsolution> GetProjectSolutions()
    {
      return litiko.CollegiateAgencies.Projectsolutions.GetAll();
    }

    public override void BeforeExecute(Sungero.Reporting.Server.BeforeExecuteEventArgs e)
    {      
      AgendaTemplate.MeetingMembers = litiko.Eskhata.PublicFunctions.Meeting.GetMeetingCategoryMembers(litiko.Eskhata.Meetings.As(AgendaTemplate.Entity.Meeting), false, false);
    }

    public virtual IQueryable<litiko.Eskhata.IMeeting> GetMeeting()
    {
      return litiko.Eskhata.Meetings.GetAll().Where(x => x.Id == AgendaTemplate.Entity.Meeting.Id);
    }

    public virtual IQueryable<litiko.Eskhata.IAgenda> GetDoc()
    {
      return litiko.Eskhata.Agendas.GetAll().Where(x => x.Id == AgendaTemplate.Entity.Id);
    }

  }
}