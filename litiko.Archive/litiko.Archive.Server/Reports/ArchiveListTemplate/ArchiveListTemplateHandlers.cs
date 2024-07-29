using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.Archive
{
  partial class ArchiveListTemplateServerHandlers
  {

    public override void BeforeExecute(Sungero.Reporting.Server.BeforeExecuteEventArgs e)
    {
      //ArchiveListTemplate.Entity.LeadingDocument.RegistrationDate.Value.ToString("YY");
    }

    public virtual IQueryable<litiko.Archive.IArchiveList> GetDoc()
    {
      return litiko.Archive.ArchiveLists.GetAll().Where(x => x.Id == ArchiveListTemplate.Entity.Id);
    }

  }
}