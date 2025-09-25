using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Integration.ExchangeDocument;

namespace litiko.Integration.Shared
{
  partial class ExchangeDocumentFunctions
  {

    /// <summary>
    /// 
    /// </summary>       
    public void Function()
    {
      
    }
    /// <summary>
    /// Заполнить имя документа.
    /// </summary>    
    public virtual void FillName()
    {
      var name = string.Empty;
      var integrationSystem = _obj.IntegrationSystem;
      var integrationMethod = _obj.IntegrationMethod;
      
      if (integrationSystem != null && integrationMethod != null)
        name = string.Format("{0} - {1} - {2}", integrationSystem.Name, integrationMethod.Name, _obj.Id);
      
      if (_obj.Name != name)
        _obj.Name = name;            
    }
  }
}