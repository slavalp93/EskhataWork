using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Content.PublicFunctions;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Commons.Constants;
using litiko.Eskhata.Module.Contracts.Structures.Module;
using Sungero.Docflow;

namespace litiko.Eskhata.Module.ContractsUI.Client
{
  partial class ModuleFunctions
  {

    public virtual void DeleteMigratedPartiesAsync()
    {
      var managerRole = Sungero.CoreEntities.Roles.GetAll().FirstOrDefault(r=>r.Name == "Менеджеры модуля \"Договоры\"");
      
      var currentUser = Employees.Current;
      
      if(!currentUser.IncludedIn(managerRole))
      {
        Dialogs.ShowMessage("Недостаточно прав для выполнения данной операции");
        return;
      }
      litiko.Eskhata.Module.Parties.PublicFunctions.Module.Remote.RunAsyncDeleteMigratedParties();
      Dialogs.NotifyMessage("Запущено фоновое удаление мигрированных контрагентов. Система уведомит вас по завершении.");
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual void ImportCounterpariesAsync()
    {
      var managerRole = Sungero.CoreEntities.Roles.GetAll().FirstOrDefault(r=>r.Name == "Менеджеры модуля \"Договоры\"");
      
      var currentUser = Employees.Current;
      
      if(!currentUser.IncludedIn(managerRole))
      {
        Dialogs.ShowMessage("Недостаточно прав для выполнения данной операции");
        return;
      }
      
      var dialog = Dialogs.CreateInputDialog("Миграция контрагентов");
      var fileInput = dialog.AddFileSelect("Выберите файл XML", true);
      fileInput.WithFilter("XML", "xml");

      if (dialog.Show() != DialogButtons.Ok) return;

      string fileBase64 = Convert.ToBase64String(fileInput.Value.Content);
      var msg = litiko.Eskhata.Module.Parties.PublicFunctions.Module.Remote.StartAsyncImportParties(fileBase64, fileInput.Value.Name);
      
      Dialogs.ShowMessage(msg);
    }
    /// <summary>
    /// Удаление мигрированных договоров (через асинхронный обработчик).
    /// </summary>
    public virtual void DeleteMigratedContractsAsync()
    {
      var managerRole = Sungero.CoreEntities.Roles.GetAll().FirstOrDefault(r=>r.Name == "Менеджеры модуля \"Договоры\"");
      
      var currentUser = Employees.Current;
      
      if(!currentUser.IncludedIn(managerRole))
      {
        Dialogs.ShowMessage("Недостаточно прав для выполнения данной операции");
        return;
      }
      else
      {
        litiko.Eskhata.Module.Contracts.PublicFunctions.Module.Remote.RunAsyncDeleteMigratedContracts();
        
        Dialogs.NotifyMessage("Запущена фоновое удаление мигрированных договоров. Система уведомит вас по завершении.");
      }
    }

    /// <summary>
    /// Импорт договоров из UI (перевод в фоновый режим).
    /// </summary>
    public virtual void ImportContractsFromUIAsync()
    {
      var managerRole = Sungero.CoreEntities.Roles.GetAll().FirstOrDefault(r=>r.Name == "Менеджеры модуля \"Договоры\"");
      
      var currentUser = Employees.Current;
      
      if(!currentUser.IncludedIn(managerRole))
      {
        Dialogs.ShowMessage("Недостаточно прав для выполнения данной операции");
        return;
      }
      var dialog = Dialogs.CreateInputDialog("Миграция договоров");
      
      var fileInput = dialog.AddFileSelect("Выберите файл XML", true);
      fileInput.WithFilter("XML", "xml");

      if (dialog.Show() != DialogButtons.Ok) return;

      byte[] fileBytes = fileInput.Value.Content;
      string fileName = fileInput.Value.Name;

      string fileBase64 = Convert.ToBase64String(fileBytes);
      
      try
      {
        var resultMessage = litiko.Eskhata.Module.Contracts.PublicFunctions.Module.Remote.StartAsyncImportContracts(fileBase64, fileName);
        
        Dialogs.ShowMessage(resultMessage, MessageType.Information);
      }
      catch (Exception ex)
      {
        Dialogs.ShowMessage($"Не удалось запустить миграцию: {ex.Message}", MessageType.Error);
      }
    }

    /*/// <summary>
    /// Импорт контрагентов (оставляем как есть, если там данных немного,
    /// но логика аналогична - при больших объемах тоже лучше в фон).
    /// </summary>
    public virtual void ImportCounterparties()
    {
      var dialog = Dialogs.CreateInputDialog("Импорт контрагентов (XML)");
      
      var fileInput = dialog.AddFileSelect("Выберите файл XML", true);
      fileInput.WithFilter("XML", "xml");

      if (dialog.Show() != DialogButtons.Ok) return;

      byte[] fileBytes = fileInput.Value.Content;
      string fileName = fileInput.Value.Name;
      string fileBase64 = Convert.ToBase64String(fileBytes);

      try
      {
        var result = litiko.Eskhata.Module.Parties.PublicFunctions.Module.Remote.ImportCounterpartyFromXml(fileBase64, fileName);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"📦 Обработка завершена. Всего: {result.TotalCount}");
        sb.AppendLine($"✅ Успешно: {result.ImportedCount}");
        sb.AppendLine($"❌ Ошибок: {result.Errors.Count}");

        if (result.Errors.Any())
        {
          sb.AppendLine("\nСписок ошибок:");
          foreach(var err in result.Errors)
            sb.AppendLine("- " + err);
        }

        var icon = result.Errors.Any() ? MessageType.Warning : MessageType.Information;
        Dialogs.ShowMessage(sb.ToString(), icon);
      }
      catch (Exception ex)
      {
        Dialogs.ShowMessage($"Ошибка: {ex.Message}", MessageType.Error);
      }
    }*/
      
  }
}