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
        sb.AppendLine($"📦 Обработка файла завершена. Всего записей: {result.TotalCount}");
        sb.AppendLine("--------------------------------");

        sb.AppendLine("🏢 Компании:");
        sb.AppendLine($"• Всего: {result.TotalCompanies}");
        
        if (result.ImportedCompanies > 0)   
          sb.AppendLine($"• ✨ Создано новых: {result.ImportedCompanies}");
        
        if (result.DuplicateCompanies > 0)
          sb.AppendLine($"• 🔄 Дубликатов: {result.DuplicateCompanies}");
        
        if (result.TotalCompanies > 0 && result.ImportedCompanies == 0 && result.DuplicateCompanies == 0)
          sb.AppendLine("• ⚠️ Не импортировано (см. ошибки):");

        sb.AppendLine();

        sb.AppendLine("👤 Физические лица:");
        sb.AppendLine($"• Всего: {result.TotalPersons}");
        
        if (result.ImportedPersons > 0)
          sb.AppendLine($"• ✨ Создано новых: {result.ImportedPersons}");
        
        if (result.DuplicatePersons > 0)
          sb.AppendLine($"• 🔄 Дубликатов: {result.DuplicatePersons}");

        sb.AppendLine("--------------------------------");
        
        var totalDuplicates = result.DuplicateCompanies + result.DuplicatePersons;
        
        sb.AppendLine($"✅ Успешно создано: {result.ImportedCount}");
        sb.AppendLine($"♻️ Найдено дублей: {totalDuplicates}");
        sb.AppendLine($"❌ Ошибок: {result.Errors.Count}");

        if (result.Errors.Any())
        {
          sb.AppendLine("\nСписок ошибок:");
          foreach(var err in result.Errors)
            sb.AppendLine("- " + err);
        }
        else
        {
          sb.AppendLine();
          sb.AppendLine("Ошибок нет ✅");
        }

        var icon = (result.Errors != null && result.Errors.Any()) ? MessageType.Warning : MessageType.Information;
        Dialogs.ShowMessage(sb.ToString(), icon);
      }
      catch (Exception ex)
      {
        Dialogs.ShowMessage($"Ошибка: {ex.Message}", MessageType.Error);
      }
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
    }
  }
}