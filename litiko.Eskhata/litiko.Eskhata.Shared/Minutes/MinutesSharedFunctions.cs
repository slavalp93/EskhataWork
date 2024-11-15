using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Minutes;

namespace litiko.Eskhata.Shared
{
  partial class MinutesFunctions
  {
    /// <summary>
    /// Обработать добавление документа как основного вложения в задачу.
    /// </summary>
    /// <param name="task">Задача.</param>
    /// <remarks>Только для задач, создаваемых пользователем вручную.</remarks>
    [Public]
    public override void DocumentAttachedInMainGroup(Sungero.Workflow.ITask task)
    {      
      var approvalTask = Sungero.Docflow.ApprovalTasks.As(task);
      if (approvalTask != null)
      {
        var meeting = litiko.Eskhata.Meetings.As(_obj.Meeting);
        if (meeting != null)
        {
          approvalTask.OtherGroup.All.Add(meeting);
          
          foreach (var element in meeting.ProjectSolutionslitiko.Where(x => x.ProjectSolution != null))
            if (!approvalTask.OtherGroup.All.Contains(element.ProjectSolution))
              approvalTask.OtherGroup.All.Add(element.ProjectSolution);
        }
      }

    }
  }
}