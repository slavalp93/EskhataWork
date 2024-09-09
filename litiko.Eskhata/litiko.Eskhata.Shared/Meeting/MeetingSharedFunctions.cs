using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Meeting;

namespace litiko.Eskhata.Shared
{
  partial class MeetingFunctions
  {

    /// <summary>
    /// Установить кворум
    /// </summary>       
    public void SetQuorum(litiko.CollegiateAgencies.IMeetingCategory category)
    {
      if (category != null)
      {
        if (!category.Quorum.HasValue)
          _obj.Quorumlitiko = null;        
        else
        {
          int quorumLimit = category.Quorum.Value;
          int presentCount = _obj.Presentlitiko.Count;
          if (presentCount >= quorumLimit)
            _obj.Quorumlitiko = litiko.Eskhata.Meeting.Quorumlitiko.Present;
          else
            _obj.Quorumlitiko = litiko.Eskhata.Meeting.Quorumlitiko.NotPresent;
        }
      }      
    }
    
    /// <summary>
    /// Получить список участников категории совещания.
    /// </summary>
    /// <param name="onlyMembers">Признак отображения только списка участников.</param>
    /// <param name="withJobTitle">Признак отображения должности участников.</param>
    /// <returns>Список участников категории совещания.</returns>
    [Public]
    public virtual string GetMeetingCategoryMembers(bool onlyMembers, bool withJobTitle)
    {
      if (_obj.MeetingCategorylitiko == null)
        return null;
      
      var employees = _obj.MeetingCategorylitiko.Members.Select(x => x.Member).ToList();

      if (_obj.Secretary != null)
        employees.Insert(0, _obj.Secretary);
      if (_obj.President != null)
        employees.Insert(0, _obj.President);

      if (onlyMembers)
        employees = employees.Where(x => !Equals(x, _obj.President))
          .Where(x => !Equals(x, _obj.Secretary))
          .ToList();
      
      //return Sungero.Company.PublicFunctions.Employee.Remote.GetEmployeesNumberedList(employees, withJobTitle);
      var employeesList = new List<string>();
      foreach (Sungero.Company.IEmployee employee in employees)
      {
        string fio = string.Empty;
        if (withJobTitle)
          fio = string.Format("{0} ({1})", employee.Name, employee.JobTitle?.Name);
        else
          fio = employee.Name;
        
        if (!string.IsNullOrEmpty(fio))
          employeesList.Add(fio);
      }
      
      return string.Join(Environment.NewLine, employeesList);
    }    

  }
}