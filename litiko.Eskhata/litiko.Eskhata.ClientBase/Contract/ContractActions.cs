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
    public virtual void StartContractsBatchImportlitiko(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var errors = Functions.Contract.Remote.ImportContractsFromXml(_obj);
      
      if (errors.Any())
      {
        Dialogs.ShowMessage("Импорт выполнен с ошибками:\n" + string.Join("\n", errors));
      }
      else
      {
        Dialogs.ShowMessage("Импорт выполнен успешно.");
      }
    }


    public virtual bool CanStartContractsBatchImportlitiko(Sungero.Domain.Client.CanExecuteActionArgs e)
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
      return base.CanCreateFromTemplate(e); //!_obj.IsStandard.GetValueOrDefault()
    }

  }


}