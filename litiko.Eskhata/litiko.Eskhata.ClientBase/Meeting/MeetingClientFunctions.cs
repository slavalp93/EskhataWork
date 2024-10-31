using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Meeting;
using Sungero.Domain.Shared;
using Sungero.Metadata;

namespace litiko.Eskhata.Client
{
  partial class MeetingFunctions
  {
    /// <summary>
    /// Есть ли права у текущего пользователя на выполнение действия?
    /// </summary>
    [Public]
    public bool CurrentUserHasAccess()
    {
      return Equals(Users.Current, Users.As(_obj.Secretary)) || 
        Users.Current.IncludedIn(Roles.Administrators) || 
        Substitutions.UsersWhoSubstitute(Users.As(_obj.Secretary)).Any(u => Equals(u, Users.Current));
    }
    
    /// <summary>
    /// Перенести вопросы в другое заседание
    /// </summary>       
    public void MoveToAnotherMeeting()
    {
      if (!CurrentUserHasAccess())
      {
        Dialogs.ShowMessage(litiko.Eskhata.Meetings.Resources.NotAccessToAction);
        return;
      }

      if (_obj.ProjectSolutionslitiko.Any())
      {
        var dialog = Dialogs.CreateInputDialog(litiko.Eskhata.Meetings.Resources.MoveToAnotherDialogTittle);
        var btnOk = dialog.Buttons.AddOk();
        var btnCancel = dialog.Buttons.AddCancel();        
        
        // Принудительно увеличиваем ширину диалога.
        var fakeControl = dialog.AddString("123456789012345678910123456789012345678910123456789012345678910", false);
        fakeControl.IsVisible = false;        
        
        var psValue = dialog.AddSelectMany(litiko.Eskhata.Meetings.Resources.Questions, true, litiko.CollegiateAgencies.Projectsolutions.Null)
          .From(_obj.ProjectSolutionslitiko.Where(x => x.ProjectSolution != null).Select(x => x.ProjectSolution));
        
        // Запланированные совещания по категории
        var plannedMeetings = litiko.Eskhata.Meetings.GetAll()
          .Where(x => x.DateTime >= Calendar.Now)
          .Where(x => x.Status == Sungero.Meetings.Meeting.Status.Active)
          .Where(x => Equals(x.MeetingCategorylitiko, _obj.MeetingCategorylitiko))
          .Where(x => x.Id != _obj.Id);
        
        var cntrlInNewMeeting = dialog.AddBoolean(litiko.Eskhata.Meetings.Resources.InNewMeeting, false);
        var cntrlMeeting = dialog.AddSelect(litiko.Eskhata.Meetings.Resources.Meeting, true, litiko.Eskhata.Meetings.Null).From(plannedMeetings);
        
        cntrlInNewMeeting.SetOnValueChanged(x => {
                                         if (x.NewValue.GetValueOrDefault() == true)
                                         {
                                           cntrlMeeting.IsRequired = false;
                                           cntrlMeeting.Value = null;
                                           cntrlMeeting.IsEnabled = false;
                                         }
                                         else
                                         {
                                           cntrlMeeting.IsRequired = true;
                                           cntrlMeeting.IsEnabled = true;
                                         }
                                       });
        
        var result = dialog.Show();
        if (result == btnOk)
        {
          List<litiko.CollegiateAgencies.IProjectsolution> projectSolutions = psValue.Value.ToList();
          var meeting = litiko.Eskhata.Meetings.Null;
          if (!cntrlInNewMeeting.Value.GetValueOrDefault())
          {
            meeting = cntrlMeeting.Value;
          }
          else
          {
            meeting = litiko.Eskhata.Meetings.As(Sungero.Meetings.PublicFunctions.Meeting.Remote.CreateMeeting());
            meeting.MeetingCategorylitiko = _obj.MeetingCategorylitiko;
          }
          
          foreach (var document in projectSolutions)
          {
            // Блокировки?!
            var record = _obj.ProjectSolutionslitiko.Where(x => Equals(x.ProjectSolution, document)).FirstOrDefault();
            _obj.ProjectSolutionslitiko.Remove(record);
                        
            var newRecord = meeting.ProjectSolutionslitiko.AddNew();
            newRecord.ProjectSolution = document;
          }
          
          meeting.ShowModal();         
        }        
      }
    }

