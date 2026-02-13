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
//    public virtual void ExportBusData()
//    {
//      var hozDogIds = new List<string> {
//        "57227376326", "76733521958", "76075666319", "82317544435", "69857910867",
//        "81559622395", "69130476865", "77802499802", "35733172640", "63167682556",
//        "61907589585", "60449322840", "77797991516", "1840886148", "9697688878",
//        "12969032762", "27051363972", "39291663179", "4695340469", "12487526163",
//        "10846060607", "1840876789", "1840878612", "1848633541", "1848634362",
//        "39488424213", "61719849499", "63997493763", "59466099513", "26090430823",
//        "73024456932", "72704382656", "75537394341", "69855836419", "41602534099",
//        "78500824380", "82356433507"
//      };
//
//      // 2. Создаем диалог
//      var dialog = Dialogs.CreateTaskDialog("Экспорт данных из Шины",
//                                            "Выберите, какие договоры выгрузить в XML.");
//      
//      // Кнопка для выделенных в гриде записей
//      var btnSelected = dialog.Buttons.AddCustom("Выделенные записи");
//      // Кнопка для списка по умолчанию
//      var btnDefault = dialog.Buttons.AddCustom("Список по умолчанию (Тест)");
//      
//      dialog.Buttons.AddCancel();
//
//      var result = dialog.Show();
//
//      if (result == DialogButtons.Cancel)
//        return;
//
//      List<string> idsToExport = new List<string>();
//
//      if (result == btnDefault)
//      {
//        idsToExport = hozDogIds;
//      }
//      else if (result == btnSelected)
//      {
//        
//        foreach (var entity in hozDogIds)
//        {
//          idsToExport.Add(entity.ToString());
//        }
//      }
//
//      if (!idsToExport.Any())
//      {
//        Dialogs.ShowMessage("Список для выгрузки пуст.", MessageType.Warning);
//        return;
//      }
//
//      // 3. Вызов Асинхронного обработчика
//      var asyncArgs = litiko.Eskhata.Module.Contracts.AsyncHandlers.ExportFromBuslitiko.Create();
//      asyncArgs.UserId = Users.Current.Id;
//      
//      // Превращаем список в строку "123,456,789"
//      asyncArgs.ExternalIds = string.Join(",", idsToExport);
//      
//      asyncArgs.ExecuteAsync();
//
//      Dialogs.NotifyMessage($"Запущен экспорт {idsToExport.Count} договоров. Ожидайте уведомление.");
//    }

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
    
    
//    /// <summary>
//    /// 
//    /// </summary>
//    public virtual void ShowIp()
//    {
//      var ip = litiko.Eskhata.Module.Contracts.Functions.Module.Remote.GetMyIpAddress();
//      Dialogs.ShowMessage($"IP адрес сервера Directum: {ip}");
//    }

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