using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace litiko.ContractsEskhata.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      CreateApprovalRoles();
      CreateDocumentKinds();
      
      CreateOrUpdateRole(Resources.RoleContractsManagersName, Resources.RoleContractsManagersDescription, Constants.Module.RoleGuid.ContractsManagers);                   
    }
    
    #region Роли согласования
    
    /// <summary>
    /// Создать роли согласования.
    /// </summary>    
    public static void CreateApprovalRoles()
    {
      CreateApprovalRole(litiko.ContractsEskhata.ApprovalRole.Type.RespLawyer, "Ответственный юрист");
      CreateApprovalRole(litiko.ContractsEskhata.ApprovalRole.Type.RespAccountant, "Ответственный бухгалтер");
      CreateApprovalRole(litiko.ContractsEskhata.ApprovalRole.Type.RespAHD, "Ответственный сотрудник АХД");
    }
    
    /// <summary>
    /// Создание роли.
    /// </summary>
    public static void CreateApprovalRole(Enumeration roleType, string description)
    {

      var role = ApprovalRoles.GetAll().Where(r => Equals(r.Type, roleType)).FirstOrDefault();
      if (role == null)
      {
        role = ApprovalRoles.Create();
        role.Type = roleType;
      }
      if (role.Description != description)
        role.Description = description;
      
      if (role.State.IsChanged || role.State.IsInserted)
      {
        role.Save();
        InitializationLogger.DebugFormat("Создана/обновлена роль {0}", description);        
      }

    }    
    #endregion
    
    #region Виды документов
    /// <summary>
    /// Создать виды документов.
    /// </summary>    
    public static void CreateDocumentKinds()
    {
      
      #region Settings
      InitializationLogger.Debug("Init: Create document kinds.");
      
      var registrable = Sungero.Docflow.DocumentKind.NumberingType.Registrable;
      var numerable = Sungero.Docflow.DocumentKind.NumberingType.Numerable;
      var notNumerable = Sungero.Docflow.DocumentKind.NumberingType.NotNumerable;
      
      var actions = new Sungero.Domain.Shared.IActionInfo[] {
        Sungero.Docflow.OfficialDocuments.Info.Actions.SendForFreeApproval,
        Sungero.Docflow.OfficialDocuments.Info.Actions.SendForApproval
      };
      #endregion         
      
      #region Юридическое заключение
      var aviabledDocumentKinds = Sungero.Docflow.PublicFunctions.DocumentKind.GetAvailableDocumentKinds(typeof(Sungero.Docflow.IAddendum));
      var docKind = aviabledDocumentKinds
        .Where(x => x.Status == Sungero.CoreEntities.DatabookEntry.Status.Active && x.Name == "Юридическое заключение")
        .FirstOrDefault();
      
      if (docKind == null)
      {
        Sungero.Docflow.PublicInitializationFunctions.Module.
        CreateDocumentKind("Юридическое заключение",
                           "Юридическое заключение",
                           notNumerable,
                           Sungero.Docflow.DocumentKind.DocumentFlow.Inner,
                           true,
                           false,
                           litiko.CollegiateAgencies.PublicConstants.Module.DocumentTypeGuids.Addendum,
                           actions,
                           Constants.Module.DocumentKindGuids.LegalOpinion,
                           false);      
      }            
      #endregion                 
      
    }    
    #endregion    

    /// <summary>
    /// Создать или обновить роль.
    /// </summary>
    /// <param name="roleName">Название роли.</param>
    /// <param name="roleDescription">Описание роли.</param>
    /// <param name="roleGuid">Guid роли.</param>
    /// <returns>Новая роль.</returns>
    [Public]
    public static IRole CreateOrUpdateRole(string roleName, string roleDescription, Guid roleGuid)
    {      
      var role = Roles.GetAll(r => r.Sid == roleGuid).FirstOrDefault();            
      if (role == null)
        role = Roles.GetAll(r => r.Name == roleName).FirstOrDefault();
      
      if (role == null)
      {
        InitializationLogger.DebugFormat("Init: Create Role {0}", roleName);
        role = Roles.Create();
        role.Name = roleName;
        role.Description = roleDescription;
        role.Sid = roleGuid;
        role.IsSystem = true;
      }
      else
      {
        if (role.Sid != roleGuid)
        {
          InitializationLogger.DebugFormat("Role '{0}'(Sid = {1}) update Sid '{2}'", role.Name, role.Sid, roleDescription);
          role.Sid = roleGuid;
        }
        if (role.Name != roleName)
        {
          InitializationLogger.DebugFormat("Role '{0}'(Sid = {1}) renamed as '{2}'", role.Name, role.Sid, roleName);
          role.Name = roleName;          
        }
        if (role.Description != roleDescription)
        {
          InitializationLogger.DebugFormat("Role '{0}'(Sid = {1}) update Description '{2}'", role.Name, role.Sid, roleDescription);
          role.Description = roleDescription;          
        }
        if (!role.IsSystem.GetValueOrDefault())
        {
          InitializationLogger.DebugFormat("Role '{0}'(Sid = {1}) update IsSystem '{2}'", role.Name, role.Sid, roleDescription);
          role.IsSystem = true;
        }
      }
      
      if (role.State.IsInserted || role.State.IsChanged)
        role.Save();
        
      return role;
    }      
    
  }
}
