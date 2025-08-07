using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace litiko.NSI.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      // Выдать права на справочники
      var allUsers = Roles.AllUsers;
      if (allUsers != null)
      {
        InitializationLogger.Debug("Init: Grant rights on NSIBases to all users.");
        NSI.NSIBases.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Read);
        NSI.NSIBases.AccessRights.Save();
                
        ResponsibilityMatrices.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Read);
        ResponsibilityMatrices.AccessRights.Save();
                
        ContractsVsPaymentDocs.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Read);
        ContractsVsPaymentDocs.AccessRights.Save();
                
        TaxRates.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Read);
        TaxRates.AccessRights.Save();
        
        FrequencyOfPayments.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Read);
        FrequencyOfPayments.AccessRights.Save();
        
        CurrencyRates.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Read);
        CurrencyRates.AccessRights.Save();        
      }
    }
  }
}
