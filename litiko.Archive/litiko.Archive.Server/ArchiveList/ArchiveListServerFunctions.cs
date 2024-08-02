using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Archive.ArchiveList;

namespace litiko.Archive.Server
{
  partial class ArchiveListFunctions
  {
    /// <summary>
    /// Сформировать отчет Шаблон списока документов в новую версию документа.
    /// </summary>
    [Remote]
    public virtual void FillArchiveListTemplate()
    {
      var report = litiko.Archive.Reports.GetArchiveListTemplate();
      report.Entity = _obj;  
      report.ExportTo(_obj);
      _obj.Save();
    }
    
    /// <summary>
    /// Проверить отражение передачи в архив
    /// </summary>
    [Public, Remote(IsPure = true)]
    public bool IsAllTransferedToArchive()
    {                
      return !_obj.CaseFiles.Where(x => x.CaseFile != null).Any(x => x.CaseFile.Archivelitiko == null);
    }    
  }
}