using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.CollegiateAgencies.Projectsolution;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;


namespace litiko.CollegiateAgencies.Server
{
  partial class ProjectsolutionFunctions
  {
    /// <summary>
    /// Создать проект решения.
    /// </summary>
    /// <returns>Проект решения.</returns>
    [Remote, Public]
    public static IProjectsolution CreateProjectsolution()
    {
      return Projectsolutions.Create();
    }
    
    /// <summary>
    /// Создать пояснительную записку.
    /// </summary>
    /// <returns>Пояснительная записка.</returns>
    [Remote, Public]
    public static Sungero.Docflow.IAddendum CreateExplanatoryNote()
    {
      var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(litiko.CollegiateAgencies.PublicConstants.Module.DocumentKindGuids.ExplanatoryNote);
      if (docKind == null)
        return null;
      
      var newDoc = Sungero.Docflow.Addendums.Create();
      newDoc.DocumentKind = docKind;
      return newDoc;
    }

    /// <summary>
    /// Создать выписку из протокола.
    /// </summary>
    /// <returns>Выписка из протокола.</returns>
    [Remote, Public]
    public static Sungero.Docflow.IAddendum CreateExtractProtocol()
    {
      var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(litiko.CollegiateAgencies.PublicConstants.Module.DocumentKindGuids.ExtractProtocol);
      if (docKind == null)
        return null;
      
      var newDoc = Sungero.Docflow.Addendums.Create();
      newDoc.DocumentKind = docKind;
      return newDoc;
    }

    /// <summary>
    /// Создать постановление.
    /// </summary>
    /// <returns>Постановление.</returns>
    [Remote, Public]
    public static Sungero.Docflow.IAddendum CreateResolution()
    {
      var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(litiko.CollegiateAgencies.PublicConstants.Module.DocumentKindGuids.Resolution);
      if (docKind == null)
        return null;
      
      var newDoc = Sungero.Docflow.Addendums.Create();
      newDoc.DocumentKind = docKind;
      return newDoc;
    }
    
    /// <summary>
    /// Пункты решения (RU).
    /// </summary>
    /// <param name="projectSolution">Проект решения.</param>
    /// <returns>Значение табличного реквизита Постановили карточки Проекта решения.</returns>
    [Converter("GetProjectSolutionDecidedRU")]
    public static string GetProjectSolutionDecidedRU(IProjectsolution projectSolution)
    {
      if (!projectSolution.Decided.Any())
        return null;
      
      return string.Join("\n", projectSolution.Decided
                         .OrderBy(element => element.Number)
                         .Select(element => $"{element.Number}. {element.DecisionRU}")
                        );
    }

    /// <summary>
    /// Пункты решения (TJ).
    /// </summary>
    /// <param name="projectSolution">Проект решения.</param>
    /// <returns>Значение табличного реквизита Постановили карточки Проекта решения.</returns>
    [Converter("GetProjectSolutionDecidedTJ")]
    public static string GetProjectSolutionDecidedTJ(IProjectsolution projectSolution)
    {
      if (!projectSolution.Decided.Any())
        return null;
      
      return string.Join("\n", projectSolution.Decided
                         .OrderBy(element => element.Number)
                         .Select(element => $"{element.Number}. {element.DecisionTJ}")
                        );
    }

    /// <summary>
    /// Пункты решения (EN).
    /// </summary>
    /// <param name="projectSolution">Проект решения.</param>
    /// <returns>Значение табличного реквизита Постановили карточки Проекта решения.</returns>
    [Converter("GetProjectSolutionDecidedEN")]
    public static string GetProjectSolutionDecidedEN(IProjectsolution projectSolution)
    {
      if (!projectSolution.Decided.Any())
        return null;
      
      return string.Join("\n", projectSolution.Decided
                         .OrderBy(element => element.Number)
                         .Select(element => $"{element.Number}. {element.DecisionEN}")
                        );
    }

