using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.AcquaintanceFinishAssignment;

namespace litiko.Eskhata.Client
{
  partial class AcquaintanceFinishAssignmentActions
  {
    public override void ShowAcquaintanceReport(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      //base.ShowAcquaintanceReport(e);
      if (!_obj.DocumentGroup.OfficialDocuments.Any())
      {
        e.AddError(AcquaintanceTasks.Resources.DocumentCantBeEmpty);
        return;
      }           
      
      var task = AcquaintanceTasks.As(_obj.Task);
      var document = _obj.DocumentGroup.OfficialDocuments.FirstOrDefault();
      if (document != null && task != null)
      {
        var report = litiko.RecordManagementEskhata.Reports.GetAcquaintanceApprovalSheet();
        report.Document = document;
        report.Task = task;
        report.Open();
      }
    }

    public override bool CanShowAcquaintanceReport(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanShowAcquaintanceReport(e);
    }

    public virtual void ConvertToPDFWithAcqList(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var document = _obj.DocumentGroup.OfficialDocuments.FirstOrDefault();
      
      Eskhata.Module.Docflow.Structures.Module.IConversionToPdfResult result = null;
      result = RecordManagementEskhata.PublicFunctions.Module.ConvertToPdfWithSignatureMark(document, _obj.Task);
      
      if (result.HasErrors)
      {
        Dialogs.NotifyMessage(result.ErrorMessage);
        return;
      }
      e.CloseFormAfterAction = true;
      Dialogs.ShowMessage(Sungero.Docflow.OfficialDocuments.Resources.ConvertionInProgress, litiko.Eskhata.AcquaintanceFinishAssignments.Resources.CloseDocumentAndOpenLater, MessageType.Information);
      return;
    }
    

    public virtual bool CanConvertToPDFWithAcqList(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.DocumentGroup.OfficialDocuments.FirstOrDefault()?.HasVersions == true;
    }

  }

}