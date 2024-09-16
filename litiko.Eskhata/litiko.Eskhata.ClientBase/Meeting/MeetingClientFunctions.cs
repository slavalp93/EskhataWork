using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Meeting;

namespace litiko.Eskhata.Client
{
  partial class MeetingFunctions
  {

    /// <summary>
    /// Выбрать решения и отправить на заочное голосование
    /// </summary>       
    public void SendToVote()
    {
      if (_obj.ProjectSolutionslitiko.Any())
      {
        var dialog = Dialogs.CreateInputDialog("Выбор вопросов для голосования");
        var btnOk = dialog.Buttons.AddOk();
        var btnCancel = dialog.Buttons.AddCancel();
        
        var projectSolutions = dialog.AddSelectMany("Вопросы для отправки", true, litiko.CollegiateAgencies.Projectsolutions.Null).From(_obj.ProjectSolutionslitiko.Where(x => x.ProjectSolution != null).Select(x => x.ProjectSolution));
        
        var result = dialog.Show();
        if (result == btnOk)
        {
          Dialogs.NotifyMessage(projectSolutions.Value.Count().ToString());
        }
      }
    }

    /// <summary>
    /// Обновить результаты голосования
    /// </summary>       
    public void UpdateVoting()
    {
      if (_obj.ProjectSolutionslitiko.Any())
      {        
        foreach (var element in _obj.ProjectSolutionslitiko.Where(x => x.ProjectSolution != null))
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
                    
          var isAccepted = element.Yes > element.No;
          if (!Equals(element.Accepted, isAccepted))
            element.Accepted = isAccepted;
        }
      }
    }

  }
}