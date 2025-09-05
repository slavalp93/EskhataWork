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
      }      
    }
  }
}
