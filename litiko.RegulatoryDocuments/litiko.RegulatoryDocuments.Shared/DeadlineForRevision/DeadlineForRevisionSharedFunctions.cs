using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.RegulatoryDocuments.DeadlineForRevision;

namespace litiko.RegulatoryDocuments.Shared
{
  partial class DeadlineForRevisionFunctions
  {
    /// <summary>
    /// Заполнить имя.
    /// </summary>
    public void FillName()
    {      
      if (_obj.DocumentKind == null)
        return;
      
      var name = string.Empty;
      
      /* Имя в формате:
        <Вид документа> - <Срок> (лет)
       */
            
      name += _obj.DocumentKind.Name;        
      if (_obj.Deadline != null)
        name += " - " + _obj.Deadline + " (лет)";                    
      
      name = Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols(name);
      
      _obj.Name = name;
    }
  }
}