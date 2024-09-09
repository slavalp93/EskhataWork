using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Agenda;

namespace litiko.Eskhata.Server
{
  partial class AgendaFunctions
  {
    /// <summary>
    /// Сформировать отчет Шаблон повестки в новую версию документа.
    /// </summary>
    [Remote]
    public virtual void FillAgendaTemplate()
    {
      var report = litiko.Eskhata.Reports.GetAgendaTemplate();
      report.Entity = _obj;  
      report.ExportTo(_obj);
      _obj.Save();
    }
  }
}