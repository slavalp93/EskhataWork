using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace litiko.Eskhata.Module.ContractsUI.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      GrantRightsOnFolder();
    }
    
    public static void GrantRightsOnFolder()
    {
      InitializationLogger.Debug("Выдача права на вычисляемую папку 'Реестр мигрированных договоров'");
      
      var contractsResponsible = Roles.GetAll().Where(n => n.Sid == Sungero.Docflow.Constants.Module.RoleGuid.ContractsResponsible).FirstOrDefault();
      
      if (contractsResponsible == null)
        return;
      
      litiko.Eskhata.Module.ContractsUI.SpecialFolders.MigratedContractslitiko.AccessRights.Grant(contractsResponsible, DefaultAccessRightsTypes.FullAccess);
      litiko.Eskhata.Module.ContractsUI.SpecialFolders.MigratedContractslitiko.AccessRights.Save();
    }
  }
}
