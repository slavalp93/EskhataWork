using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.CollegiateAgencies.Projectsolution;

namespace litiko.CollegiateAgencies.Shared
{
  partial class ProjectsolutionFunctions
  {
    /// <summary>
    /// Обработка включения в совещание
    /// </summary>
    [Public]
    public void ProcessIncludingInMeeting(litiko.Eskhata.IMeeting meeting, bool isFromMeeting)
    {
      if (meeting != null)
      {
        if (_obj.MeetingCategory != null && !Equals(_obj.MeetingCategory, meeting.MeetingCategorylitiko))
          meeting.MeetingCategorylitiko = _obj.MeetingCategory;
        
        if (!isFromMeeting && !meeting.ProjectSolutionslitiko.Any(x => Equals(x.ProjectSolution, _obj)))
          meeting.ProjectSolutionslitiko.AddNew().ProjectSolution = _obj;
        
        if (_obj.Speaker != null && !meeting.Members.Any(x => Equals(Sungero.Company.Employees.As(x.Member), _obj.Speaker)))
          meeting.Members.AddNew().Member = _obj.Speaker;
        
        if (_obj.InvitedEmployees.Any())
        {
          foreach (Sungero.Company.IEmployee employee in _obj.InvitedEmployees.Where(x => x.Employee != null).Select(x => x.Employee))
          {
            if (!meeting.InvitedEmployeeslitiko.Any(x => Equals(x.Employee, employee)))
              meeting.InvitedEmployeeslitiko.AddNew().Employee = employee;
          }
        }

        if (_obj.InvitedExternal.Any())
        {
          foreach (Sungero.Parties.IContact contact in _obj.InvitedExternal.Where(x => x.Contact != null).Select(x => x.Contact))
          {
            if (!meeting.InvitedExternallitiko.Any(x => Equals(x.Contact, contact)))
              meeting.InvitedExternallitiko.AddNew().Contact = contact;
          }
        }        
        
        /* Вынести из функции
        if (!meeting.State.IsInserted && meeting.State.IsChanged)
          meeting.Save();
        */
      }    
    }
  }
}