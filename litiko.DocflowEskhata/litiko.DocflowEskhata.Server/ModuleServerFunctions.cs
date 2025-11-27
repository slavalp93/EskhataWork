using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DotNetEnv;
using System.IO;


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
    public static List<string> TranslateList(List<string> texts, string direction)
    {
      Logger.Debug("--- НАЧАЛО ЗАПРОСА ПЕРЕВОДА ---");

      if (texts == null || !texts.Any())
      {
        Logger.Debug("Список текстов пуст.");
        return new List<string>();
      }

      string apiKey = GetApiKey();
            
      if (string.IsNullOrEmpty(apiKey))
      {
        Logger.Error("!!! ОШИБКА: API Key пустой. Проверьте БД или впишите hardcodedKey.");
        return texts.Select(x => "").ToList();
      }

      string promptDirection = direction == "ru->tj" ? "Russian to Tajik" :
        direction == "tj->ru" ? "Tajik to Russian" :
        "Russian to English";

      var jsonPayload = JsonConvert.SerializeObject(texts);
      
      var fullPrompt = $"Translate this JSON array from {promptDirection}. Output ONLY valid JSON array of strings. No markdown.\nInput: {jsonPayload}";

      Logger.Debug($"Запрос к Gemini ({direction}). Элементов: {texts.Count}.");

      try
      {
        var responseJson = SendRequestToGemini(apiKey, fullPrompt);
        
        Logger.Debug($"RAW GEMINI RESPONSE: {responseJson}");

        if (!string.IsNullOrEmpty(responseJson))
        {
          var cleanJson = responseJson.Replace("```json", "").Replace("```", "").Trim();
          
          try
          {
            var resultList = JsonConvert.DeserializeObject<List<string>>(cleanJson);
            if (resultList != null)
            {
              Logger.Debug($"Успешно распарсено элементов: {resultList.Count}");
              return resultList;
            }
          }
          catch (Exception parseEx)
          {
            Logger.Error($"Ошибка парсинга JSON ответа: {parseEx.Message}. CleanJson: {cleanJson}");
          }
        }
        else
        {
          Logger.Error("Ответ от SendRequestToGemini пустой (null).");
        }
      }
      catch (Exception ex)
      {
        Logger.Error($"Глобальная ошибка в TranslateList: {ex.Message}");
      }

      Logger.Debug("Возвращаем пустой список из-за ошибок.");
      return texts.Select(x => "").ToList();
    }

    private static string SendRequestToGemini(string apiKey, string prompt)
    {
      string modelName = "gemini-2.0-flash-lite-preview-02-05"; 
      
      var url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={apiKey}";

      using (var client = new HttpClient())
      {
        client.Timeout = TimeSpan.FromSeconds(60);

        var payload = new JObject
        {
          ["contents"] = new JArray { new JObject { ["parts"] = new JArray { new JObject { ["text"] = prompt } } } },
          ["generationConfig"] = new JObject { ["temperature"] = 0 },
          ["safetySettings"] = new JArray
          {
            new JObject { ["category"] = "HARM_CATEGORY_HARASSMENT", ["threshold"] = "BLOCK_NONE" },
            new JObject { ["category"] = "HARM_CATEGORY_HATE_SPEECH", ["threshold"] = "BLOCK_NONE" },
            new JObject { ["category"] = "HARM_CATEGORY_SEXUALLY_EXPLICIT", ["threshold"] = "BLOCK_NONE" },
            new JObject { ["category"] = "HARM_CATEGORY_DANGEROUS_CONTENT", ["threshold"] = "BLOCK_NONE" }
          }
        };

        var content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json");

        int maxRetries = 3;
        int currentTry = 0;

        while (currentTry < maxRetries)
        {
          currentTry++;
          
          try
          {
            var response = client.PostAsync(url, content).Result;
            var responseString = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
              var parsed = JObject.Parse(responseString);
              var text = parsed["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();
              
              if (text == null)
              {
                var finishReason = parsed["candidates"]?[0]?["finishReason"]?.ToString();
                Logger.Error($"Gemini: Текст пустой. FinishReason: {finishReason}. Ответ: {responseString}");
                return null;
              }
              
              return text;
            }
            else if ((int)response.StatusCode == 429)
            {
              Logger.Debug($"Gemini 429 (Busy). Attempt {currentTry}...");
              System.Threading.Thread.Sleep(5000);
              continue;
            }

            else if ((int)response.StatusCode == 404 && currentTry == 1)
            {
              Logger.Error("gemini-2.0-flash-lite-preview-02-05 not found (404). Switching to gemini-flash-latest...");
              url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent?key={apiKey}";
              continue;
            }
            else
            {
              Logger.Error($"Gemini API Error: {response.StatusCode} || {responseString}");
              return null;
            }
          }
          catch (Exception ex)
          {
            Logger.Error($"Http Error attempt {currentTry}: {ex.Message}");
            System.Threading.Thread.Sleep(2000);
          }
        }
      }
      return null;
    }

    private static string GetApiKey()
    {
      try
      {
        var key = Constants.Module.ApiKey2;
        var selectCommand = string.Format(Queries.Module.SelectApiKeyGemini, key);
        var result = Sungero.Docflow.PublicFunctions.Module.ExecuteScalarSQLCommand(selectCommand);

        if (result != null && !(result is DBNull))
          return result.ToString().Trim();
      }
      catch (Exception ex)
      {
        Logger.Error("ОШИБКА БАЗЫ ДАННЫХ (GetApiKey):", ex);
      }
      return string.Empty;
    }
    
    [Public, Remote(IsPure = true)]
    public static string TranslateRuToTj(string text) => TranslateList(new List<string> { text }, "ru->tj").FirstOrDefault();

    [Public, Remote(IsPure = true)]
    public static string TranslateTjToRu(string text) => TranslateList(new List<string> { text }, "tj->ru").FirstOrDefault();

    [Public, Remote(IsPure = true)]
    public static string TranslateRuToEn(string text) => TranslateList(new List<string> { text }, "ru->en").FirstOrDefault();
  }
}


