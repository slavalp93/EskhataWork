using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.NSI.Mapping;

namespace litiko.NSI.Client
{


  internal static class MappingStaticActions
  {

    public static bool CanExport(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return Users.Current.IncludedIn(Roles.Administrators);
    }

    public static void Export(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var zip = NSI.Functions.Module.Remote.ExportMapping();
      zip.Export();
    }

    public static bool CanImport(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return Users.Current.IncludedIn(Roles.Administrators);
    }

    public static void Import(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var dialog = Dialogs.CreateInputDialog("Импорт справочника из xml");
      
      var fileInput = dialog.AddFileSelect("Выберите файл XML", true);
      fileInput.WithFilter("XML", "xml");

      if (dialog.Show() != DialogButtons.Ok) 
        return;

      byte[] fileBytes = fileInput.Value.Content;
      string fileName = fileInput.Value.Name;

      string fileBase64 = Convert.ToBase64String(fileBytes);
      try
      {
        var result = Functions.Module.Remote.ImportMappingFromXml(fileBase64);                

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