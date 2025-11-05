using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.CollegiateAgencies.Server
{
  public class ModuleAsyncHandlers
  {
    /// <summary>
    /// Выполнить задание
    /// </summary>
    /// <param name="args"></param>
    public virtual void PerformAssignment(litiko.CollegiateAgencies.Server.AsyncHandlerInvokeArgs.PerformAssignmentInvokeArgs args)
    {
      if (args.RetryIteration > 100)
      {        
        args.Retry = false;
        return;
      }
      
      var assignment = Eskhata.ApprovalSimpleAssignments.Get(args.assignmentId);
      if (!Locks.TryLock(assignment))
      {
        args.Retry = true;
        Logger.DebugFormat($"Assignment is locked. (ID:{assignment.Id})");
        return;        
      }
      
      try
      {
        assignment.Complete(Eskhata.ApprovalSimpleAssignment.Result.Complete);
        Logger.DebugFormat($"Assignment performed. (ID:{assignment.Id})");
      }
      finally
      {
        Locks.Unlock(assignment);
      }                  
    }

    /// <summary>
    /// Добавить права на изменение документа.
    /// </summary>
    /// <param name="args"></param>    
    public virtual void AddAccessRights(litiko.CollegiateAgencies.Server.AsyncHandlerInvokeArgs.AddAccessRightsInvokeArgs args)
    {
      if (args.RetryIteration > 100)
      {        
        args.Retry = false;
        return;
      }
      
      var document = Sungero.Content.ElectronicDocuments.Get(args.DocId);
      //var employee = Sungero.Company.Employees.Get(args.EmployeeId);
      var recepient = Recipients.Get(args.EmployeeId);
      if (!document.AccessRights.IsGrantedDirectly(DefaultAccessRightsTypes.Change, recepient))
      {
        document.AccessRights.Grant(recepient, DefaultAccessRightsTypes.Change);
        document.AccessRights.Save();
      }
    }
    
    /// <summary>
    /// Добавить результат голосования в проеты решений.
    /// </summary>
    /// <param name="args"></param>
    public virtual void AddVoitingResults(litiko.CollegiateAgencies.Server.AsyncHandlerInvokeArgs.AddVoitingResultsInvokeArgs args)
    {
      if (args.RetryIteration > 100)
      {
        Logger.DebugFormat("AddVoitingResults: превышено количество попыток (100) добавления результата голосования по заданию id - {0}.", args.AssignmentId);
        args.Retry = false;
        return;
      }

      Logger.DebugFormat("AddVoitingResults: Запуск добавления результата голосования по заданию id - {0}. Итерация - {1}", args.AssignmentId, args.RetryIteration);
      long votingAssignmentId = args.AssignmentId;      
      
      var votingAssignment = Sungero.Workflow.Assignments.GetAll(x => x.Id == votingAssignmentId).FirstOrDefault();
      if (votingAssignment == null)
      {
        Logger.DebugFormat("AddVoitingResults: Не найдено задание голосования id - {0}.", votingAssignmentId);
        return;
      }
      
      var votingCollection = new List<litiko.CollegiateAgencies.Structures.Module.IVotingInfo>();                     

      if (litiko.Eskhata.ApprovalSimpleAssignments.Is(votingAssignment))
        votingCollection = litiko.Eskhata.ApprovalSimpleAssignments.As(votingAssignment)
          .Votinglitiko
          .Select(v => litiko.CollegiateAgencies.Structures.Module.VotingInfo.Create(v.Decision,
                                                                                    v.Yes.GetValueOrDefault(),
                                                                                    v.No.GetValueOrDefault(),
                                                                                    v.Abstained.GetValueOrDefault(),
                                                                                    v.Comment))
          .ToList();
      else if (litiko.Eskhata.ApprovalCheckingAssignments.Is(votingAssignment))
        votingCollection = litiko.Eskhata.ApprovalCheckingAssignments.As(votingAssignment)
          .Votinglitiko
          .Select(v => litiko.CollegiateAgencies.Structures.Module.VotingInfo.Create(v.Decision,
                                                                                    v.Yes.GetValueOrDefault(),
                                                                                    v.No.GetValueOrDefault(),
                                                                                    v.Abstained.GetValueOrDefault(),
                                                                                    v.Comment))
          .ToList();
      
      if (!votingCollection.Any())
      {
        Logger.DebugFormat("AddVoitingResults: В задании id - {0} нет голосуемых решений.", votingAssignmentId);
        return;        
      }
      
      var voterEmployee = Sungero.Company.Employees.As(votingAssignment.Performer);
      
      foreach (var element in votingCollection)
      {
        var projectSolution = element.Decision;
        var votedYes = element.Yes;
        var votedNo = element.No;
        var votedAbstained = element.Abstained;        
        var comment = element.Comment;
        
        if (projectSolution != null)
        {
          bool needChange = false;
          var existingRecord = projectSolution.Voting.Where(x => Equals(x.Member, voterEmployee)).FirstOrDefault();
          if (existingRecord != null)
          {
            if (existingRecord.Yes.GetValueOrDefault() != votedYes || existingRecord.No.GetValueOrDefault() != votedNo || existingRecord.Abstained.GetValueOrDefault() != votedAbstained || existingRecord.Comment != comment)
              needChange = true;
          }
          else
            needChange = true;

          if (needChange)
          {
            Logger.DebugFormat("AddVoitingResults: Обновление документа (Проект решения) id = {0} результатами голосования Сотрудника id {1}.", projectSolution.Id, voterEmployee.Id);
            
            if (!Locks.TryLock(projectSolution))
            {
              Logger.DebugFormat("AddVoitingResults: Документ (Проект решения) id = {0} заблокирован. Отправлено на повторную попытку.", projectSolution.Id);
              args.Retry = true;
              return;
            }          
            
            try
            {  
              // Обновление результа голосования по voterEmployee.
              litiko.CollegiateAgencies.IProjectsolutionVoting recordToUpdate;
              if (existingRecord != null)
                recordToUpdate = existingRecord;
              else
                recordToUpdate = projectSolution.Voting.AddNew();
              
              if (!Equals(recordToUpdate.Member, voterEmployee))
                recordToUpdate.Member = voterEmployee;              
              
              if (recordToUpdate.Yes.GetValueOrDefault() != votedYes)
                recordToUpdate.Yes = votedYes;
              
              if (recordToUpdate.No.GetValueOrDefault() != votedNo)
                recordToUpdate.No = votedNo;
              
              if (recordToUpdate.Abstained.GetValueOrDefault() != votedAbstained)
                recordToUpdate.Abstained = votedAbstained;
              
              if (recordToUpdate.Comment != comment)
                recordToUpdate.Comment = comment;
              
              if (projectSolution.State.IsChanged)
              {
                projectSolution.Save();
                Logger.DebugFormat("AddVoitingResults: Обновление документа (Проект решения) id = {0} результатами голосования Сотрудника id {1} успешно.", projectSolution.Id, voterEmployee.Id);
              }
              else
                Logger.DebugFormat("AddVoitingResults: Нет изменений по документу (Проект решения) id = {0} по результатам голосования Сотрудника id {1}.", projectSolution.Id, voterEmployee.Id);                              
              
            }
            catch (Exception ex)
            {
              Logger.ErrorFormat("AddVoitingResults: Не удалось изменить документ (Проект решения) id = {0}. Ошибка: {1} {2}", projectSolution.Id, ex.Message, ex.StackTrace);            
              args.Retry = true;
            }
            finally
            {
              Locks.Unlock(projectSolution);
            }                    
          }
          else
            Logger.DebugFormat("AddVoitingResults: Не требуется обновление документа (Проект решения) id = {0}.", projectSolution.Id);
        }
      }
      Logger.DebugFormat("AddVoitingResults: Завершено добавление результата голосования по заданию id - {1}.", args.AssignmentId);  
    }

  }
}