    /// <summary>
    /// Выбрать решения и отправить на заочное голосование
    /// </summary>       
    public void SendToVote()
    {
      if (!CurrentUserHasAccess())
      {
        Dialogs.ShowMessage(litiko.Eskhata.Meetings.Resources.NotAccessToAction);
        return;
      }      
      
      if (_obj.ProjectSolutionslitiko.Any(x => x.VotingType == litiko.Eskhata.MeetingProjectSolutionslitiko.VotingType.Extramural))
      {
        var dialog = Dialogs.CreateInputDialog(litiko.Eskhata.Meetings.Resources.SendToVoteDialogTittle);
        var btnOk = dialog.Buttons.AddOk();
        var btnCancel = dialog.Buttons.AddCancel();        
        
        // Принудительно увеличиваем ширину диалога.
        var fakeControl = dialog.AddString("123456789012345678910123456789012345678910123456789012345678910", false);
        fakeControl.IsVisible = false;        
        
        var psValue = dialog.AddSelectMany(litiko.Eskhata.Meetings.Resources.Questions, true, litiko.CollegiateAgencies.Projectsolutions.Null)
          .From(_obj.ProjectSolutionslitiko.Where(x => x.ProjectSolution != null && x.VotingType == litiko.Eskhata.MeetingProjectSolutionslitiko.VotingType.Extramural).Select(x => x.ProjectSolution));
        
        var result = dialog.Show();
        if (result == btnOk)
        {
          List<litiko.CollegiateAgencies.IProjectsolution> projectSolutions = psValue.Value.ToList();
          var task = Functions.Meeting.Remote.CreateTaskForVoting(_obj, projectSolutions);
          
          if (task != null)
            task.ShowModal();
          else
          {
            Dialogs.ShowMessage(litiko.Eskhata.Meetings.Resources.VotingTaskIsNotCreated);
            return;          
          }
        }
      }
    }

    /// <summary>
    /// Обновить результаты голосования
    /// </summary>       
    public void UpdateVoting()
    {      
      if (!CurrentUserHasAccess())
      {
        Dialogs.ShowMessage(litiko.Eskhata.Meetings.Resources.NotAccessToAction);
        return;
      }
      
      if (_obj.ProjectSolutionslitiko.Any())
      {        
        foreach (var element in _obj.ProjectSolutionslitiko.Where(x => x.ProjectSolution != null && x.VotingType == litiko.Eskhata.MeetingProjectSolutionslitiko.VotingType.Extramural || 
                                                                 x.VotingType == litiko.Eskhata.MeetingProjectSolutionslitiko.VotingType.Intramural))
        {
          var projectSolution = element.ProjectSolution;
          
          var votedYes = projectSolution.Voting.Count(x => x.Yes.GetValueOrDefault());
          if (!Equals(element.Yes, votedYes))
            element.Yes = votedYes;
          
          var votedNo = projectSolution.Voting.Count(x => x.No.GetValueOrDefault());
          if (!Equals(element.No, votedNo))
            element.No = votedNo;
          
          var votedAbstained = projectSolution.Voting.Count(x => x.Abstained.GetValueOrDefault());
          if (!Equals(element.Abstained, votedAbstained))
            element.Abstained = votedAbstained;
                    
          if (!projectSolution.Voting.Any(x => !x.Yes.GetValueOrDefault() && !x.No.GetValueOrDefault() && !x.Abstained.GetValueOrDefault()))
          {
            var isAccepted = element.Yes > element.No;
            if (!Equals(element.Accepted, isAccepted))
              element.Accepted = isAccepted;          
          }          
        }
      }
    }

    #region Диалог создания поручений по совещанию
    
