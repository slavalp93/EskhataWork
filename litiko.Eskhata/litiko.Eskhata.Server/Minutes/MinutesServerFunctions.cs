using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Minutes;

namespace litiko.Eskhata.Server
{
  partial class MinutesFunctions
  {
    /// <summary>
    /// Сформировать отчет Шаблон протокола в новую версию документа.
    /// </summary>
    [Remote]
    public virtual void FillMinutesTemplate()
    {
      var report = litiko.Eskhata.Reports.GetMinutesTemplate();
      report.Entity = _obj;  
      report.ExportTo(_obj);
      _obj.Save();
    }
  }
}