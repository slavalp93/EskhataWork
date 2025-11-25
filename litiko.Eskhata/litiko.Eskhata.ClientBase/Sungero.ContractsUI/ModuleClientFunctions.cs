using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Content.PublicFunctions;
using Sungero.Core;
using Sungero.CoreEntities;
//using litiko.Eskhata.Contract;
using Sungero.Commons.Constants;
using litiko.Eskhata.Module.Contracts.Structures.Module;
using Sungero.Docflow;

namespace litiko.Eskhata.Module.ContractsUI.Client
{
  partial class ModuleFunctions
  {

    public virtual void DeleteByKeyword()
    {
      var dialog = Dialogs.CreateInputDialog("Удаление по ключевому слову");
      var keywordInput = dialog.AddString("Введите слово для поиска:", true);
      
      if (dialog.Show() != DialogButtons.Ok) return;

      var keyword = keywordInput.Value;

      var ids = litiko.Eskhata.Module.Contracts.PublicFunctions.Module.Remote.GetContractIdsByKeyword(keyword);

      if (!ids.Any())
      {
        Dialogs.ShowMessage($"По запросу '{keyword}' ничего не найдено.");
        return;
      }
      
      var confirmDialog = Dialogs.CreateTaskDialog("Внимание!",
                                                   $"Найдено документов: {ids.Count}.\nКритерий поиска: '{keyword}'\n\nУДАЛИТЬ ИХ БЕЗВОЗВРАТНО?",
                                                   MessageType.Question);

      var btnYes = confirmDialog.Buttons.AddYes();
      confirmDialog.Buttons.AddNo();

      if (confirmDialog.Show() != btnYes) return;

      int success = 0;
      int errors = 0;
      string lastError = "";

      foreach (var id in ids)
      {
        try
        {
          litiko.Eskhata.Module.Contracts.PublicFunctions.Module.Remote.DeleteContractById(id);
          success++;
        }
        catch (Exception ex)
        {
          errors++;
          lastError = ex.Message;
        }
      }

      var msg = $"Готово!\n✅ Удалено: {success}\n❌ Ошибок: {errors}";
      if (errors > 0) msg += $"\nПример ошибки: {lastError}";
      
      Dialogs.ShowMessage(msg, errors > 0 ? MessageType.Warning : MessageType.Information);
    }

    public virtual void ImportCounterparties()
    {
      var result = litiko.Eskhata.Module.Parties.PublicFunctions.Module.Remote.ImportCounterpartyFromXml();

      var message = new System.Text.StringBuilder();
      message.AppendLine("📦 Импорт контрагентов завершён.");

      message.AppendLine($"📦 Всего контрагентов в файле: {result.TotalCount}");
      message.AppendLine($"✅ Всего успешно импортировано: {result.ImportedCount}");

      message.AppendLine();
      message.AppendLine("🏢 Компании:");
      message.AppendLine($"• Всего в файле: {result.TotalCompanies}");
      message.AppendLine($"• Импортировано: {result.ImportedCompanies}");

      message.AppendLine();
      message.AppendLine("👤 Физические лица:");
      message.AppendLine($"• Всего в файле: {result.TotalPersons}");
      message.AppendLine($"• Импортировано: {result.ImportedPersons}");

      if (result.SkippedEntities != null && result.SkippedEntities.Any())
      {
        message.AppendLine();
        message.AppendLine("ℹ️ Контрагенты пропущены (уже есть в системе):");
        foreach (var name in result.SkippedEntities)
          message.AppendLine(" • " + name);
      }

      if (result.Errors != null && result.Errors.Any())
      {
        message.AppendLine();
        message.AppendLine("⚠️ Ошибки импорта:");
        foreach (var error in result.Errors)
          message.AppendLine(" • " + error);
      }
      else
      {
        message.AppendLine();
        message.AppendLine("Все контрагенты успешно обработаны без ошибок ✅");
      }

      Dialogs.ShowMessage(message.ToString());
    }

    public virtual void ImportContractsFromUI()
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
        var result = litiko.Eskhata.Module.Contracts.PublicFunctions.Module.Remote.ImportContractsFromXmlUI(fileBase64, fileName);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Всего записей в файле: {result.TotalCount}");
        sb.AppendLine("--------------------------------");
        sb.AppendLine($"✅ Создано новых: {result.ImportedCount}");
        sb.AppendLine($"🔄 Пропущено (дубли): {result.DuplicateCount}");
        sb.AppendLine($"❌ Ошибок: {result.Errors.Count}");
        
        if (result.Errors.Any())
        {
          sb.AppendLine("\nСписок ошибок (первые 10):");
          foreach(var err in result.Errors.Take(10))
            sb.AppendLine("- " + err);
        }

        var icon = result.Errors.Any() ? MessageType.Warning : MessageType.Information;
        
        Dialogs.ShowMessage(sb.ToString(), icon);
      }
      catch (Exception ex)
      {
        Dialogs.ShowMessage($"Ошибка: {ex.Message}", MessageType.Error);
      }
    }
  }
}