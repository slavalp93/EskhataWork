using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace litiko.Integration.Server
{
  public partial class ModuleInitializer
  {

    public override bool IsModuleVisible()
    {
      return Users.Current.IncludedIn(Constants.Module.SynchronizationResponsibleRoleGuid) || Users.Current.IncludedIn(Roles.Administrators);
    }

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {      
      CreateIntegrationSystem("ABS");
      
      // Some Comment Test
      
      var integrationSystem = IntegrationSystems.GetAll(r => r.Name == "ABS").FirstOrDefault();
      if (integrationSystem != null)
      {
        CreateIntegrationMethod(Constants.Module.IntegrationMethods.R_DR_GET_DEPART, integrationSystem);
        CreateIntegrationMethod(Constants.Module.IntegrationMethods.R_DR_GET_EMPLOYEES, integrationSystem);
        CreateIntegrationMethod(Constants.Module.IntegrationMethods.R_DR_GET_BUSINESSUNITS, integrationSystem);
        CreateIntegrationMethod(Constants.Module.IntegrationMethods.R_DR_GET_COMPANY, integrationSystem);
        CreateIntegrationMethod(Constants.Module.IntegrationMethods.R_DR_GET_BANK, integrationSystem);
        CreateIntegrationMethod(Constants.Module.IntegrationMethods.R_DR_GET_PERSON, integrationSystem);
        CreateIntegrationMethod(Constants.Module.IntegrationMethods.R_DR_GET_COUNTRIES, integrationSystem);
        CreateIntegrationMethod(Constants.Module.IntegrationMethods.R_DR_GET_OKOPF, integrationSystem);
        CreateIntegrationMethod(Constants.Module.IntegrationMethods.R_DR_GET_OKFS, integrationSystem);
        CreateIntegrationMethod(Constants.Module.IntegrationMethods.R_DR_GET_OKONH, integrationSystem);
        CreateIntegrationMethod(Constants.Module.IntegrationMethods.R_DR_GET_OKVED, integrationSystem);
        CreateIntegrationMethod(Constants.Module.IntegrationMethods.R_DR_GET_COMPANYKINDS, integrationSystem);
        CreateIntegrationMethod(Constants.Module.IntegrationMethods.R_DR_GET_TYPESOFIDCARDS, integrationSystem);
        CreateIntegrationMethod(Constants.Module.IntegrationMethods.R_DR_GET_ECOLOG, integrationSystem);
        CreateIntegrationMethod(Constants.Module.IntegrationMethods.R_DR_GET_MARITALSTATUSES, integrationSystem);       
        CreateIntegrationMethod(Constants.Module.IntegrationMethods.R_DR_GET_CURRENCY_RATES, integrationSystem);
        CreateIntegrationMethod(Constants.Module.IntegrationMethods.R_DR_GET_PAYMENT_REGIONS, integrationSystem);
        CreateIntegrationMethod(Constants.Module.IntegrationMethods.R_DR_GET_TAX_REGIONS, integrationSystem);
        CreateIntegrationMethod(Constants.Module.IntegrationMethods.R_DR_GET_CONTRACT_VID, integrationSystem);
        CreateIntegrationMethod(Constants.Module.IntegrationMethods.R_DR_GET_CONTRACT_TYPE, integrationSystem);        
      }
      
      GrantRightsOnEntities();
    }
    
    /// <summary>
    /// Создать Интеграционную систему.
    /// </summary>
    /// <param name="name">Наименование.</param>    
    public static void CreateIntegrationSystem(string name)
    {                  
      var isExist = IntegrationSystems.GetAll(r => r.Name == name).Any();
      if (!isExist)
      {
        InitializationLogger.DebugFormat("Init: Create Integration system {0}", name);
        var newRecord = IntegrationSystems.Create();
        newRecord.Name = name;
        newRecord.Save();
      }            
    }
    
    /// <summary>
    /// Создать Метод интеграции.
    /// </summary>
    /// <param name="name">Наименование.</param>    
    public static void CreateIntegrationMethod(string name, IIntegrationSystem integrationSystem)
    {                  
      var method = IntegrationMethods.GetAll(r => Equals(r.IntegrationSystem, integrationSystem) && r.Name == name).FirstOrDefault();
      if (method == null)
      {
        InitializationLogger.DebugFormat("Init: Create Integration method {0}", name);
        method = IntegrationMethods.Create();        
      }
      
      if (!Equals(method.IntegrationSystem, integrationSystem))
        method.IntegrationSystem = integrationSystem;
        
      if (method.Name != name)
        method.Name = name;
      
      if (method.State.IsChanged || method.State.IsInserted)
        method.Save();
    } 

    /// <summary>
    /// Выдать права на сущности модуля.
    /// </summary>
    public static void GrantRightsOnEntities()
    {
      InitializationLogger.Debug("Init: Grant rights on entities.");
      
      // "Ответственные за синхронизацию с учетными системами"
      var roleSynchronizationResponsible = Roles.GetAll().Where(x => x.Sid == Constants.Module.SynchronizationResponsibleRoleGuid).FirstOrDefault();
      if (roleSynchronizationResponsible != null)
      {
        ExchangeDocuments.AccessRights.Grant(roleSynchronizationResponsible, DefaultAccessRightsTypes.FullAccess);
        ExchangeDocuments.AccessRights.Save();
        IntegrationSystems.AccessRights.Grant(roleSynchronizationResponsible, DefaultAccessRightsTypes.FullAccess);
        IntegrationSystems.AccessRights.Save();
        IntegrationMethods.AccessRights.Grant(roleSynchronizationResponsible, DefaultAccessRightsTypes.Change);
        IntegrationMethods.AccessRights.Save();
        ExchangeQueues.AccessRights.Grant(roleSynchronizationResponsible, DefaultAccessRightsTypes.FullAccess);
        ExchangeQueues.AccessRights.Save();
      }
      
      // "Ответственные за контрагентов"
      var roleCounterpartiesResponsible = Roles.GetAll().Where(x => x.Sid == Sungero.Docflow.PublicConstants.Module.RoleGuid.CounterpartiesResponsibleRole).FirstOrDefault();
      if (roleCounterpartiesResponsible != null)
      {
        ExchangeDocuments.AccessRights.Grant(roleCounterpartiesResponsible, DefaultAccessRightsTypes.Create);
        ExchangeDocuments.AccessRights.Save();
        IntegrationSystems.AccessRights.Grant(roleCounterpartiesResponsible, DefaultAccessRightsTypes.Read);
        IntegrationSystems.AccessRights.Save();
        IntegrationMethods.AccessRights.Grant(roleCounterpartiesResponsible, DefaultAccessRightsTypes.Read);
        IntegrationMethods.AccessRights.Save();
        ExchangeQueues.AccessRights.Grant(roleCounterpartiesResponsible, DefaultAccessRightsTypes.Create);
        ExchangeQueues.AccessRights.Save();
      }      
    }
  }
}