    /// <summary>
    /// Отобразить диалог создания поручений по совещанию.
    /// </summary>
    /// <param name="meeting">Совещание.</param>
    /// <param name="e">Аргументы действия, чтобы показывать ошибки валидации.</param>
    [Public]
    public void CreateActionItemsFromMeetingDialog(Sungero.Core.IValidationArgs e)
    {      
      var currentUser = Sungero.Company.Employees.Current;
      if (currentUser == null || currentUser.IsSystem == true)
      {
        Dialogs.NotifyMessage(Sungero.Docflow.OfficialDocuments.Resources.ActionItemCreationDialogLoginAsEmployeeError);
        return;
      }
      
      var minutes = litiko.Eskhata.Functions.Meeting.Remote.GetMinutes(_obj);
      if (minutes == null)
      {
        Dialogs.NotifyMessage(litiko.Eskhata.Meetings.Resources.ProtocolNotFound);
        return;      
      }
      
      var dialogHeightNormal = 160;
      var dialogHeightSmall = 80;
      var existingActionItems = Sungero.Docflow.PublicFunctions.OfficialDocument.Remote.GetCreatedActionItems(minutes);
            
      var draftActionItems = existingActionItems.Where(x => x.Status == Sungero.RecordManagement.ActionItemExecutionTask.Status.Draft &&
                                                       x.ParentTask == null && x.ParentAssignment == null && x.IsDraftResolution != true).ToList();
      
      var dialogItems = new List<Sungero.RecordManagement.IActionItemExecutionTask>();
      
      var hasNotDeletedActionItems = false;
      var hasBeenSent = false;
      var beforeExitDialogText = string.Empty;
      
      var stepExistingItems = existingActionItems.Any();
      
      if (!stepExistingItems)
      {
        if (!this.TryCreateActionItemsFromMeeting(dialogItems, e))
          return;
      }

      var dialog = Dialogs.CreateInputDialog(Sungero.Docflow.OfficialDocuments.Resources.ActionItemCreationDialog);
      dialog.Height = dialogHeightSmall;      
      
      var next = dialog.Buttons.AddCustom(Sungero.Docflow.OfficialDocuments.Resources.ActionItemCreationDialogContinueButtonText);
      var close = dialog.Buttons.AddCustom(Sungero.Docflow.OfficialDocuments.Resources.ActionItemCreationDialogCloseButtonText);
      var cancel = dialog.Buttons.AddCustom(Sungero.Docflow.OfficialDocuments.Resources.CancelButtonText);
      var existingLink = dialog.AddHyperlink(Sungero.Docflow.OfficialDocuments.Resources.ActionItemCreationDialogExistedActionItems);
      existingLink.IsVisible = false;
      var failedLink = dialog.AddHyperlink(Sungero.Docflow.OfficialDocuments.Resources.ActionItemCreationDialogNotFilledActionItems);
      failedLink.IsVisible = false;
      
      // Принудительно увеличиваем ширину диалога для корректного отображения кнопок.
      var fakeControl = dialog.AddString("123", false);
      fakeControl.IsVisible = false;
      
      Action<CommonLibrary.InputDialogRefreshEventArgs> refresh = _ =>
      {
        if (stepExistingItems)
        {
          dialog.Height = dialogHeightNormal;
          next.Name = Sungero.Docflow.OfficialDocuments.Resources.ActionItemCreationDialogContinueButtonText;
          close.IsVisible = false;
          cancel.IsVisible = true;
          
          var descriptionText = string.Empty;
          var prefix = string.Empty;
          var actionItemDraftExist = Sungero.Docflow.OfficialDocuments.Resources.ActionItemCreationDialogDraftExists +
            Environment.NewLine + Environment.NewLine;
          
          if (draftActionItems.Any())
          {
            prefix = actionItemDraftExist;
            descriptionText += Sungero.Docflow.OfficialDocuments.Resources.ActionItemCreationDialogDraftWillBeDelete +
              Environment.NewLine + Environment.NewLine;
          }
          
          if (existingActionItems.Where(с => с.Status != Sungero.RecordManagement.ActionItemExecutionTask.Status.Draft).Any())
          {
            prefix = actionItemDraftExist;
            descriptionText += Sungero.Docflow.OfficialDocuments.Resources.ActionItemCreationDialogInProcessExists_Web;
          }
          
          if (existingActionItems.Count() == 0)
          {
            descriptionText += Sungero.Docflow.OfficialDocuments.Resources.ActionItemCreationDialogNoDraftAndInProgressExist +
              Environment.NewLine + Environment.NewLine;
            descriptionText += Sungero.Docflow.OfficialDocuments.Resources.ActionItemCreationDialogToCreateActionItemsPressNext;
          }
          
          dialog.Text = prefix + descriptionText;
          
          existingLink.IsVisible = existingActionItems.Any();
        }
        else
        {
          close.IsVisible = true;
          cancel.IsVisible = false;
          
          failedLink.IsVisible = NeedFillPropertiesItems(dialogItems).Any();

          var isAllSent = dialogItems.All(d => d.Status != Sungero.RecordManagement.ActionItemExecutionTask.Status.Draft);
          next.IsVisible = !isAllSent;
          
          existingLink.IsVisible = dialogItems.Any();
          existingLink.Title = dialogItems.Any() ?
            Sungero.Docflow.OfficialDocuments.Resources.ActionItemCreationDialogCreatedActionItems :
            existingLink.Title;
          
          next.Name = Sungero.Docflow.OfficialDocuments.Resources.ActionItemCreationDialogSendForExecutionButtonText;
          close.Name  = isAllSent ?
            Sungero.Docflow.OfficialDocuments.Resources.ActionItemCreationDialogCloseButtonText :
            Sungero.Docflow.OfficialDocuments.Resources.ActionItemCreationDialogDeleteAndCloseButtonText;
          
          dialog.Text = string.Empty;
          if (hasNotDeletedActionItems)
            dialog.Text += Sungero.Docflow.OfficialDocuments.Resources.ActionItemCreationDialogSomeActionItemsCouldNotBeDeleted +
              Environment.NewLine + Environment.NewLine;

          if (!hasBeenSent && dialogItems.Any())
            dialog.Text += Sungero.Docflow.OfficialDocuments.Resources.ActionItemCreationDialogSuccessfullyCreated +
              Environment.NewLine + Environment.NewLine;
          
          dialog.Text += Sungero.Docflow.OfficialDocuments.Resources
            .ActionItemCreationDialogCreateCompletedActionItemsFormat(dialogItems.Count) + Environment.NewLine;
          
          if (dialogItems.Where(i => i.Status == Sungero.RecordManagement.ActionItemExecutionTask.Status.InProcess).Any())
            dialog.Text += string.Format("  - {0} - {1}{2}", Sungero.Docflow.OfficialDocuments.Resources.ActionItemCreationDialogSended,
                                         dialogItems.Count(i => i.Status == Sungero.RecordManagement.ActionItemExecutionTask.Status.InProcess),
                                         Environment.NewLine);
          
          if (NeedFillPropertiesItems(dialogItems).Any())
          {
            dialog.Height = dialogHeightNormal;
            var dialogItemsNeedFillProperties = NeedFillPropertiesItems(dialogItems).ToList();
            
            var notFilledAssigneeCount = dialogItemsNeedFillProperties.Count(t => t.Assignee == null);
            if (notFilledAssigneeCount != 0)
              dialog.Text += string.Format("  - {0} - {1}{2}", Sungero.Docflow.OfficialDocuments.Resources.ActionItemCreationDialogNeedFillAssignee,
                                           notFilledAssigneeCount, Environment.NewLine);
            
            var notFilledDeadlineCount = dialogItemsNeedFillProperties.Count(t => t.Deadline == null && t.HasIndefiniteDeadline != true);
            if (notFilledDeadlineCount != 0)
              dialog.Text += string.Format("  - {0} - {1}{2}", Sungero.Docflow.OfficialDocuments.Resources.ActionItemCreationDialogNeedFillDeadline,
                                           notFilledDeadlineCount, Environment.NewLine);
            
            var notFilledActionItemCount = dialogItemsNeedFillProperties.Count(t => string.IsNullOrWhiteSpace(t.ActionItem));
            if (notFilledActionItemCount != 0)
              dialog.Text += string.Format("  - {0} - {1}{2}", Sungero.Docflow.OfficialDocuments.Resources.ActionItemCreationDialogNeedFillSubject,
                                           notFilledActionItemCount, Environment.NewLine);
          }
          
          // В Web перед закрытием диалога вызывается refresh. Исключаем кратковременное отображение некорректных данных в диалоге.
          if (!string.IsNullOrEmpty(beforeExitDialogText))
            dialog.Text = beforeExitDialogText;
        }
        
      };
      
      failedLink.SetOnExecute(() =>
                              {
                                // Список "Требуют заполнения".
                                NeedFillPropertiesItems(dialogItems).ToList().ShowModal();
                                dialogItems = RefreshDialogItems(dialogItems);
                                refresh.Invoke(null);
                              });
      
      existingLink.SetOnExecute(() =>
                                {
                                  // Список "Поручения".
                                  if (stepExistingItems)
                                  {
                                    existingActionItems.ToList().ShowModal();
                                    existingActionItems = Sungero.Docflow.PublicFunctions.OfficialDocument.Remote.GetCreatedActionItems(minutes);
                                    draftActionItems = existingActionItems
                                      .Where(m => m.Status == Sungero.RecordManagement.ActionItemExecutionTask.Status.Draft &&
                                             m.ParentTask == null && m.ParentAssignment == null && m.IsDraftResolution != true).ToList();
                                    refresh.Invoke(null);
                                  }
                                  else
                                  {
                                    // Список "Созданные поручения".
                                    dialogItems.ToList().ShowModal();
                                    dialogItems = RefreshDialogItems(dialogItems);
                                    refresh.Invoke(null);
                                  }
                                });
      
      dialog.SetOnButtonClick(x =>
                              {
                                x.CloseAfterExecute = false;
                                
                                if (x.Button == next)
                                {
                                  if (stepExistingItems)
                                  {
                                    if (this.TryCreateActionItemsFromMeeting(dialogItems, e))
                                    {
                                      if (!TryDeleteActionItemTasks(draftActionItems))
                                        hasNotDeletedActionItems = true;
                                      stepExistingItems = false;
                                      refresh.Invoke(null);
                                    }
                                    else
                                      x.CloseAfterExecute = true;
                                  }
                                  else
                                  {
                                    if (NeedFillPropertiesItems(dialogItems).Any())
                                    {
                                      x.AddError(Sungero.Docflow.OfficialDocuments.Resources.ActionItemCreationDialogNeedFillBeforeSending);
                                    }
                                    else
                                    {
                                      var tasksToStart = NoNeedFillPropertiesItems(dialogItems).ToList();
                                      //Sungero.Docflow.PublicFunctions.OfficialDocument.StartActionItemTasksFromDialog(minutes, tasksToStart);                                      
                                      litiko.Eskhata.PublicFunctions.Meeting.StartActionItemTasksFromDialog(_obj, tasksToStart);
                                      hasBeenSent = true;
                                      x.CloseAfterExecute = true;
                                    }
                                  }
                                }
                                
                                if (x.Button == close)
                                {
                                  x.CloseAfterExecute = true;
                                  
                                  if (dialogItems.All(d => d.Status != Sungero.RecordManagement.ActionItemExecutionTask.Status.Draft))
                                    return;
                                  
                                  if (TryDeleteActionItemTasks(dialogItems.Where(i => i.Status == Sungero.RecordManagement.ActionItemExecutionTask.Status.Draft).ToList()))
                                    Dialogs.NotifyMessage(Sungero.Docflow.OfficialDocuments.Resources.ActionItemCreationDialogDraftWhereDeleted);
                                  else
                                  {
                                    hasNotDeletedActionItems = true;
                                    Dialogs.NotifyMessage(Sungero.Docflow.OfficialDocuments.Resources.ActionItemCreationDialogSomeActionItemsDraftNotDeleted);
                                  }
                                }
                              });
      dialog.SetOnRefresh(refresh);
      dialog.Show();
      
    }
    
    
    /// <summary>
    /// Создать поручения по совещанию.
    /// </summary>
    /// <param name="newActionItems">Созданные поручения.</param>
    /// <param name="e">Аргументы действия.</param>
    /// <returns>True, если поручения созданы успешно. False, если не создано ни одного или были ошибки.</returns>
    private bool TryCreateActionItemsFromMeeting(List<Sungero.RecordManagement.IActionItemExecutionTask> newActionItems, IValidationArgs e)
    {
      try
      {
        newActionItems.Clear();        
        newActionItems.AddRange(litiko.Eskhata.PublicFunctions.Meeting.Remote.CreateActionItemsFromMeeting(_obj));
      }
      catch (AppliedCodeException ex)
      {
        e.AddError(ex.Message);
        return false;
      }
      
      if (newActionItems.Count == 0)
      {
        e.AddInformation(litiko.Eskhata.Meetings.Resources.NoExecutionActionItemsCreated);
        return false;
      }
      return true;
    }
    
