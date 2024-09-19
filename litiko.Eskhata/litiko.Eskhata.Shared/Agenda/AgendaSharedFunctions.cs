using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Agenda;

namespace litiko.Eskhata.Shared
{
  partial class AgendaFunctions
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
        }        
      }

    }
    
    /// <summary>
    /// Заполнить имя.
    /// </summary>
    public override void FillName()
    {
      var documentKind = _obj.DocumentKind;
      
      if (documentKind != null && !documentKind.GenerateDocumentName.Value && _obj.Name == Sungero.Docflow.Resources.DocumentNameAutotext)
        _obj.Name = string.Empty;
      
      if (documentKind == null || !documentKind.GenerateDocumentName.Value)
        return;
      
      var name = string.Empty;
      if (_obj.Meeting != null)
      {
        var meeting = _obj.Meeting;
        
        /* Имя в формате:
        <Вид документа> от <дата документа> по <тема совещания>.
         */
        string regDate = _obj.RegistrationDate.HasValue ? _obj.RegistrationDate.Value.ToString("dd.mm.yy") : string.Empty;
        name += string.Format("{0} от {1} по {2}.", documentKind.ShortName, regDate, meeting.Name);
      }
      
      if (string.IsNullOrWhiteSpace(name))
        name = Sungero.Docflow.Resources.DocumentNameAutotext;      
      
      name = Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols(name);
      
      _obj.Name = Sungero.Docflow.PublicFunctions.OfficialDocument.AddClosingQuote(name, _obj);
      
    }    
  }
}