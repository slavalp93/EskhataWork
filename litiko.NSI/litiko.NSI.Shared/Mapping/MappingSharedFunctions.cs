using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.NSI.Mapping;

namespace litiko.NSI.Shared
{
  partial class MappingFunctions
  {
    /// <summary>
    /// Обновить карточку.
    /// </summary>
    public void RefreshForm()
    {      
      _obj.State.Properties.DestDocumentKind.IsVisible = _obj.EntityType == NSI.Mapping.EntityType.DocumentKind;
      _obj.State.Properties.DestCategory.IsVisible = _obj.EntityType == NSI.Mapping.EntityType.ContrCategory;
    }
  }
}