    /// <summary>
    /// Удаление поручений, созданных по документу.
    /// </summary>
    /// <param name="tasks">Список задач, которые необходимо удалить.</param>
    /// <returns>True, если все поручения были успешно удалены.</returns>
    private static bool TryDeleteActionItemTasks(List<Sungero.RecordManagement.IActionItemExecutionTask> tasks)
    {
      var hasFailedTask = false;
      // Удаление производится по одной задаче из-за платформенного бага 62797.
      foreach (var task in tasks)
      {
        if (!Functions.Meeting.Remote.TryDeleteActionItemTask(task.Id))
          hasFailedTask = true;
      }
      
      return !hasFailedTask;
    }
    
    /// <summary>
    /// Обновление списка поручений.
    /// </summary>
    /// <param name="items">Список поручений.</param>
    /// <returns>Обновленный список поручений.</returns>
    private static List<Sungero.RecordManagement.IActionItemExecutionTask> RefreshDialogItems(List<Sungero.RecordManagement.IActionItemExecutionTask> items)
    {
      return litiko.Eskhata.Functions.Meeting.Remote.GetActionItemsExecutionTasks(items.Select(t => t.Id).ToList());
    }
    
    /// <summary>
    /// Выбрать из списка недозаполненные поручения.
    /// </summary>
    /// <param name="items">Список поручений.</param>
    /// <returns>Недозаполненные поручения.</returns>
    private static IEnumerable<Sungero.RecordManagement.IActionItemExecutionTask> NeedFillPropertiesItems(List<Sungero.RecordManagement.IActionItemExecutionTask> items)
    {
      return items.Where(t => t.IsCompoundActionItem != true && t.Status == Sungero.RecordManagement.ActionItemExecutionTask.Status.Draft &&
                         (t.Assignee == null || (t.Deadline == null && t.HasIndefiniteDeadline != true) || string.IsNullOrWhiteSpace(t.ActionItem)));
    }
    
    /// <summary>
    /// Выбрать из списка корректно заполненные поручения.
    /// </summary>
    /// <param name="items">Список поручений.</param>
    /// <returns>Корректно заполненные поручения.</returns>
    private static IEnumerable<Sungero.RecordManagement.IActionItemExecutionTask> NoNeedFillPropertiesItems(List<Sungero.RecordManagement.IActionItemExecutionTask> items)
    {
      return items.Where(t => t.IsCompoundActionItem != true && t.Status == Sungero.RecordManagement.ActionItemExecutionTask.Status.Draft &&
                         t.Assignee != null && (t.Deadline != null || t.HasIndefiniteDeadline == true) && !string.IsNullOrWhiteSpace(t.ActionItem) || t.IsCompoundActionItem == true);
      
    }
    
    #endregion           
  }
}