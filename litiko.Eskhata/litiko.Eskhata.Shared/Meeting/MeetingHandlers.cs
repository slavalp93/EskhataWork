using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Meeting;

namespace litiko.Eskhata
{
  partial class MeetingAbsentlitikoSharedCollectionHandlers
  {

    public virtual void AbsentlitikoDeleted(Sungero.Domain.Shared.CollectionPropertyDeletedEventArgs e)
    {
      var deletedEmployee = _deleted.Employee;
      if (deletedEmployee != null && !_obj.Presentlitiko.Any(x => Equals(deletedEmployee, x.Employee)))
        _obj.Presentlitiko.AddNew().Employee = deletedEmployee;
    }
  }

  partial class MeetingSharedHandlers
  {

    public virtual void MeetingCategorylitikoChanged(litiko.Eskhata.Shared.MeetingMeetingCategorylitikoChangedEventArgs e)
    {
      var selectedCategory = e.NewValue;
      if (selectedCategory != null && !Equals(selectedCategory, e.OldValue))
      {
        _obj.Members.Clear();
        _obj.Presentlitiko.Clear();
        
        if (selectedCategory.Members.Any())
        {
          foreach (var element in selectedCategory.Members.Where(x => x.Member != null).Select(x => x.Member))
          {
            var member = Recipients.As(element);
            if (member != null && !_obj.Members.Any(x => Equals(x.Member, member)))
            {
              var newMember = _obj.Members.AddNew();
              newMember.Member = member;
            }
            
            if (!_obj.Presentlitiko.Any(x => Equals(x.Employee, element)))
            {
              var newMember = _obj.Presentlitiko.AddNew();
              newMember.Employee = element;
            }            
          }        
        }
        
        if (selectedCategory.President != null && !Equals(selectedCategory.President, _obj.President))
          _obj.President = selectedCategory.President;
        
        if (selectedCategory.Secretary != null && !Equals(selectedCategory.Secretary, _obj.Secretary))
          _obj.Secretary = selectedCategory.Secretary;

        Functions.Meeting.SetQuorum(_obj, selectedCategory);
      }      
    }
  }

  partial class MeetingProjectSolutionslitikoSharedCollectionHandlers
  {

    public virtual void ProjectSolutionslitikoDeleted(Sungero.Domain.Shared.CollectionPropertyDeletedEventArgs e)
    {
      int minNumber = 1;
      foreach (var element in _obj.ProjectSolutionslitiko)
        element.Number = minNumber++; 

      var doc = _deleted.ProjectSolution;
      if (doc != null)
      {
        if (doc.AccessRights.CanUpdate())
        {
          doc.Meeting = null;
          doc.IncludedInAgenda = false;
          doc.Save();        
        }
        
        #region Удалить приглашенных сотрудников        
        if (_obj.InvitedEmployeeslitiko.Any(x => doc.InvitedEmployees.Any(y => Equals(y.Employee, x.Employee))))
        {
          List<Sungero.Company.IEmployee> employeesInDeleted = doc.InvitedEmployees.Select(empl => empl.Employee).ToList();
          List<litiko.CollegiateAgencies.IProjectsolution> anotherPS = _obj.ProjectSolutionslitiko.Where(ps => !Equals(ps.ProjectSolution, doc)).Select(x => x.ProjectSolution).ToList();
          List<Sungero.Company.IEmployee> employeesInAnotherPS = anotherPS.SelectMany(ps => ps.InvitedEmployees.Select(empl => empl.Employee)).ToList();
          var employeesToDelete = employeesInDeleted
            .Where(e1 => !employeesInAnotherPS.Any(e2 => e2.Id == e1.Id))
            .ToList();
          foreach (var employee in employeesToDelete)
          {
            foreach (var element in _obj.InvitedEmployeeslitiko.Where(x => Equals(employee, x.Employee)).ToList())
            {
              _obj.InvitedEmployeeslitiko.Remove(element);
            }
          }
        }
        #endregion
        
        #region Удалить приглашенных внешних        
        if (_obj.InvitedExternallitiko.Any(x => doc.InvitedExternal.Any(y => Equals(y.Contact, x.Contact))))
        {
          List<Sungero.Parties.IContact> contactsInDeleted = doc.InvitedExternal.Select(c => c.Contact).ToList();
          List<litiko.CollegiateAgencies.IProjectsolution> anotherPS = _obj.ProjectSolutionslitiko.Where(ps => !Equals(ps.ProjectSolution, doc)).Select(x => x.ProjectSolution).ToList();
          List<Sungero.Parties.IContact> contactsInAnotherPS = anotherPS.SelectMany(ps => ps.InvitedExternal.Select(c => c.Contact)).ToList();
          var contactsToDelete = contactsInDeleted
            .Where(e1 => !contactsInAnotherPS.Any(e2 => e2.Id == e1.Id))
            .ToList();
          foreach (var contact in contactsToDelete)
          {
            foreach (var element in _obj.InvitedExternallitiko.Where(x => Equals(contact, x.Contact)).ToList())
            {
              _obj.InvitedExternallitiko.Remove(element);
            }
          }
        }
        #endregion        
      }
    }

