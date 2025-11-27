using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace litiko.Eskhata.Module.Parties.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      CreateDefaultDueDiligenceWebsites();
      CreateDistributionListCounterparty();
      //UpdateBanksFromCBR();
      CreateCounterpartyIndices();
      CreateIdentityDocumentKinds();
    }

    public override bool IsModuleVisible()
    {
      //return base.IsModuleVisible();
            
      // "Ответственные за контрагентов"
      return Users.Current.IncludedIn(Sungero.Docflow.PublicConstants.Module.RoleGuid.CounterpartiesResponsibleRole) ||
        // "Ответственные за договоры"
        Users.Current.IncludedIn(Sungero.Docflow.PublicConstants.Module.RoleGuid.ContractsResponsible) ||
        // "Администраторы"
        Users.Current.IncludedIn(Roles.Administrators);
    }
  }
}
