using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Contract;

namespace litiko.Eskhata.Client
{
  partial class ContractActions
  {
    public virtual void Requestlitiko(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var request = Integration.PublicFunctions.Module.Remote.GetAndProcessExchangeDoc();
      
      request.show();
    }

    public virtual bool CanRequestlitiko(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void CreateLegalOpinionlitiko(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var addendum = Functions.Contract.Remote.CreateLegalOpinion();
      if (addendum == null)
      {
        e.AddError(litiko.Eskhata.Contracts.Resources.DocumentKindNotFound);
        return;
      }
      
      addendum.LeadingDocument = _obj;
      addendum.Show();      
    }

    public virtual bool CanCreateLegalOpinionlitiko(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public override void CreateFromFile(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.CreateFromFile(e);
    }

    public override bool CanCreateFromFile(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !_obj.IsStandard.GetValueOrDefault() && base.CanCreateFromTemplate(e);
    }

  }


}