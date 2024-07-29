using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.CaseFile;

namespace litiko.Eskhata.Server
{
  partial class CaseFileFunctions
  {
    /// <summary>
    /// Получить кол-во документов в деле.
    /// </summary>
    /// <returns>Количество документов.</returns>
    [Public, Remote]
    public int CaseFileDocumentsCount()
    {
      int count = 0;
      AccessRights.AllowRead(
        () =>
        {
          count = OfficialDocuments.GetAll().Where(d => Equals(d.CaseFile, _obj)).Count();
        });      
      
      return count;
      
    }
  }
}