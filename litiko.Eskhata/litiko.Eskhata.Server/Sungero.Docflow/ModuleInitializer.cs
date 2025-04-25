using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace litiko.Eskhata.Module.Docflow.Server
{
  public partial class ModuleInitializer
  {

    public override bool IsModuleVisible()
    {
      //return base.IsModuleVisible();
      
      // "Ответственные за настройку регистрации"
      return Users.Current.IncludedIn(Sungero.Docflow.PublicConstants.Module.RoleGuid.RegistrationManagersRole) ||        
        // "Администраторы"
        Users.Current.IncludedIn(Roles.Administrators);      
    }
  }
}
