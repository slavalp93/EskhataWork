using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Order;

namespace litiko.Eskhata.Shared
{
  partial class OrderFunctions
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
        #region В задачу по согласованию приказа с видом = NormativeOrder вложить ВНД, Пояснительную записку и Лист согласования.        
        var normativeOrderKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(litiko.RecordManagementEskhata.PublicConstants.Module.DocumentKindGuids.NormativeOrder);
        if (normativeOrderKind != null && Equals(_obj.DocumentKind, normativeOrderKind) && _obj.LeadingDocument != null && litiko.RegulatoryDocuments.RegulatoryDocuments.Is(_obj.LeadingDocument))
        {
          var regulatoryDocument = litiko.RegulatoryDocuments.RegulatoryDocuments.As(_obj.LeadingDocument);
          if (!approvalTask.OtherGroup.All.Contains(regulatoryDocument))
            approvalTask.OtherGroup.All.Add(regulatoryDocument);
          
          var docKindExplanatoryNote = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(litiko.CollegiateAgencies.PublicConstants.Module.DocumentKindGuids.ExplanatoryNote);
          if (docKindExplanatoryNote != null)
          {
            var explanatoryNotes = litiko.Eskhata.Addendums.GetAll().Where(d => Equals(d.LeadingDocument, regulatoryDocument) && Equals(d.DocumentKind, docKindExplanatoryNote));
            foreach (var document in explanatoryNotes)
              if (!approvalTask.OtherGroup.All.Contains(document))
                approvalTask.OtherGroup.All.Add(document);
          }
            
          var docKindApprovalSheetIRD = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(litiko.RegulatoryDocuments.PublicConstants.Module.DocumentKindGuids.ApprovalSheetIRD);
          if (docKindApprovalSheetIRD != null)
          {
            var appSheets = litiko.Eskhata.Addendums.GetAll().Where(d => Equals(d.LeadingDocument, regulatoryDocument) && Equals(d.DocumentKind, docKindApprovalSheetIRD));
            foreach (var document in appSheets)
              if (!approvalTask.OtherGroup.All.Contains(document))
                approvalTask.OtherGroup.All.Add(document);
          }            
        }
        #endregion        
      }

    }  
  }
}