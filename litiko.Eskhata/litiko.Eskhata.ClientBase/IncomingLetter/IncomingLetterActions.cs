using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.IncomingLetter;

namespace litiko.Eskhata.Client
{
  partial class IncomingLetterActions
  {


    public virtual bool CanCreateChecklist(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !_obj.State.IsInserted;
    }

    public virtual void CreateChecklist(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var checklist = Sungero.Docflow.Addendums.Create();
      var checklistKindGuid = DocflowEskhata.PublicConstants.Module.DocumentKindGuids.Checklist;
      checklist.DocumentKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(checklistKindGuid);
      checklist.LeadingDocument = _obj;
      checklist.ShowModal();
      _obj.Relations.Add(Sungero.Docflow.PublicConstants.Module.AddendumRelationName, checklist);
      
    }
  }
}