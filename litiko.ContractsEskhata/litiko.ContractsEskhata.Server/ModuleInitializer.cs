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
  }
}
