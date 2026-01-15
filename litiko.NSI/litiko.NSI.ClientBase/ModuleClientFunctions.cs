using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.NSI.Client
{
  public class ModuleFunctions
  {
    /// <summary>
    /// Запуск импорта справочника на клиенте
    /// </summary>
    public void ImportClientAction(string entityType)
    {
      if (string.IsNullOrEmpty(entityType))
        return;
      
      var dialog = Dialogs.CreateInputDialog("Импорт справочника из xml");
      
      var fileInput = dialog.AddFileSelect("Выберите файл XML", true);
      fileInput.WithFilter("XML", "xml");

      if (dialog.Show() != DialogButtons.Ok) 
        return;

      byte[] fileBytes = fileInput.Value.Content;
      string fileBase64 = Convert.ToBase64String(fileBytes);
      try
      {
        Structures.Module.IResultImportXml result;
        switch (entityType)
        {
          case Constants.Module.ImportEntityTypes.Mapping:
            result = Functions.Module.Remote.ImportMappingFromXml(fileBase64);
            break;

          case Constants.Module.ImportEntityTypes.ResponsibilityMatrix:
            result = Functions.Module.Remote.ImportResponsibilityMatrix(fileBase64);
            break;

          case Constants.Module.ImportEntityTypes.ContractsVsPaymentDoc:
            result = Functions.Module.Remote.ImportContractsVsPaymentDoc(fileBase64);
            break;

          case Constants.Module.ImportEntityTypes.TaxRate:
            result = Functions.Module.Remote.ImportTaxRate(fileBase64);
            break;            

          default:
            throw new Exception("Неизвестный тип справочника.");
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Всего: {result.TotalCount}");
        sb.AppendLine("--------------------------------");
        sb.AppendLine($"✅ Создано новых: {result.ImportedCount}");
        sb.AppendLine($"🔄 Обновлено: {result.ChangedCount}");
        sb.AppendLine($"⏭ Пропущено: {result.SkippedCount}");
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