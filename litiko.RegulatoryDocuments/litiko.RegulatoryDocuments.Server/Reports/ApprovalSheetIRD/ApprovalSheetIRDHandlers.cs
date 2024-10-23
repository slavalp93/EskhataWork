using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.RegulatoryDocuments
{
  partial class ApprovalSheetIRDServerHandlers
  {

    public override void BeforeExecute(Sungero.Reporting.Server.BeforeExecuteEventArgs e)
    {
      //ApprovalSheetIRD
      //ApprovalSheetIRD.Entity.ForWhom.GetValueOrDefault().ToString()
    }

    public virtual IQueryable<litiko.RegulatoryDocuments.IRegulatoryDocument> GetDoc()
    {
      return litiko.RegulatoryDocuments.RegulatoryDocuments.GetAll(x => x.Id == ApprovalSheetIRD.Entity.Id);
    }

  }
}