using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace Eskhata.EskhataDocflow.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      CreateDocumentKinds();
      GrantRightsOnDatabooks();
    }
    
    public static void GrantRightsOnDatabooks()
    {
      InitializationLogger.Debug("Init: Grant rights on databooks.");
      EskhataDocflow.IncomingLettersCategories.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Read);
      EskhataDocflow.IncomingLettersCategories.AccessRights.Save();
    }
    
    public static void CreateDocumentKinds()
    {
      InitializationLogger.Debug("Init: Create document kinds.");
      
      var registrable = Sungero.Docflow.DocumentKind.NumberingType.Registrable;
      var numerable = Sungero.Docflow.DocumentKind.NumberingType.Numerable;
      var notNumerable = Sungero.Docflow.DocumentKind.NumberingType.NotNumerable;
      
      var actions = new Sungero.Domain.Shared.IActionInfo[] {
        Sungero.Docflow.OfficialDocuments.Info.Actions.SendActionItem,
        Sungero.Docflow.OfficialDocuments.Info.Actions.SendForFreeApproval,
        Sungero.Docflow.OfficialDocuments.Info.Actions.SendForApproval,
        Sungero.Docflow.OfficialDocuments.Info.Actions.SendForAcquaintance };
      
      Sungero.Docflow.PublicInitializationFunctions.Module.
        CreateDocumentKind(Eskhata.EskhataDocflow.Resources.ChecklistKindName,
                           Eskhata.EskhataDocflow.Resources.ChecklistKindName,
                           notNumerable,
                           Sungero.Docflow.DocumentKind.DocumentFlow.Inner,
                           true,
                           false,
                           Constants.Module.DocumentTypeGuids.Addendum,
                           new Sungero.Domain.Shared.IActionInfo[]
                           {
                             
                           },
                           Constants.Module.DocumentKindGuids.Checklist,
                           false);
    }
  }
}
