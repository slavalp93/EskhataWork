using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.NSI.Mapping;

namespace litiko.NSI
{
  partial class MappingClientHandlers
  {

    public virtual void DestCategoryValueInput(litiko.NSI.Client.MappingDestCategoryValueInputEventArgs e)
    {
      _obj.DestName = e.NewValue?.Name;
      _obj.DestId = e.NewValue?.Id;
      _obj.DestExternalId = e.NewValue?.ExternalIdlitiko;      
    }

    public virtual void DestDocumentKindValueInput(litiko.NSI.Client.MappingDestDocumentKindValueInputEventArgs e)
    {
      _obj.DestName = e.NewValue?.Name;
      _obj.DestId = e.NewValue?.Id;
      _obj.DestExternalId = e.NewValue?.ExternalIdlitiko;
    }

    public virtual void EntityTypeValueInput(Sungero.Presentation.EnumerationValueInputEventArgs e)
    {
      Functions.Mapping.RefreshForm(_obj);
    }

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      Functions.Mapping.RefreshForm(_obj);
    }

  }
}