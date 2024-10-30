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
        var relatedDocuments = new List<Sungero.Content.IElectronicDocument>();
        //var relatedDocument = Sungero.Content.ElectronicDocuments.Null;
        if (_obj.RelationDirection == litiko.RegulatoryDocuments.AddRelatedDocInAttStage.RelationDirection.RelatedFrom)
          relatedDocuments.AddRange(document.Relations.GetRelatedFrom(_obj.RelationType.Name)
                                    .Where(d => Sungero.Docflow.OfficialDocuments.Is(d) && Equals(Sungero.Docflow.OfficialDocuments.As(d).DocumentKind, _obj.DocumentKind)));
        else
          relatedDocuments.AddRange(document.Relations.GetRelated(_obj.RelationType.Name)
                                    .Where(d => Sungero.Docflow.OfficialDocuments.Is(d) && Equals(Sungero.Docflow.OfficialDocuments.As(d).DocumentKind, _obj.DocumentKind)));
        foreach (var relatedDocument in relatedDocuments)
        {
          if (!approvalTask.OtherGroup.All.Contains(relatedDocument))          
            approvalTask.OtherGroup.All.Add(relatedDocument);            
                    
          if (_obj.CopyAccessRights.GetValueOrDefault())
          {            
            Sungero.Docflow.PublicFunctions.OfficialDocument.CopyAccessRightsToDocument(document, Sungero.Docflow.OfficialDocuments.As(relatedDocument), Guid.Empty);
            relatedDocument.AccessRights.Save();
          }
        }
        
        if (approvalTask.State.IsChanged)
          approvalTask.Save();

      }
      catch (Exception ex)
      {
        result = this.GetRetryResult(string.Empty);
      }
      return result;
    }
  }
}