using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;


namespace litiko.Eskhata.Module.Parties.Client
{
  partial class ModuleFunctions
  {
    /*public virtual void ImportCounterparty()
    {
      // Вызов удалённого метода и получение результата
      var result = litiko.Eskhata.Module.Parties.PublicFunctions.Module.Remote.ImportCounterpartyFromXml();

      // Формирование сообщения для пользователя
      var message = new System.Text.StringBuilder();

      message.AppendLine("📦 Импорт договоров завершён.");
      message.AppendLine($"📦 Всего документов в файле: {result.TotalCount}");
      
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
    }*/

  }
}