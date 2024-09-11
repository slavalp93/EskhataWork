using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.CollegiateAgencies.ApprovalRole;

namespace litiko.CollegiateAgencies.Server
{
  partial class ApprovalRoleFunctions
  {
    public override Sungero.Company.IEmployee GetRolePerformer(Sungero.Docflow.IApprovalTask task)
    {
      var document = task.DocumentGroup.OfficialDocuments.FirstOrDefault();
      if (document == null)
        return null;
      
      #region Докладчик
      if (_obj.Type == litiko.CollegiateAgencies.ApprovalRole.Type.Speaker)
      {        
        var projectSolutuionDoc = Projectsolutions.As(document);
        if (projectSolutuionDoc != null && projectSolutuionDoc.Speaker != null)
          return projectSolutuionDoc.Speaker;
            
        return null;
      }        
      #endregion
      
      #region Секретарь по категории
      if (_obj.Type == litiko.CollegiateAgencies.ApprovalRole.Type.SecretaryByCat)
      {      
        if (Projectsolutions.Is(document))          
          return Projectsolutions.As(document).MeetingCategory?.Secretary;

        if (litiko.Eskhata.Agendas.Is(document))
        {
          var meeting = litiko.Eskhata.Meetings.As(litiko.Eskhata.Agendas.As(document).Meeting);
          return meeting?.MeetingCategorylitiko?.Secretary;
        }

        if (litiko.Eskhata.Minuteses.Is(document))
        {
          var meeting = litiko.Eskhata.Meetings.As(litiko.Eskhata.Minuteses.As(document).Meeting);
          return meeting?.MeetingCategorylitiko?.Secretary;
        }
            
        return null;
      }
      #endregion
        
      #region Председатель по категории
      if (_obj.Type == litiko.CollegiateAgencies.ApprovalRole.Type.PresidentByCat)
      {          
        if (Projectsolutions.Is(document))          
          return Projectsolutions.As(document).MeetingCategory?.President;
          
        if (litiko.Eskhata.Agendas.Is(document))
        {
          var meeting = litiko.Eskhata.Meetings.As(litiko.Eskhata.Agendas.As(document).Meeting);
          return meeting?.MeetingCategorylitiko?.President;
        }
        
        if (litiko.Eskhata.Minuteses.Is(document))
        {
          var meeting = litiko.Eskhata.Meetings.As(litiko.Eskhata.Minuteses.As(document).Meeting);
          return meeting?.MeetingCategorylitiko?.President;
        }                
        
        return null;
      }
      #endregion      
        
      return base.GetRolePerformer(task);
    }
    
    /// <summary>
    /// Получить сотрудников роли согласования с несколькими участниками.
    /// </summary>
    /// <param name="task">Задача.</param>
    /// <returns>Список сотрудников.</returns>
    [Remote(IsPure = true), Public]
    public override List<Sungero.Company.IEmployee> GetRolePerformers(Sungero.Docflow.IApprovalTask task)
    {            
      if (_obj.Type == Sungero.Docflow.ApprovalRoleBase.Type.Approvers || _obj.Type == Sungero.Docflow.ApprovalRoleBase.Type.Addressees)
        return base.GetRolePerformers(task);
      
      var result = new List<Sungero.Company.IEmployee>();
      var document = task.DocumentGroup.OfficialDocuments.FirstOrDefault();
      
      if (document == null)
        return null;
      
      #region Участники совещания
      if (_obj.Type == litiko.CollegiateAgencies.ApprovalRole.Type.MeetingMembers)
      {
        if (litiko.Eskhata.Agendas.Is(document))
        {
          var agenda = litiko.Eskhata.Agendas.As(document);
          if (agenda.Meeting != null)
          {
            foreach (var element in agenda.Meeting.Members.Where(x => x.Member != null && Sungero.Company.Employees.Is(x.Member)))
            {
              result.Add(Sungero.Company.Employees.As(element.Member));
            }
          }        
        }        
        
        if (litiko.Eskhata.Minuteses.Is(document))
        {
          var minutes = litiko.Eskhata.Minuteses.As(document);
          if (minutes.Meeting != null)
          {
            foreach (var element in minutes.Meeting.Members.Where(x => x.Member != null && Sungero.Company.Employees.Is(x.Member)))
            {
              result.Add(Sungero.Company.Employees.As(element.Member));
            }
          }        
        }                
      }
      #endregion

      #region Приглашенные сотрудники
      if (_obj.Type == litiko.CollegiateAgencies.ApprovalRole.Type.MeetingInvited)
      {
        if (litiko.Eskhata.Agendas.Is(document))
        {
          var agenda = litiko.Eskhata.Agendas.As(document);
          if (agenda.Meeting != null)
          {
            foreach (var element in litiko.Eskhata.Meetings.As(agenda.Meeting).InvitedEmployeeslitiko.Where(x => x.Employee != null))
            {
              result.Add(element.Employee);
            }
          }        
        }
        
        if (litiko.Eskhata.Minuteses.Is(document))
        {
          var minutes = litiko.Eskhata.Minuteses.As(document);
          if (minutes.Meeting != null)
          {
            foreach (var element in litiko.Eskhata.Meetings.As(minutes.Meeting).InvitedEmployeeslitiko.Where(x => x.Employee != null))
            {
              result.Add(element.Employee);
            }
          }        
        }
      }
      #endregion
      
      return result;      
    }
  }
}