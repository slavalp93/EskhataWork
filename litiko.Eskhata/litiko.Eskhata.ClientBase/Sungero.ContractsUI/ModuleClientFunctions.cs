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
      litiko.Eskhata.Module.Parties.PublicFunctions.Module.Remote.RunAsyncDeleteMigratedParties();
      Dialogs.NotifyMessage("Запущено удаление мигрированных контрагентов.");
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual void ImportCounterpariesAsync()
    {
      var dialog = Dialogs.CreateInputDialog("Импорт контрагентов (Асинхронно)");
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
      // 1. Вызываем удаленную функцию, которая просто ставит задачу в очередь
      // Используем .Remote, так как функция находится на сервере
      litiko.Eskhata.Module.Contracts.PublicFunctions.Module.Remote.RunAsyncDeleteMigratedContracts();
      
      // 2. Мгновенно уведомляем пользователя
      Dialogs.NotifyMessage("Запущена фоновая очистка мигрированных договоров. Система уведомит вас по завершении.");
    }

    /// <summary>
    /// Импорт договоров из UI (перевод в фоновый режим).
    /// </summary>
    public virtual void ImportContractsFromUIAsync()
    {
      var dialog = Dialogs.CreateInputDialog("Импорт договоров (XML)");
      
      var fileInput = dialog.AddFileSelect("Выберите файл XML", true);
      fileInput.WithFilter("XML", "xml");

      if (dialog.Show() != DialogButtons.Ok) return;

      byte[] fileBytes = fileInput.Value.Content;
      string fileName = fileInput.Value.Name;

      string fileBase64 = Convert.ToBase64String(fileBytes);
      
      try
      {
        var resultMessage = litiko.Eskhata.Module.Contracts.PublicFunctions.Module.Remote.StartAsyncImportContracts(fileBase64, fileName);
        // Показываем сообщение от сервера (что импорт запущен)
        Dialogs.ShowMessage(resultMessage, MessageType.Information);
      }
      catch (Exception ex)
      {
        Dialogs.ShowMessage($"Не удалось запустить импорт: {ex.Message}", MessageType.Error);
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