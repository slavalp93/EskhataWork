using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Shared;
using Sungero.Metadata;
using litiko.CollegiateAgencies.ApprovalVotingTaskStage;

namespace litiko.CollegiateAgencies.Server
{
  partial class ApprovalVotingTaskStageFunctions
  {
    /// <summary>
    /// Создание подзадач по голосованию в процессе согласования по регламенту.
    /// </summary>
    /// <param name="approvalTask">Задача на согласование по регламенту.</param>
    /// <returns>Результат выполнения кода.</returns>
    public override Sungero.Docflow.Structures.ApprovalFunctionStageBase.ExecutionResult Execute(Sungero.Docflow.IApprovalTask approvalTask)
    {
      Logger.DebugFormat("CreateVotingTaskStage. Start, approval task (ID={0}) (StartId={1}) (Iteration={2}) (StageNumber={3}).",
                         approvalTask.Id, approvalTask.StartId, approvalTask.Iteration, approvalTask.StageNumber);            
      
      #region Предпроверки
      var document = approvalTask.DocumentGroup.OfficialDocuments.SingleOrDefault();
      if (document == null)
      {
        Logger.ErrorFormat("CreateVotingTaskStage. Primary document not found. Approval task (ID={0}) (StartId={1}) (Iteration={2}) (StageNumber={3}).",
                           approvalTask.Id, approvalTask.StartId, approvalTask.Iteration, approvalTask.StageNumber);
        return this.GetErrorResult(Sungero.Docflow.Resources.PrimaryDocumentNotFoundError);
      }
      
      var desigions = Eskhata.ApprovalTasks.As(approvalTask).Desigionslitiko.Select(x => x.Desigion).ToList();
      if (!desigions.Any())
      {
        Logger.DebugFormat("CreateVotingTaskStage. Desigions is empty. Approval task (ID={0}) (StartId={1}) (Iteration={2}) (StageNumber={3}).",
                           approvalTask.Id, approvalTask.StartId, approvalTask.Iteration, approvalTask.StageNumber);
        return this.GetErrorResult("Desigions is empty.");
      }

      var voters = Eskhata.ApprovalTasks.As(approvalTask).Voterslitiko.Select(x => x.Employee).ToList();
      if (!voters.Any())
      {
        Logger.DebugFormat("CreateVotingTaskStage. Voters is empty. Approval task (ID={0}) (StartId={1}) (Iteration={2}) (StageNumber={3}).",
                           approvalTask.Id, approvalTask.StartId, approvalTask.Iteration, approvalTask.StageNumber);
        return this.GetErrorResult("Voters is empty.");      
      }
      
      var firstDesigion = desigions.FirstOrDefault();
      var availableApprovalRules = Sungero.Docflow.PublicFunctions.OfficialDocument.Remote.GetApprovalRules(firstDesigion);
      var votingApprovalRule = availableApprovalRules.Where(r => r.Name == litiko.CollegiateAgencies.PublicConstants.Module.VotingApprovalRule2Name).FirstOrDefault();
      if (votingApprovalRule == null)
      {
        Logger.DebugFormat("CreateVotingTaskStage. ApprovalRule not found. Approval task (ID={0}) (StartId={1}) (Iteration={2}) (StageNumber={3}).",
                           approvalTask.Id, approvalTask.StartId, approvalTask.Iteration, approvalTask.StageNumber);
        return this.GetErrorResult("ApprovalRule not found.");         
      }
      
      #endregion
                
      var meeting = CollegiateAgencies.Projectsolutions.As(firstDesigion).Meeting;
      foreach (var desigion in desigions)
      {
        //var subTask = Eskhata.ApprovalTasks.CreateAsSubtask(approvalTask);
        var subTask = Eskhata.ApprovalTasks.Create();
        try
        {          
          subTask.Desigionslitiko.AddNew().Desigion = desigion;
          
          subTask.Author = approvalTask.Author;
          subTask.DocumentGroup.All.Add(desigion);
          
          /*
          foreach (var element in subTask.AddendaGroup.All)
            subTask.AddendaGroup.All.Remove(element);           
          */
         
          if (meeting != null && !subTask.OtherGroup.All.Contains(meeting))
            subTask.OtherGroup.All.Add(meeting);                    
          
          subTask.Subject = "Голосование: " + litiko.CollegiateAgencies.Projectsolutions.As(desigion).Name;
          subTask.ActiveText = "Прошу проголосовать.";
          subTask.ApprovalRule = votingApprovalRule;
          subTask.MainApprovalTasklitiko = Eskhata.ApprovalTasks.As(approvalTask);
          
          subTask.Start();
          Logger.DebugFormat("CreateVotingTaskStage. Subtask started. Voting subtask (ID={0}), approval task (ID={1}) (StartId={2}) (Iteration={3}) (StageNumber={4}) for document (ID={5})",
                             subTask.Id, approvalTask.Id, approvalTask.StartId, approvalTask.Iteration, approvalTask.StageNumber, document.Id);          
          
          // Добавить связь с главной задачей
          //LinkApprovalTaskAndApprovalSubTask(Eskhata.ApprovalTasks.As(approvalTask), subTask);
          
        }
        catch (Exception ex)
        {
          Logger.ErrorFormat("CreateVotingTaskStage. Subtask start error. Voting subtask (ID={0}), approval task (ID={1}) (StartId={2}) (Iteration={3}) (StageNumber={4}) for document (ID={5})",
                             ex, subTask.Id, approvalTask.Id, approvalTask.StartId, approvalTask.Iteration, approvalTask.StageNumber, document.Id);
          return this.GetErrorResult(ex.Message);
        }          
      }
      
      return this.GetSuccessResult();
    }
    
