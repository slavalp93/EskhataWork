using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Contract;

namespace litiko.Eskhata.Server
{
  partial class ContractFunctions
  {
    /// <summary>
    /// Создать юрид. заключение.
    /// </summary>
    /// <returns>Юридическое заключение.</returns>
    [Remote, Public]
    public static Sungero.Docflow.IAddendum CreateLegalOpinion()
    {            
      var aviabledDocumentKinds = Sungero.Docflow.PublicFunctions.DocumentKind.GetAvailableDocumentKinds(typeof(Sungero.Docflow.IAddendum));
      var docKind = aviabledDocumentKinds
        .Where(x => x.Status == Sungero.CoreEntities.DatabookEntry.Status.Active && x.Name == "Юридическое заключение")
        .FirstOrDefault();
      
      if (docKind == null)
        return null;
      
      var newDoc = Sungero.Docflow.Addendums.Create();
      newDoc.DocumentKind = docKind;
      return newDoc;
    }    
  }
}