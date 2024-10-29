using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.RegulatoryDocuments.AddRelatedDocInAttStage;

namespace litiko.RegulatoryDocuments
{
  partial class AddRelatedDocInAttStageClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);
      
      _obj.State.Properties.DocumentKind.IsRequired = true;
      _obj.State.Properties.RelationType.IsRequired = true;
      _obj.State.Properties.CopyAccessRights.IsRequired = true;
      _obj.State.Properties.RelationDirection.IsRequired = true;
    }

  }
}