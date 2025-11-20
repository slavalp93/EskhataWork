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
        // Запуск удалённого импорта
        var result = litiko.Eskhata.Module.Contracts.PublicFunctions.Module.Remote.ImportContractsFromXmlUI();

        // Формирование финального сообщения
        var message = new System.Text.StringBuilder();
        message.AppendLine("📦 Импорт договоров завершён.");
        message.AppendLine($"📄 Всего документов в файле: {result.TotalCount}");
        message.AppendLine($"✅ Успешно импортировано: {result.ImportedCount}");

        if (result.Errors.Any())
        {
          message.AppendLine();
          message.AppendLine("⚠️ Возникли ошибки при импорте:");

          foreach (var error in result.Errors)
            message.AppendLine(" • " + error);

          message.AppendLine();
          message.AppendLine("Проверьте лог или XML-файл.");
        }
        else
        {
          message.AppendLine();
          message.AppendLine("Все документы успешно импортированы без ошибок 🎉");
        }

        // Показ результата
        Dialogs.ShowMessage(message.ToString(), MessageType.Information);
      }
      catch (Exception ex)
      {
        // Ловим фатальные ошибки
        Logger.Error($"Critical error while importing contracts: {ex.Message}", ex);

        Dialogs.ShowMessage(
          $"❌ Критическая ошибка при импорте договоров:\n{ex.Message}\nПодробности доступны в логах.",
          MessageType.Error);
      }
    }
  }
}