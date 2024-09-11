using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.CollegiateAgencies.Projectsolution;

namespace litiko.CollegiateAgencies.Client
{
  partial class ProjectsolutionActions
  {

    public override void SendForApproval(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (_obj.LeadingDocument == null)
      {
        e.AddWarning(litiko.CollegiateAgencies.Projectsolutions.Resources.TheExplanatoryNoteFieldIsNotFilledIn);
        return;
      }
      
      base.SendForApproval(e);
    }

    public override bool CanSendForApproval(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanSendForApproval(e);
    }

    public virtual void CreateExplanatoryNote(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var addendum = Functions.Projectsolution.Remote.CreateExplanatoryNote();
      var addendumId = addendum.Id;
      addendum.LeadingDocument = _obj;
      addendum.ShowModal();
      
      if (Sungero.Docflow.Addendums.GetAll().Any(x => x.Id == addendumId))
      {
        _obj.LeadingDocument = addendum;
        _obj.Save();
      }
    }

    public virtual bool CanCreateExplanatoryNote(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void IncludeInAgenda(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (_obj.Meeting != null && _obj.IncludedInAgenda.Value)
      {
        e.AddInformation(Resources.AlreadyIncludedMessageFormat(_obj.Meeting.DisplayName));
        return;
      }
      
      var dialog = Dialogs.CreateInputDialog(Resources.DialogTittle, Resources.DialogText);
      var btnCreateNew = dialog.Buttons.AddCustom(Resources.DialogBtnCreateNew);
      var btnSelectExisting = dialog.Buttons.AddCustom(Resources.DialogBtnSelectExisting);
      
      var result = dialog.Show();
      
      #region Создание нового совещание
      if (result == btnCreateNew)
      {
        var meeting = litiko.Eskhata.Meetings.As(Sungero.Meetings.PublicFunctions.Meeting.Remote.CreateMeeting());
        var meetingId = meeting.Id;
        Functions.Projectsolution.ProcessIncludingInMeeting(_obj, meeting, false);
        
        meeting.ShowModal();
        
        // Если совещание было сохранено
        if (litiko.Eskhata.Meetings.GetAll().Any(x => x.Id == meetingId))
        {
          _obj.Meeting = meeting;
          _obj.IncludedInAgenda = true;
          //_obj.Save();
        }
      }      
      #endregion
      
      #region Включение в существующее совещанее
      if (result == btnSelectExisting)
      {
        // Запланированные совещания по категории из проекта решения
        var plannedMeetings = litiko.Eskhata.Meetings.GetAll()
          .Where(x => x.DateTime >= Calendar.Now)
          .Where(x => x.Status == Sungero.Meetings.Meeting.Status.Active)
          .Where(x => Equals(x.MeetingCategorylitiko, _obj.MeetingCategory));
        var meeting = plannedMeetings.ShowSelect(Resources.ScheduledMeetings);
        
        if (meeting != null)
        {
          if (meeting.AccessRights.CanUpdate())
          {
            Functions.Projectsolution.ProcessIncludingInMeeting(_obj, meeting, false);
            
            _obj.Meeting = meeting;
            _obj.IncludedInAgenda = true;
            //_obj.Save();
          }
          else
          {
            e.AddInformation(Resources.NoRightsToChangeMeetingFormat(meeting.DisplayName));
            return;
          }
          
        }
      }      
      #endregion

    }

    public virtual bool CanIncludeInAgenda(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return Equals(Sungero.Company.Employees.As(Users.Current), _obj.MeetingCategory?.Secretary);
    }

    public virtual void TransferFromAgenda(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      
    }

    public virtual bool CanTransferFromAgenda(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

}