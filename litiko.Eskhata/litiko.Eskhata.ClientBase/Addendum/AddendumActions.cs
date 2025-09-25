using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Addendum;

namespace litiko.Eskhata.Client
{
  partial class AddendumActions
  {

    public override void CreateFromTemplate(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var docKindExtractProtocol = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(litiko.CollegiateAgencies.PublicConstants.Module.DocumentKindGuids.ExtractProtocol);
      if (docKindExtractProtocol != null && Equals(_obj.DocumentKind, docKindExtractProtocol))
      {
        try
        {       
          if (_obj.State.IsInserted)
            _obj.Save();
          
          litiko.CollegiateAgencies.PublicFunctions.Module.Remote.CreateMinutesBody(_obj, true);
          e.AddInformation(litiko.Eskhata.Resources.VersionCreatedSuccessfully);
        }
        catch (Exception ex)
        {
          e.AddError(ex.Message);
          throw;
        }      
      }
      else
        base.CreateFromTemplate(e);              
    }

    public override bool CanCreateFromTemplate(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanCreateFromTemplate(e);
    }

  }

}