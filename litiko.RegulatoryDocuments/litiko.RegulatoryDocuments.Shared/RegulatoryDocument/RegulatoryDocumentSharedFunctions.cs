using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.RegulatoryDocuments.RegulatoryDocument;

namespace litiko.RegulatoryDocuments.Shared
{
  partial class RegulatoryDocumentFunctions
  {
    /// <summary>
    /// Установить обязательность свойств в зависимости от заполненных данных.
    /// </summary>
    public override void SetRequiredProperties()
    {
      base.SetRequiredProperties();
        
      _obj.State.Properties.LeadingDocument.IsRequired = Equals(_obj.Type, litiko.RegulatoryDocuments.RegulatoryDocument.Type.IsChange) || 
        Equals(_obj.Type, litiko.RegulatoryDocuments.RegulatoryDocument.Type.IsUpdate);
    }
  }
}