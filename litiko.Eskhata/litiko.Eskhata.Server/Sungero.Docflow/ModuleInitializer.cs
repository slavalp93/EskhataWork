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

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      base.Initializing(e);
      
      // Забрать права на изменение Видов документов у роли "Ответственные за настройку регистрации".
      var registrationManagers = Roles.GetAll().SingleOrDefault(n => n.Sid == Sungero.Docflow.PublicConstants.Module.RoleGuid.RegistrationManagersRole);
      if (registrationManagers != null)
      {
        if (Sungero.Docflow.DocumentKinds.AccessRights.IsGrantedDirectly(DefaultAccessRightsTypes.Change, registrationManagers))
        {
          Sungero.Docflow.DocumentKinds.AccessRights.Revoke(registrationManagers, DefaultAccessRightsTypes.Change);
          Sungero.Docflow.DocumentKinds.AccessRights.Save();
        }        
      }
    }

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
