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

  }
}
