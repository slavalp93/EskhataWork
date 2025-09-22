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
    
    private static string AskGemini(string text, string direction)
    {

      string envDirectoryPath = "C:\\RxData\\git_repository\\Eskhata_Work";
      
      string envFilePath = Path.Combine(envDirectoryPath, ".env");
      
      Env.Load(envFilePath);
      
      var apiKey1 = Env.GetString("apiKey1");
      var apiKey2 = Env.GetString("apiKey2");
      
      var apiKeys = new[]
      {
        apiKey1,
        apiKey2
      };
      
      if (string.IsNullOrWhiteSpace(text))
        return string.Empty;
      
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
          translationInstruction = "Ты — профессиональный переводчик. Верни только перевод текста, без комментариев.";
          break;
      }
      
      var fullPrompt = $"{translationInstruction}\n\n---\n\n{text}";
      
      foreach (var apiKey in apiKeys)
      {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={apiKey}";
        
        using (var client = new HttpClient())
        {
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
              var translation = parsed["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();
              return translation?.Trim() ?? string.Empty;
            }
            catch (Exception ex)
            {
              Logger.Error($"Ошибка парсинга ответа от Gemini: {ex.Message} || Ответ: {responseJson}");
              continue;
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