    /// <summary>
    /// Проверить состояние этапа выполнения сценария.
    /// </summary>
    /// <param name="approvalTask">Задача на согласование по регламенту.</param>
    /// <returns>Состояние этапа.</returns>    
    public override Sungero.Docflow.Structures.ApprovalFunctionStageBase.ExecutionResult CheckCompletionState(Sungero.Docflow.IApprovalTask approvalTask)
    {
      var logPrefix = "ApprovalVotingTaskStage. CheckCompletionState";
      Logger.DebugFormat("{0}. Start. ApprovalTask Id: {1}", logPrefix, approvalTask.Id);             
            
      var hasActiveSubTasks = Eskhata.ApprovalTasks.GetAll()
        .Any(t => t.MainApprovalTasklitiko != null
               && t.MainApprovalTasklitiko.Id == approvalTask.Id
               && t.Status == Eskhata.ApprovalTask.Status.InProcess);
      
      if (!hasActiveSubTasks)
      {
        Logger.DebugFormat("{0}. Success. ApprovalTask Id: {1}.", logPrefix, approvalTask.Id);
        return this.GetSuccessResult();
      }
      else
        Logger.DebugFormat("{0}. Not success - there are active subtasks. ApprovalTask Id: {1}.", logPrefix, approvalTask.Id);
      
      return this.GetRetryResult(string.Empty);
    }    
    
    /*
    /// <summary>
    /// Связать основную задачу Голосование и задачу Голосование по решению.
    /// </summary>
    /// <param name="approvalTask">Основная задача Голосование.</param>
    /// <param name="subTask">Задача Голосование по решению.</param>
    public static void LinkApprovalTaskAndApprovalSubTask(Eskhata.IApprovalTask approvalTask, Eskhata.IApprovalTask subTask)
    {
      var externalLink = Sungero.Domain.ModuleFunctions.CreateExternalLink();
      externalLink.EntityId = approvalTask.Id;
      externalLink.EntityTypeGuid = approvalTask.GetEntityMetadata().GetOriginal().NameGuid;
      externalLink.ExternalEntityId = subTask.Id.ToString();
      externalLink.ExternalEntityTypeId = subTask.GetEntityMetadata().GetOriginal().NameGuid.ToString();
      externalLink.AdditionalInfo = GetApprovalTaskAdditionalInfoKey(approvalTask);
      externalLink.IsDeleted = false;
      externalLink.Save();
      
      Logger.DebugFormat("ApprovalVotingTaskStage. Approval task (ID={0}) (StartId={1}) (Iteration={2}) (StageNumber={3}) linked with approval task (ID={4}))",
                         approvalTask.Id, approvalTask.StartId, approvalTask.Iteration, approvalTask.StageNumber, subTask.Id);
    }
    
    /// <summary>
    /// Сформировать дополнительную информацию для ссылки, связывающей задачу Голосование и задачу Голосование по решению.
    /// </summary>
    /// <param name="approvalTask">Задача Голосование.</param>
    /// <returns>Дополнительная информация для ссылки, связывающей задачу Голосование и задачу Голосование по решению.</returns>
    public static string GetApprovalTaskAdditionalInfoKey(Eskhata.IApprovalTask approvalTask)
    {
      return GetApprovalTaskAdditionalInfoKey(approvalTask, approvalTask.StageNumber);
    }

    /// <summary>
    /// Сформировать дополнительную информацию для ссылки, связывающей задачу Голосование и задачу Голосование по решению.
    /// </summary>
    /// <param name="approvalTask">Задача Голосование.</param>
    /// <param name="stageNumber">Номер этапа.</param>
    /// <returns>Дополнительная информация для ссылки, связывающей задачу Голосование и задачу Голосование по решению.</returns>
    public static string GetApprovalTaskAdditionalInfoKey(Eskhata.IApprovalTask approvalTask, int? stageNumber)
    {
      return string.Format("{0}_{1}_{2}_{3}", Constants.ApprovalVotingTaskStage.ApprovalVotingTaskStageLinkCode,
                           approvalTask.StartId, approvalTask.Iteration, stageNumber);
    }
    
    /// <summary>
    /// Найти ссылки задач Голосование по решению для задачи Голосование.
    /// </summary>
    /// <param name="approvalTask">Задача на согласование по регламенту.</param>
    /// <returns>Ссылка с задачей на рассмотрение документа.</returns>
    [Public]
    public static List<Sungero.Domain.Shared.IExternalLink> GetApprovalTaskExternalLink(IApprovalTask approvalTask)
    {
      var additionalInfo = GetApprovalTaskAdditionalInfoKey(approvalTask);
      return Sungero.Docflow.PublicFunctions.Module.GetExternalLinks(approvalTask, additionalInfo);
    }
    
    /// <summary>
    /// Найти ссылку с задачей на рассмотрение документа для задачи на согласование по регламенту.
    /// </summary>
    /// <param name="approvalTask">Задача на согласование по регламенту.</param>
    /// <param name="stageNumber">Номер этапа.</param>
    /// <returns>Ссылка с задачей на рассмотрение документа.</returns>
    [Public]
    public static Sungero.Domain.Shared.IExternalLink GetApprovalTaskExternalLink(IApprovalTask approvalTask, int? stageNumber)
    {
      var additionalInfo = GetApprovalTaskAdditionalInfoKey(approvalTask, stageNumber);
      return Docflow.PublicFunctions.Module.GetExternalLink(approvalTask, additionalInfo);
    }    
    */
  }
}