using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace litiko.DocflowEskhata.Server
{
  public class ModuleFunctions
  {

    /// <summary>
    /// Создать и заполнить временную таблицу для конвертов.
    /// </summary>
    /// <param name="reportSessionId">Идентификатор отчета.</param>
    /// <param name="outgoingDocuments">Список Исходящих документов.</param>
    /// <param name="contractualDocuments">Список Договорных документов.</param>
    /// <param name="accountingDocuments">Список Финансовых документов.</param>
    public static void FillEnvelopeTable(string reportSessionId,
                                         List<Sungero.Docflow.IOutgoingDocumentBase> outgoingDocuments,
                                         List<Sungero.Docflow.IContractualDocumentBase> contractualDocuments,
                                         List<Sungero.Docflow.IAccountingDocumentBase> accountingDocuments)
    {
      var id = 1;
      var dataTable = new List<Structures.Module.EnvelopeReportTableLine>();
      
      var documents = new List<Structures.Module.AddresseeAndSender>();
      foreach (var document in outgoingDocuments)
      {
        foreach (var addressee in document.Addressees.OrderBy(a => a.Number))
        {
          var envelopeInfo = Structures.Module.AddresseeAndSender.Create(addressee.Correspondent, document.BusinessUnit);
          documents.Add(envelopeInfo);
        }
      }
      foreach (var document in contractualDocuments)
      {
        var envelopeInfo = Structures.Module.AddresseeAndSender.Create(document.Counterparty, document.BusinessUnit);
        documents.Add(envelopeInfo);
      }
      foreach (var document in accountingDocuments)
      {
        var envelopeInfo = Structures.Module.AddresseeAndSender.Create(document.Counterparty, document.BusinessUnit);
        documents.Add(envelopeInfo);
      }
      
      foreach (var document in documents)
      {
        var tableLine = Structures.Module.EnvelopeReportTableLine.Create();
        // Идентификатор отчета.
        tableLine.ReportSessionId = reportSessionId;
        // ИД.
        tableLine.Id = id++;
        
        var correspondent = document.Addresse;
        var correspondentZipCode = string.Empty;
        var correspondentAddress = string.Empty;
        var correspondentName = string.Empty;
        if (correspondent != null)
        {
          var addressToParse = !string.IsNullOrEmpty(correspondent.PostalAddress)
            ? correspondent.PostalAddress
            : correspondent.LegalAddress;
          var zipCodeToParsingResult = Functions.Module.ParseZipCode(addressToParse);
          correspondentZipCode = zipCodeToParsingResult.ZipCode;
          correspondentAddress = zipCodeToParsingResult.Address;
          var person = Sungero.Parties.People.As(correspondent);
          if (person != null)
            correspondentName = Sungero.Parties.PublicFunctions.Person.GetFullName(person, Sungero.Core.DeclensionCase.Dative);
          else
            correspondentName = correspondent.Name;
        }
        
        var businessUnit = document.Sender;
        var businessUnitZipCode = string.Empty;
        var businessUnitAddress = string.Empty;
        var businessUnitName = string.Empty;
        if (businessUnit != null)
        {
          var addressFromParse = !string.IsNullOrEmpty(businessUnit.PostalAddress)
            ? businessUnit.PostalAddress
            : businessUnit.LegalAddress;
          var zipCodeFromParsingResult = Functions.Module.ParseZipCode(addressFromParse);
          businessUnitZipCode = zipCodeFromParsingResult.ZipCode;
          businessUnitAddress = zipCodeFromParsingResult.Address;
          businessUnitName = businessUnit.Name;
        }
        
        tableLine.ToName = correspondentName;
        tableLine.ToPlace = correspondentAddress;
        // Если нет индекса, установить 6 пробелов, чтобы индекс выглядел как сетка, а не 000000.
        tableLine.ToZipCode = string.IsNullOrEmpty(correspondentZipCode) ? "      " : correspondentZipCode;
        
        tableLine.FromName = businessUnitName;
        tableLine.FromPlace = businessUnitAddress;
        tableLine.FromZipCode = businessUnitZipCode;
        
        dataTable.Add(tableLine);
      }
      
      Sungero.Docflow.PublicFunctions.Module.WriteStructuresToTable(Constants.EnvelopeB4Report.EnvelopesTableName, dataTable);
    }
    
    /// <summary>
    /// Получить индекс и адрес без индекса.
    /// </summary>
    /// <param name="address">Адрес с индексом.</param>
    /// <returns>Структуры с индексом и адресом без индекса.</returns>
    public static Structures.Module.ZipCodeAndAddress ParseZipCode(string address)
    {
      if (string.IsNullOrEmpty(address))
        return Structures.Module.ZipCodeAndAddress.Create(string.Empty, string.Empty);
      
      // Индекс распознавать с ",", чтобы их удалить из адреса. В адресе на конверте индекса быть не должно.
      var zipCodeRegex = ",*\\s*([0-9]{6}),*";
      var zipCodeMatch = System.Text.RegularExpressions.Regex.Match(address, zipCodeRegex);
      var zipCode = zipCodeMatch.Success ? zipCodeMatch.Groups[1].Value : string.Empty;
      if (!string.IsNullOrEmpty(zipCode))
        address = address.Replace(zipCodeMatch.Value, string.Empty).Trim();
      
      return Structures.Module.ZipCodeAndAddress.Create(zipCode, address);
    }
    
    [Public, Remote(IsPure = true)]
    public static string TranslateRuToTj(string text)
    {
      return AskGemini(text, "ru->tj");
    }
    
    [Public, Remote(IsPure = true)]
    public static string TranslateTjToRu(string text)
    {
      return AskGemini(text, "tj->ru");
    }
    
    [Public, Remote(IsPure = true)]
    public static string TranslateRuToEn(string text)
    {
      return AskGemini(text, "ru->en");
    }
    
    #region ChatGPT
    //    private static string AskChatGPT(string text, string direction)
    //    {
    //      var apiKeys = new[]
    //      {
    //        Resources.ApiKeyGPT1,
    //        Resources.ApiKeyGPT2
    //      };
//
    //      if(string.IsNullOrWhiteSpace(text))
    //        return string.Empty;
//
    //      var url = "https://api.openai.com/v1/chat/completions";
//
    //      string systemPrompt;
//
    //      switch (direction)
    //      {
    //        case "ru->tj":
    //          systemPrompt = "You are a professional translator. Translate from Russian to Tajik. Return only the translation, no comments.";
    //          break;
    //        case "tj->ru":
    //          systemPrompt = "You are a professional translator. Translate from Tajik to Russian. Return only the translation, no comments.";
    //          break;
    //        case "ru->en":
    //          systemPrompt = "You are a professional translator. Translate from Russian to English. Return only the translation, no comments.";
    //          break;
    //        default:
    //          systemPrompt = "You are a professional translator. Return only the translation, no comments.";
    //          break;
    //      }
//
    //      foreach (var apiKey in apiKeys)
    //      {
    //        using(var client = new HttpClient())
    //        {
    //          client.DefaultRequestHeaders.Clear();
    //          client.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);
//
    //          var payload = new JObject
    //          {
    //            ["model"] = "gpt-4o-mini",
    //            ["temperature"] = 0,
    //            ["messages"] = new JArray
    //            {
    //              new JObject { ["role"] = "system", ["content"] = systemPrompt },
    //              new JObject { ["role"] = "user",   ["content"] = text }
    //            }
    //          };
//
    //          var content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json");
    //          var response = client.PostAsync(url, content).Result;
    //          var responseJson = response.Content.ReadAsStringAsync().Result;
//
    //          if (response.IsSuccessStatusCode)
    //          {
    //            var parsed = JObject.Parse(responseJson);
    //            var translation = parsed["choices"]?[0]?["message"]?["content"]?.ToString();
    //            return translation?.Trim() ?? string.Empty;
    //          }
    //          else if((int)response.StatusCode == 429)
    //          {
    //            Logger.Error($"Ключ превысил лимит. Пробуем следующий.");
    //            continue;
    //          }
    //          else
    //          {
    //            Logger.Error($"{response.StatusCode} || {responseJson}");
    //            throw new Exception($"Повторите попытку через минуту.");
    //          }
    //        }
    //      }
//
    //      throw new Exception("Превышен лимит запросов, пожалуйста повторите попытку через минуту");
    //    }
    #endregion
    
    private static string AskGemini(string text, string direction)
    {
      // 1. ЗАМЕНИТЕ ЭТИ КЛЮЧИ НА ВАШИ КЛЮЧИ GEMINI API
      var apiKeys = new[]
      {
        Resources.ApiKeyGemini1, // Замените на реальный ключ
        Resources.ApiKeyGemini2  // Замените на реальный ключ
      };
      
      if (string.IsNullOrWhiteSpace(text))
        return string.Empty;
      
      // 2. ФОРМИРУЕМ ИНСТРУКЦИЮ ДЛЯ ПЕРЕВОДА
      string translationInstruction;
      
      switch (direction)
      {
        case "ru->tj":
          translationInstruction = "Ты — профессиональный переводчик. Переведи следующий текст с русского на таджикский. Верни только перевод, без каких-либо комментариев и пояснений.";
          break;
        case "tj->ru":
          translationInstruction = "Ты — профессиональный переводчик. Переведи следующий текст с таджикского на русский. Верни только перевод, без каких-либо комментариев и пояснений.";
          break;
        case "ru->en":
          translationInstruction = "Ты — профессиональный переводчик. Переведи следующий текст с русского на английский. Верни только перевод, без каких-либо комментариев и пояснений.";
          break;
        default:
          // Общая инструкция на случай, если направление не указано
          translationInstruction = "Ты — профессиональный переводчик. Верни только перевод текста, без комментариев.";
          break;
      }
      
      // Объединяем инструкцию и сам текст
      var fullPrompt = $"{translationInstruction}\n\n---\n\n{text}";
      
      foreach (var apiKey in apiKeys)
      {
        // 3. ИЗМЕНЕН URL И СПОСОБ ПЕРЕДАЧИ КЛЮЧА
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={apiKey}";
        
        using (var client = new HttpClient())
        {
          // 4. ИЗМЕНЕНА СТРУКТУРА ТЕЛА ЗАПРОСА (PAYLOAD)
          var payload = new JObject
          {
            ["contents"] = new JArray
            {
              new JObject
              {
                ["parts"] = new JArray
                {
                  new JObject { ["text"] = fullPrompt }
                }
              }
            },
            // Добавляем конфигурацию для более предсказуемого результата
            ["generationConfig"] = new JObject
            {
              ["temperature"] = 0
            }
          };
          
          var content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json");
          var response = client.PostAsync(url, content).Result;
          var responseJson = response.Content.ReadAsStringAsync().Result;
                   
          if (response.IsSuccessStatusCode)
          {
            try
            {
              var parsed = JObject.Parse(responseJson);
              // 5. ИЗМЕНЕН ПУТЬ К ТЕКСТУ В ОТВЕТЕ
              var translation = parsed["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();
              return translation?.Trim() ?? string.Empty;
            }
            catch (Exception ex)
            {
              // Логирование ошибки парсинга, если структура ответа неожиданная
              Logger.Error($"Ошибка парсинга ответа от Gemini: {ex.Message} || Ответ: {responseJson}");
              continue; // Попробовать следующий ключ
            }
          }
          else if ((int)response.StatusCode == 429) // Too Many Requests
          {
            Logger.Error($"Ключ превысил лимит. Пробуем следующий.");
            continue;
          }
          else
          {
            Logger.Error($"Ошибка от Gemini API: {response.StatusCode} || {responseJson}");
            // Не бросаем исключение сразу, чтобы дать шанс другому ключу
          }
        }
      }
      
      // Если все ключи не сработали
      throw new Exception("Превышен лимит запросов или все ключи недействительны, пожалуйста повторите попытку через минуту");
    }
  }
}