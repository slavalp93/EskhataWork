using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.OutgoingDocumentBase;

namespace litiko.Eskhata.Shared
{
  partial class OutgoingDocumentBaseFunctions
  {
    public override void SetRequiredProperties()
    {
      
      base.SetRequiredProperties();
      _obj.State.Properties.Addressees.Properties.DeliveryMethod.IsRequired = _obj.IsManyAddressees == true;
      _obj.State.Properties.DeliveryMethod.IsRequired = _obj.IsManyAddressees == false;
    }
  }
}