    /// <summary>
    /// Пункты решения протокола (RU).
    /// </summary>
    /// <param name="projectSolution">Проект решения.</param>
    /// <returns>Значение табличного реквизита Постановили карточки Проекта решения вкладки Протокол.</returns>
    [Public, Converter("GetProjectSolutionDecidedMinutesRU")]
    public static string GetProjectSolutionDecidedMinutesRU(IProjectsolution projectSolution)
    {
      if (!projectSolution.DecidedMinutes.Any())
        return null;
      
      return string.Join("\n", projectSolution.DecidedMinutes
                         .OrderBy(element => element.Number)
                         .Select(element => $"{element.Number}. {element.DecisionRU}")
                        );
    }

    /// <summary>
    /// Пункты решения протокола (TJ).
    /// </summary>
    /// <param name="projectSolution">Проект решения.</param>
    /// <returns>Значение табличного реквизита Постановили карточки Проекта решения вкладки Протокол.</returns>
    [Public, Converter("GetProjectSolutionDecidedMinutesTJ")]
    public static string GetProjectSolutionDecidedMinutesTJ(IProjectsolution projectSolution)
    {
      if (!projectSolution.DecidedMinutes.Any())
        return null;
      
      return string.Join("\n", projectSolution.DecidedMinutes
                         .OrderBy(element => element.Number)
                         .Select(element => $"{element.Number}. {element.DecisionTJ}")
                        );
    }
    
    /// <summary>
    /// Пункты решения протокола (EN).
    /// </summary>
    /// <param name="projectSolution">Проект решения.</param>
    /// <returns>Значение табличного реквизита Постановили карточки Проекта решения вкладки Протокол.</returns>
    [Public, Converter("GetProjectSolutionDecidedMinutesEN")]
    public static string GetProjectSolutionDecidedMinutesEN(IProjectsolution projectSolution)
    {
      if (!projectSolution.DecidedMinutes.Any())
        return null;
      
      return string.Join("\n", projectSolution.DecidedMinutes
                         .OrderBy(element => element.Number)
                         .Select(element => $"{element.Number}. {element.DecisionEN}")
                        );
    }
    
    /// <summary>
    /// Получить наименования приложений к проекту решения.
    /// </summary>
    /// <param name="docId">ИД проекта решения.</param>
    /// <returns>Наименования приложений.</returns>
    [Public]
    public static string GetProjectSolutionAddendumSubjects(long docId)
    {
      var document = Projectsolutions.GetAll().Where(x => x.Id == docId).FirstOrDefault();
      if (document == null)
        return string.Empty;
      
      return string.Join("\n", document.Relations.GetRelated().Select(d => Sungero.Docflow.OfficialDocuments.As(d).Subject));
    }

    //    [Public]
    //    public static string AskChatGPTTJ(string text)
    //    {
    //      var apiKey = "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";
//
    //      if(string.IsNullOrWhiteSpace(text))
    //        return string.Empty;
//
    //      try
    //      {
    //        using (var client = new HttpClient())
    //        {
    //          client.DefaultRequestHeaders.Clear();
    //          client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
//
    //          var url = "https://api.openai.com/v1/chat/completions";
//
    //          // Формируем тело запроса
    //          var requestJson = new JObject
    //          {
    //            ["model"] = "gpt-4o-mini",
    //            ["messages"] = new JArray
    //            {
    //              new JObject
    //              {
    //                ["role"] = "system",
    //                ["content"] = "Ты профессиональный переводчик. Переведи текст с таджикского на русский без объяснений."
    //              },
    //              new JObject
    //              {
    //                ["role"] = "user",
    //                ["content"] = text
    //              }
    //            }
    //          };
//
    //          var content = new StringContent(requestJson.ToString(), Encoding.UTF8, "application/json");
    //          var response = client.PostAsync(url, content).Result;
//
    //          if (!response.IsSuccessStatusCode)
    //          {
    //            var error = response.Content.ReadAsStringAsync().Result;
    //            throw new Exception($"Ошибка OpenAI API: {response.StatusCode} {error}");
    //          }
//
    //          var responseJson = response.Content.ReadAsStringAsync().Result;
    //          var parsed = JObject.Parse(responseJson);
    //          var translation = parsed["choices"]?[0]?["message"]?["content"]?.ToString();
//
    //          return translation ?? string.Empty;
    //        }
    //      }
    //      catch (Exception ex)
    //      {
    //        Logger.Error("Ошибка при обращении к OpenAI API: " + ex.Message, ex);
    //        return string.Empty;
    //      }
  }
}