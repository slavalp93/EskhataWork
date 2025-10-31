using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Content.PublicFunctions;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Contract;
using Sungero.Commons.Constants;

namespace litiko.Eskhata.Module.ContractsUI.Client
{
  partial class ModuleFunctions
  {
    public virtual void ImportContracts()
    {
      var contract = Eskhata.Contracts.GetAll().FirstOrDefault();
      if (contract != null)
      {
        var result = Eskhata.Functions.Contract.Remote.ImportContractsFromXml(contract);


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
    }
  }
}