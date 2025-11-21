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

    public virtual void DeleteData()
    {
      var dialog = Dialogs.CreateTaskDialog("Очистка данных",
          "Вы уверены, что хотите УДАЛИТЬ тестовые договоры ('РБ-1...')?",
          MessageType.Question);
      
      // Добавляем кнопку удаления
      var deleteBtn = dialog.Buttons.AddCustom("Удалить");
      dialog.Buttons.AddCancel();
      
      if (dialog.Show() != deleteBtn) return;

      // 1. Запрашиваем список ID (Сервер)
      var ids = litiko.Eskhata.Module.Contracts.PublicFunctions.Module.Remote.GetTestContractIds();
      
      if (!ids.Any())
      {
        Dialogs.ShowMessage("Договоры для удаления не найдены.");
        return;
      }

      int success = 0;
      int errors = 0;
      
      // 2. Запускаем цикл НА КЛИЕНТЕ
      // Удаляем по одному. Ошибки не останавливают процесс.
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
          // Можно вывести ошибку в консоль браузера, если нужно
        }
      }

      Dialogs.ShowMessage($"Готово!\n✅ Удалено: {success}\n❌ Ошибок: {errors}", MessageType.Information);
    }
    
    public virtual void ImportCounterparties()
    {
      // Вызов удалённого метода и получение результата
      var result = litiko.Eskhata.Module.Parties.PublicFunctions.Module.Remote.ImportCounterpartyFromXml();

      var message = new System.Text.StringBuilder();
      message.AppendLine("📦 Импорт контрагентов завершён.");
      
      // Общие данные
      message.AppendLine($"📦 Всего контрагентов в файле: {result.TotalCount}");
      message.AppendLine($"✅ Всего успешно импортировано: {result.ImportedCount}");
      
      // Компании
      message.AppendLine();
      message.AppendLine("🏢 Компании:");
      message.AppendLine($"• Всего в файле: {result.TotalCompanies}");
      message.AppendLine($"• Импортировано: {result.ImportedCompanies}");

      // Физические лица
      message.AppendLine();
      message.AppendLine("👤 Физические лица:");
      message.AppendLine($"• Всего в файле: {result.TotalPersons}");
      message.AppendLine($"• Импортировано: {result.ImportedPersons}");

      // Пропущенные сущности
      if (result.SkippedEntities != null && result.SkippedEntities.Any())
      {
        message.AppendLine();
        message.AppendLine("ℹ️ Контрагенты пропущены (уже есть в системе):");
        foreach (var name in result.SkippedEntities)
          message.AppendLine(" • " + name);
      }

      // Ошибки импорта
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


    public virtual void ImportContract()
    {
      try
      {
        // Мы просто вызываем метод. Он ничего не возвращает (void), поэтому "var result =" убираем.
        litiko.Eskhata.Module.Contracts.PublicFunctions.Module.Remote.ImportContractsFromXmlUI();

        // Сообщаем пользователю, что процесс ушел в фон
        Dialogs.ShowMessage(
          "🚀 Импорт договоров успешно запущен в фоновом режиме.\n\n" +
          "Вы можете продолжать работу. По завершении обработки (через несколько минут) " +
          "вам придет уведомление (Задание) с детальной статистикой и списком ошибок.",
          MessageType.Information);
      }
      catch (Exception ex)
      {
        // Этот блок сработает, только если не найден файл или упал сам запуск асинхронного обработчика
        Logger.Error($"Critical error while starting import: {ex.Message}", ex);

        Dialogs.ShowMessage(
          $"❌ Не удалось запустить импорт:\n{ex.Message}\nПроверьте наличие файла и права доступа.",
          MessageType.Error);
      }
    }
  }
}