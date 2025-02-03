using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.OfficialDocument;

namespace litiko.Eskhata
{
  partial class OfficialDocumentClientHandlers
  {

    public override void ShowingSignDialog(Sungero.Domain.Client.ShowingSignDialogEventArgs e)
    {
      base.ShowingSignDialog(e);
      
      if (_obj.LastVersionApproved.GetValueOrDefault())
      {
        e.CanApprove = false;
        e.CanEndorse = false;
        e.Hint.Add(litiko.Eskhata.Resources.LastVersionApproved);
      }       
    }

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);
      
      // Архивные реквизиты отображать для: Входящее письмо, Исходящее письмо, Приказ, Распоряжение, Нормативный документ, Документы модуля Совещания
      bool showArchiveProperties = false;
      if (Sungero.RecordManagement.IncomingLetters.Is(_obj) || Sungero.RecordManagement.OutgoingLetters.Is(_obj) || Sungero.RecordManagement.Orders.Is(_obj)
          || Sungero.RecordManagement.CompanyDirectives.Is(_obj) || Sungero.Meetings.Agendas.Is(_obj) || Sungero.Meetings.Minuteses.Is(_obj))
        showArchiveProperties = true;
      
      _obj.State.Properties.Archivelitiko.IsVisible = showArchiveProperties;
      _obj.State.Properties.TransferredToArchivelitiko.IsVisible = showArchiveProperties;
    }

  }
}