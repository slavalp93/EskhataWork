using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.RegulatoryDocuments.AddRelatedDocInAttStage;

namespace litiko.RegulatoryDocuments.Server
{
  partial class AddRelatedDocInAttStageFunctions
  {
    /// <summary>
    /// Выполнить сценарий.
    /// </summary>
    /// <param name="approvalTask">Задача на согласование по регламенту.</param>
    /// <returns>Результат выполнения сценария.</returns>
    public override Sungero.Docflow.Structures.ApprovalFunctionStageBase.ExecutionResult Execute(Sungero.Docflow.IApprovalTask approvalTask)
    {
      var result = base.Execute(approvalTask);
      var document = approvalTask.DocumentGroup.OfficialDocuments.FirstOrDefault();
      if (document == null)
        return this.GetErrorResult(Resources.DocumentNotFound);            
        
      try
      {
        var relatedDocument = Sungero.Content.ElectronicDocuments.Null;
        if (_obj.RelationDirection == litiko.RegulatoryDocuments.AddRelatedDocInAttStage.RelationDirection.RelatedFrom)
          relatedDocument = document.Relations.GetRelatedFrom(_obj.RelationType.Name)
            .Where(d => Sungero.Docflow.OfficialDocuments.Is(d) && Equals(Sungero.Docflow.OfficialDocuments.As(d).DocumentKind, _obj.DocumentKind))
            .FirstOrDefault();
        else
          relatedDocument = document.Relations.GetRelated(_obj.RelationType.Name)
            .Where(d => Sungero.Docflow.OfficialDocuments.Is(d) && Equals(Sungero.Docflow.OfficialDocuments.As(d).DocumentKind, _obj.DocumentKind))
            .FirstOrDefault();          
        if (relatedDocument != null)
        {
          if (!approvalTask.OtherGroup.All.Contains(relatedDocument))
          {
            approvalTask.OtherGroup.All.Add(relatedDocument);
            approvalTask.Save();
          }
          
          if (_obj.CopyAccessRights.GetValueOrDefault())
          {            
            Sungero.Docflow.PublicFunctions.OfficialDocument.CopyAccessRightsToDocument(document, Sungero.Docflow.OfficialDocuments.As(relatedDocument), Guid.Empty);
            relatedDocument.AccessRights.Save();
          }
        }

      }
      catch (Exception ex)
      {
        result = this.GetRetryResult(string.Empty);
      }
      return result;
    }
  }
}