    public virtual void ProjectSolutionslitikoAdded(Sungero.Domain.Shared.CollectionPropertyAddedEventArgs e)
    {      
      _added.Number = (_obj.ProjectSolutionslitiko.Max(a => a.Number) ?? 0) + 1;
      
      // Тип голосования      
      if (_obj.Votinglitiko == litiko.Eskhata.Meeting.Votinglitiko.Intramural)
        _added.VotingType = litiko.Eskhata.MeetingProjectSolutionslitiko.VotingType.Intramural;
      if (_obj.Votinglitiko == litiko.Eskhata.Meeting.Votinglitiko.extramural)
        _added.VotingType = litiko.Eskhata.MeetingProjectSolutionslitiko.VotingType.Extramural;
      if (_obj.Votinglitiko == litiko.Eskhata.Meeting.Votinglitiko.NoVoting)
        _added.VotingType = litiko.Eskhata.MeetingProjectSolutionslitiko.VotingType.NoVoting;
    }
  }

  partial class MeetingPresentlitikoSharedCollectionHandlers
  {

    public virtual void PresentlitikoDeleted(Sungero.Domain.Shared.CollectionPropertyDeletedEventArgs e)
    {
      Functions.Meeting.SetQuorum(_obj, _obj.MeetingCategorylitiko);
      
      var deletedEmployee = _deleted.Employee;
      if (deletedEmployee != null && !_obj.Absentlitiko.Any(x => Equals(deletedEmployee, x.Employee)))
        _obj.Absentlitiko.AddNew().Employee = deletedEmployee;
      
    }

    public virtual void PresentlitikoAdded(Sungero.Domain.Shared.CollectionPropertyAddedEventArgs e)
    {
      Functions.Meeting.SetQuorum(_obj, _obj.MeetingCategorylitiko);
    }
  }


  partial class MeetingPresentlitikoSharedHandlers
  {

    public virtual void PresentlitikoEmployeeChanged(litiko.Eskhata.Shared.MeetingPresentlitikoEmployeeChangedEventArgs e)
    {
      var selectedEmployee = e.NewValue;
      if (selectedEmployee != null && selectedEmployee != e.OldValue && _obj.Meeting.Absentlitiko.Any(x => Equals(selectedEmployee, x.Employee)))
      {
        foreach (var element in _obj.Meeting.Absentlitiko.Where(x => Equals(selectedEmployee, x.Employee)).ToList())
        {
          _obj.Meeting.Absentlitiko.Remove(element);
        }
      }      
    }
  }

  partial class MeetingAbsentlitikoSharedHandlers
  {

    public virtual void AbsentlitikoEmployeeChanged(litiko.Eskhata.Shared.MeetingAbsentlitikoEmployeeChangedEventArgs e)
    {
      var selectedEmployee = e.NewValue;
      if (selectedEmployee != null && selectedEmployee != e.OldValue && _obj.Meeting.Presentlitiko.Any(x => Equals(selectedEmployee, x.Employee)))
      {
        foreach (var element in _obj.Meeting.Presentlitiko.Where(x => Equals(selectedEmployee, x.Employee)).ToList())
        {
          _obj.Meeting.Presentlitiko.Remove(element);
        }
      }
    }
  }
}