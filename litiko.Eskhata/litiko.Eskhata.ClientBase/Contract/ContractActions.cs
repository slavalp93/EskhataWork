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

    /*public virtual void StartContractsBatchImportlitiko(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var result = Functions.Contract.Remote.ImportContractsFromXml(_obj);

      var message = new System.Text.StringBuilder();

      message.AppendLine("📦 Импорт договоров завершён.");
      message.AppendLine($"✅ Успешно импортировано: {result.ImportedCount}");

      if (result.Errors.Any())
      {
        message.AppendLine();
        message.AppendLine("⚠️ Ошибки импорта:");
        foreach (var error in result.Errors)
          message.AppendLine(" • " + error);
      }
      else
      {
        message.AppendLine();
        message.AppendLine("Все документы успешно импортированы без ошибок ✅");
      }

      Dialogs.ShowMessage(message.ToString());
    }



    public virtual bool CanStartContractsBatchImportlitiko(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }
*/

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

  }


}