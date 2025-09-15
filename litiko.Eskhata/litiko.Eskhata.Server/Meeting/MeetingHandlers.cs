using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Meeting;

namespace litiko.Eskhata
{
  partial class MeetingProjectSolutionslitikoProjectSolutionPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> ProjectSolutionslitikoProjectSolutionFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      var meetingCategory = _root.MeetingCategorylitiko;
      if (meetingCategory != null)
        query = query.Where(x => Equals(x.MeetingCategory, meetingCategory) && !x.IncludedInAgenda.Value && 
                            x.InternalApprovalState == Sungero.Docflow.OfficialDocument.InternalApprovalState.Signed 
                           );      
      
      var alreadySelected = _root.ProjectSolutionslitiko.Where(x => x.ProjectSolution != null).Select(x => x.ProjectSolution.Id).ToList();
      query = query.Where(x => !alreadySelected.Contains(x.Id));            
      
      return query;
    }
  }

  partial class MeetingServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      _obj.Location = litiko.Eskhata.Meetings.Resources.DefaultLocation;
    }

    public override void Saving(Sungero.Domain.SavingEventArgs e)
    {

      #region 31.01.2025 из базовой обработки убрано выдачу прав на документы по совещанию
      // base.Saving(e);
      
      // Выдать права на совещание.
      var secretary = _obj.Secretary;
      if (secretary != null && !_obj.AccessRights.IsGrantedDirectly(DefaultAccessRightsTypes.Change, secretary))
        _obj.AccessRights.Grant(secretary, DefaultAccessRightsTypes.Change);
      
      var president = _obj.President;
      if (president != null && !_obj.AccessRights.IsGrantedDirectly(DefaultAccessRightsTypes.Change, president))
        _obj.AccessRights.Grant(president, DefaultAccessRightsTypes.Change);

      var members = _obj.Members.Select(m => m.Member).ToList();
      foreach (var member in members)
        if (!_obj.AccessRights.IsGrantedDirectly(DefaultAccessRightsTypes.Read, member))
          _obj.AccessRights.Grant(member, DefaultAccessRightsTypes.Read);      
      #endregion
            
      if (_obj.State.Properties.ProjectSolutionslitiko.IsChanged && !e.Params.Contains(litiko.CollegiateAgencies.PublicConstants.Module.ParamNames.DontUpdateProjectSolution))
      {
        foreach (var element in _obj.ProjectSolutionslitiko.Where(x => x.ProjectSolution != null))
        {
          var doc = element.ProjectSolution;
          if (!Equals(doc.Meeting, _obj))
            doc.Meeting = _obj;
        }
      }

    }
  }

  partial class MeetingAbsentlitikoEmployeePropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> AbsentlitikoEmployeeFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      var aviabledMemberIDs = _root.Members.Where(x => x.Member != null).Select(m => m.Member.Id).ToList();
      if (_root.Secretary != null && !aviabledMemberIDs.Contains(_root.Secretary.Id))
        aviabledMemberIDs.Add(_root.Secretary.Id);
      if (_root.President != null && !aviabledMemberIDs.Contains(_root.President.Id))
        aviabledMemberIDs.Add(_root.President.Id);
      
      foreach (var element in _root.InvitedEmployeeslitiko.Where(x => x.Employee != null))
      {
        if (!aviabledMemberIDs.Contains(element.Employee.Id))
          aviabledMemberIDs.Add(element.Employee.Id);
      }
      
      // 28.07.2025
      var presidentId = _root?.MeetingCategorylitiko?.President?.Id;
      if (presidentId.HasValue && !aviabledMemberIDs.Contains(presidentId.Value))
        aviabledMemberIDs.Add(_root.MeetingCategorylitiko.President.Id);
      
      query = query.Where(x => aviabledMemberIDs.Contains(x.Id));      
      return query;
    }
  }

  partial class MeetingMembersMemberPropertyFilteringServerHandler<T>
  {

    public override IQueryable<T> MembersMemberFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      query = base.MembersMemberFiltering(query, e);
      query = query.Where(x => Sungero.Company.Employees.Is(x));
      return query;
    }
  }

}