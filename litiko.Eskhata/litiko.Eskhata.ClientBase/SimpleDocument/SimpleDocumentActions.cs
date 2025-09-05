using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.SimpleDocument;

namespace litiko.Eskhata.Client
{
  partial class SimpleDocumentActions
  {
    public virtual void CreateProjectSolutionlitiko(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var newDoc = litiko.CollegiateAgencies.PublicFunctions.Projectsolution.Remote.CreateProjectsolution();  
      
      // Через параметр передаем ИД текущего док-та. При сохранении ПР будет связан с ним
      ((Sungero.Domain.Shared.IExtendedEntity)newDoc).Params[litiko.RegulatoryDocuments.PublicConstants.Module.CreatedFromIRD_ID] = _obj.Id;
      newDoc.Show();            
    }

    public virtual bool CanCreateProjectSolutionlitiko(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !_obj.State.IsInserted && !_obj.State.IsChanged && (Equals(_obj.Author, Users.Current) || Users.Current.IncludedIn(Roles.Administrators));
    }

  }

}