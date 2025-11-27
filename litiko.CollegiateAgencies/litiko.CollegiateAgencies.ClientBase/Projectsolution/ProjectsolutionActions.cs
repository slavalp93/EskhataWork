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
    public virtual void TranslateTjToRu(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var inputs = new List<string>();
      
      inputs.Add(_obj.SubjectTJ ?? "");
      inputs.Add(_obj.ListenedTJ ?? "");
      
      foreach(var d in _obj.Decided)
        inputs.Add(d.DecisionTJ ?? "");

      if (inputs.All(string.IsNullOrWhiteSpace))
      {
        Dialogs.NotifyMessage("Нет данных на таджикском языке для перевода.");
        return;
      }

      try
      {
        var outputs = litiko.DocflowEskhata.PublicFunctions.Module.Remote.TranslateList(inputs, "tj->ru");

        if (outputs.Count != inputs.Count)
        {
          e.AddWarning("Не удалось перевести данные (ошибка сервера).");
          return;
        }

        int i = 0;
        
        if (!string.IsNullOrWhiteSpace(outputs[i])) _obj.Subject = outputs[i];
        i++;

        if (!string.IsNullOrWhiteSpace(outputs[i])) _obj.ListenedRU = outputs[i];
        i++;

        foreach (var decided in _obj.Decided)
        {
          if (!string.IsNullOrWhiteSpace(outputs[i])) decided.DecisionRU = outputs[i];
          i++;
        }

        Dialogs.NotifyMessage("Перевод TJ->RU выполнен.");
      }
      catch (Exception ex)
      {
        e.AddWarning("Ошибка перевода TJ->RU: " + ex.Message);
      }
    }

    public virtual bool CanTranslateTjToRu(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void TranslateRuToEn(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var inputs = new List<string>();
      inputs.Add(_obj.ListenedRU ?? "");

      foreach(var d in _obj.Decided)
        inputs.Add(d.DecisionRU ?? "");

      if (inputs.All(string.IsNullOrWhiteSpace)) return;

      try
      {
        var outputs = litiko.DocflowEskhata.PublicFunctions.Module.Remote.TranslateList(inputs, "ru->en");

        if (outputs.Count != inputs.Count) return;

        int i = 0;
        if (!string.IsNullOrWhiteSpace(outputs[i])) _obj.ListenedEN = outputs[i];
        i++;

        foreach (var decided in _obj.Decided)
        {
          if (!string.IsNullOrWhiteSpace(outputs[i])) decided.DecisionEN = outputs[i];
          i++;
        }
        
        Dialogs.NotifyMessage("Перевод RU->EN выполнен.");
      }
      catch (Exception ex)
      {
        e.AddWarning("Ошибка перевода RU->EN: " + ex.Message);
      }
    }

    public virtual bool CanTranslateRuToEn(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void TranslateRuToTjToEn(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var inputs = new List<string>();
      inputs.Add(_obj.Subject ?? "");
      inputs.Add(_obj.ListenedRU ?? "");
      foreach(var d in _obj.Decided) inputs.Add(d.DecisionRU ?? "");

      if (inputs.All(string.IsNullOrWhiteSpace))
      {
        Dialogs.NotifyMessage("Нет данных на русском языке.");
        return;
      }

      try
      {
        var tjOutputs = litiko.DocflowEskhata.PublicFunctions.Module.Remote.TranslateList(inputs, "ru->tj");
        
        if (tjOutputs.Count == inputs.Count)
        {
          int i = 0;
          if (!string.IsNullOrWhiteSpace(tjOutputs[i])) _obj.SubjectTJ = tjOutputs[i];
          i++;
          if (!string.IsNullOrWhiteSpace(tjOutputs[i])) _obj.ListenedTJ = tjOutputs[i];
          i++;
          foreach(var d in _obj.Decided)
          {
            if (!string.IsNullOrWhiteSpace(tjOutputs[i])) d.DecisionTJ = tjOutputs[i];
            i++;
          }
        }

        TranslateRuToEn(e);

        Dialogs.NotifyMessage("Перевод RU->TJ и RU->EN выполнен.");
      }
      catch (Exception ex)
      {
        e.AddWarning("Ошибка комплексного перевода: " + ex.Message);
      }
    }

    public virtual bool CanTranslateRuToTjToEn(Sungero.Domain.Client.CanExecuteActionArgs e)
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
      if (_obj.LeadingDocument == null && _obj.MeetingCategory.Name != "Заседание Тендерной комиссии")
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