using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using CommonLibrary;

namespace litiko.Archive
{
  partial class ArchiveListTemplateServerHandlers
  {

    public override void BeforeExecute(Sungero.Reporting.Server.BeforeExecuteEventArgs e)
    {      
      ArchiveListTemplate.cntCaseFiles = ArchiveListTemplate.Entity.CaseFiles.Count;
      ArchiveListTemplate.cntCaseFilesInWords = StringUtils.NumberToWords(ArchiveListTemplate.Entity.CaseFiles.Count);
      
      int uniqueBunchCount = ArchiveListTemplate.Entity.CaseFiles.Select(x => x.Bunch).Distinct().Count();
      ArchiveListTemplate.cntBranch = uniqueBunchCount;
      ArchiveListTemplate.cntBranchInWords = StringUtils.NumberToWords((long)uniqueBunchCount);
    }

    public virtual IQueryable<litiko.Archive.IArchiveList> GetDoc()
    {
      return litiko.Archive.ArchiveLists.GetAll().Where(x => x.Id == ArchiveListTemplate.Entity.Id);
    }

  }
}