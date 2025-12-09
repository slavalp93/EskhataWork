using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.CollegiateAgencies.Projectsolution;

namespace litiko.CollegiateAgencies.Client
{
  partial class ProjectsolutionFunctions
  {

    /// <summary>
    /// Заполнить голосующих
    /// </summary>       
    public void FillInVoters()
    {
      if (_obj.Meeting != null)
      {
        if (_obj.Meeting.Votinglitiko.GetValueOrDefault() == litiko.Eskhata.Meeting.Votinglitiko.extramural || _obj.Meeting.Votinglitiko.GetValueOrDefault() == litiko.Eskhata.Meeting.Votinglitiko.NoVoting)
        {
          Dialogs.ShowMessage(litiko.CollegiateAgencies.Projectsolutions.Resources.OnlyForIntramural);
          return;        
        }
        
        if (!litiko.Eskhata.PublicFunctions.Meeting.CurrentUserHasAccess(_obj.Meeting))
        {
          Dialogs.ShowMessage(litiko.Eskhata.Meetings.Resources.NotAccessToAction);
          return;
        }
                
        _obj.Voting.Clear();
        if (_obj.Meeting.President != null)
          _obj.Voting.AddNew().Member = _obj.Meeting.President;
            
        foreach (var element in _obj.Meeting.Presentlitiko.Where(x => x.Employee != null))
          if (!_obj.Voting.Any(x => Equals(x.Member, element.Employee)))
            _obj.Voting.AddNew().Member = element.Employee;

      }
      
      
    }
    

    /// <summary>
    /// Выбрать все "За"
    /// </summary>  
    public void SelectAllVoting() 
    {
      if (_obj.Meeting.Votinglitiko.GetValueOrDefault() != litiko.Eskhata.Meeting.Votinglitiko.Intramural
          && _obj.Meeting.Votinglitiko.GetValueOrDefault() != litiko.Eskhata.Meeting.Votinglitiko.IntExt)
      {
        Dialogs.ShowMessage(litiko.Eskhata.Meetings.Resources.VotingNotIntramural);
        return;
      }

      foreach (var element in _obj.Voting)
      {
        element.Yes = true;
        element.No = false;
        element.Abstained = false;
        element.State.Properties.Comment.IsRequired = false;
      }
    } 
    
    /// <summary>
    /// Перенести вопросы и решения из повестки в протокол
    /// </summary>       
    public void TransferFromAgenda()
    {
      if (_obj.Meeting == null || !litiko.Eskhata.PublicFunctions.Meeting.CurrentUserHasAccess(_obj.Meeting))
      {
        Dialogs.ShowMessage(litiko.Eskhata.Meetings.Resources.NotAccessToAction);
        return;
      }      
      
      bool isConfirmed = true;
      if (_obj.ListenedRUMinutes != null || _obj.ListenedTJMinutes != null || _obj.ListenedENMinutes != null || _obj.DecidedMinutes.Any())
      {
        var dialog = Dialogs.CreateConfirmDialog("Будут полностью заменены данные текущей вкладки!");
        isConfirmed = dialog.Show();
      }
              
      if (isConfirmed)
      {
        _obj.ListenedRUMinutes = _obj.ListenedRU;
        _obj.ListenedTJMinutes = _obj.ListenedTJ;
        _obj.ListenedENMinutes = _obj.ListenedEN;
        
        _obj.DecidedMinutes.Clear();
        foreach (var element in _obj.Decided)
        {
          var newRecord = _obj.DecidedMinutes.AddNew();
          newRecord.Number = element.Number;
          newRecord.DecisionRU = element.DecisionRU;
          newRecord.DecisionTJ = element.DecisionTJ;
          newRecord.DecisionEN = element.DecisionEN;
        }      
      }      
    }
  }
}