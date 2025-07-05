using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Agenda;

namespace litiko.Eskhata
{
  partial class AgendaServerHandlers
  {

    public override void Saving(Sungero.Domain.SavingEventArgs e)
    {
      base.Saving(e);
      
      // Выдать права роли "Дополнительные члены Правления"
      var meeting = litiko.Eskhata.Meetings.As(_obj.Meeting);
      if (meeting != null && meeting?.MeetingCategorylitiko?.Name == "Заседание Правления")
      {                                
        var roleAdditionalBoardMembers = Roles.GetAll(r => r.Sid == litiko.CollegiateAgencies.PublicConstants.Module.RoleGuid.AdditionalBoardMembers).FirstOrDefault();
        if (roleAdditionalBoardMembers != null)
        {
          if (!_obj.AccessRights.IsGrantedDirectly(DefaultAccessRightsTypes.Change, roleAdditionalBoardMembers))
            _obj.AccessRights.Grant(roleAdditionalBoardMembers, DefaultAccessRightsTypes.Change);             
        }                
      }        
    }

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      base.BeforeSave(e);
      
      // Связать проекты решений по совещанию с повесткой
      if (_obj.Meeting != null && litiko.Eskhata.Meetings.As(_obj.Meeting).ProjectSolutionslitiko.Where(x => x.ProjectSolution != null).Any())
      {
        foreach (var element in litiko.Eskhata.Meetings.As(_obj.Meeting).ProjectSolutionslitiko.Where(x => x.ProjectSolution != null))
        {
          var projectSolution = element.ProjectSolution;
          if (projectSolution.AccessRights.CanRead() && !_obj.Relations.GetRelated(Sungero.Docflow.PublicConstants.Module.AddendumRelationName).Contains(projectSolution))
            _obj.Relations.Add(Sungero.Docflow.PublicConstants.Module.AddendumRelationName, projectSolution);
        }
      }        
    }
  }

}