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
    public virtual void Translate(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      //      var dialog = Dialogs.CreateInputDialog("Введите текст для перевода");
      //      var question = dialog.AddMultilineString("Текст", true);
      //      dialog.Buttons.AddOkCancel();
//
      //      if (dialog.Show() != DialogButtons.Ok || string.IsNullOrWhiteSpace(question.Value))
      //        return;
//
      //      try
      //      {
      //        // Локальный вызов — достаточно передать только вопрос
      //        var answer = litiko.DocflowEskhata.PublicFunctions.Module.Remote.AskGemini(question.Value);
//
      //        if (string.IsNullOrWhiteSpace(answer))
      //          Dialogs.ShowMessage("Пустой ответ Gemini.", MessageType.Warning);
      //        else
      //          Dialogs.ShowMessage("Результат: " + answer, "Смысловой перевод может быть не точным, перепроверьте переведенный текст");
      //      }
      //      catch (Exception ex)
      //      {
      //        e.AddError("Не удалось получить ответ от Gemini: " + ex.Message);
      //      }
      
      try
      {
        string sourseText = "";
        string targetField = "";
        
        var ruText = 
        
        var formViews = Sungero.CoreEntities.FormViews.GetAllMatches(entity
                                                                     
                                                                     // определяем направление перевода
                                                                     if(!string.IsNullOrEmpty(contex.s
                                                                     }
      catch (Exception ex)
      {
        
        throw;
      }
      

    }

    public virtual bool CanTranslate(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void CreateResolution(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var addendum = Functions.Projectsolution.Remote.CreateResolution();
      addendum.LeadingDocument = _obj;
      addendum.OurSignatory = _obj.Meeting?.President;
      addendum.Show();
    }

    public virtual bool CanCreateResolution(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      var roleCreationResolutions = Roles.GetAll(r => r.Sid == Constants.Module.RoleGuid.CreationResolutions).SingleOrDefault();
      return !_obj.State.IsChanged && (Users.Current.IncludedIn(roleCreationResolutions) || Users.Current.IncludedIn(Roles.Administrators));
    }

    public virtual void CreateExtractProtocol(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var addendum = Functions.Projectsolution.Remote.CreateExtractProtocol();
      addendum.LeadingDocument = _obj;
      addendum.Show();
    }

    public virtual bool CanCreateExtractProtocol(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !_obj.State.IsChanged && (Equals(Users.Current, Users.As(_obj.Meeting?.Secretary)) || Users.Current.IncludedIn(Roles.Administrators));
    }


    public override void SendForApproval(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (_obj.LeadingDocument == null)
      {
        e.AddWarning(litiko.CollegiateAgencies.Projectsolutions.Resources.TheExplanatoryNoteFieldIsNotFilledIn);
        return;
      }
      
      if (!_obj.HasVersions || _obj.LastVersion.Body == null)
      {
        Dialogs.ShowMessage(litiko.CollegiateAgencies.Resources.NoVersionMessage, MessageType.Warning);
        throw new OperationCanceledException();
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
      return !_obj.State.IsChanged && (Equals(Users.Current, _obj.Author) || Equals(Users.Current, Users.As(_obj.PreparedBy)) || Users.Current.IncludedIn(Roles.Administrators));
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
        ((Sungero.Domain.Shared.IExtendedEntity)meeting).Params[Constants.Module.ParamNames.DontUpdateProjectSolution] = true;
        var meetingId = meeting.Id;
        Functions.Projectsolution.ProcessIncludingInMeeting(_obj, meeting, false);
        
        meeting.ShowModal();
        
        // Если совещание было сохранено
        if (litiko.Eskhata.Meetings.GetAll().Any(x => x.Id == meetingId))
        {
          _obj.Meeting = meeting;
          _obj.Save();
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
            ((Sungero.Domain.Shared.IExtendedEntity)meeting).Params[Constants.Module.ParamNames.DontUpdateProjectSolution] = true;
            Functions.Projectsolution.ProcessIncludingInMeeting(_obj, meeting, false);
            
            _obj.Meeting = meeting;
            _obj.Save();
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
      return _obj.InternalApprovalState == InternalApprovalState.Signed && (Equals(Sungero.Company.Employees.As(Users.Current), _obj.MeetingCategory?.Secretary) ||
                                                                            Substitutions.UsersWhoSubstitute(Users.As(_obj.MeetingCategory?.Secretary)).Any(u => Equals(u, Users.Current)) ||
                                                                            Users.Current.IncludedIn(Roles.Administrators));
    }

  }

}