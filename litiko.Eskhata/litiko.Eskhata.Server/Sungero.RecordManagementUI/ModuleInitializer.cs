using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace litiko.Eskhata.Module.RecordManagementUI.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      base.Initializing(e);
     
      var clerkRole = Roles.GetAll().Where(r => r.Sid == Sungero.Docflow.Constants.Module.RoleGuid.ClerksRole).FirstOrDefault();
      
      litiko.Eskhata.Module.RecordManagementUI.SpecialFolders.OnRegisterOrdersAndCompanyDirectiveslitiko.AccessRights.Grant(clerkRole, DefaultAccessRightsTypes.Read);
      litiko.Eskhata.Module.RecordManagementUI.SpecialFolders.OnRegisterOrdersAndCompanyDirectiveslitiko.AccessRights.Save();
      InitializationLogger.Debug("Выданы права на вычисляемую папку 'Регистрация приказов и распоряжений'");
       
      litiko.Eskhata.Module.RecordManagementUI.SpecialFolders.OnRegisterIncomingLetterslitiko.AccessRights.Grant(clerkRole, DefaultAccessRightsTypes.Read);
      litiko.Eskhata.Module.RecordManagementUI.SpecialFolders.OnRegisterIncomingLetterslitiko.AccessRights.Save();
      InitializationLogger.Debug("Выданы права на вычисляемую папку 'Регистрация входящих писем'");
      
      litiko.Eskhata.Module.RecordManagementUI.SpecialFolders.OnRegisterOutgoingLetterslitiko.AccessRights.Grant(clerkRole, DefaultAccessRightsTypes.Read);
      litiko.Eskhata.Module.RecordManagementUI.SpecialFolders.OnRegisterOutgoingLetterslitiko.AccessRights.Save();
      InitializationLogger.Debug("Выданы права на вычисляемую папку 'Регистрация исходящих писем'");
      
      litiko.Eskhata.Module.RecordManagementUI.SpecialFolders.OnRegisterRegulatoryDocumentslitiko.AccessRights.Grant(clerkRole, DefaultAccessRightsTypes.Read);
      litiko.Eskhata.Module.RecordManagementUI.SpecialFolders.OnRegisterRegulatoryDocumentslitiko.AccessRights.Save();
      InitializationLogger.Debug("Выданы права на вычисляемую папку 'Регистрация ВНД'");
     
      
    }
  }
}
