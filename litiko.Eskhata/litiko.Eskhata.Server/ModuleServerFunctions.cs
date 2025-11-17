using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.Eskhata.Server
{
  public class ModuleFunctions
  {

    /// <summary>
    /// Создание документа Список документов для передачи в архив по списку дел
    /// </summary>
    /// <param name="caseFiles">Список дел</param>
    /// <returns>Документ Список документов для передачи в архив</returns>
    [Public, Remote]
    public litiko.Archive.IArchiveList CreateArchiveListByCaseFiles(List<litiko.Eskhata.ICaseFile> caseFiles)
    {
      var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Archive.PublicConstants.Module.DocumentKindGuids.ArchiveList);
      if (docKind == null)
        return null;
      
      var doc = litiko.Archive.ArchiveLists.Create();
      doc.DocumentKind = docKind;
      foreach (var caseFile in caseFiles)
      {
        var newRecord = doc.CaseFiles.AddNew();
        newRecord.CaseFile = caseFile;
      }
       
      return doc;
    }
      
  }
}