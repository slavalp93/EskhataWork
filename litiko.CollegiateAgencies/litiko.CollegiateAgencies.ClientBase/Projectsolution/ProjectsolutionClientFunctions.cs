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
    /// Перенести вопросы и решения из повестки в протокол
    /// </summary>       
    public void TransferFromAgenda()
    {
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