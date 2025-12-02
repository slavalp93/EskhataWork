using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.IO;
using System.Xml;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using CommonLibrary;
using litiko.Integration.Structures.Module;
using Sungero.Contracts;
using Sungero.Docflow.Server;
using Sungero.FinancialArchive.OutgoingTaxInvoice;

namespace litiko.Integration.Server
{
  public class ModuleFunctions
  {

    #region Отправка / получение ответа от внешней информационной системы
    
    /// <summary>
    /// Отправка запроса во внешнюю информационную систему.
    /// </summary>
    /// <param name="exchDoc">Документ обмена</param>
    /// <param name="lastId">0 - новый запрос, >0 - запрос очередной части предыдущего запроса</param></param>
    /// <param name="entity">Сущность, если запрос по конкретному объекту</param></param>
    /// <returns>Строка с ошибкой или пустая строка</returns>
    [Public, Remote]
    public string SendRequestToIS(IExchangeDocument exchDoc, long lastId, Sungero.Domain.Shared.IEntity entity)
    {
      var logPostfix = string.Format("ExchangeDocId = '{0}'", exchDoc.Id);
      var logPrefix = "Integration. SendRequestToIS.";
      Logger.DebugFormat("{0} Start. {1}", logPrefix, logPostfix);
      
      var method = exchDoc.IntegrationMethod;
      
      var errorMessage = string.Empty;
      var statusRequestToIS = exchDoc.StatusRequestToIS;
      var requestToISInfo = exchDoc.RequestToISInfo;
      
      try
      {
        Uri uri = new Uri(Hyperlinks.Get(exchDoc));
        string ipAdress = Dns.GetHostAddresses(uri.Host).FirstOrDefault().ToString();
        
        //string application_key = $"{uri.Scheme}://{uri.Host}/" +"Integration/odata/Integration/ProcessResponseFromIS##";
        //string application_key = $"{ipAdress}/Integration/odata/Integration/ProcessResponseFromIS##";
        
        string application_key = $"{uri.Host}/Integration/odata/Integration/ProcessResponseFromIS##";
        
        //string application_key = "172.20.70.75/Integration/odata/Integration/ProcessResponseFromIS##";
        
        string url = method.IntegrationSystem.ServiceUrl;
        var xmlRequestBody = string.Empty;
        
        if (method.Name == Constants.Module.IntegrationMethods.R_DR_GET_COMPANY || method.Name == Constants.Module.IntegrationMethods.R_DR_GET_PERSON)
        {
          var counterparty = Sungero.Parties.Counterparties.As(entity);
          if (counterparty != null)
            xmlRequestBody = Integration.Resources.RequestXMLTemplateForCompanyFormat(exchDoc.Id, application_key, method.Name, lastId, counterparty.TIN);
        }
        else if (method.Name == Constants.Module.IntegrationMethods.R_DR_GET_BANK)
        {
          var bank = litiko.Eskhata.Banks.As(entity);
          if (bank != null)
            xmlRequestBody = Integration.Resources.RequestXMLTemplateForBankFormat(exchDoc.Id, application_key, method.Name, lastId, bank.BIC);
        }
        else if (method.Name == Constants.Module.IntegrationMethods.R_DR_GET_CURRENCY_RATES)
        {
          // Получить дату последней успешной интеграции
          var lastExchangeDoc = ExchangeDocuments.GetAll()
            .Where(x => Equals(x.IntegrationMethod, method))
            .Where(x => x.StatusRequestToRX == Integration.ExchangeDocument.StatusRequestToRX.ReceivedFull)
            .OrderByDescending(x => x.Created)
            .FirstOrDefault();
          DateTime lastExchangeDate;
          if (lastExchangeDoc != null && lastExchangeDoc.Created.HasValue)
            lastExchangeDate = lastExchangeDoc.Created.Value.Date;
          else
            lastExchangeDate = Calendar.Today.AddDays(-10).Date;
          
          xmlRequestBody = Integration.Resources.RequestXMLTemplateForCurrencyRatesFormat(exchDoc.Id, application_key, method.Name, lastId, lastExchangeDate.ToString("dd.MM.yyyy"));
        }
        else if (method.Name == Constants.Module.IntegrationMethods.R_DR_SET_CONTRACT || method.Name == Constants.Module.IntegrationMethods.R_DR_SET_PAYMENT_DOCUMENT)
        {
          var document = Sungero.Docflow.OfficialDocuments.As(entity);
          if (document != null)
            xmlRequestBody = PublicFunctions.Module.BuildDocumentXml(document, exchDoc.Id, application_key, method.Name, lastId);
        }
        else
          xmlRequestBody = Integration.Resources.RequestXMLTemplateFormat(exchDoc.Id, application_key, method.Name, lastId);

        if (method.SaveRequestToIS.Value)
        {
          var exchQueue = ExchangeQueues.Create();
          exchQueue.ExchangeDocument = exchDoc;
          exchQueue.Xml = Encoding.UTF8.GetBytes(xmlRequestBody);
          exchQueue.Name = Integration.Resources.VersionRequestToISFormat(lastId);
          exchQueue.Save();
        } 

        using (HttpClient client = new HttpClient())
        {
          // Установка заголовков запроса
          client.DefaultRequestHeaders.Accept.Clear();
          client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/xml"));

          // Создание контента для отправки
          StringContent content = new StringContent(xmlRequestBody, Encoding.UTF8, "application/xml");

          // Отправка POST-запроса
          HttpResponseMessage response = client.PostAsync(url, content).Result;

          // Проверка успешности запроса
          response.EnsureSuccessStatusCode();

          // Чтение ответа как строки
          string responseBody = response.Content.ReadAsStringAsync().Result;

          if (IsValidXml(responseBody))
          {
            // Парсинг ответа
            XElement xmlResponse = XElement.Parse(responseBody);
            
            // Обработка ответа
            string state = xmlResponse.Element("response")?.Element("state")?.Value;
            string stateMsg = xmlResponse.Element("response")?.Element("state_msg")?.Value;
            
            if (state == "1")
            {
              Logger.DebugFormat("Response successful. State message: {0}", stateMsg);
              statusRequestToIS = Integration.ExchangeDocument.StatusRequestToIS.Sent;
            }
            else
            {
              Logger.DebugFormat("Response failed. State message: {0}", stateMsg);
              statusRequestToIS = Integration.ExchangeDocument.StatusRequestToIS.Error;
            }
            
            requestToISInfo = stateMsg;
            
            // Сохранение ответа
            if (method.SaveResponseFromIS.Value)
            {
              var exchQueue = ExchangeQueues.Create();
              exchQueue.ExchangeDocument = exchDoc;
              exchQueue.Xml = Encoding.UTF8.GetBytes(responseBody);
              exchQueue.Name = Integration.Resources.VersionResponseFromISFormat(lastId);
              exchQueue.Save();
            }
          }
          else
          {
            errorMessage = string.Format("Response is not valid XML: {0}", responseBody);
          }
        }

      }
      catch (HttpRequestException e)
      {
        // Обработка исключений сети и HTTP
        errorMessage = string.Format("Request error: {0}", e.Message);
      }
      catch (XmlException e)
      {
        // Обработка ошибок XML парсинга
        errorMessage = string.Format("XML error: {0}", e.Message);
      }
      catch (Exception e)
      {
        // Обработка других ошибок
        errorMessage = string.Format("Unexpected error: {0}", e.Message);
      }
      
      if (!string.IsNullOrEmpty(errorMessage))
      {
        requestToISInfo = errorMessage;
        statusRequestToIS = Integration.ExchangeDocument.StatusRequestToIS.Error;
        Logger.Error(errorMessage);
      }      
      
      // Обновляем exchDoc асинхронным обработчиком для не Online-запросов
      if (!exchDoc.IsOnline.GetValueOrDefault() && (exchDoc.StatusRequestToIS != statusRequestToIS || exchDoc.RequestToISInfo != requestToISInfo))
      {        
        var asyncHandler = Integration.AsyncHandlers.UpdateExchangeDoc.Create();
        asyncHandler.DocId = exchDoc.Id;
        asyncHandler.RequestToISInfo = requestToISInfo;
        asyncHandler.StatusRequestToIS = statusRequestToIS.ToString();
        asyncHandler.IncreaseNumberOfPackages = false;
        asyncHandler.ExecuteAsync();
      }
      
      Logger.DebugFormat("{0} Finish. {1}", logPrefix, logPostfix);
      
      return errorMessage;
    }
    
    /// <summary>
    /// Обработка ответа от внешней информационной системы.
    /// </summary>
    /// <param name="xmlData">xml с запрошенными данными.</param>
    /// <returns>xml с информацией о результате сохранения в RX</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public byte[] ProcessResponseFromIS(byte[] xmlData)
    {                            
      var logPrefix = "Integration. ProcessResponseFromIS.";
      Logger.DebugFormat("{0} Start.", logPrefix);

      const string invalidParamValue = "???";

      string sessionIdStr = string.Empty;
      long sessionId;
      string dictionary = string.Empty;
      string lastIdStr = string.Empty;
      long lastId;

      string sessionIdForResponse = invalidParamValue;
      string dictionaryForResponse = invalidParamValue;

      bool hasError = false;
      
      #region Предпроверки
      
      // Пустое тело запроса
      if (xmlData == null || xmlData.Length == 0)
      {
        return BuildErrorResponse(logPrefix, "Request body is empty", invalidParamValue, invalidParamValue);
      }

      XmlDocument xmlDoc = new XmlDocument();
      XmlElement root;

      using (var xmlStream = new MemoryStream(xmlData))
      {
        try
        {
          var settings = new XmlReaderSettings
          {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
          };

          using (var reader = XmlReader.Create(xmlStream, settings))
          {
            xmlDoc.Load(reader);
          }
        }
        catch (XmlException ex)
        {
          return BuildErrorResponse(
            logPrefix,
            string.Format("Invalid XML format: {0}", ex.Message),
            invalidParamValue,
            invalidParamValue);
        }
        catch (Exception ex)
        {
          return BuildErrorResponse(
            logPrefix,
            string.Format("Error while parsing XML: {0}", ex.Message),
            invalidParamValue,
            invalidParamValue);
        }
      }

      root = xmlDoc.DocumentElement;
      if (root == null)
      {
        return BuildErrorResponse(
          logPrefix,
          "Root XML element is absent",
          invalidParamValue,
          invalidParamValue);
      }      

      byte[] errorResponse;

      // head
      var headNode = RequireNode(root, "head",
                                 "head node is absent",
                                 logPrefix,
                                 invalidParamValue,
                                 invalidParamValue,
                                 out errorResponse);
      if (headNode == null)
        return errorResponse;

      // head/session_id
      var sessionIdNode = RequireNode(headNode, "session_id",
                                      "session_id node is absent",
                                      logPrefix,
                                      invalidParamValue,
                                      invalidParamValue,
                                      out errorResponse);
      if (sessionIdNode == null)
        return errorResponse;

      sessionIdStr = sessionIdNode.InnerText;

      if (!long.TryParse(sessionIdStr, out sessionId))
      {
        return BuildErrorResponse(
          logPrefix,
          "Invalid value in session_id",
          invalidParamValue,
          invalidParamValue);
      }

      sessionIdForResponse = sessionId.ToString();
      var logPostfix = string.Format("ExchangeDocId = '{0}'", sessionId);

      // request
      var requestNode = RequireNode(root, "request",
                                    "request node is absent",
                                    logPrefix,
                                    sessionIdForResponse,
                                    invalidParamValue,
                                    out errorResponse);
      if (requestNode == null)
        return errorResponse;

      // request/dictionary
      var dictionaryNode = RequireNode(requestNode, "dictionary",
                                       "dictionary node is absent",
                                       logPrefix,
                                       sessionIdForResponse,
                                       invalidParamValue,
                                       out errorResponse);
      if (dictionaryNode == null)
        return errorResponse;

      dictionary = dictionaryNode.InnerText;
      if (string.IsNullOrWhiteSpace(dictionary))
      {
        return BuildErrorResponse(
          logPrefix,
          "Invalid value in dictionary",
          sessionIdForResponse,
          invalidParamValue);
      }

      dictionaryForResponse = dictionary;

      // request/lastId
      var lastIdNode = RequireNode(requestNode, "lastId",
                                   "lastId node is absent",
                                   logPrefix,
                                   sessionIdForResponse,
                                   dictionaryForResponse,
                                   out errorResponse);
      if (lastIdNode == null)
        return errorResponse;

      lastIdStr = lastIdNode.InnerText;
      if (!long.TryParse(lastIdStr, out lastId))
      {
        return BuildErrorResponse(
          logPrefix,
          "Invalid value in lastId",
          sessionIdForResponse,
          dictionaryForResponse);
      }

      // request/Data
      var dataNode = RequireNode(requestNode, "Data",
                                 "Data node is absent",
                                 logPrefix,
                                 sessionIdForResponse,
                                 dictionaryForResponse,
                                 out errorResponse);
      if (dataNode == null)
        return errorResponse;

      var exchDoc = ExchangeDocuments.GetAll()
        .FirstOrDefault(d => d.Id == sessionId);
      if (exchDoc == null)
      {
        return BuildErrorResponse(
          logPrefix,
          "Request for session id not found. Session Id=" + sessionId,
          sessionIdForResponse,
          dictionaryForResponse);
      }

      #endregion

      var statusRequestToRX = exchDoc.StatusRequestToRX;
      var requestToRXInfo = exchDoc.RequestToRXInfo;

      try
      {
        var exchQueue = ExchangeQueues.Create();
        exchQueue.ExchangeDocument = exchDoc;
        exchQueue.Xml = xmlData;
        exchQueue.Name = Integration.Resources.VersionRequestToRXFormat(lastId);
        exchQueue.Save();

        // обновить статусы в exchDoc
        if (lastId > 0)
          statusRequestToRX = Integration.ExchangeDocument.StatusRequestToRX.ReceivedPart;
        else
          statusRequestToRX = Integration.ExchangeDocument.StatusRequestToRX.ReceivedFull;

        if (requestToRXInfo != "Saved")
          requestToRXInfo = "Saved";
      }
      catch (Exception ex)
      {
        hasError = true;
        var errorMessage = ex.Message;

        Logger.ErrorFormat("{0} ErrorMessage: {1}. {2}", logPrefix, errorMessage, logPostfix);

        statusRequestToRX = Integration.ExchangeDocument.StatusRequestToRX.Error;
        requestToRXInfo = errorMessage;

        return Encoding.UTF8.GetBytes(
          Integration.Resources.ResponseXMLTemplateFormat(sessionIdForResponse, dictionaryForResponse, 2, errorMessage));
      }

      if (!exchDoc.IsOnline.GetValueOrDefault())
      {
        // Обновляем exchDoc асинхронным обработчиком для не Online-запросов
        var asyncHandler = Integration.AsyncHandlers.UpdateExchangeDoc.Create();
        asyncHandler.DocId = exchDoc.Id;
        asyncHandler.StatusRequestToRX = statusRequestToRX.ToString();
        asyncHandler.RequestToRXInfo = requestToRXInfo;
        asyncHandler.IncreaseNumberOfPackages = true;
        asyncHandler.ExecuteAsync();

        if (!hasError)
        {
          if (lastId > 0)
          {
            // вызвать получение остальной части пакета
            SendRequestToIS(exchDoc, lastId, null);
          }
          else
          {
            // TODO: возможно убрать ?
            Thread.Sleep(1000); // Пауза на 1 сек.

            // запустить обработчик пакета
            var asyncHandlerImportData = Integration.AsyncHandlers.ImportData.Create();
            asyncHandlerImportData.ExchangeDocId = exchDoc.Id;
            asyncHandlerImportData.ExecuteAsync();
          }
        }
      }

      Logger.DebugFormat("{0} Finish. {1}", logPrefix, logPostfix);
      return Encoding.UTF8.GetBytes(
        Integration.Resources.ResponseXMLTemplateFormat(sessionIdForResponse, dictionaryForResponse, 1, "Saved"));
    }
    
    /// <summary>
    /// Унифицированное формирование ответа об ошибке + логирование.
    /// </summary>
    private byte[] BuildErrorResponse(string logPrefix, string errorMessage, string sessionIdForResponse, string dictionaryForResponse)
    {
      Logger.ErrorFormat("{0} ErrorMessage: {1}.", logPrefix, errorMessage);    
      return Encoding.UTF8.GetBytes(Integration.Resources.ResponseXMLTemplateFormat(sessionIdForResponse, dictionaryForResponse, 2, errorMessage));
    }

    /// <summary>
    /// Обязательный узел: если нет — логируем и возвращаем готовый errorResponse.
    /// </summary>
    private XmlNode RequireNode(XmlNode parent, string xPath, string errorIfMissing, string logPrefix, string sessionIdForResponse, string dictionaryForResponse, out byte[] errorResponse)
    {
      errorResponse = null;

      if (parent == null)
      {
        errorResponse = BuildErrorResponse(logPrefix, errorIfMissing, sessionIdForResponse, dictionaryForResponse);
        return null;
      }

      var node = parent.SelectSingleNode(xPath);
      if (node == null)
      {
        errorResponse = BuildErrorResponse(logPrefix, errorIfMissing, sessionIdForResponse, dictionaryForResponse);
      }

      return node;
    }    

    [Public, Remote(IsPure = true)]
    public static long WaitForGettingDataFromIS(long exchDocId, int intervalMilliseconds, int maxAttempts)
    {
      for (int attempt = 0; attempt < maxAttempts; attempt++)
      {
        var exchQueue = ExchangeQueues.GetAll()
          .Where(x => x.ExchangeDocument.Id == exchDocId)
          .Where(x => x.Name == Integration.Resources.VersionRequestToRXFormat(0).ToString())
          .FirstOrDefault();
        
        if (exchQueue != null)
        {
          return exchQueue.Id;
        }
        
        Thread.Sleep(intervalMilliseconds); // Ожидание
      }
      
      return 0; // Условие не выполнено за maxAttempts попыток
    }

    /// <summary>
    /// Ф-я для запуска фоновых процессов по интеграции с внешней системой.
    /// </summary>
    /// <param name="integrationMethodName">Наименование метода интеграции.</param>
    public static void BackgroundProcessStart(string integrationMethodName)
    {
      int lastId = 0;
      
      var integrationMethod = IntegrationMethods.GetAll().Where(x => x.Name == integrationMethodName).FirstOrDefault();
      if (integrationMethod == null)
        throw AppliedCodeException.Create(string.Format("Integration method {0} not found", integrationMethodName));
            
      var exchDoc = Integration.ExchangeDocuments.Create();
      exchDoc.IntegrationMethod = integrationMethod;
      exchDoc.IsOnline = false;
      exchDoc.Save();
      
      var errorMessage = Functions.Module.SendRequestToIS(exchDoc, lastId, null);
      if (!string.IsNullOrEmpty(errorMessage))
        throw AppliedCodeException.Create(errorMessage);
    }
    
    #endregion

    #region Создание/обновление сущьностей по XML-данным от внешней информационной системы.
    
    /// <summary>
    /// Обработка подразделений.
    /// </summary>
    /// <param name="dataElements">Информация по подразделиям в виде XElement.</param>
    /// <returns>Список ошибок (List<string>)</returns>
    public List<string> R_DR_GET_DEPART(System.Collections.Generic.IEnumerable<System.Xml.Linq.XElement> dataElements)
    {
      Logger.Debug("R_DR_GET_DEPART - Start");
      var errorList = new List<string>();
      int countAll = dataElements.Count();
      int countChanged = 0;
      int countNotChanged = 0;
      int countErrors = 0;
      
      foreach (var element in dataElements.OrderBy(e => (int)e.Element("Level")))
      {
        
        Transactions.Execute(() =>
                             {
                               var isCode = element.Element("Code")?.Value;
                               var isId = element.Element("ID")?.Value;
                               var isMainDepartmentID = element.Element("MainDepartmentID")?.Value;
                               var isNameRU = element.Element("NameRU")?.Value;
                               var isShortName = element.Element("ShortName")?.Value;
                               var isState = element.Element("State")?.Value;
                               var isHeadOfDepartment = element.Element("HeadOfDepartment")?.Value;
                               var bankExternalID = "10598717";
                               
                               try
                               {
                                 if (string.IsNullOrEmpty(isId) || string.IsNullOrEmpty(isCode) || string.IsNullOrEmpty(isNameRU) || string.IsNullOrEmpty(isState))
                                   throw AppliedCodeException.Create(string.Format("Not all required fields are filled in. ID:{0}, Code:{1}, NameRU:{2}, State:{3}", isId, isCode, isNameRU, isState));
                                 
                                 var mainDepartment = Eskhata.Departments.Null;
                                 if (!string.IsNullOrEmpty(isMainDepartmentID))
                                 {
                                   mainDepartment = Eskhata.Departments.GetAll().Where(d => d.ExternalId == isMainDepartmentID).FirstOrDefault();
                                   if (mainDepartment == null && isState == "1")
                                     throw AppliedCodeException.Create(string.Format("The parent department with ID={0} was not found.", isMainDepartmentID));
                                 }
                                 
                                 var department = Eskhata.Departments.GetAll().Where(d => d.ExternalId == isId).FirstOrDefault();
                                 if (department != null)
                                   Logger.DebugFormat("Department with ExternalId:{0} was found. Id:{1}, Name:{2}", isId, department.Id, department.Name);
                                 else
                                 {
                                   if (isState == "1")
                                   {
                                     department = Eskhata.Departments.Create();
                                     department.ExternalId = isId;
                                     Logger.DebugFormat("Create new Department with ExternalId:{0}. Id:{1}", isId, department.Id);
                                   }
                                   else
                                   {
                                     Logger.DebugFormat("New Department with ExternalId:{0} was not created, because it has Sate:{1}", isId, isState);
                                     countNotChanged++;
                                   }
                                 }
                                 
                                 if (department != null)
                                 {
                                   if (isState == "0")
                                   {
                                     if (Equals(department.Status, Eskhata.Department.Status.Active))
                                     {
                                       Logger.DebugFormat("Change Status: current:{0}, new:{1}", "Active", "Closed");
                                       department.Status = Eskhata.Department.Status.Closed;
                                     }
                                   }
                                   else
                                   {
                                     var businessUnit = litiko.Eskhata.BusinessUnits.GetAll().Where(x => x.ExternalId == bankExternalID).SingleOrDefault();
                                     if (businessUnit != null && !Equals(department.BusinessUnit, businessUnit))
                                     {
                                       Logger.DebugFormat("Change BusinessUnit: current:{0}, new:{1}", department.BusinessUnit?.Name, businessUnit?.Name);
                                       department.BusinessUnit = businessUnit;
                                     }
                                     
                                     if (Equals(department.Status, Eskhata.Department.Status.Closed))
                                     {
                                       Logger.DebugFormat("Change Status: current:{0}, new:{1}", "Closed", "Active");
                                       department.Status = Eskhata.Department.Status.Active;
                                     }
                                     
                                     if (department.ExternalCodelitiko != isCode)
                                     {
                                       Logger.DebugFormat("Change ExternalCode: current:{0}, new:{1}", department.ExternalCodelitiko, isCode);
                                       department.ExternalCodelitiko = isCode;
                                     }
                                     
                                     if (department.Name != isNameRU)
                                     {
                                       Logger.DebugFormat("Change Name: current:{0}, new:{1}", department.Name, isNameRU);
                                       department.Name = isNameRU;
                                     }
                                     
                                     if (department.ShortName != isShortName)
                                     {
                                       Logger.DebugFormat("Change ShortName: current:{0}, new:{1}", department.ShortName, isShortName);
                                       department.ShortName = isShortName;
                                     }
                                     
                                     if (!Equals(department.HeadOffice, mainDepartment))
                                     {
                                       Logger.DebugFormat("Change HeadOffice: current:{0}, new:{1}", department.HeadOffice?.Name, mainDepartment?.Name);
                                       department.HeadOffice = mainDepartment;
                                     }
                                     
                                     var boss = Sungero.Company.Employees.Null;
                                     if (!string.IsNullOrEmpty(isHeadOfDepartment))
                                     {
                                       boss = Sungero.Company.Employees.GetAll().Where(e => e.ExternalId == isHeadOfDepartment).FirstOrDefault();
                                       if (boss == null)
                                         Logger.DebugFormat("The employee with ExternalId={0} was not found.", isHeadOfDepartment);
                                     }
                                     if (!Equals(department.Manager, boss))
                                     {
                                       Logger.DebugFormat("Change Manager: current:{0}, new:{1}", department?.Manager?.Name, boss?.Name);
                                       department.Manager = boss;
                                     }
                                     
                                   }
                                   
                                   if (department.State.IsInserted || department.State.IsChanged)
                                   {
                                     department.Save();
                                     Logger.DebugFormat("Department successfully saved. ExternalId:{0}, Id:{1}", isId, department.Id);
                                     countChanged++;
                                   }
                                   else
                                   {
                                     Logger.DebugFormat("There are no changes in department. ExternalId:{0}, Id:{1}", isId, department.Id);
                                     countNotChanged++;
                                   }
                                 }
                               }
                               catch (Exception ex)
                               {
                                 var errorMessage = string.Format("Error when processing department with ExternalId:{0}. Description: {1}. StackTrace: {2}", isId, ex.Message, ex.StackTrace);
                                 Logger.Error(errorMessage);
                                 errorList.Add(errorMessage);
                                 countErrors++;
                               }
                             });
      }
      Logger.DebugFormat("R_DR_GET_DEPART - Total: CountAll:{0} CountChanged:{1} CountNotChanged:{2} CountErrors:{3}", countAll, countChanged, countNotChanged, countErrors);
      
      Logger.Debug("R_DR_GET_DEPART - Finish");
      return errorList;
    }
    
    /// <summary>
    /// Обработка наших организаций.
    /// </summary>
    /// <param name="dataElements">Информация по нашим организациям в виде XElement.</param>
    /// <returns>Список ошибок (List<string>)</returns>
    public List<string> R_DR_GET_BUSINESSUNITS(System.Collections.Generic.IEnumerable<System.Xml.Linq.XElement> dataElements)
    {
      Logger.Debug("R_DR_GET_BUSINESSUNITS - Start");
      var errorList = new List<string>();
      int countAll = dataElements.Count();
      int countChanged = 0;
      int countNotChanged = 0;
      int countErrors = 0;
      
      foreach (var element in dataElements)
      {
        Transactions.Execute(() =>
                             {
                               var isId = element.Element("ID")?.Value;
                               var isName = element.Element("NAME")?.Value;
                               
                               var isLongName = element.Element("LONG_NAME")?.Value;
                               var isIName = element.Element("I_NAME")?.Value;
                               var isRezident = element.Element("REZIDENT")?.Value;
                               var isNuRezident = element.Element("NU_REZIDENT")?.Value;
                               var isINN = element.Element("INN")?.Value;
                               var isKPP = element.Element("KPP")?.Value;
                               var isOKPO = element.Element("KOD_OKPO")?.Value;
                               var isOKOPF = element.Element("FORMA")?.Value;
                               var isOKFS = element.Element("OWNERSHIP")?.Value;
                               var isCodeOKONHelements = element.Element("CODE_OKONH").Elements("element");
                               var isCodeOKVEDelements = element.Element("CODE_OKVED").Elements("element");
                               var isRegistnum = element.Element("REGIST_NUM")?.Value;
                               var isNumbers = element.Element("NUMBERS")?.Value;
                               var isBusiness = element.Element("BUSINESS")?.Value;
                               var isPS_REF = element.Element("PS_REF")?.Value;
                               var isCountry = element.Element("COUNTRY")?.Value;
                               var isPostAdress = element.Element("PostAdress")?.Value;
                               var isLegalAdress = element.Element("LegalAdress")?.Value;
                               var isPhone = element.Element("Phone")?.Value;
                               var isEmail = element.Element("Email")?.Value;
                               var isWebSite = element.Element("WebSite")?.Value;
                               
                               try
                               {
                                 if (string.IsNullOrEmpty(isId) || string.IsNullOrEmpty(isName))
                                   throw AppliedCodeException.Create(string.Format("Not all required fields are filled in. ID:{0}, NAME:{1}", isId, isName));
                                 
                                 var businessUnit = Eskhata.BusinessUnits.GetAll().Where(d => d.ExternalId == isId).FirstOrDefault();
                                 if (businessUnit != null)
                                   Logger.DebugFormat("BusinessUnit with ExternalId:{0} was found. Id:{1}, Name:{2}", isId, businessUnit.Id, businessUnit.Name);
                                 else
                                 {
                                   businessUnit = Eskhata.BusinessUnits.Create();
                                   businessUnit.ExternalId = isId;
                                   Logger.DebugFormat("Create new BusinessUnit with ExternalId:{0}. Id:{1}", isId, businessUnit.Id);
                                 }
                                 
                                 if (businessUnit.Name != isName)
                                 {
                                   Logger.DebugFormat("Change Name: current:{0}, new:{1}", businessUnit.Name, isName);
                                   businessUnit.Name = isName;
                                 }
                                 
                                 if(!string.IsNullOrEmpty(isLongName) && businessUnit.LegalName != isLongName)
                                 {
                                   Logger.DebugFormat("Change LegalName: current:{0}, new:{1}", businessUnit.LegalName, isLongName);
                                   businessUnit.LegalName = isLongName;
                                 }
                                 
                                 if(!string.IsNullOrEmpty(isIName) && businessUnit.Inamelitiko != isIName)
                                 {
                                   Logger.DebugFormat("Change Inamelitiko: current:{0}, new:{1}", businessUnit.Inamelitiko, isIName);
                                   businessUnit.Inamelitiko = isIName;
                                 }

                                 if(!string.IsNullOrEmpty(isNuRezident))
                                 {
                                   bool isNuRezidentBool = isNuRezident == "1" ? true : false;
                                   if(businessUnit.NUNonrezidentlitiko != !isNuRezidentBool)
                                   {
                                     Logger.DebugFormat("Change NUNonrezidentlitiko: current:{0}, new:{1}", businessUnit.NUNonrezidentlitiko, !isNuRezidentBool);
                                     businessUnit.NUNonrezidentlitiko = !isNuRezidentBool;
                                   }
                                 }
                                 
                                 if(!string.IsNullOrEmpty(isRezident))
                                 {
                                   bool isRezidentBool = isRezident == "1" ? true : false;
                                   if(businessUnit.Nonresident != !isRezidentBool)
                                   {
                                     Logger.DebugFormat("Change Nonresident: current:{0}, new:{1}", businessUnit.Nonresident, !isRezidentBool);
                                     businessUnit.Nonresident = !isRezidentBool;
                                   }
                                 }
                                 
                                 if(!string.IsNullOrEmpty(isINN) && businessUnit.TIN != isINN)
                                 {
                                   Logger.DebugFormat("Change TIN: current:{0}, new:{1}", businessUnit.TIN, isINN);
                                   businessUnit.TIN = isINN;
                                 }
                                 
                                 if(!string.IsNullOrEmpty(isKPP) && businessUnit.TRRC != isKPP)
                                 {
                                   Logger.DebugFormat("Change TRRC: current:{0}, new:{1}", businessUnit.TRRC, isKPP);
                                   businessUnit.TRRC = isKPP;
                                 }
                                 
                                 if(!string.IsNullOrEmpty(isOKPO) && businessUnit.NCEO != isOKPO)
                                 {
                                   Logger.DebugFormat("Change NCEO: current:{0}, new:{1}", businessUnit.NCEO, isOKPO);
                                   businessUnit.NCEO = isOKPO;
                                 }
                                 
                                 if(!string.IsNullOrEmpty(isOKOPF))
                                 {
                                   var okopf = litiko.NSI.OKOPFs.GetAll().Where(x => x.ExternalId == isOKOPF).FirstOrDefault();
                                   if(okopf != null && !Equals(businessUnit.OKOPFlitiko, okopf))
                                   {
                                     Logger.DebugFormat("Change OKOPFlitiko: current:{0}, new:{1}", businessUnit.OKOPFlitiko?.Name, okopf.Name);
                                     businessUnit.OKOPFlitiko = okopf;
                                   }
                                 }

                                 if(!string.IsNullOrEmpty(isOKFS))
                                 {
                                   var okfs = litiko.NSI.OKFSes.GetAll().Where(x => x.ExternalId == isOKFS).FirstOrDefault();
                                   if(okfs != null && !Equals(businessUnit.OKFSlitiko, okfs))
                                   {
                                     Logger.DebugFormat("Change OKFSlitiko: current:{0}, new:{1}", businessUnit.OKOPFlitiko?.Name, okfs.Name);
                                     businessUnit.OKFSlitiko = okfs;
                                   }
                                 }
                                 
                                 if(isCodeOKONHelements.Any())
                                 {
                                   var elementValues = isCodeOKONHelements.Select(x => x.Value).ToList();
                                   if(businessUnit.OKONHlitiko.Select(x => x.OKONH.ExternalId).Any(x => !elementValues.Contains(x)))
                                   {
                                     businessUnit.OKONHlitiko.Clear();
                                     Logger.DebugFormat("Change OKONHlitiko: Clear");
                                   }
                                   
                                   foreach (var isCodeOKONH in isCodeOKONHelements)
                                   {
                                     var okonh = litiko.NSI.OKONHs.GetAll().Where(x => x.ExternalId == isCodeOKONH.Value).FirstOrDefault();
                                     if(okonh != null && !businessUnit.OKONHlitiko.Any(x => Equals(x.OKONH, okonh)))
                                     {
                                       var newRecord = businessUnit.OKONHlitiko.AddNew();
                                       newRecord.OKONH = okonh;
                                       Logger.DebugFormat("Change OKONHlitiko: added:{0}", okonh.Name);
                                     }
                                   }
                                 }
                                 
                                 if(isCodeOKVEDelements.Any())
                                 {
                                   var elementValues = isCodeOKVEDelements.Select(x => x.Value).ToList();
                                   if(businessUnit.OKVEDlitiko.Select(x => x.OKVED.ExternalId).Any(x => !elementValues.Contains(x)))
                                   {
                                     businessUnit.OKVEDlitiko.Clear();
                                     Logger.DebugFormat("Change OKVEDlitiko: Clear");
                                   }
                                   
                                   foreach (var isCodeOKVED in isCodeOKVEDelements)
                                   {
                                     var okved = litiko.NSI.OKVEDs.GetAll().Where(x => x.ExternalId == isCodeOKVED.Value).FirstOrDefault();
                                     if(okved != null && !businessUnit.OKVEDlitiko.Any(x => Equals(x.OKVED, okved)))
                                     {
                                       var newRecord = businessUnit.OKVEDlitiko.AddNew();
                                       newRecord.OKVED = okved;
                                       Logger.DebugFormat("Change OKVEDlitiko: added:{0}", okved.Name);
                                     }
                                   }
                                 }
                                 
                                 if(!string.IsNullOrEmpty(isRegistnum) && businessUnit.RegNumlitiko != isRegistnum)
                                 {
                                   Logger.DebugFormat("Change RegNumlitiko: current:{0}, new:{1}", businessUnit.RegNumlitiko, isRegistnum);
                                   businessUnit.RegNumlitiko = isRegistnum;
                                 }
                                 
                                 if(!string.IsNullOrEmpty(isNumbers) && businessUnit.Numberslitiko != int.Parse(isNumbers))
                                 {
                                   Logger.DebugFormat("Change Numberslitiko: current:{0}, new:{1}", businessUnit.Numberslitiko, isNumbers);
                                   businessUnit.Numberslitiko = int.Parse(isNumbers);
                                 }
                                 
                                 if(!string.IsNullOrEmpty(isBusiness) && businessUnit.Businesslitiko != isBusiness)
                                 {
                                   Logger.DebugFormat("Change Businesslitiko: current:{0}, new:{1}", businessUnit.Businesslitiko, isBusiness);
                                   businessUnit.Businesslitiko = isBusiness;
                                 }
                                 
                                 if(!string.IsNullOrEmpty(isPS_REF))
                                 {
                                   var enterpriseType = litiko.NSI.EnterpriseTypes.GetAll().Where(x => x.ExternalId == isPS_REF).FirstOrDefault();
                                   if(enterpriseType != null && !Equals(businessUnit.EnterpriseTypelitiko, enterpriseType))
                                   {
                                     Logger.DebugFormat("Change EnterpriseTypelitiko: current:{0}, new:{1}", businessUnit.EnterpriseTypelitiko?.Name, enterpriseType.Name);
                                     businessUnit.EnterpriseTypelitiko = enterpriseType;
                                   }
                                 }
                                 
                                 if(!string.IsNullOrEmpty(isCountry))
                                 {
                                   var country = litiko.Eskhata.Countries.GetAll().Where(x => x.ExternalIdlitiko == isCountry).FirstOrDefault();
                                   if(country != null && !Equals(businessUnit.Countrylitiko, country))
                                   {
                                     Logger.DebugFormat("Change Countrylitiko: current:{0}, new:{1}", businessUnit.Countrylitiko?.Name, country.Name);
                                     businessUnit.Countrylitiko = country;
                                   }
                                 }

                                 if(!string.IsNullOrEmpty(isPostAdress) && businessUnit.PostalAddress != isPostAdress)
                                 {
                                   Logger.DebugFormat("Change PostalAddress: current:{0}, new:{1}", businessUnit.PostalAddress, isPostAdress);
                                   businessUnit.PostalAddress = isPostAdress;
                                 }

                                 if(!string.IsNullOrEmpty(isLegalAdress) && businessUnit.LegalAddress != isLegalAdress)
                                 {
                                   Logger.DebugFormat("Change LegalAddress: current:{0}, new:{1}", businessUnit.LegalAddress, isLegalAdress);
                                   businessUnit.LegalAddress = isLegalAdress;
                                 }

                                 if(!string.IsNullOrEmpty(isPhone) && businessUnit.Phones != isPhone)
                                 {
                                   Logger.DebugFormat("Change Phones: current:{0}, new:{1}", businessUnit.Phones, isPhone);
                                   businessUnit.Phones = isPhone;
                                 }

                                 if(!string.IsNullOrEmpty(isEmail) && businessUnit.Email != isEmail)
                                 {
                                   Logger.DebugFormat("Change Email: current:{0}, new:{1}", businessUnit.Email, isEmail);
                                   businessUnit.Email = isEmail;
                                 }

                                 if(!string.IsNullOrEmpty(isWebSite) && businessUnit.Homepage != isWebSite)
                                 {
                                   Logger.DebugFormat("Change Homepage: current:{0}, new:{1}", businessUnit.Homepage, isWebSite);
                                   businessUnit.Homepage = isWebSite;
                                 }
                                 
                                 if (businessUnit.State.IsInserted || businessUnit.State.IsChanged)
                                 {
                                   businessUnit.Save();
                                   Logger.DebugFormat("BusinessUnit successfully saved. ExternalId:{0}, Id:{1}", isId, businessUnit.Id);
                                   countChanged++;
                                 }
                                 else
                                 {
                                   Logger.DebugFormat("There are no changes in BusinessUnit. ExternalId:{0}, Id:{1}", isId, businessUnit.Id);
                                   countNotChanged++;
                                 }
                               }
                               catch (Exception ex)
                               {
                                 var errorMessage = string.Format("Error when processing BusinessUnit with ExternalId:{0}. Description: {1}. StackTrace: {2}", isId, ex.Message, ex.StackTrace);
                                 Logger.Error(errorMessage);
                                 errorList.Add(errorMessage);
                                 countErrors++;
                               }
                             });
      }
      
      Logger.DebugFormat("R_DR_GET_BUSINESSUNITS - Total: CountAll:{0} CountChanged:{1} CountNotChanged:{2} CountErrors:{3}", countAll, countChanged, countNotChanged, countErrors);
      Logger.Debug("R_DR_GET_BUSINESSUNITS - Finish");
      return errorList;
    }
    
    /// <summary>
    /// Обработка сотрудников.
    /// </summary>
    /// <param name="dataElements">Информация по сотрудникам в виде XElement.</param>
    /// <returns>Список ошибок (List<string>)</returns>
    public List<string> R_DR_GET_EMPLOYEES(System.Collections.Generic.IEnumerable<System.Xml.Linq.XElement> dataElements)
    {
      Logger.Debug("R_DR_GET_EMPLOYEES - Start");
      var errorList = new List<string>();
      int countAll = dataElements.Count();
      int countChanged = 0;
      int countNotChanged = 0;
      int countErrors = 0;
      
      foreach (var element in dataElements)
      {
        Transactions.Execute(() =>
                             {
                               var isId = element.Element("ID")?.Value;
                               var isState = element.Element("State")?.Value;
                               var isFirstNameRu = element.Element("FirstNameRU")?.Value;
                               var isLastNameRu = element.Element("LastNameRU")?.Value;
                               var isDepartmentID = element.Element("DepartmentID")?.Value;
                               
                               var isFirstNameTG = element.Element("FirstNameTG")?.Value;
                               var isLastNameTG = element.Element("LastNameTG")?.Value;
                               var isMiddleNameRU = element.Element("MiddleNameRU")?.Value;
                               var isMiddleNameTG = element.Element("MiddleNameTG")?.Value;
                               
                               var isPersonnelNumber = element.Element("isPersonnelNumber")?.Value;
                               var isPhone = element.Element("Phone")?.Value;
                               var isJobTittle = element.Element("JobTitle");
                               var isPerson = element.Element("FASE");
                               var isEmployeeEmail = element.Element("FASE").Element("Email")?.Value;
                               
                               try
                               {
                                 if (string.IsNullOrEmpty(isId) || string.IsNullOrEmpty(isState) || string.IsNullOrEmpty(isFirstNameRu) || string.IsNullOrEmpty(isLastNameRu) || string.IsNullOrEmpty(isDepartmentID))
                                   throw AppliedCodeException.Create(string.Format("Not all required fields are filled in. ID:{0}, Sate:{1}, FirstNameRU:{2}, LastNameRu:{3}, DepartmentID:{4}", isId, isState, isFirstNameRu, isLastNameRu, isDepartmentID));
                                 
                                 var employee = Sungero.Company.Employees.GetAll().Where(d => d.ExternalId == isId).FirstOrDefault();
                                 if (employee != null)
                                   Logger.DebugFormat("Employee with ExternalId:{0} was found. Id:{1}, Name:{2}", isId, employee.Id, employee.Name);
                                 else
                                 {
                                   if (isState == "1")
                                   {
                                     employee = Sungero.Company.Employees.Create();
                                     employee.ExternalId = isId;
                                     Logger.DebugFormat("Create new Employee with ExternalId:{0}. Id:{1}", isId, employee.Id);
                                   }
                                   else
                                   {
                                     Logger.DebugFormat("New Employee with ExternalId:{0} was not created, because it has Sate:{1}", isId, isState);
                                     countNotChanged++;
                                   }
                                 }
                                 
                                 if (employee != null)
                                 {
                                   if (isState == "0")
                                   {
                                     if (Equals(employee.Status, Sungero.Company.Employee.Status.Active))
                                     {
                                       Logger.DebugFormat("Change Status: current:{0}, new:{1}", "Active", "Closed");
                                       employee.Status = Sungero.Company.Employee.Status.Closed;
                                     }
                                   }
                                   else
                                   {
                                     if (Equals(employee.Status, Sungero.Company.Employee.Status.Closed))
                                     {
                                       Logger.DebugFormat("Change Status: current:{0}, new:{1}", "Closed", "Active");
                                       employee.Status = Sungero.Company.Employee.Status.Active;
                                     }
                                     
                                     // employee.Name обновляется автоматически при сохранении employee по данным person
                                     
                                     var department = Eskhata.Departments.GetAll().Where(x => x.ExternalId == isDepartmentID).FirstOrDefault();
                                     if (department == null)
                                       throw AppliedCodeException.Create(string.Format("Department with ID={0} was not found.", isDepartmentID));
                                     else
                                     {
                                       if(!Equals(employee.Department, department))
                                       {
                                         Logger.DebugFormat("Change Department: current:{0}, new:{1}", employee.Department?.Name, department.Name);
                                         employee.Department = department;
                                       }
                                     }
                                     
                                     if (employee.PersonnelNumber != isPersonnelNumber)
                                     {
                                       Logger.DebugFormat("Change PersonnelNumber: current:{0}, new:{1}", employee.PersonnelNumber, isPersonnelNumber);
                                       employee.PersonnelNumber = isPersonnelNumber;
                                     }

                                     if (employee.Name != "Сайфидинов Акмалчон Толибчонович" && employee.Phone != isPhone)
                                     {
                                       Logger.DebugFormat("Change Phone: current:{0}, new:{1}", employee.Phone, isPhone);
                                       employee.Phone = isPhone;
                                     }
                                   }
                                   
                                   var jobTittleResult = ProcessingJobTittle(isJobTittle);
                                   var jobTittle = jobTittleResult.jobTittle;
                                   if(!Equals(employee.JobTitle, jobTittle))
                                   {
                                     Logger.DebugFormat("Change JobTitle: current:{0}, new:{1}", employee.JobTitle?.Name, jobTittle?.Name);
                                     employee.JobTitle = jobTittle;
                                   }
                                   
                                   var fioInfo = Structures.Module.FIOInfo.Create(isLastNameRu, isFirstNameRu, isMiddleNameRU, isLastNameTG, isFirstNameTG, isMiddleNameTG);
                                   var personResult = ProcessingPerson(isPerson, fioInfo, null);
                                   var person = personResult.person;
                                   if(!Equals(employee.Person, person))
                                   {
                                     Logger.DebugFormat("Change Person: current:{0}, new:{1}", employee.Person?.Id, person?.Id);
                                     employee.Person = person;
                                   }
                                   
                                   
                                   if (!string.IsNullOrEmpty(isEmployeeEmail) && employee.Email != isEmployeeEmail)
                                   {
                                     Logger.DebugFormat("Change Email: current:{0}, new:{1}", employee.Email, isEmployeeEmail);
                                     employee.Email = isEmployeeEmail;
                                     
                                     bool needNotify = !string.IsNullOrEmpty(employee.Email) ? true : false;
                                     if (employee.NeedNotifyAssignmentsSummary != needNotify)
                                     {
                                       Logger.DebugFormat("Change NeedNotifyAssignmentsSummary: current:{0}, new:{1}", employee.NeedNotifyAssignmentsSummary, needNotify);
                                       employee.NeedNotifyAssignmentsSummary = needNotify;
                                     }
                                     
                                     if (employee.NeedNotifyExpiredAssignments != needNotify)
                                     {
                                       Logger.DebugFormat("Change NeedNotifyExpiredAssignments: current:{0}, new:{1}", employee.NeedNotifyExpiredAssignments, needNotify);
                                       employee.NeedNotifyExpiredAssignments = needNotify;
                                     }
                                     
                                     if (employee.NeedNotifyNewAssignments != needNotify)
                                     {
                                       Logger.DebugFormat("Change NeedNotifyNewAssignments: current:{0}, new:{1}", employee.NeedNotifyNewAssignments, needNotify);
                                       employee.NeedNotifyNewAssignments = needNotify;
                                     }
                                   }

                                   if (string.IsNullOrEmpty(employee.Email))
                                   {
                                     if (employee.NeedNotifyAssignmentsSummary != false)
                                       employee.NeedNotifyAssignmentsSummary = false;
                                     if (employee.NeedNotifyExpiredAssignments != false)
                                       employee.NeedNotifyExpiredAssignments = false;
                                     if (employee.NeedNotifyNewAssignments != false)
                                       employee.NeedNotifyNewAssignments = false;
                                   }
                                   
                                   if (employee.State.IsInserted || employee.State.IsChanged)
                                   {
                                     employee.Save();
                                     Logger.DebugFormat("Employee successfully saved. ExternalId:{0}, Id:{1}", isId, employee.Id);
                                     countChanged++;
                                   }
                                   else if (jobTittleResult.isCreatedOrUpdated || personResult.isCreatedOrUpdated)
                                   {
                                     countChanged++;
                                   }
                                   else
                                   {
                                     Logger.DebugFormat("There are no changes in Employee. ExternalId:{0}, Id:{1}", isId, employee.Id);
                                     countNotChanged++;
                                   }
                                 }
                               }
                               catch (Exception ex)
                               {
                                 var errorMessage = string.Format("Error when processing Employee with ExternalId:{0}. Description: {1}. StackTrace: {2}", isId, ex.Message, ex.StackTrace);
                                 Logger.Error(errorMessage);
                                 errorList.Add(errorMessage);
                                 countErrors++;
                               }
                             });
      }
      
      Logger.DebugFormat("R_DR_GET_EMPLOYEES - Total: CountAll:{0} CountChanged:{1} CountNotChanged:{2} CountErrors:{3}", countAll, countChanged, countNotChanged, countErrors);
      Logger.Debug("R_DR_GET_EMPLOYEES - Finish");
      return errorList;
    }
    
    /// <summary>
    /// Обработка стран.
    /// </summary>
    /// <param name="dataElements">Информация по странам в виде XElement.</param>
    /// <returns>Список ошибок (List<string>)</returns>
    public List<string> R_DR_GET_COUNTRIES(System.Collections.Generic.IEnumerable<System.Xml.Linq.XElement> dataElements)
    {
      Logger.Debug("R_DR_GET_COUNTRIES - Start");
      var errorList = new List<string>();
      int countAll = dataElements.Count();
      int countChanged = 0;
      int countNotChanged = 0;
      int countErrors = 0;
      
      foreach (var element in dataElements)
      {
        Transactions.Execute(() =>
                             {
                               var isId = element.Element("ID")?.Value;
                               var isValue = element.Element("VALUE")?.Value;
                               var isCode = element.Element("CODE")?.Value;
                               
                               var isName = element.Element("NAME")?.Value;
                               var isALFA_2 = element.Element("ALFA_2")?.Value;
                               var isALFA_3 = element.Element("ALFA_3")?.Value;
                               var isINAME = element.Element("INAME")?.Value;
                               var isOFFSHARE = element.Element("OFFSHARE")?.Value;
                               
                               try
                               {
                                 if (string.IsNullOrEmpty(isId) || string.IsNullOrEmpty(isValue) || string.IsNullOrEmpty(isCode))
                                   throw AppliedCodeException.Create(string.Format("Not all required fields are filled in. ID:{0}, VALUE:{1}, CODE:{2}", isId, isValue, isCode));
                                 
                                 var country = Eskhata.Countries.GetAll().Where(x => x.ExternalIdlitiko == isId).FirstOrDefault();
                                 if (country != null)
                                   Logger.DebugFormat("country with ExternalId:{0} was found. Id:{1}, Name:{2}", isId, country.Id, country.Name);
                                 else
                                 {
                                   country = Eskhata.Countries.Create();
                                   country.ExternalIdlitiko = isId;
                                   Logger.DebugFormat("Create new Сountry with ExternalId:{0}. Id:{1}", isId, country.Id);
                                 }

                                 if (country.Name != isValue)
                                 {
                                   Logger.DebugFormat("Change Name: current:{0}, new:{1}", country.Name, isValue);
                                   country.Name = isValue;
                                 }
                                 
                                 if (country.Code != isCode)
                                 {
                                   Logger.DebugFormat("Change Code: current:{0}, new:{1}", country.Code, isCode);
                                   country.Code = isCode;
                                 }
                                 
                                 if (country.FullNamelitiko != isName)
                                 {
                                   Logger.DebugFormat("Change FullNamelitiko: current:{0}, new:{1}", country.FullNamelitiko, isName);
                                   country.FullNamelitiko = isName;
                                 }
                                 
                                 if (country.Alfa2litiko != isALFA_2)
                                 {
                                   Logger.DebugFormat("Change Alfa2litiko: current:{0}, new:{1}", country.Alfa2litiko, isALFA_2);
                                   country.Alfa2litiko = isALFA_2;
                                 }

                                 if (country.Alfa3litiko != isALFA_3)
                                 {
                                   Logger.DebugFormat("Change Alfa3litiko: current:{0}, new:{1}", country.Alfa3litiko, isALFA_3);
                                   country.Alfa3litiko = isALFA_3;
                                 }

                                 if (country.INamelitiko != isINAME)
                                 {
                                   Logger.DebugFormat("Change INamelitiko: current:{0}, new:{1}", country.INamelitiko, isINAME);
                                   country.INamelitiko = isINAME;
                                 }

                                 bool isOffShareBool = isOFFSHARE == "1" ? true : false;
                                 if(country.IsOffsharelitiko != isOffShareBool)
                                 {
                                   Logger.DebugFormat("Change IsOffsharelitiko: current:{0}, new:{1}", country.IsOffsharelitiko, isOffShareBool);
                                   country.IsOffsharelitiko = isOffShareBool;
                                 }
                                 
                                 if (country.State.IsInserted || country.State.IsChanged)
                                 {
                                   country.Save();
                                   Logger.DebugFormat("Сountry successfully saved. ExternalId:{0}, Id:{1}", isId, country.Id);
                                   countChanged++;
                                 }
                                 else
                                 {
                                   Logger.DebugFormat("There are no changes in country. ExternalId:{0}, Id:{1}", isId, country.Id);
                                   countNotChanged++;
                                 }
                               }
                               catch (Exception ex)
                               {
                                 var errorMessage = string.Format("Error when processing country with ExternalId:{0}. Description: {1}. StackTrace: {2}", isId, ex.Message, ex.StackTrace);
                                 Logger.Error(errorMessage);
                                 errorList.Add(errorMessage);
                                 countErrors++;
                               }
                             });
      }
      Logger.DebugFormat("R_DR_GET_COUNTRIES - Total: CountAll:{0} CountChanged:{1} CountNotChanged:{2} CountErrors:{3}", countAll, countChanged, countNotChanged, countErrors);
      
      Logger.Debug("R_DR_GET_COUNTRIES - Finish");
      return errorList;
    }
    
    /// <summary>
    /// Обработка справочника ОКФС.
    /// </summary>
    /// <param name="dataElements">Информация по ОКФС в виде XElement.</param>
    /// <returns>Список ошибок (List<string>)</returns>
    public List<string> R_DR_GET_OKFS(System.Collections.Generic.IEnumerable<System.Xml.Linq.XElement> dataElements)
    {
      Logger.Debug("R_DR_GET_OKFS - Start");
      var errorList = new List<string>();
      int countAll = dataElements.Count();
      int countChanged = 0;
      int countNotChanged = 0;
      int countErrors = 0;
      
      foreach (var element in dataElements)
      {
        Transactions.Execute(() =>
                             {
                               var isId = element.Element("ID")?.Value;
                               var isName = element.Element("NAME")?.Value;
                               
                               var isCode = element.Element("CODE")?.Value;
                               var isCountry = element.Element("COUNTRY")?.Value;
                               var isParentEntry = element.Element("UPPER")?.Value;
                               
                               try
                               {
                                 if (string.IsNullOrEmpty(isId) || string.IsNullOrEmpty(isName))
                                   throw AppliedCodeException.Create(string.Format("Not all required fields are filled in. ID:{0}, NAME:{1}", isId, isName));
                                 
                                 var entity = NSI.OKFSes.GetAll().Where(x => x.ExternalId == isId).FirstOrDefault();
                                 if (entity != null)
                                   Logger.DebugFormat("OKFS with ExternalId:{0} was found. Id:{1}, Name:{2}", isId, entity.Id, entity.Name);
                                 else
                                 {
                                   entity = NSI.OKFSes.Create();
                                   entity.ExternalId = isId;
                                   Logger.DebugFormat("Create new OKFS with ExternalId:{0}. Id:{1}", isId, entity.Id);
                                 }
                                 
                                 if (entity.Name != isName)
                                 {
                                   Logger.DebugFormat("Change Name: current:{0}, new:{1}", entity.Name, isName);
                                   entity.Name = isName;
                                 }
                                 
                                 if (entity.Code != isCode)
                                 {
                                   Logger.DebugFormat("Change Code: current:{0}, new:{1}", entity.Code, isCode);
                                   entity.Code = isCode;
                                 }
                                 
                                 if(!string.IsNullOrEmpty(isCountry))
                                 {
                                   var country = litiko.Eskhata.Countries.GetAll().Where(x => x.ExternalIdlitiko == isCountry).FirstOrDefault();
                                   if(country != null && !Equals(entity.Country, country))
                                   {
                                     Logger.DebugFormat("Change Country: current:{0}, new:{1}", entity.Country?.Name, country.Name);
                                     entity.Country = country;
                                   }
                                 }
                                 
                                 if(!string.IsNullOrEmpty(isParentEntry))
                                 {
                                   var parentEntry = NSI.OKFSes.GetAll().Where(x => x.ExternalId == isParentEntry).FirstOrDefault();
                                   if(parentEntry != null && !Equals(entity.ParentEntry, parentEntry))
                                   {
                                     Logger.DebugFormat("Change ParentEntry: current:{0}, new:{1}", entity.ParentEntry?.Name, parentEntry.Name);
                                     entity.ParentEntry = parentEntry;
                                   }
                                 }
                                 
                                 if (entity.State.IsInserted || entity.State.IsChanged)
                                 {
                                   entity.Save();
                                   Logger.DebugFormat("OKFS successfully saved. ExternalId:{0}, Id:{1}", isId, entity.Id);
                                   countChanged++;
                                 }
                                 else
                                 {
                                   Logger.DebugFormat("There are no changes in OKFS. ExternalId:{0}, Id:{1}", isId, entity.Id);
                                   countNotChanged++;
                                 }
                               }
                               catch (Exception ex)
                               {
                                 var errorMessage = string.Format("Error when processing OKFS with ExternalId:{0}. Description: {1}. StackTrace: {2}", isId, ex.Message, ex.StackTrace);
                                 Logger.Error(errorMessage);
                                 errorList.Add(errorMessage);
                                 countErrors++;
                               }
                             });
      }
      Logger.DebugFormat("R_DR_GET_OKFS - Total: CountAll:{0} CountChanged:{1} CountNotChanged:{2} CountErrors:{3}", countAll, countChanged, countNotChanged, countErrors);
      
      Logger.Debug("R_DR_GET_OKFS - Finish");
      return errorList;
    }

    /// <summary>
    /// Обработка справочника ОКОНХ.
    /// </summary>
    /// <param name="dataElements">Информация по ОКОНХ в виде XElement.</param>
    /// <returns>Список ошибок (List<string>)</returns>
    public List<string> R_DR_GET_OKONH(System.Collections.Generic.IEnumerable<System.Xml.Linq.XElement> dataElements)
    {
      Logger.Debug("R_DR_GET_OKONH - Start");
      var errorList = new List<string>();
      int countAll = dataElements.Count();
      int countChanged = 0;
      int countNotChanged = 0;
      int countErrors = 0;
      
      foreach (var element in dataElements)
      {
        Transactions.Execute(() =>
                             {
                               var isId = element.Element("ID")?.Value;
                               var isName = element.Element("NAME")?.Value;
                               
                               var isCode = element.Element("CODE")?.Value;
                               var isCountry = element.Element("COUNTRY")?.Value;
                               var isEnvironmentalRisk = element.Element("ESH_EKRISC")?.Value;
                               
                               try
                               {
                                 if (string.IsNullOrEmpty(isId) || string.IsNullOrEmpty(isName))
                                   throw AppliedCodeException.Create(string.Format("Not all required fields are filled in. ID:{0}, NAME:{1}", isId, isName));
                                 
                                 var entity = NSI.OKONHs.GetAll().Where(x => x.ExternalId == isId).FirstOrDefault();
                                 if (entity != null)
                                   Logger.DebugFormat("OKONH with ExternalId:{0} was found. Id:{1}, Name:{2}", isId, entity.Id, entity.Name);
                                 else
                                 {
                                   entity = NSI.OKONHs.Create();
                                   entity.ExternalId = isId;
                                   Logger.DebugFormat("Create new OKONH with ExternalId:{0}. Id:{1}", isId, entity.Id);
                                 }
                                 
                                 if (entity.Name != isName)
                                 {
                                   Logger.DebugFormat("Change Name: current:{0}, new:{1}", entity.Name, isName);
                                   entity.Name = isName;
                                 }
                                 
                                 if (entity.Code != isCode)
                                 {
                                   Logger.DebugFormat("Change Code: current:{0}, new:{1}", entity.Code, isCode);
                                   entity.Code = isCode;
                                 }
                                 
                                 if(!string.IsNullOrEmpty(isCountry))
                                 {
                                   var country = litiko.Eskhata.Countries.GetAll().Where(x => x.ExternalIdlitiko == isCountry).FirstOrDefault();
                                   if(country != null && !Equals(entity.Country, country))
                                   {
                                     Logger.DebugFormat("Change Country: current:{0}, new:{1}", entity.Country?.Name, country.Name);
                                     entity.Country = country;
                                   }
                                 }
                                 
                                 if(!string.IsNullOrEmpty(isEnvironmentalRisk))
                                 {
                                   var environmentalRisk = NSI.EnvironmentalRisks.GetAll().Where(x => x.ExternalId == isEnvironmentalRisk).FirstOrDefault();
                                   if(environmentalRisk != null && !Equals(entity.EnvironmentalRisk, environmentalRisk))
                                   {
                                     Logger.DebugFormat("Change EnvironmentalRisk: current:{0}, new:{1}", entity.EnvironmentalRisk?.Name, environmentalRisk.Name);
                                     entity.EnvironmentalRisk = environmentalRisk;
                                   }
                                 }
                                 
                                 if (entity.State.IsInserted || entity.State.IsChanged)
                                 {
                                   entity.Save();
                                   Logger.DebugFormat("OKONH successfully saved. ExternalId:{0}, Id:{1}", isId, entity.Id);
                                   countChanged++;
                                 }
                                 else
                                 {
                                   Logger.DebugFormat("There are no changes in OKONH. ExternalId:{0}, Id:{1}", isId, entity.Id);
                                   countNotChanged++;
                                 }
                               }
                               catch (Exception ex)
                               {
                                 var errorMessage = string.Format("Error when processing OKONH with ExternalId:{0}. Description: {1}. StackTrace: {2}", isId, ex.Message, ex.StackTrace);
                                 Logger.Error(errorMessage);
                                 errorList.Add(errorMessage);
                                 countErrors++;
                               }
                             });
      }
      Logger.DebugFormat("R_DR_GET_OKONH - Total: CountAll:{0} CountChanged:{1} CountNotChanged:{2} CountErrors:{3}", countAll, countChanged, countNotChanged, countErrors);
      
      Logger.Debug("R_DR_GET_OKONH - Finish");
      return errorList;
    }

    /// <summary>
    /// Обработка справочника ОКВЭД.
    /// </summary>
    /// <param name="dataElements">Информация по ОКВЭД в виде XElement.</param>
    /// <returns>Список ошибок (List<string>)</returns>
    public List<string> R_DR_GET_OKVED(System.Collections.Generic.IEnumerable<System.Xml.Linq.XElement> dataElements)
    {
      Logger.Debug("R_DR_GET_OKVED - Start");
      var errorList = new List<string>();
      int countAll = dataElements.Count();
      int countChanged = 0;
      int countNotChanged = 0;
      int countErrors = 0;
      
      foreach (var element in dataElements)
      {
        Transactions.Execute(() =>
                             {
                               var isId = element.Element("ID")?.Value;
                               var isName = element.Element("NAME")?.Value;
                               
                               var isCode = element.Element("CODE")?.Value;
                               var isHighRisk = element.Element("HIGH_RISK")?.Value;
                               var isLicensing = element.Element("NEEDS_LICENSE")?.Value;
                               
                               try
                               {
                                 if (string.IsNullOrEmpty(isId) || string.IsNullOrEmpty(isName))
                                   throw AppliedCodeException.Create(string.Format("Not all required fields are filled in. ID:{0}, NAME:{1}", isId, isName));
                                 
                                 var entity = NSI.OKVEDs.GetAll().Where(x => x.ExternalId == isId).FirstOrDefault();
                                 if (entity != null)
                                   Logger.DebugFormat("OKVED with ExternalId:{0} was found. Id:{1}, Name:{2}", isId, entity.Id, entity.Name);
                                 else
                                 {
                                   entity = NSI.OKVEDs.Create();
                                   entity.ExternalId = isId;
                                   Logger.DebugFormat("Create new OKVED with ExternalId:{0}. Id:{1}", isId, entity.Id);
                                 }
                                 
                                 if (entity.Name != isName)
                                 {
                                   Logger.DebugFormat("Change Name: current:{0}, new:{1}", entity.Name, isName);
                                   entity.Name = isName;
                                 }
                                 
                                 if (entity.Code != isCode)
                                 {
                                   Logger.DebugFormat("Change Code: current:{0}, new:{1}", entity.Code, isCode);
                                   entity.Code = isCode;
                                 }
                                 
                                 if(!string.IsNullOrEmpty(isHighRisk))
                                 {
                                   bool isHighRiskBool = isHighRisk == "1" ? true : false;
                                   if(entity.IsHighRisk != isHighRiskBool)
                                   {
                                     Logger.DebugFormat("Change IsHighRisk: current:{0}, new:{1}", entity.IsHighRisk, isHighRiskBool);
                                     entity.IsHighRisk = isHighRiskBool;
                                   }
                                 }
                                 
                                 if(!string.IsNullOrEmpty(isLicensing))
                                 {
                                   bool isLicensingBool = isLicensing == "1" ? true : false;
                                   if(entity.IsLicensing != isLicensingBool)
                                   {
                                     Logger.DebugFormat("Change IsLicensing: current:{0}, new:{1}", entity.IsLicensing, isLicensingBool);
                                     entity.IsLicensing = isLicensingBool;
                                   }
                                 }
                                 
                                 if (entity.State.IsInserted || entity.State.IsChanged)
                                 {
                                   entity.Save();
                                   Logger.DebugFormat("OKONH successfully saved. ExternalId:{0}, Id:{1}", isId, entity.Id);
                                   countChanged++;
                                 }
                                 else
                                 {
                                   Logger.DebugFormat("There are no changes in OKONH. ExternalId:{0}, Id:{1}", isId, entity.Id);
                                   countNotChanged++;
                                 }
                               }
                               catch (Exception ex)
                               {
                                 var errorMessage = string.Format("Error when processing OKONH with ExternalId:{0}. Description: {1}. StackTrace: {2}", isId, ex.Message, ex.StackTrace);
                                 Logger.Error(errorMessage);
                                 errorList.Add(errorMessage);
                                 countErrors++;
                               }
                             });
      }
      Logger.DebugFormat("R_DR_GET_OKVED - Total: CountAll:{0} CountChanged:{1} CountNotChanged:{2} CountErrors:{3}", countAll, countChanged, countNotChanged, countErrors);
      
      Logger.Debug("R_DR_GET_OKVED - Finish");
      return errorList;
    }

    /// <summary>
    /// Обработка справочника ОКОПФ.
    /// </summary>
    /// <param name="dataElements">Информация по ОКОПФ в виде XElement.</param>
    /// <returns>Список ошибок (List<string>)</returns>
    public List<string> R_DR_GET_OKOPF(System.Collections.Generic.IEnumerable<System.Xml.Linq.XElement> dataElements)
    {
      Logger.Debug("R_DR_GET_OKOPF - Start");
      var errorList = new List<string>();
      int countAll = dataElements.Count();
      int countChanged = 0;
      int countNotChanged = 0;
      int countErrors = 0;
      
      foreach (var element in dataElements)
      {
        Transactions.Execute(() =>
                             {
                               var isId = element.Element("ID")?.Value;
                               var isName = element.Element("NAME")?.Value;
                               
                               var isCode = element.Element("CODE")?.Value;
                               var isCountry = element.Element("COUNTRY")?.Value;
                               var isParentEntry = element.Element("UPPER")?.Value;
                               var isShortName = element.Element("SHORT_NAME")?.Value;
                               
                               try
                               {
                                 if (string.IsNullOrEmpty(isId) || string.IsNullOrEmpty(isName))
                                   throw AppliedCodeException.Create(string.Format("Not all required fields are filled in. ID:{0}, NAME:{1}", isId, isName));
                                 
                                 var entity = NSI.OKOPFs.GetAll().Where(x => x.ExternalId == isId).FirstOrDefault();
                                 if (entity != null)
                                   Logger.DebugFormat("OKOPF with ExternalId:{0} was found. Id:{1}, Name:{2}", isId, entity.Id, entity.Name);
                                 else
                                 {
                                   entity = NSI.OKOPFs.Create();
                                   entity.ExternalId = isId;
                                   Logger.DebugFormat("Create new OKOPF with ExternalId:{0}. Id:{1}", isId, entity.Id);
                                 }
                                 
                                 if (entity.Name != isName)
                                 {
                                   Logger.DebugFormat("Change Name: current:{0}, new:{1}", entity.Name, isName);
                                   entity.Name = isName;
                                 }
                                 
                                 if (entity.Code != isCode)
                                 {
                                   Logger.DebugFormat("Change Code: current:{0}, new:{1}", entity.Code, isCode);
                                   entity.Code = isCode;
                                 }
                                 
                                 if(!string.IsNullOrEmpty(isCountry))
                                 {
                                   var country = litiko.Eskhata.Countries.GetAll().Where(x => x.ExternalIdlitiko == isCountry).FirstOrDefault();
                                   if(country != null && !Equals(entity.Country, country))
                                   {
                                     Logger.DebugFormat("Change Country: current:{0}, new:{1}", entity.Country?.Name, country.Name);
                                     entity.Country = country;
                                   }
                                 }
                                 
                                 if(!string.IsNullOrEmpty(isParentEntry))
                                 {
                                   var parentEntry = NSI.OKOPFs.GetAll().Where(x => x.ExternalId == isParentEntry).FirstOrDefault();
                                   if(parentEntry != null && !Equals(entity.ParentEntry, parentEntry))
                                   {
                                     Logger.DebugFormat("Change ParentEntry: current:{0}, new:{1}", entity.ParentEntry?.Name, parentEntry.Name);
                                     entity.ParentEntry = parentEntry;
                                   }
                                 }

                                 if (entity.ShortName != isShortName)
                                 {
                                   Logger.DebugFormat("Change ShortName: current:{0}, new:{1}", entity.ShortName, isShortName);
                                   entity.ShortName = isShortName;
                                 }
                                 
                                 if (entity.State.IsInserted || entity.State.IsChanged)
                                 {
                                   entity.Save();
                                   Logger.DebugFormat("OKOPF successfully saved. ExternalId:{0}, Id:{1}", isId, entity.Id);
                                   countChanged++;
                                 }
                                 else
                                 {
                                   Logger.DebugFormat("There are no changes in OKOPF. ExternalId:{0}, Id:{1}", isId, entity.Id);
                                   countNotChanged++;
                                 }
                               }
                               catch (Exception ex)
                               {
                                 var errorMessage = string.Format("Error when processing OKOPF with ExternalId:{0}. Description: {1}. StackTrace: {2}", isId, ex.Message, ex.StackTrace);
                                 Logger.Error(errorMessage);
                                 errorList.Add(errorMessage);
                                 countErrors++;
                               }
                             });
      }
      Logger.DebugFormat("R_DR_GET_OKOPF - Total: CountAll:{0} CountChanged:{1} CountNotChanged:{2} CountErrors:{3}", countAll, countChanged, countNotChanged, countErrors);
      
      Logger.Debug("R_DR_GET_OKOPF - Finish");
      return errorList;
    }
    
    /// <summary>
    /// Обработка справочника Виды предприятий.
    /// </summary>
    /// <param name="dataElements">Информация по видам предприятий в виде XElement.</param>
    /// <returns>Список ошибок (List<string>)</returns>
    public List<string> R_DR_GET_COMPANYKINDS(System.Collections.Generic.IEnumerable<System.Xml.Linq.XElement> dataElements)
    {
      Logger.Debug("R_DR_GET_COMPANYKINDS - Start");
      var errorList = new List<string>();
      int countAll = dataElements.Count();
      int countChanged = 0;
      int countNotChanged = 0;
      int countErrors = 0;
      
      foreach (var element in dataElements)
      {
        Transactions.Execute(() =>
                             {
                               var isId = element.Element("ID")?.Value;
                               var isName = element.Element("PS_NAME")?.Value;
                               
                               var isCode = element.Element("PS_CODE")?.Value;
                               
                               try
                               {
                                 if (string.IsNullOrEmpty(isId) || string.IsNullOrEmpty(isName))
                                   throw AppliedCodeException.Create(string.Format("Not all required fields are filled in. ID:{0}, NAME:{1}", isId, isName));
                                 
                                 var entity = NSI.EnterpriseTypes.GetAll().Where(x => x.ExternalId == isId).FirstOrDefault();
                                 if (entity != null)
                                   Logger.DebugFormat("EnterpriseType with ExternalId:{0} was found. Id:{1}, Name:{2}", isId, entity.Id, entity.Name);
                                 else
                                 {
                                   entity = NSI.EnterpriseTypes.Create();
                                   entity.ExternalId = isId;
                                   Logger.DebugFormat("Create new EnterpriseType with ExternalId:{0}. Id:{1}", isId, entity.Id);
                                 }
                                 
                                 if (entity.Name != isName)
                                 {
                                   Logger.DebugFormat("Change Name: current:{0}, new:{1}", entity.Name, isName);
                                   entity.Name = isName;
                                 }
                                 
                                 if (entity.Code != isCode)
                                 {
                                   Logger.DebugFormat("Change Code: current:{0}, new:{1}", entity.Code, isCode);
                                   entity.Code = isCode;
                                 }
                                 
                                 if (entity.State.IsInserted || entity.State.IsChanged)
                                 {
                                   entity.Save();
                                   Logger.DebugFormat("EnterpriseType successfully saved. ExternalId:{0}, Id:{1}", isId, entity.Id);
                                   countChanged++;
                                 }
                                 else
                                 {
                                   Logger.DebugFormat("There are no changes in EnterpriseType. ExternalId:{0}, Id:{1}", isId, entity.Id);
                                   countNotChanged++;
                                 }
                               }
                               catch (Exception ex)
                               {
                                 var errorMessage = string.Format("Error when processing EnterpriseType with ExternalId:{0}. Description: {1}. StackTrace: {2}", isId, ex.Message, ex.StackTrace);
                                 Logger.Error(errorMessage);
                                 errorList.Add(errorMessage);
                                 countErrors++;
                               }
                             });
      }
      Logger.DebugFormat("R_DR_GET_COMPANYKINDS - Total: CountAll:{0} CountChanged:{1} CountNotChanged:{2} CountErrors:{3}", countAll, countChanged, countNotChanged, countErrors);
      
      Logger.Debug("R_DR_GET_COMPANYKINDS - Finish");
      return errorList;
    }
    
    /// <summary>
    /// Обработка справочника Типы удостоверений личности.
    /// </summary>
    /// <param name="dataElements">Информация по типам удостоверений личности в виде XElement.</param>
    /// <returns>Список ошибок (List<string>)</returns>
    public List<string> R_DR_GET_TYPESOFIDCARDS(System.Collections.Generic.IEnumerable<System.Xml.Linq.XElement> dataElements)
    {
      Logger.Debug("R_DR_GET_TYPESOFIDCARDS - Start");
      var errorList = new List<string>();
      int countAll = dataElements.Count();
      int countChanged = 0;
      int countNotChanged = 0;
      int countErrors = 0;
      /**/
      foreach (var element in dataElements)
      {        
        Transactions.Execute(() =>
        {
          var isId = element.Element("ID")?.Value;
          var isName = element.Element("NAME")?.Value;
          
          var codeValue = element.Element("CODE")?.Value;
          var isCode = string.IsNullOrWhiteSpace(codeValue) ? "???" : (codeValue.Length >= 3 ? codeValue.Substring(0, 3) : codeValue);
          
          try
          {                        
            if (string.IsNullOrEmpty(isId) || string.IsNullOrEmpty(isName))
              throw AppliedCodeException.Create(string.Format("Not all required fields are filled in. ID:{0}, NAME:{1}", isId, isName));  
            
            var entity = Sungero.Parties.IdentityDocumentKinds.GetAll().Where(x => x.SID == isId).FirstOrDefault();
            if (entity != null)
              Logger.DebugFormat("IdentityDocumentKind with SID:{0} was found. Id:{1}, Name:{2}", isId, entity.Id, entity.Name);
            else
            {              
              entity = Sungero.Parties.IdentityDocumentKinds.Create();
              entity.SID = isId;
              entity.SpecifyIdentitySeries = true;
              entity.SpecifyIdentityAuthorityCode = false;
              entity.SpecifyIdentityExpirationDate = false;
              entity.SpecifyBirthPlace = false;
              entity.Note = codeValue.Length >= 3 ? codeValue : string.Empty;
              Logger.DebugFormat("Create new IdentityDocumentKind with SID:{0}. Id:{1}", isId, entity.Id);
            }             
            
            if (entity.Name != isName)
            {
              Logger.DebugFormat("Change Name: current:{0}, new:{1}", entity.Name, isName);
              entity.Name = isName;              
            }
            
            if (entity.Code != isCode)
            {
              Logger.DebugFormat("Change Code: current:{0}, new:{1}", entity.Code, isCode);
              entity.Code = isCode;              
            }                                                            
            
            if (entity.State.IsInserted || entity.State.IsChanged)
            {
              entity.Save();                                          
              Logger.DebugFormat("IdentityDocumentKind successfully saved. SID:{0}, Id:{1}", isId, entity.Id);
              countChanged++;
            }
            else
            {
              Logger.DebugFormat("There are no changes in IdentityDocumentKind. SID:{0}, Id:{1}", isId, entity.Id);
              countNotChanged++;
            }
          }
          catch (Exception ex)
          {
            var errorMessage = string.Format("Error when processing IdentityDocumentKind with ExternalId:{0}. Description: {1}. StackTrace: {2}", isId, ex.Message, ex.StackTrace);
            Logger.Error(errorMessage);
            errorList.Add(errorMessage);
            countErrors++;
          }
        });
      }
      /**/
      Logger.DebugFormat("R_DR_GET_TYPESOFIDCARDS - Total: CountAll:{0} CountChanged:{1} CountNotChanged:{2} CountErrors:{3}", countAll, countChanged, countNotChanged, countErrors);
      
      Logger.Debug("R_DR_GET_TYPESOFIDCARDS - Finish");
      return errorList;
    }

    /// <summary>
    /// Обработка справочника Экологические риски.
    /// </summary>
    /// <param name="dataElements">Информация по экологическим рискам в виде XElement.</param>
    /// <returns>Список ошибок (List<string>)</returns>
    public List<string> R_DR_GET_ECOLOG(System.Collections.Generic.IEnumerable<System.Xml.Linq.XElement> dataElements)
    {
      Logger.Debug("R_DR_GET_ECOLOG - Start");
      var errorList = new List<string>();
      int countAll = dataElements.Count();
      int countChanged = 0;
      int countNotChanged = 0;
      int countErrors = 0;
      
      foreach (var element in dataElements)
      {
        Transactions.Execute(() =>
                             {
                               var isId = element.Element("ID")?.Value;
                               var isName = element.Element("NAME")?.Value;
                               
                               var isCode = element.Element("CODE")?.Value;
                               
                               try
                               {
                                 if (string.IsNullOrEmpty(isId) || string.IsNullOrEmpty(isName))
                                   throw AppliedCodeException.Create(string.Format("Not all required fields are filled in. ID:{0}, NAME:{1}", isId, isName));
                                 
                                 var entity = NSI.EnvironmentalRisks.GetAll().Where(x => x.ExternalId == isId).FirstOrDefault();
                                 if (entity != null)
                                   Logger.DebugFormat("EnvironmentalRisk with ExternalId:{0} was found. Id:{1}, Name:{2}", isId, entity.Id, entity.Name);
                                 else
                                 {
                                   entity = NSI.EnvironmentalRisks.Create();
                                   entity.ExternalId = isId;
                                   Logger.DebugFormat("Create new EnvironmentalRisk with ExternalId:{0}. Id:{1}", isId, entity.Id);
                                 }
                                 
                                 if (entity.Name != isName)
                                 {
                                   Logger.DebugFormat("Change Name: current:{0}, new:{1}", entity.Name, isName);
                                   entity.Name = isName;
                                 }
                                 
                                 if (entity.Code != isCode)
                                 {
                                   Logger.DebugFormat("Change Code: current:{0}, new:{1}", entity.Code, isCode);
                                   entity.Code = isCode;
                                 }
                                 
                                 if (entity.State.IsInserted || entity.State.IsChanged)
                                 {
                                   entity.Save();
                                   Logger.DebugFormat("EnvironmentalRisk successfully saved. ExternalId:{0}, Id:{1}", isId, entity.Id);
                                   countChanged++;
                                 }
                                 else
                                 {
                                   Logger.DebugFormat("There are no changes in EnvironmentalRisk. ExternalId:{0}, Id:{1}", isId, entity.Id);
                                   countNotChanged++;
                                 }
                               }
                               catch (Exception ex)
                               {
                                 var errorMessage = string.Format("Error when processing EnvironmentalRisk with ExternalId:{0}. Description: {1}. StackTrace: {2}", isId, ex.Message, ex.StackTrace);
                                 Logger.Error(errorMessage);
                                 errorList.Add(errorMessage);
                                 countErrors++;
                               }
                             });
      }
      Logger.DebugFormat("R_DR_GET_ECOLOG - Total: CountAll:{0} CountChanged:{1} CountNotChanged:{2} CountErrors:{3}", countAll, countChanged, countNotChanged, countErrors);
      
      Logger.Debug("R_DR_GET_ECOLOG - Finish");
      return errorList;
    }

    /// <summary>
    /// Обработка справочника Семейное положение.
    /// </summary>
    /// <param name="dataElements">Информация по симейным положениям в виде XElement.</param>
    /// <returns>Список ошибок (List<string>)</returns>
    public List<string> R_DR_GET_MARITALSTATUSES(System.Collections.Generic.IEnumerable<System.Xml.Linq.XElement> dataElements)
    {
      Logger.Debug("R_DR_GET_MARITALSTATUSES - Start");
      var errorList = new List<string>();
      int countAll = dataElements.Count();
      int countChanged = 0;
      int countNotChanged = 0;
      int countErrors = 0;
      
      foreach (var element in dataElements)
      {
        Transactions.Execute(() =>
                             {
                               var isId = element.Element("ID")?.Value;
                               var isName = element.Element("NAME")?.Value;
                               
                               var isCode = element.Element("CODE")?.Value;
                               
                               try
                               {
                                 if (string.IsNullOrEmpty(isId) || string.IsNullOrEmpty(isName))
                                   throw AppliedCodeException.Create(string.Format("Not all required fields are filled in. ID:{0}, NAME:{1}", isId, isName));
                                 
                                 var entity = NSI.FamilyStatuses.GetAll().Where(x => x.ExternalId == isId).FirstOrDefault();
                                 if (entity != null)
                                   Logger.DebugFormat("FamilyStatus with ExternalId:{0} was found. Id:{1}, Name:{2}", isId, entity.Id, entity.Name);
                                 else
                                 {
                                   entity = NSI.FamilyStatuses.Create();
                                   entity.ExternalId = isId;
                                   Logger.DebugFormat("Create new FamilyStatus with ExternalId:{0}. Id:{1}", isId, entity.Id);
                                 }
                                 
                                 if (entity.Name != isName)
                                 {
                                   Logger.DebugFormat("Change Name: current:{0}, new:{1}", entity.Name, isName);
                                   entity.Name = isName;
                                 }
                                 
                                 if (entity.Code != isCode)
                                 {
                                   Logger.DebugFormat("Change Code: current:{0}, new:{1}", entity.Code, isCode);
                                   entity.Code = isCode;
                                 }
                                 
                                 if (entity.State.IsInserted || entity.State.IsChanged)
                                 {
                                   entity.Save();
                                   Logger.DebugFormat("FamilyStatus successfully saved. ExternalId:{0}, Id:{1}", isId, entity.Id);
                                   countChanged++;
                                 }
                                 else
                                 {
                                   Logger.DebugFormat("There are no changes in FamilyStatus. ExternalId:{0}, Id:{1}", isId, entity.Id);
                                   countNotChanged++;
                                 }
                               }
                               catch (Exception ex)
                               {
                                 var errorMessage = string.Format("Error when processing FamilyStatus with ExternalId:{0}. Description: {1}. StackTrace: {2}", isId, ex.Message, ex.StackTrace);
                                 Logger.Error(errorMessage);
                                 errorList.Add(errorMessage);
                                 countErrors++;
                               }
                             });
      }
      Logger.DebugFormat("R_DR_GET_MARITALSTATUSES - Total: CountAll:{0} CountChanged:{1} CountNotChanged:{2} CountErrors:{3}", countAll, countChanged, countNotChanged, countErrors);
      
      Logger.Debug("R_DR_GET_MARITALSTATUSES - Finish");
      return errorList;
    }
    
    /// <summary>
    /// Обработка Организации.
    /// </summary>
    /// <param name="exchDocID">ИД документа обмена</param>
    /// <param name="counterparty">Организация</param>
    /// <returns>Список ошибок (List< string>)</returns>
    [Remote]
    public List<string> R_DR_GET_COMPANY(long exchDocID, Eskhata.ICounterparty counterparty)
    {
      Logger.Debug("R_DR_GET_COMPANY - Start");
      var errorList = new List<string>();
      
      var exchDoc = ExchangeDocuments.Get(exchDocID);
      var versionFullXML = exchDoc.Versions.Where(v => v.Note == Integration.Resources.VersionRequestToRXFull && v.AssociatedApplication.Extension == "xml" && v.Body.Size > 0).FirstOrDefault();
      if (versionFullXML == null)
      {
        errorList.Add("Version with full XML data not found.");
        return errorList;
      }
      
      XDocument xmlDoc = XDocument.Load(versionFullXML.Body.Read());
      var dataElements = xmlDoc.Descendants("Data").Elements("element");
      if (!dataElements.Any())
      {
        errorList.Add("Empty Data node.");
        return errorList;
      }
      
      var element = dataElements.FirstOrDefault();
      
      var isId = element.Element("ID")?.Value;
      var isName = element.Element("NAME")?.Value.Trim();
      var isINN = element.Element("INN")?.Value;
      
      var isLongName = element.Element("LONG_NAME")?.Value.Trim();
      var isIName = element.Element("I_NAME")?.Value.Trim();
      var isRezident = element.Element("REZIDENT")?.Value;
      var isNuRezident = element.Element("NU_REZIDENT")?.Value;
      var isKPP = element.Element("KPP")?.Value;
      var isOKPO = element.Element("KOD_OKPO")?.Value;
      var isOKOPF = element.Element("FORMA")?.Value;
      var isOKFS = element.Element("OWNERSHIP")?.Value;
      var isCodeOKONHelements = element.Element("CODE_OKONH").Elements("element");
      var isCodeOKVEDelements = element.Element("CODE_OKVED").Elements("element");
      var isRegistnum = element.Element("REGIST_NUM")?.Value;
      var isNumbers = element.Element("NUMBERS")?.Value;
      var isBusiness = element.Element("BUSINESS")?.Value;
      var isPS_REF = element.Element("PS_REF")?.Value;
      var isCountry = element.Element("COUNTRY")?.Value;
      //var isRegion = element.Element("Region")?.Value;
      var isCity = element.Element("City")?.Value;       
      var isPostAdress = element.Element("PostAdress")?.Value;
      var isLegalAdress = element.Element("LegalAdress")?.Value;
      var isPhone = element.Element("Phone")?.Value;
      var isEmail = element.Element("Email")?.Value;
      var isWebSite = element.Element("WebSite")?.Value;
      
      var isContacts = element.Element("Contacts").Elements("element");
      
      try
      {
        var company = Eskhata.Companies.As(counterparty);
        
        if (string.IsNullOrEmpty(isId) || string.IsNullOrEmpty(isName))
          throw AppliedCodeException.Create(string.Format("Not all required fields are filled in. ID:{0}, NAME:{1}, INN:{2}", isId, isName, isINN));
        
        if (company.ExternalId != isId)
        {
          Logger.DebugFormat("Change ExternalId: current:{0}, new:{1}", company.ExternalId, isId);
          company.ExternalId = isId;                        
        }

        if (company.Name != isName)
        {
          Logger.DebugFormat("Change Name: current:{0}, new:{1}", company.Name, isName);
          company.Name = isName;                              
        }

        if(!string.IsNullOrEmpty(isLongName) && company.LegalName != isLongName)
        {
          Logger.DebugFormat("Change LegalName: current:{0}, new:{1}", company.LegalName, isLongName);
          company.LegalName = isLongName;                  
        }
        
        if(!string.IsNullOrEmpty(isIName) && company.Inamelitiko != isIName)
        {
          Logger.DebugFormat("Change Inamelitiko: current:{0}, new:{1}", company.Inamelitiko, isIName);
          company.Inamelitiko = isIName;             
        }   

        if(!string.IsNullOrEmpty(isNuRezident))
        {
          bool isNuRezidentBool = isNuRezident == "1" ? true : false;
          if(company.NUNonrezidentlitiko != !isNuRezidentBool)
          {
            Logger.DebugFormat("Change NUNonrezidentlitiko: current:{0}, new:{1}", company.NUNonrezidentlitiko, !isNuRezidentBool);
            company.NUNonrezidentlitiko = !isNuRezidentBool;                      
          }               
        }            
            
        if(!string.IsNullOrEmpty(isRezident))
        {
          bool isRezidentBool = isRezident == "1" ? true : false;
          if(company.Nonresident != !isRezidentBool)
          {
            Logger.DebugFormat("Change Nonresident: current:{0}, new:{1}", company.Nonresident, !isRezidentBool);
            company.Nonresident = !isRezidentBool;             
          }               
        }            
            
        if(!string.IsNullOrEmpty(isKPP) && company.TRRC != isKPP)
        {
          Logger.DebugFormat("Change TRRC: current:{0}, new:{1}", company.TRRC, isKPP);
          company.TRRC = isKPP;                          
        }
        
        if(!string.IsNullOrEmpty(isOKPO) && company.NCEO != isOKPO)
        {
          Logger.DebugFormat("Change NCEO: current:{0}, new:{1}", company.NCEO, isOKPO);
          company.NCEO = isOKPO;                          
        }            
            
        if(!string.IsNullOrEmpty(isOKOPF))
        {
          var okopf = litiko.NSI.OKOPFs.GetAll().Where(x => x.ExternalId == isOKOPF).FirstOrDefault();
          if(okopf != null && !Equals(company.OKOPFlitiko, okopf))
          {
            Logger.DebugFormat("Change OKOPFlitiko: current:{0}, new:{1}", company.OKOPFlitiko?.Name, okopf.Name);
            company.OKOPFlitiko = okopf;                                
          }            
        }             

        if(!string.IsNullOrEmpty(isOKFS))
        {
          var okfs = litiko.NSI.OKFSes.GetAll().Where(x => x.ExternalId == isOKFS).FirstOrDefault();
          if(okfs != null && !Equals(company.OKFSlitiko, okfs))
          {
            Logger.DebugFormat("Change OKFSlitiko: current:{0}, new:{1}", company.OKOPFlitiko?.Name, okfs.Name);
            company.OKFSlitiko = okfs;                                
          }            
        }
        
        if(isCodeOKONHelements.Any())
        {
          var firstValue = isCodeOKONHelements.Select(x => x.Value).FirstOrDefault();
          if (firstValue != null)
          {
            var nsiRecord = litiko.NSI.OKONHs.GetAll().FirstOrDefault(x => x.ExternalId == firstValue);
            if(nsiRecord != null && !Equals(company.OKONHlitiko, nsiRecord))
            {
              Logger.DebugFormat("Change OKONHlitiko: current:{0}, new:{1}", company.OKONHlitiko?.Name, nsiRecord.Name);
              company.OKONHlitiko = nsiRecord;               
            }
          }                       
        }
        
        if(isCodeOKVEDelements.Any())
        {
          var firstValue = isCodeOKVEDelements.Select(x => x.Value).FirstOrDefault();
          if (firstValue != null)
          {
            var nsiRecord = litiko.NSI.OKVEDs.GetAll().FirstOrDefault(x => x.ExternalId == firstValue);
            if(nsiRecord != null && !Equals(company.OKVEDlitiko, nsiRecord))
            {
              Logger.DebugFormat("Change OKVEDlitiko: current:{0}, new:{1}", company.OKVEDlitiko?.Name, nsiRecord.Name);
              company.OKVEDlitiko = nsiRecord;               
            }
          }                        
        }
        
        if(!string.IsNullOrEmpty(isRegistnum) && company.RegNumlitiko != isRegistnum)
        {
          Logger.DebugFormat("Change RegNumlitiko: current:{0}, new:{1}", company.RegNumlitiko, isRegistnum);
          company.RegNumlitiko = isRegistnum;                          
        }
        
        if(!string.IsNullOrEmpty(isNumbers) && company.Numberslitiko != int.Parse(isNumbers))
        {
          Logger.DebugFormat("Change Numberslitiko: current:{0}, new:{1}", company.Numberslitiko, isNumbers);
          company.Numberslitiko = int.Parse(isNumbers);                          
        }
        
        if(!string.IsNullOrEmpty(isBusiness) && company.Businesslitiko != isBusiness)
        {
          Logger.DebugFormat("Change Businesslitiko: current:{0}, new:{1}", company.Businesslitiko, isBusiness);
          company.Businesslitiko = isBusiness;                          
        }
        
        if(!string.IsNullOrEmpty(isPS_REF))
        {
          var enterpriseType = litiko.NSI.EnterpriseTypes.GetAll().Where(x => x.ExternalId == isPS_REF).FirstOrDefault();
          if(enterpriseType != null && !Equals(company.EnterpriseTypelitiko, enterpriseType))
          {
            Logger.DebugFormat("Change EnterpriseTypelitiko: current:{0}, new:{1}", company.EnterpriseTypelitiko?.Name, enterpriseType.Name);
            company.EnterpriseTypelitiko = enterpriseType;                                
          }            
        }
        
        if(!string.IsNullOrEmpty(isCountry))
        {
          var country = litiko.Eskhata.Countries.GetAll().Where(x => x.ExternalIdlitiko == isCountry).FirstOrDefault();
          if(country != null && !Equals(company.Countrylitiko, country))
          {
            Logger.DebugFormat("Change Countrylitiko: current:{0}, new:{1}", company.Countrylitiko?.Name, country.Name);
            company.Countrylitiko = country;                                
          }            
        }
        
        /*
        if (!string.IsNullOrEmpty(isRegion))
        {
          var region = Eskhata.Regions.GetAll().Where(x => x.ExternalIdlitiko == isRegion).FirstOrDefault();
          if (region != null && !Equals(company.Region, region))
          {
            Logger.DebugFormat("Change Region: current:{0}, new:{1}", company.Region?.Name, region?.Name);
            company.Region = region;                    
          }
        }
        */
  
        if (!string.IsNullOrEmpty(isCity))
        {
          var city = Eskhata.Cities.GetAll().Where(x => x.ExternalIdlitiko == isCity).FirstOrDefault();
          if (city != null && !Equals(company.City, city))
          {
            Logger.DebugFormat("Change City: current:{0}, new:{1}", company.City?.Name, city?.Name);
            company.City = city;                    
          }
        }        

        if(!string.IsNullOrEmpty(isPostAdress) && company.PostalAddress != isPostAdress)
        {
          Logger.DebugFormat("Change PostalAddress: current:{0}, new:{1}", company.PostalAddress, isPostAdress);
          company.PostalAddress = isPostAdress;                          
        }

        if(!string.IsNullOrEmpty(isLegalAdress) && company.LegalAddress != isLegalAdress)
        {
          Logger.DebugFormat("Change LegalAddress: current:{0}, new:{1}", company.LegalAddress, isLegalAdress);
          company.LegalAddress = isLegalAdress;                          
        }

        if(!string.IsNullOrEmpty(isPhone) && company.Phones != isPhone)
        {
          Logger.DebugFormat("Change Phones: current:{0}, new:{1}", company.Phones, isPhone);
          company.Phones = isPhone;                          
        }

        if(!string.IsNullOrEmpty(isEmail) && company.Email != isEmail)
        {
          Logger.DebugFormat("Change Email: current:{0}, new:{1}", company.Email, isEmail);
          company.Email = isEmail;                          
        }

        if(!string.IsNullOrEmpty(isWebSite) && company.Homepage != isWebSite)
        {
          Logger.DebugFormat("Change Homepage: current:{0}, new:{1}", company.Homepage, isWebSite);
          company.Homepage = isWebSite;                          
        }

        if(isContacts.Any())
        {
          foreach (var isContactElement in isContacts)
          {
            var isContactID = isContactElement.Element("ID")?.Value;
            var isRange = isContactElement.Element("RANGE")?.Value;
            var isPerson = isContactElement.Element("FASE");
            
            if (!string.IsNullOrEmpty(isContactID))
            {
              var contact = litiko.Eskhata.Contacts.GetAll().Where(x => x.ExternalIdlitiko == isContactID).FirstOrDefault();
              if (contact == null)
              {
                contact = litiko.Eskhata.Contacts.Create();
                contact.ExternalIdlitiko = isContactID;
                Logger.DebugFormat("Create new Contact with ExternalId:{0}. Id:{1}", isContactID, contact.Id);
              }
              else
                Logger.DebugFormat("Contact with ExternalId:{0} was found. Id:{1}, Name:{2}", isContactID, contact.Id, contact.Name);
              
              if (!Equals(contact.Company, company))
              {
                Logger.DebugFormat("Change Contact.Company: current:{0}, new:{1}", contact.Company?.Name, company?.Name);
                contact.Company = company;
              }
              
              if (!string.IsNullOrEmpty(isRange) && contact.JobTitle != isRange)
              {
                Logger.DebugFormat("Change Contact.JobTitle: current:{0}, new:{1}", contact.JobTitle, isRange);
                contact.JobTitle = isRange;
              }
              
              var personResult = ProcessingPerson(isPerson, null, null);
              var person = personResult.person;
              if(!Equals(contact.Person, person))
              {
                Logger.DebugFormat("Change Contact.Person: current:{0}, new:{1}", contact.Person?.Id, person?.Id);
                contact.Person = person;
              }
              
              if (contact.State.IsChanged || contact.State.IsInserted)
              {
                contact.Save();
                Logger.DebugFormat("Contact successfully saved. ExternalId:{0}, Id:{1}", isContactID, contact.Id);
              }

            }
          }
          
        }
        
        if (company.State.IsChanged)
        {
          //company.Save();
          Logger.DebugFormat("Company successfully changed, but not saved. The user can save the changes independently. ExternalId:{0}, Id:{1}", isId, company.Id);
        }
        else
        {
          Logger.DebugFormat("There are no changes in Company. ExternalId:{0}, Id:{1}", isId, company.Id);
        }
        
      }
      catch (Exception ex)
      {
        var errorMessage = string.Format("Error when processing Company with ExternalId:{0}. Description: {1}. StackTrace: {2}", isId, ex.Message, ex.StackTrace);
        Logger.Error(errorMessage);
        errorList.Add(errorMessage);
      }

      Logger.Debug("R_DR_GET_COMPANY - Finish");
      return errorList;
    }
    
    /// <summary>
    /// Обработка Банка.
    /// </summary>
    /// <param name="exchDocID">ИД документа обмена</param>
    /// <param name="counterparty">Банк</param>
    /// <returns>Список ошибок (List<string>)</returns>
    [Remote]
    public List<string> R_DR_GET_BANK(long exchDocID, Eskhata.ICounterparty counterparty)
    {
      Logger.Debug("R_DR_GET_BANK - Start");
      var errorList = new List<string>();
      
      var exchDoc = ExchangeDocuments.Get(exchDocID);
      var versionFullXML = exchDoc.Versions.Where(v => v.Note == Integration.Resources.VersionRequestToRXFull && v.AssociatedApplication.Extension == "xml" && v.Body.Size > 0).FirstOrDefault();
      if (versionFullXML == null)
      {
        errorList.Add("Version with full XML data not found.");
        return errorList;
      }
      
      XDocument xmlDoc = XDocument.Load(versionFullXML.Body.Read());
      var dataElements = xmlDoc.Descendants("Data").Elements("element");
      if (!dataElements.Any())
      {
        errorList.Add("Empty Data node.");
        return errorList;
      }
      
      var element = dataElements.FirstOrDefault();
      
      var isId = element.Element("ID")?.Value;
      var isName = element.Element("NAME")?.Value;
      var isBIC = element.Element("BIC")?.Value;
      
      var isLongName = element.Element("LONG_NAME")?.Value;
      var isIName = element.Element("I_NAME")?.Value;
      var isSWIFT = element.Element("SWIFT")?.Value;
      var isRezident = element.Element("REZIDENT")?.Value;
      var isNuRezident = element.Element("NU_REZIDENT")?.Value;
      var isINN = element.Element("INN")?.Value;
      var isKPP = element.Element("KPP")?.Value;
      var isCorrAcc = element.Element("CorrAcc")?.Value;
      var isIsSettlements = element.Element("IsSettlements")?.Value;
      var isIsLoroCorrespondent = element.Element("IsLoroCorrespondent")?.Value;
      var isIsNostroCorrespondent = element.Element("IsNostroCorrespondent")?.Value;
      var isCountry = element.Element("COUNTRY")?.Value;
      //var isRegion = element.Element("Region")?.Value;
      var isCity = element.Element("City")?.Value;       
      var isPostAdress = element.Element("PostAdress")?.Value;
      var isLegalAdress = element.Element("LegalAdress")?.Value;
      var isPhone = element.Element("Phone")?.Value;
      var isEmail = element.Element("Email")?.Value;
      var isWebSite = element.Element("WebSite")?.Value;
      
      var isContacts = element.Element("Contacts").Elements("element");
      
      try
      {
        var bank = Eskhata.Banks.As(counterparty);
        
        if (string.IsNullOrEmpty(isId) || string.IsNullOrEmpty(isName))
          throw AppliedCodeException.Create(string.Format("Not all required fields are filled in. ID:{0}, NAME:{1}, BIC:{2}", isId, isName, isBIC));
        
        if (bank.ExternalId != isId)
        {
          Logger.DebugFormat("Change ExternalId: current:{0}, new:{1}", bank.ExternalId, isId);
          bank.ExternalId = isId;                        
        }

        if (bank.Name != isName)
        {
          Logger.DebugFormat("Change Name: current:{0}, new:{1}", bank.Name, isName);
          bank.Name = isName;                              
        }

        if(!string.IsNullOrEmpty(isLongName) && bank.LegalName != isLongName)
        {
          Logger.DebugFormat("Change LegalName: current:{0}, new:{1}", bank.LegalName, isLongName);
          bank.LegalName = isLongName;                  
        }
        
        if(!string.IsNullOrEmpty(isIName) && bank.Inamelitiko != isIName)
        {
          Logger.DebugFormat("Change Inamelitiko: current:{0}, new:{1}", bank.Inamelitiko, isIName);
          bank.Inamelitiko = isIName;                 
        }   

        if (!string.IsNullOrEmpty(isSWIFT) && bank.SWIFT != isSWIFT)
        {
          Logger.DebugFormat("Change Inamelitiko: current:{0}, new:{1}", bank.SWIFT, isSWIFT);
          bank.SWIFT = isSWIFT;                     
        }
        
        if(!string.IsNullOrEmpty(isNuRezident))
        {
          bool isNuRezidentBool = isNuRezident == "1" ? true : false;
          if(bank.NUNonrezidentlitiko != !isNuRezidentBool)
          {
            Logger.DebugFormat("Change NUNonrezidentlitiko: current:{0}, new:{1}", bank.NUNonrezidentlitiko, !isNuRezidentBool);
            bank.NUNonrezidentlitiko = !isNuRezidentBool;                      
          }               
        }            
            
        if(!string.IsNullOrEmpty(isRezident))
        {
          bool isRezidentBool = isRezident == "1" ? true : false;
          if(bank.Nonresident != !isRezidentBool)
          {
            Logger.DebugFormat("Change Nonresident: current:{0}, new:{1}", bank.Nonresident, !isRezidentBool);
            bank.Nonresident = !isRezidentBool;             
          }               
        }            
            
        if(!string.IsNullOrEmpty(isINN) && bank.TIN != isINN)
        {
          Logger.DebugFormat("Change TIN: current:{0}, new:{1}", bank.TIN, isINN);
          bank.TIN = isINN;                         
        }

        if(!string.IsNullOrEmpty(isKPP) && bank.TRRC != isKPP)
        {
          Logger.DebugFormat("Change TRRC: current:{0}, new:{1}", bank.TRRC, isKPP);
          bank.TRRC = isKPP;                          
        }
            
        if(!string.IsNullOrEmpty(isCorrAcc) && bank.CorrespondentAccountlitiko != isCorrAcc)
        {
          Logger.DebugFormat("Change CorrespondentAccountlitiko: current:{0}, new:{1}", bank.CorrespondentAccountlitiko, isCorrAcc);
          bank.CorrespondentAccountlitiko = isCorrAcc;         
        }        

        if(!string.IsNullOrEmpty(isIsSettlements))
        {
          bool isIsSettlementsBool = isIsSettlements == "1" ? true : false;
          if(bank.SettlParticipantlitiko != isIsSettlementsBool)
          {
            Logger.DebugFormat("Change SettlParticipantlitiko: current:{0}, new:{1}", bank.SettlParticipantlitiko, isIsSettlementsBool);
            bank.SettlParticipantlitiko = isIsSettlementsBool;             
          }            
        }

        if(!string.IsNullOrEmpty(isIsLoroCorrespondent))
        {
          bool isIsLoroCorrespondentBool = isIsLoroCorrespondent == "1" ? true : false;
          if(bank.LoroCorrespondentlitiko != isIsLoroCorrespondentBool)
          {
            Logger.DebugFormat("Change LoroCorrespondentlitiko: current:{0}, new:{1}", bank.LoroCorrespondentlitiko, isIsLoroCorrespondentBool);
            bank.LoroCorrespondentlitiko = isIsLoroCorrespondentBool;             
          }            
        }        

        if(!string.IsNullOrEmpty(isIsNostroCorrespondent))
        {
          bool isIsNostroCorrespondentBool = isIsNostroCorrespondent == "1" ? true : false;
          if(bank.NostroCorrespondentlitiko != isIsNostroCorrespondentBool)
          {
            Logger.DebugFormat("Change NostroCorrespondentlitiko: current:{0}, new:{1}", bank.NostroCorrespondentlitiko, isIsNostroCorrespondentBool);
            bank.NostroCorrespondentlitiko = isIsNostroCorrespondentBool;             
          }            
        }
        
        if(!string.IsNullOrEmpty(isCountry))
        {
          var country = litiko.Eskhata.Countries.GetAll().Where(x => x.ExternalIdlitiko == isCountry).FirstOrDefault();
          if(country != null && !Equals(bank.Countrylitiko, country))
          {
            Logger.DebugFormat("Change Countrylitiko: current:{0}, new:{1}", bank.Countrylitiko?.Name, country.Name);
            bank.Countrylitiko = country;
          }            
        }
        
        /*
        if (!string.IsNullOrEmpty(isRegion))
        {
          var region = Eskhata.Regions.GetAll().Where(x => x.ExternalIdlitiko == isRegion).FirstOrDefault();
          if (region != null && !Equals(bank.Region, region))
          {
            Logger.DebugFormat("Change Region: current:{0}, new:{1}", bank.Region?.Name, region?.Name);
            bank.Region = region;                    
          }
        }
        */
        
        if (!string.IsNullOrEmpty(isCity))
        {
          var city = Eskhata.Cities.GetAll().Where(x => x.ExternalIdlitiko == isCity).FirstOrDefault();
          if (city != null && !Equals(bank.City, city))
          {
            Logger.DebugFormat("Change City: current:{0}, new:{1}", bank.City?.Name, city?.Name);
            bank.City = city;                    
          }
        }        

        if(!string.IsNullOrEmpty(isPostAdress) && bank.PostalAddress != isPostAdress)
        {
          Logger.DebugFormat("Change PostalAddress: current:{0}, new:{1}", bank.PostalAddress, isPostAdress);
          bank.PostalAddress = isPostAdress;                         
        }

        if(!string.IsNullOrEmpty(isLegalAdress) && bank.LegalAddress != isLegalAdress)
        {
          Logger.DebugFormat("Change LegalAddress: current:{0}, new:{1}", bank.LegalAddress, isLegalAdress);
          bank.LegalAddress = isLegalAdress;                          
        }

        if(!string.IsNullOrEmpty(isPhone) && bank.Phones != isPhone)
        {
          Logger.DebugFormat("Change Phones: current:{0}, new:{1}", bank.Phones, isPhone);
          bank.Phones = isPhone;                          
        }

        if(!string.IsNullOrEmpty(isEmail) && bank.Email != isEmail)
        {
          Logger.DebugFormat("Change Email: current:{0}, new:{1}", bank.Email, isEmail);
          bank.Email = isEmail;                          
        }

        if(!string.IsNullOrEmpty(isWebSite) && bank.Homepage != isWebSite)
        {
          Logger.DebugFormat("Change Homepage: current:{0}, new:{1}", bank.Homepage, isWebSite);
          bank.Homepage = isWebSite;                          
        }

        if(isContacts.Any())
        {
          foreach (var isContactElement in isContacts)
          {
            var isContactID = isContactElement.Element("ID")?.Value;
            var isRange = isContactElement.Element("RANGE")?.Value;
            var isPerson = isContactElement.Element("FASE");
            
            if (!string.IsNullOrEmpty(isContactID))
            {
              var contact = litiko.Eskhata.Contacts.GetAll().Where(x => x.ExternalIdlitiko == isContactID).FirstOrDefault();
              if (contact == null)
              {
                contact = litiko.Eskhata.Contacts.Create();
                contact.ExternalIdlitiko = isContactID;
                Logger.DebugFormat("Create new Contact with ExternalId:{0}. Id:{1}", isContactID, contact.Id);
              }
              else
                Logger.DebugFormat("Contact with ExternalId:{0} was found. Id:{1}, Name:{2}", isContactID, contact.Id, contact.Name);
              
              if (!Equals(contact.Company, bank))
              {
                Logger.DebugFormat("Change Contact.Company: current:{0}, new:{1}", contact.Company?.Name, bank?.Name);
                contact.Company = bank;
              }
              
              if (!string.IsNullOrEmpty(isRange) && contact.JobTitle != isRange)
              {
                Logger.DebugFormat("Change Contact.JobTitle: current:{0}, new:{1}", contact.JobTitle, isRange);
                contact.JobTitle = isRange;
              }
              
              var personResult = ProcessingPerson(isPerson, null, null);
              var person = personResult.person;
              if(!Equals(contact.Person, person))
              {
                Logger.DebugFormat("Change Contact.Person: current:{0}, new:{1}", contact.Person?.Id, person?.Id);
                contact.Person = person;
              }
              
              if (contact.State.IsChanged || contact.State.IsInserted)
              {
                contact.Save();
                Logger.DebugFormat("Contact successfully saved. ExternalId:{0}, Id:{1}", isContactID, contact.Id);
              }
              
            }
          }
          
        }
        
        if (bank.State.IsChanged)
        {
          //company.Save();
          Logger.DebugFormat("Bank successfully changed, but not saved. The user can save the changes independently. ExternalId:{0}, Id:{1}", isId, bank.Id);
        }
        else
        {
          Logger.DebugFormat("There are no changes in Bank. ExternalId:{0}, Id:{1}", isId, bank.Id);
        }
        
      }
      catch (Exception ex)
      {
        var errorMessage = string.Format("Error when processing Bank with ExternalId:{0}. Description: {1}. StackTrace: {2}", isId, ex.Message, ex.StackTrace);
        Logger.Error(errorMessage);
        errorList.Add(errorMessage);
      }
      
      Logger.Debug("R_DR_GET_BANK - Finish");
      return errorList;
    }
    
    /// <summary>
    /// Обработка курсов валют.
    /// </summary>
    /// <param name="dataElements">Информация по курсам валют в виде XElement.</param>
    /// <returns>Список ошибок (List<string>)</returns>
    public List<string> R_DR_GET_CURRENCY_RATES(System.Collections.Generic.IEnumerable<System.Xml.Linq.XElement> dataElements)
    {
      Logger.Debug("R_DR_GET_CURRENCY_RATES - Start");
      var errorList = new List<string>();
      int countAll = dataElements.Count();
      int countChanged = 0;
      int countNotChanged = 0;
      int countErrors = 0;
      
      foreach (var element in dataElements)
      {
        Transactions.Execute(() =>
        {
          // TODO Проверить, приходит ли в пакете ID курса валюты (по ТЗ этого поля нет)
          //var isId = element.Element("ID")?.Value;
          
          var isCurrencyAlphaCode = element.Element("CurrencyAlphaCode")?.Value;
          var isRateDate = element.Element("RateDate")?.Value;          
          var isRate = element.Element("Rate")?.Value;        
          
          try
          {                        
            if (string.IsNullOrEmpty(isCurrencyAlphaCode) || string.IsNullOrEmpty(isRateDate) || string.IsNullOrEmpty(isRate))
              throw AppliedCodeException.Create(string.Format("Not all required fields are filled in. CurrencyAlphaCode:{0}, RateDate:{1}, Rate:{2}", isCurrencyAlphaCode, isRateDate, isRate));  
            
            DateTime rateDate;
            if (!Calendar.TryParseDate(isRateDate, out rateDate))
              throw AppliedCodeException.Create(string.Format("Failed to convert value to date:{0}", isRateDate));
            
            double rate;
            if (!Double.TryParse(isRate, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out rate))
              throw AppliedCodeException.Create(string.Format("Failed to convert value to double:{0}", isRate));
            
            var currency = litiko.Eskhata.Currencies.GetAll()
              .Where(x => x.Status == Sungero.CoreEntities.DatabookEntry.Status.Active && x.AlphaCode == isCurrencyAlphaCode)
              .FirstOrDefault();
            
            if (currency != null)
            {
              var entity = litiko.NSI.CurrencyRates.GetAll()
                .Where(x => Equals(x.Currency, currency))
                .Where(x => Equals(x.Date, rateDate))
                .FirstOrDefault();
              
              if (entity != null)
                Logger.DebugFormat("Currency rate {0} on date {1} already exists", isCurrencyAlphaCode, isRateDate);
              else
              {              
                entity = NSI.CurrencyRates.Create();
                //entity.ExternalId = isId;
                entity.Currency = currency;
                entity.Date = rateDate;
                Logger.DebugFormat("Create new Currency rate {0} on date {1}", isCurrencyAlphaCode, isRateDate);
              }

                                   if (entity.Rate != rate)
                                   {
                                     Logger.DebugFormat("Change rate: current:{0}, new:{1}", entity.Rate, rate);
                                     entity.Rate = rate;
                                   }
                                   
                                   if (entity.State.IsInserted || entity.State.IsChanged)
                                   {
                                     entity.Save();
                                     Logger.DebugFormat("Currency rate successfully saved. Id:{0}", entity.Id);
                                     countChanged++;
                                   }
                                   else
                                   {
                                     Logger.DebugFormat("There are no changes in Currency rate. Id:{1}", entity.Id);
                                     countNotChanged++;
                                   }
                                 }
                                 else
                                   Logger.DebugFormat("Currency with AlphaCode:{0} not found.", isCurrencyAlphaCode);
                                 
                               }
                               catch (Exception ex)
                               {
                                 var errorMessage = string.Format("Error when processing Currency rate. Description: {0}. StackTrace: {1}", ex.Message, ex.StackTrace);
                                 Logger.Error(errorMessage);
                                 errorList.Add(errorMessage);
                                 countErrors++;
                               }
                             });
      }
      Logger.DebugFormat("R_DR_GET_CURRENCY_RATES - Total: CountAll:{0} CountChanged:{1} CountNotChanged:{2} CountErrors:{3}", countAll, countChanged, countNotChanged, countErrors);
      
      Logger.Debug("R_DR_GET_CURRENCY_RATES - Finish");
      return errorList;
    }
    
    /// <summary>
    /// Обработка справочника Регионы оплаты.
    /// </summary>
    /// <param name="dataElements">Информация по регионам оплаты в виде XElement.</param>
    /// <returns>Список ошибок (List<string>)</returns>
    public List<string> R_DR_GET_PAYMENT_REGIONS(System.Collections.Generic.IEnumerable<System.Xml.Linq.XElement> dataElements)
    {
      Logger.Debug("R_DR_GET_PAYMENT_REGIONS - Start");
      var errorList = new List<string>();
      int countAll = dataElements.Count();
      int countChanged = 0;
      int countNotChanged = 0;
      int countErrors = 0;
      
      foreach (var element in dataElements)
      {
        Transactions.Execute(() =>
                             {
                               var isId = element.Element("ID")?.Value;
                               var isName = element.Element("Name")?.Value;
                               var isCode = element.Element("Code")?.Value;
                               var isLabel = element.Element("Label")?.Value;
                               
                               try
                               {
                                 if (string.IsNullOrEmpty(isId) || string.IsNullOrEmpty(isName) || string.IsNullOrEmpty(isCode) || string.IsNullOrEmpty(isLabel))
                                   throw AppliedCodeException.Create(string.Format("Not all required fields are filled in. ID:{0}, Name:{1}, Code:{2}, Label:{3}", isId, isName, isCode, isLabel));
                                 
                                 var entity = NSI.PaymentRegions.GetAll().Where(x => x.ExternalId == isId).FirstOrDefault();
                                 if (entity != null)
                                   Logger.DebugFormat("Payment region with ExternalId:{0} was found. Id:{1}, Name:{2}", isId, entity.Id, entity.Name);
                                 else
                                 {
                                   entity = NSI.PaymentRegions.Create();
                                   entity.ExternalId = isId;
                                   Logger.DebugFormat("Create new Payment region with ExternalId:{0}. Id:{1}", isId, entity.Id);
                                 }
                                 
                                 if (entity.Name != isName)
                                 {
                                   Logger.DebugFormat("Change Name: current:{0}, new:{1}", entity.Name, isName);
                                   entity.Name = isName;
                                 }
                                 
                                 if (entity.Code != isCode)
                                 {
                                   Logger.DebugFormat("Change Code: current:{0}, new:{1}", entity.Code, isCode);
                                   entity.Code = isCode;
                                 }

                                 if (entity.Marker != isLabel)
                                 {
                                   Logger.DebugFormat("Change Label: current:{0}, new:{1}", entity.Marker, isLabel);
                                   entity.Marker = isLabel;
                                 }
                                 
                                 if (entity.State.IsInserted || entity.State.IsChanged)
                                 {
                                   entity.Save();
                                   Logger.DebugFormat("Payment region successfully saved. ExternalId:{0}, Id:{1}", isId, entity.Id);
                                   countChanged++;
                                 }
                                 else
                                 {
                                   Logger.DebugFormat("There are no changes in Payment region. ExternalId:{0}, Id:{1}", isId, entity.Id);
                                   countNotChanged++;
                                 }
                               }
                               catch (Exception ex)
                               {
                                 var errorMessage = string.Format("Error when processing Payment region with ExternalId:{0}. Description: {1}. StackTrace: {2}", isId, ex.Message, ex.StackTrace);
                                 Logger.Error(errorMessage);
                                 errorList.Add(errorMessage);
                                 countErrors++;
                               }
                             });
      }
      Logger.DebugFormat("R_DR_GET_PAYMENT_REGIONS - Total: CountAll:{0} CountChanged:{1} CountNotChanged:{2} CountErrors:{3}", countAll, countChanged, countNotChanged, countErrors);
      
      Logger.Debug("R_DR_GET_PAYMENT_REGIONS - Finish");
      return errorList;
    }
    
    /// <summary>
    /// Обработка справочника Регионы объектов аренды.
    /// </summary>
    /// <param name="dataElements">Информация по регионам объектов аренды в виде XElement.</param>
    /// <returns>Список ошибок (List<string>)</returns>
    public List<string> R_DR_GET_TAX_REGIONS(System.Collections.Generic.IEnumerable<System.Xml.Linq.XElement> dataElements)
    {
      Logger.Debug("R_DR_GET_TAX_REGIONS - Start");
      var errorList = new List<string>();
      int countAll = dataElements.Count();
      int countChanged = 0;
      int countNotChanged = 0;
      int countErrors = 0;
      
      foreach (var element in dataElements)
      {
        Transactions.Execute(() =>
                             {
                               var isId = element.Element("ID")?.Value;
                               var isName = element.Element("Name")?.Value;
                               var isCode = element.Element("Code")?.Value;
                               
                               try
                               {
                                 if (string.IsNullOrEmpty(isId) || string.IsNullOrEmpty(isName) || string.IsNullOrEmpty(isCode))
                                   throw AppliedCodeException.Create(string.Format("Not all required fields are filled in. ID:{0}, Name:{1}, Code:{2}", isId, isName, isCode));
                                 
                                 var entity = NSI.RegionOfRentals.GetAll().Where(x => x.ExternalId == isId).FirstOrDefault();
                                 if (entity != null)
                                   Logger.DebugFormat("Region of rental with ExternalId:{0} was found. Id:{1}, Name:{2}", isId, entity.Id, entity.Name);
                                 else
                                 {
                                   entity = NSI.RegionOfRentals.Create();
                                   entity.ExternalId = isId;
                                   Logger.DebugFormat("Create new Region of rental with ExternalId:{0}. Id:{1}", isId, entity.Id);
                                 }
                                 
                                 if (entity.Name != isName)
                                 {
                                   Logger.DebugFormat("Change Name: current:{0}, new:{1}", entity.Name, isName);
                                   entity.Name = isName;
                                 }
                                 
                                 if (entity.Code != isCode)
                                 {
                                   Logger.DebugFormat("Change Code: current:{0}, new:{1}", entity.Code, isCode);
                                   entity.Code = isCode;
                                 }
                                 
                                 if (entity.State.IsInserted || entity.State.IsChanged)
                                 {
                                   entity.Save();
                                   Logger.DebugFormat("Region of rental successfully saved. ExternalId:{0}, Id:{1}", isId, entity.Id);
                                   countChanged++;
                                 }
                                 else
                                 {
                                   Logger.DebugFormat("There are no changes in Region of rental. ExternalId:{0}, Id:{1}", isId, entity.Id);
                                   countNotChanged++;
                                 }
                               }
                               catch (Exception ex)
                               {
                                 var errorMessage = string.Format("Error when processing Region of rental with ExternalId:{0}. Description: {1}. StackTrace: {2}", isId, ex.Message, ex.StackTrace);
                                 Logger.Error(errorMessage);
                                 errorList.Add(errorMessage);
                                 countErrors++;
                               }
                             });
      }
      Logger.DebugFormat("R_DR_GET_TAX_REGIONS - Total: CountAll:{0} CountChanged:{1} CountNotChanged:{2} CountErrors:{3}", countAll, countChanged, countNotChanged, countErrors);
      
      Logger.Debug("R_DR_GET_TAX_REGIONS - Finish");
      return errorList;
    }
    
    /// <summary>
    /// Обработка справочника Виды договоров (Виды документов).
    /// </summary>
    /// <param name="dataElements">Информация по видам договоров в виде XElement.</param>
    /// <returns>Список ошибок (List<string>)</returns>
    public List<string> R_DR_GET_CONTRACT_VID(System.Collections.Generic.IEnumerable<System.Xml.Linq.XElement> dataElements)
    {
      Logger.Debug("R_DR_GET_CONTRACT_VID - Start");
      var errorList = new List<string>();
      int countAll = dataElements.Count();
      int countChanged = 0;
      int countNotChanged = 0;
      int countErrors = 0;
      
      var documentType = Sungero.Docflow.DocumentTypes.GetAll(t => t.DocumentTypeGuid == Sungero.Contracts.PublicConstants.Module.ContractGuid).FirstOrDefault();
      var registrable = Sungero.Docflow.DocumentKind.NumberingType.Registrable;
      
      var actionInfos = new Sungero.Domain.Shared.IActionInfo[] {
        Sungero.Docflow.OfficialDocuments.Info.Actions.SendForFreeApproval,
        Sungero.Docflow.OfficialDocuments.Info.Actions.SendForApproval
      };
      var actions = new List<Sungero.Docflow.IDocumentSendAction>();
      foreach (var actionInfo in actionInfos)
      {
        var internalAction = actionInfo as Sungero.Domain.Shared.IInternalActionInfo;
        var action = Sungero.Docflow.DocumentSendActions.GetAllCached(a => a.ActionGuid == internalAction.NameGuid.ToString()).FirstOrDefault();
        if (action != null)
          actions.Add(action);
      }
      
      var contractsDocumentFlow = Sungero.Docflow.DocumentKind.DocumentFlow.Contracts;
      string namePrefix = "New_";
      string shortNamePrefix = "Договор";
      
      foreach (var element in dataElements)
      {
        Transactions.Execute(() =>
                             {
                               var isId = element.Element("ID")?.Value;
                               var isName = element.Element("Name")?.Value;
                               var isCode = new string((element.Element("Code")?.Value ?? "")
                                                       .Take(10)
                                                       .ToArray());
                               
                               try
                               {
                                 if (string.IsNullOrEmpty(isId) || string.IsNullOrEmpty(isName) || string.IsNullOrEmpty(isCode))
                                   throw AppliedCodeException.Create(string.Format("Not all required fields are filled in. ID:{0}, Name:{1}, Code:{2}", isId, isName, isCode));
                                 
                                 var entity = litiko.Eskhata.DocumentKinds.GetAll().Where(x => x.ExternalIdlitiko == isId).FirstOrDefault();
                                 if (entity != null)
                                   Logger.DebugFormat("Document kind with ExternalId:{0} was found. Id:{1}, Name:{2}", isId, entity.Id, entity.Name);
                                 else
                                 {
                                   entity = litiko.Eskhata.DocumentKinds.Create();
                                   entity.ExternalIdlitiko = isId;
                                   entity.NumberingType = registrable;
                                   entity.AutoNumbering = false;
                                   entity.DocumentFlow = contractsDocumentFlow;
                                   entity.GenerateDocumentName = true;
                                   entity.ProjectsAccounting = false;
                                   entity.GrantRightsToProject = false;
                                   entity.DocumentType = documentType;
                                   entity.IsDefault = false;
                                   // Префикс в имя добавляется для того, чтобы отличать созданные автоматически записи от созданных вручную
                                   entity.Name = $"{namePrefix}{isName}";
                                   entity.ShortName = $"{shortNamePrefix} {isName}";
                                   
                                   entity.AvailableActions.Clear();
                                   foreach (var action in actions)
                                     entity.AvailableActions.AddNew().Action = action;
                                   
                                   Logger.DebugFormat("Create new Document kind with ExternalId:{0}. Id:{1}. Name:{2}", isId, entity.Id, isName);
                                 }
                                 
                                 /* 
            if (entity.Name != isName)
            {
              Logger.DebugFormat("Change Name: current:{0}, new:{1}", entity.Name, isName);
              entity.Name = isName;
            }
                                  */
                                 
                                 if (entity.Code != isCode)
                                 {
                                   Logger.DebugFormat("Change Code: current:{0}, new:{1}", entity.Code, isCode);
                                   entity.Code = isCode;
                                 }
                                 
                                 if (entity.State.IsInserted || entity.State.IsChanged)
                                 {
                                   entity.Save();
                                   Logger.DebugFormat("Document kind successfully saved. ExternalId:{0}, Id:{1}", isId, entity.Id);
                                   countChanged++;
                                 }
                                 else
                                 {
                                   Logger.DebugFormat("There are no changes in Document kind. ExternalId:{0}, Id:{1}", isId, entity.Id);
                                   countNotChanged++;
                                 }
                               }
                               catch (Exception ex)
                               {
                                 var errorMessage = string.Format("Error when processing Document kind with ExternalId:{0}. Description: {1}. StackTrace: {2}", isId, ex.Message, ex.StackTrace);
                                 Logger.Error(errorMessage);
                                 errorList.Add(errorMessage);
                                 countErrors++;
                               }
                             });
      }
      Logger.DebugFormat("R_DR_GET_CONTRACT_VID - Total: CountAll:{0} CountChanged:{1} CountNotChanged:{2} CountErrors:{3}", countAll, countChanged, countNotChanged, countErrors);
      
      Logger.Debug("R_DR_GET_CONTRACT_VID - Finish");
      return errorList;
    }

    /// <summary>
    /// Обработка справочника Типы договоров (Категории договоров).
    /// </summary>
    /// <param name="dataElements">Информация по типам договоров в виде XElement.</param>
    /// <returns>Список ошибок (List<string>)</returns>
    public List<string> R_DR_GET_CONTRACT_TYPE(System.Collections.Generic.IEnumerable<System.Xml.Linq.XElement> dataElements)
    {
      Logger.Debug("R_DR_GET_CONTRACT_TYPE - Start");
      var errorList = new List<string>();
      int countAll = dataElements.Count();
      int countChanged = 0;
      int countNotChanged = 0;
      int countErrors = 0;
      string namePrefix = "New_";
      
      foreach (var element in dataElements)
      {
        Transactions.Execute(() =>
                             {
                               var isId = element.Element("ID")?.Value;
                               var isName = element.Element("Name")?.Value;
                               
                               try
                               {
                                 if (string.IsNullOrEmpty(isId) || string.IsNullOrEmpty(isName))
                                   throw AppliedCodeException.Create(string.Format("Not all required fields are filled in. ID:{0}, Name:{1}", isId, isName));
                                 
                                 var entity = Sungero.Contracts.ContractCategories.GetAll().Where(x => x.ExternalIdlitiko == isId).FirstOrDefault();
                                 if (entity != null)
                                   Logger.DebugFormat("Contract type with ExternalId:{0} was found. Id:{1}, Name:{2}", isId, entity.Id, entity.Name);
                                 else
                                 {
                                   entity = Sungero.Contracts.ContractCategories.Create();
                                   entity.ExternalIdlitiko = isId;
                                   // Префикс в имя добавляется для того, чтобы отличать созданные автоматически записи от созданных вручную
                                   entity.Name = $"{namePrefix}{isName}";
                                   Logger.DebugFormat("Create new Contract type with ExternalId:{0}. Id:{1}", isId, entity.Id);
                                 }
                                 
                                 /*
            if (entity.Name != isName)
            {
              Logger.DebugFormat("Change Name: current:{0}, new:{1}", entity.Name, isName);
              entity.Name = isName;
            }
                                  */
                                 
                                 if (entity.State.IsInserted || entity.State.IsChanged)
                                 {
                                   entity.Save();
                                   Logger.DebugFormat("Contract type successfully saved. ExternalId:{0}, Id:{1}", isId, entity.Id);
                                   countChanged++;
                                 }
                                 else
                                 {
                                   Logger.DebugFormat("There are no changes in Contract type. ExternalId:{0}, Id:{1}", isId, entity.Id);
                                   countNotChanged++;
                                 }
                               }
                               catch (Exception ex)
                               {
                                 var errorMessage = string.Format("Error when processing Contract type with ExternalId:{0}. Description: {1}. StackTrace: {2}", isId, ex.Message, ex.StackTrace);
                                 Logger.Error(errorMessage);
                                 errorList.Add(errorMessage);
                                 countErrors++;
                               }
                             });
      }
      Logger.DebugFormat("R_DR_GET_CONTRACT_TYPE - Total: CountAll:{0} CountChanged:{1} CountNotChanged:{2} CountErrors:{3}", countAll, countChanged, countNotChanged, countErrors);
      
      Logger.Debug("R_DR_GET_CONTRACT_TYPE - Finish");
      return errorList;
    }
    
    /// <summary>
    /// Обработка Персоны.
    /// </summary>
    /// <param name="exchDocID">ИД документа обмена</param>
    /// <param name="counterparty">Персона</param>
    /// <returns>Список ошибок (List<string>)</returns>
    [Remote]
    public List<string> R_DR_GET_PERSON(long exchDocID, Eskhata.ICounterparty counterparty)
    {
      Logger.Debug("R_DR_GET_PERSON - Start");
      var errorList = new List<string>();
      
      var exchDoc = ExchangeDocuments.Get(exchDocID);
      var versionFullXML = exchDoc.Versions.Where(v => v.Note == Integration.Resources.VersionRequestToRXFull && v.AssociatedApplication.Extension == "xml" && v.Body.Size > 0).FirstOrDefault();
      if (versionFullXML == null)
      {
        errorList.Add("Version with full XML data not found.");
        return errorList;
      }
      
      XDocument xmlDoc = XDocument.Load(versionFullXML.Body.Read());
      var dataElements = xmlDoc.Descendants("Data").Elements("FASE");
      if (!dataElements.Any())
      {
        errorList.Add("Empty Data node.");
        return errorList;
      }
      
      try
      {
        var person = litiko.Eskhata.People.As(counterparty);
        var isPerson = dataElements.FirstOrDefault();
        var personResult = ProcessingPerson(isPerson, null, person);
      }
      catch (Exception ex)
      {
        var errorMessage = string.Format("Error when processing Person with Id:{0}. Description: {1}. StackTrace: {2}", counterparty.Id, ex.Message, ex.StackTrace);
        Logger.Error(errorMessage);
        errorList.Add(errorMessage);
      }
      
      Logger.Debug("R_DR_GET_PERSON - Finish");
      return errorList;
    }
    
    /// <summary>
    /// Обработка справочника Регионы.
    /// </summary>
    /// <param name="dataElements">Информация по регионам в виде XElement.</param>
    /// <returns>Список ошибок (List<string>)</returns>     
    public List<string> R_DR_GET_REGIONS(System.Collections.Generic.IEnumerable<System.Xml.Linq.XElement> dataElements)
    {          
      Logger.Debug("R_DR_GET_REGIONS - Start");
      var errorList = new List<string>();
      int countAll = dataElements.Count();
      int countChanged = 0;
      int countNotChanged = 0;
      int countErrors = 0;
      
      foreach (var element in dataElements)
      {       
        Transactions.Execute(() =>
        {
          var isId = element.Element("ID")?.Value;
          var isCountry = element.Element("Country")?.Value;
          var isName = element.Element("Name")?.Value;          
          
          try
          {                        
            if (string.IsNullOrEmpty(isId) || string.IsNullOrEmpty(isName) || string.IsNullOrEmpty(isCountry))
              throw AppliedCodeException.Create(string.Format("Not all required fields are filled in. ID:{0}, Name:{1}, Country:{2}", isId, isName, isCountry));
            
            var entity = litiko.Eskhata.Regions.GetAll().Where(x => x.ExternalIdlitiko == isId).FirstOrDefault();
            if (entity != null)
              Logger.DebugFormat("Region with ExternalId:{0} was found. Id:{1}, Name:{2}", isId, entity.Id, entity.Name);
            else
            {              
              entity = litiko.Eskhata.Regions.Create();
              entity.ExternalIdlitiko = isId;
              Logger.DebugFormat("Create new Region with ExternalId:{0}. Id:{1}", isId, entity.Id);
            }             
            
            if (entity.Name != isName)
            {
              Logger.DebugFormat("Change Name: current:{0}, new:{1}", entity.Name, isName);
              entity.Name = isName;              
            }
            
            var country = litiko.Eskhata.Countries.GetAll().Where(x => x.ExternalIdlitiko == isCountry).FirstOrDefault();
            if (!Equals(entity.Country, country))
            {
              Logger.DebugFormat("Change Country: current:{0}, new:{1}", entity.Country?.Id, country?.Id);
              entity.Country = country;              
            }                                                            
            
            if (entity.State.IsInserted || entity.State.IsChanged)
            {
              entity.Save();                                          
              Logger.DebugFormat("Region successfully saved. ExternalId:{0}, Id:{1}", isId, entity.Id);
              countChanged++;
            }
            else
            {
              Logger.DebugFormat("There are no changes in Region. ExternalId:{0}, Id:{1}", isId, entity.Id);
              countNotChanged++;
            }
          }
          catch (Exception ex)
          {
            var errorMessage = string.Format("Error when processing Region with ExternalId:{0}. Description: {1}. StackTrace: {2}", isId, ex.Message, ex.StackTrace);
            Logger.Error(errorMessage);
            errorList.Add(errorMessage);
            countErrors++;
          }
        });
      }
      Logger.DebugFormat("R_DR_GET_REGIONS - Total: CountAll:{0} CountChanged:{1} CountNotChanged:{2} CountErrors:{3}", countAll, countChanged, countNotChanged, countErrors);
      
      Logger.Debug("R_DR_GET_REGIONS - Finish"); 
      return errorList;
    }
    
    /// <summary>
    /// Обработка справочника Населенные пункты.
    /// </summary>
    /// <param name="dataElements">Информация по населенным пунктам в виде XElement.</param>
    /// <returns>Список ошибок (List<string>)</returns>     
    public List<string> R_DR_GET_CITIES(System.Collections.Generic.IEnumerable<System.Xml.Linq.XElement> dataElements)
    {          
      Logger.Debug("R_DR_GET_CITIES - Start");
      var errorList = new List<string>();
      int countAll = dataElements.Count();
      int countChanged = 0;
      int countNotChanged = 0;
      int countErrors = 0;
      
      foreach (var element in dataElements)
      {       
        Transactions.Execute(() =>
        {
          var isId = element.Element("ID")?.Value;
          var isCountry = element.Element("Country")?.Value;
          var isRegion = element.Element("Region")?.Value;
          var isName = element.Element("Name")?.Value;
          var isType = element.Element("Type")?.Value;
          
          try
          {                        
            if (string.IsNullOrEmpty(isId) || string.IsNullOrEmpty(isName) || string.IsNullOrEmpty(isCountry))
              throw AppliedCodeException.Create(string.Format("Not all required fields are filled in. ID:{0}, Name:{1}, Country:{2}", isId, isName, isCountry));
            
            var entity = litiko.Eskhata.Cities.GetAll().Where(x => x.ExternalIdlitiko == isId).FirstOrDefault();
            if (entity != null)
              Logger.DebugFormat("City with ExternalId:{0} was found. Id:{1}, Name:{2}", isId, entity.Id, entity.Name);
            else
            {              
              entity = litiko.Eskhata.Cities.Create();
              entity.ExternalIdlitiko = isId;
              Logger.DebugFormat("Create new City with ExternalId:{0}. Id:{1}", isId, entity.Id);
            }             
            
            if (entity.Name != isName)
            {
              Logger.DebugFormat("Change Name: current:{0}, new:{1}", entity.Name, isName);
              entity.Name = isName;              
            }
            
            var country = litiko.Eskhata.Countries.GetAll().Where(x => x.ExternalIdlitiko == isCountry).FirstOrDefault();
            if (!Equals(entity.Country, country))
            {
              Logger.DebugFormat("Change Country: current:{0}, new:{1}", entity.Country?.Id, country?.Id);
              entity.Country = country;              
            }                                                            
            
            if (!string.IsNullOrEmpty(isRegion))
            {
              var region = litiko.Eskhata.Regions.GetAll().Where(x => x.ExternalIdlitiko == isRegion).FirstOrDefault();
              if (!Equals(entity.Region, region))
              {
                Logger.DebugFormat("Change Region: current:{0}, new:{1}", entity.Region?.Id, region?.Id);
                entity.Region = region;              
              }                        
            }

            if (!string.IsNullOrEmpty(isType) && entity.Typelitiko != isType)
            {
              Logger.DebugFormat("Change Type: current:{0}, new:{1}", entity.Typelitiko, isType);
              entity.Typelitiko = isType;
            }
            
            if (entity.State.IsInserted || entity.State.IsChanged)
            {
              entity.Save();                                          
              Logger.DebugFormat("City successfully saved. ExternalId:{0}, Id:{1}", isId, entity.Id);
              countChanged++;
            }
            else
            {
              Logger.DebugFormat("There are no changes in City. ExternalId:{0}, Id:{1}", isId, entity.Id);
              countNotChanged++;
            }
          }
          catch (Exception ex)
          {
            var errorMessage = string.Format("Error when processing City with ExternalId:{0}. Description: {1}. StackTrace: {2}", isId, ex.Message, ex.StackTrace);
            Logger.Error(errorMessage);
            errorList.Add(errorMessage);
            countErrors++;
          }
        });
      }
      Logger.DebugFormat("R_DR_GET_CITIES - Total: CountAll:{0} CountChanged:{1} CountNotChanged:{2} CountErrors:{3}", countAll, countChanged, countNotChanged, countErrors);
      
      Logger.Debug("R_DR_GET_CITIES - Finish"); 
      return errorList;
    }    
    
    [Remote]
    public List<string> R_DR_SET_CONTRACT_Online(IExchangeDocument exchDoc, litiko.Eskhata.IOfficialDocument document)
    {
      return R_DR_SET_CONTRACT(null, exchDoc, document);
    }
    
    /// <summary>
    /// Обработка договорного документа.
    /// </summary>
    /// <param name="dataElements">Информация о договорном документе в виде XElement.</param>
    /// <param name="exchDoc">Документ обмена (при online интеграции).</param>
    /// <param name="document">Документ (при online интеграции).</param>
    /// <returns>Список ошибок (List<string>)</returns>
    public List<string> R_DR_SET_CONTRACT(System.Collections.Generic.IEnumerable<System.Xml.Linq.XElement> dataElements,IExchangeDocument exchDoc, litiko.Eskhata.IOfficialDocument document)
    {          
      Logger.Debug("R_DR_SET_CONTRACT - Start");
      
      var errorList = new List<string>();      
          
        try
        {                        
          if (dataElements == null)
          {
            XDocument xmlDoc = XDocument.Load(exchDoc.LastVersion.Body.Read());                                 
            
            string state = xmlDoc.Root.Element("request")?.Element("stateId")?.Value;
            string stateMsg = xmlDoc.Root.Element("request")?.Element("stateMsg")?.Value;
            bool statusIS = state != "0";
            if (!statusIS)
              throw AppliedCodeException.Create($"State message from IS:{stateMsg}");         
            
            dataElements = xmlDoc.Descendants("Data").Elements();            
            if (!dataElements.Any())
              throw AppliedCodeException.Create("Empty Data node.");
          }
          
          var documentElement = dataElements.FirstOrDefault(x => x.Name == "Document");
          if (documentElement == null)
            throw AppliedCodeException.Create($"Document node is absent");
          
          var counterpartyElement = dataElements.FirstOrDefault(x => x.Name == "Counterparty");
          if (counterpartyElement == null)
            throw AppliedCodeException.Create($"Counterparty node is absent");              
            
          var isId = documentElement.Element("ID")?.Value;
          var isExternalD = documentElement.Element("ExternalD")?.Value;
          if (string.IsNullOrEmpty(isId) || string.IsNullOrEmpty(isExternalD))
            throw AppliedCodeException.Create($"Not all required fields are filled in. ID:{isId}, ExternalD:{isExternalD}");
          
          long docId;
          if (!System.Int64.TryParse(isId, out docId))
            throw AppliedCodeException.Create($"Invalid value in <Document><ID> node:{isId}");
          
          bool isForcedLocked = false;
          if (document == null)
          {
            document = litiko.Eskhata.OfficialDocuments.Get(docId);
            isForcedLocked = Locks.TryLock(document);
            if (!isForcedLocked)
              throw AppliedCodeException.Create(SendDocumentStages.Resources.DocumentIsLockedFormat(document.Id));
          }          
          else
          {
            if (document.Id != docId)
              throw AppliedCodeException.Create($"The document ID:{isId} does not match");
          }                                  
          
          try          
          {
            if (document.ExternalId != isExternalD)          
              document.ExternalId = isExternalD;
            
            if (!Equals(document.IntegrationStatuslitiko, litiko.Eskhata.OfficialDocument.IntegrationStatuslitiko.Success))
              document.IntegrationStatuslitiko = litiko.Eskhata.OfficialDocument.IntegrationStatuslitiko.Success;            
            
            if (document.State.IsChanged)
            {
              if (isForcedLocked)
                document.Save();
              
              Logger.Debug($"Document:{docId} updated successfully");
            }            
            else
              Logger.Debug($"There are no changes in document:{docId}");          
          }
          finally
          {
            if (isForcedLocked)
              Locks.Unlock(document);
          }                                          
          
          XElement counterpartyDataElement = null;
          if (counterpartyElement.Element("Company") != null)
            counterpartyDataElement = counterpartyElement.Element("Company");
          else if (counterpartyElement.Element("Person") != null)
            counterpartyDataElement = counterpartyElement.Element("Person");
          
          if (counterpartyDataElement == null)
            throw AppliedCodeException.Create("Counterparty node must have <Company> or <Person> node");
          
          var isConterpartyId = counterpartyDataElement.Element("ID")?.Value;
          var isConterpartyExternalId = counterpartyDataElement.Element("ExternalD")?.Value;
          long conterpartyId;
          if (!System.Int64.TryParse(isConterpartyId, out conterpartyId))
            throw AppliedCodeException.Create($"Invalid value in <Counterparty><ID> node:{isConterpartyId}");
                    
          var counterparty = litiko.Eskhata.Counterparties.Get(conterpartyId);          
          if (counterparty.ExternalId != isConterpartyExternalId)
          {
            isForcedLocked = Locks.TryLock(counterparty);
            try
            {
              counterparty.ExternalId = isExternalD;
              counterparty.Save();
              Logger.Debug($"Counterparty:{isConterpartyExternalId} updated successfully");                      
            }
            finally
            {
              if (isForcedLocked)
                Locks.Unlock(counterparty);
            }            
          }
          else
            Logger.Debug($"There are no changes in counterparty:{isConterpartyExternalId}");
                    
        }
        catch (Exception ex)
        {
          var errorMessage = string.Format("Error when processing Document. Description: {0}. StackTrace: {1}", ex.Message, ex.StackTrace);
          Logger.Error(errorMessage);
          errorList.Add(ex.Message);
        }      
      
      Logger.Debug("R_DR_SET_CONTRACT - Finish"); 
      return errorList;
    }      
    
    #endregion
    
    #region Вспомогательные функции

    /// <summary>
    /// Обработка должности.
    /// </summary>
    /// <param name="dataElements">Информация о должности в виде XElement.</param>
    /// <returns>Структура с должностью и признаком изменения<string>)</returns>
    public Structures.Module.ProcessingJobTittleResult ProcessingJobTittle(System.Xml.Linq.XElement jobTittleData)
    {
      var isJobTittleID = jobTittleData.Element("ID")?.Value;
      var isJobTittleNameRU = jobTittleData.Element("NameRU")?.Value;
      var isJobTittleNameTG = jobTittleData.Element("NameTG")?.Value;
      
      if (string.IsNullOrEmpty(isJobTittleID) || string.IsNullOrEmpty(isJobTittleNameRU))
        throw AppliedCodeException.Create(string.Format("Not all required fields are filled in JobTittle. ID:{0}, NameRU:{1}", isJobTittleID, isJobTittleNameRU));
      
      var jobTittle = Eskhata.JobTitles.GetAll().Where(x => x.ExternalId == isJobTittleID).FirstOrDefault();
      if (jobTittle == null)
      {
        jobTittle = Eskhata.JobTitles.Create();
        jobTittle.ExternalId = isJobTittleID;
        Logger.DebugFormat("Create new JobTittle with ExternalId:{0}. Id:{1}", isJobTittleID, jobTittle.Id);
      }
      else
        Logger.DebugFormat("JobTittle with ExternalId:{0} was found. Id:{1}, Name:{2}", isJobTittleID, jobTittle.Id, jobTittle.Name);
      
      if (jobTittle.Name != isJobTittleNameRU)
      {
        Logger.DebugFormat("Change JobTittle.Name: current:{0}, new:{1}", jobTittle.Name, isJobTittleNameRU);
        jobTittle.Name = isJobTittleNameRU;
      }
      
      /* отключено 25.02.2025 по просьбе Муниры
      if (jobTittle.NameTGlitiko != isJobTittleNameTG)
      {
        Logger.DebugFormat("Change JobTittle.NameTG: current:{0}, new:{1}", jobTittle.NameTGlitiko, isJobTittleNameTG);
        jobTittle.NameTGlitiko = isJobTittleNameTG;
      }
       */
      
      var result = Structures.Module.ProcessingJobTittleResult.Create(jobTittle, false);
      if (jobTittle.State.IsChanged || jobTittle.State.IsInserted)
      {
        jobTittle.Save();
        Logger.DebugFormat("JobTittle successfully saved. ExternalId:{0}, Id:{1}", isJobTittleID, jobTittle.Id);
        result.isCreatedOrUpdated = true;
      }
      else
      {
        Logger.DebugFormat("There are no changes in JobTittle. ExternalId:{0}, Id:{1}", isJobTittleID, jobTittle.Id);
        result.isCreatedOrUpdated = false;
      }
      
      return result;
    }
    
    /// <summary>
    /// Обработка персоны.
    /// </summary>
    /// <param name="dataElements">Информация о персоне в виде XElement.</param>
    /// <returns>Структура с персоной и признаком изменения<string>)</returns>
    public Structures.Module.ProcessingPersonResult ProcessingPerson(System.Xml.Linq.XElement personData, Structures.Module.FIOInfo fioInfo, litiko.Eskhata.IPerson person)
    {              
      const string dateFormat = "dd.MM.yyyy";
      
      var isID = personData.Element("ID")?.Value;
      var isName = personData.Element("NAME")?.Value;
      var isSex = personData.Element("SEX")?.Value;
      
      var isIName = personData.Element("I_NAME")?.Value;
      var isRezident = personData.Element("REZIDENT")?.Value;
      var isNuRezident = personData.Element("NU_REZIDENT")?.Value;
      var isDateOfBirth = personData.Element("DATE_PERS")?.Value;
      var isFamilyStatus = personData.Element("MARIGE_ST")?.Value;
      var isINN = personData.Element("INN")?.Value;
      //var isCodeOKONHelements = personData.Element("CODE_OKONH").Elements("element");
      //var isCodeOKVEDelements = personData.Element("CODE_OKVED").Elements("element");
      var isCountry = personData.Element("COUNTRY")?.Value;
      //var isRegion = personData.Element("Region")?.Value;
      var isCity = personData.Element("City")?.Value;      
      var isLegalAdress = personData.Element("DOC_BIRTH_PLACE")?.Value;
      var isPostAdress = personData.Element("PostAdress")?.Value;
      var isPhone = personData.Element("Phone")?.Value;
      var isEmail = personData.Element("Email")?.Value;
      var isWebSite = personData.Element("WebSite")?.Value;
      
      var isVATApplicable = personData.Element("VATApplicable")?.Value;
      var isIIN = personData.Element("IIN")?.Value;
      var isCorrAcc = personData.Element("CorrAcc")?.Value;
      var isInternalAcc = personData.Element("InternalAcc")?.Value;
      
      var isIdentityDocument = personData.Element("IdentityDocuments")?.Element("element");
          
      string isLastNameRu = string.Empty, isFirstNameRu = string.Empty, isMiddleNameRU = string.Empty;
      string isLastNameTG = string.Empty, isFirstNameTG = string.Empty, isMiddleNameTG = string.Empty;
      if (fioInfo != null)
      {
        isLastNameRu = fioInfo.LastNameRU;
        isFirstNameRu = fioInfo.FirstNameRU;
        isMiddleNameRU = fioInfo.MiddleNameRU;
        isLastNameTG = fioInfo.LastNameTG;
        isFirstNameTG = fioInfo.FirstNameTG;
        isMiddleNameTG = fioInfo.MiddleNameTG;
        if (string.IsNullOrEmpty(isLastNameRu) || string.IsNullOrEmpty(isFirstNameRu))
          throw AppliedCodeException.Create(string.Format("Not all required fields are filled in Person. ID:{0}, LastNameRU:{1}, FirstNameRU:{2}", isID, isLastNameRu, isFirstNameRu));
      }
      else
      {
        // Вычленить Фамилию, Имя, Отчество из NAME
        string[] nameParts = isName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (nameParts.Length == 2)
        {
          isLastNameRu = nameParts[0];
          isFirstNameRu = nameParts[1];
        }
        else if (nameParts.Length == 3)
        {
          isLastNameRu = nameParts[0];
          isFirstNameRu = nameParts[1];
          isMiddleNameRU = nameParts[2];
        }
        else
          throw AppliedCodeException.Create(string.Format("Impossible to extract last name and first name from a string:{0}", isName));
      }
      
      if (string.IsNullOrEmpty(isID) || string.IsNullOrEmpty(isName) || string.IsNullOrEmpty(isSex))
        throw AppliedCodeException.Create(string.Format("Not all required fields are filled in Person. ID:{0}, NAME:{1}, SEX:{2}", isID, isName, isSex));
      
      bool needSave = true;
      if (person == null)
      {
        person = Eskhata.People.GetAll().Where(x => x.ExternalId == isID).FirstOrDefault();
        if (person == null)
        {
          person = Eskhata.People.Create();
          person.ExternalId = isID;
          Logger.DebugFormat("Create new Person with ExternalId:{0}. Id:{1}", isID, person.Id);
        }
        else
          Logger.DebugFormat("Person with ExternalId:{0} was found. Id:{1}, Name:{2}", isID, person.Id, person.Name);
      }
      else
        needSave = false;

      if (person.ExternalId != isID)
      {
        Logger.DebugFormat("Change ExternalId: current:{0}, new:{1}", person.ExternalId, isID);
        person.ExternalId = isID;        
      }
      
      if (!string.IsNullOrEmpty(isLastNameRu) && person.LastName != isLastNameRu)
      {
        Logger.DebugFormat("Change LastName: current:{0}, new:{1}", person.LastName, isLastNameRu);
        person.LastName = isLastNameRu;        
      }
      
      if (!string.IsNullOrEmpty(isFirstNameRu) && person.FirstName != isFirstNameRu)
      {
        Logger.DebugFormat("Change FirstName: current:{0}, new:{1}", person.FirstName, isFirstNameRu);
        person.FirstName = isFirstNameRu;         
      } 

      if (!string.IsNullOrEmpty(isMiddleNameRU) && person.MiddleName != isMiddleNameRU)
      {
        Logger.DebugFormat("Change MiddleName: current:{0}, new:{1}", person.MiddleName, isMiddleNameRU);
        person.MiddleName = isMiddleNameRU;          
      }       

      if (!string.IsNullOrEmpty(isLastNameTG) && person.LastNameTGlitiko != isLastNameTG)
      {
        Logger.DebugFormat("Change LastNameTGlitiko: current:{0}, new:{1}", person.LastNameTGlitiko, isLastNameTG);
        person.LastNameTGlitiko = isLastNameTG;        
      }

      if (!string.IsNullOrEmpty(isFirstNameTG) && person.FirstNameTGlitiko != isFirstNameTG)
      {
        Logger.DebugFormat("Change FirstNameTGlitiko: current:{0}, new:{1}", person.FirstNameTGlitiko, isFirstNameTG);
        person.FirstNameTGlitiko = isFirstNameTG;          
      } 

      if (!string.IsNullOrEmpty(isMiddleNameTG) && person.MiddleNameTGlitiko != isMiddleNameTG)
      {
        Logger.DebugFormat("Change MiddleNameTGlitiko: current:{0}, new:{1}", person.MiddleNameTGlitiko, isMiddleNameTG);
        person.MiddleNameTGlitiko = isMiddleNameTG;          
      }      
      
      if(isSex == "М" && !Equals(person.Sex, Eskhata.Person.Sex.Male))
      {
        Logger.DebugFormat("Change Sex: current:{0}, new:{1}", person.Info.Properties.Sex.GetLocalizedValue(person.Sex), person.Info.Properties.Sex.GetLocalizedValue(Eskhata.Person.Sex.Male));
        person.Sex = Eskhata.Person.Sex.Male;                  
      }
      else if (isSex == "Ж" && !Equals(person.Sex, Eskhata.Person.Sex.Female))
      {
        Logger.DebugFormat("Change Sex: current:{0}, new:{1}", person.Info.Properties.Sex.GetLocalizedValue(person.Sex), person.Info.Properties.Sex.GetLocalizedValue(Eskhata.Person.Sex.Female));
        person.Sex = Eskhata.Person.Sex.Female;
      }
      
      if(!string.IsNullOrEmpty(isIName) && person.Inamelitiko != isIName)
      {
        Logger.DebugFormat("Change Inamelitiko: current:{0}, new:{1}", person.Inamelitiko, isIName);
        person.Inamelitiko = isIName;                        
      }   

      if(!string.IsNullOrEmpty(isNuRezident))
      {
        bool isNuRezidentBool = isNuRezident == "1" ? true : false;
        if(person.NUNonrezidentlitiko != !isNuRezidentBool)
        {
          Logger.DebugFormat("Change NUNonrezidentlitiko: current:{0}, new:{1}", person.NUNonrezidentlitiko, !isNuRezidentBool);
          person.NUNonrezidentlitiko = !isNuRezidentBool;           
        }               
      }            
            
      if(!string.IsNullOrEmpty(isRezident))
      {
        bool isRezidentBool = isRezident == "1" ? true : false;
        if(person.Nonresident != !isRezidentBool)
        {
          Logger.DebugFormat("Change Nonresident: current:{0}, new:{1}", person.Nonresident, !isRezidentBool);
          person.Nonresident = !isRezidentBool;           
        }               
      }
      
      if(!string.IsNullOrEmpty(isDateOfBirth))
      {
        var dateOfBirth = DateTime.Parse(isDateOfBirth);
        if (!Equals(person.DateOfBirth, dateOfBirth))
        {
          var curDate = person.DateOfBirth.HasValue ? person.DateOfBirth.Value.ToString("dd.MM.yyyy") : string.Empty;
          Logger.DebugFormat("Change DateOfBirth: current:{0}, new:{1}", curDate, dateOfBirth.ToString("dd.MM.yyyy"));
          person.DateOfBirth = dateOfBirth;         
        }
      }
      
      if (!string.IsNullOrEmpty(isFamilyStatus))
      {
        var familyStatus = NSI.FamilyStatuses.GetAll().Where(x => x.ExternalId == isFamilyStatus).FirstOrDefault();
        if (familyStatus != null && !Equals(person.FamilyStatuslitiko, familyStatus))
        {
          Logger.DebugFormat("Change FamilyStatuslitiko: current:{0}, new:{1}", person.FamilyStatuslitiko?.Name, familyStatus?.Name);
          person.FamilyStatuslitiko = familyStatus;                  
        }
      }
      
      if(!string.IsNullOrEmpty(isINN) && person.TIN != isINN)
      {
        Logger.DebugFormat("Change TIN: current:{0}, new:{1}", person.TIN, isINN);
        person.TIN = isINN;                        
      }      
      
      /*      
      if(isCodeOKONHelements.Any())
      {
        var elementValues = isCodeOKONHelements.Select(x => x.Value).ToList();
        if(person.OKONHlitiko.Select(x => x.OKONH.ExternalId).Any(x => !elementValues.Contains(x)))
        {
          person.OKONHlitiko.Clear();
          Logger.DebugFormat("Change OKONHlitiko: Clear");
        }
        
        foreach (var isCodeOKONH in isCodeOKONHelements)
        {
          var okonh = litiko.NSI.OKONHs.GetAll().Where(x => x.ExternalId == isCodeOKONH.Value).FirstOrDefault();
          if (okonh != null && !person.OKONHlitiko.Any(x => Equals(x.OKONH, okonh)))
          {
            var newRecord = person.OKONHlitiko.AddNew();
            newRecord.OKONH = okonh;
            Logger.DebugFormat("Change OKONHlitiko: added:{0}", okonh.Name);
          }
        }
      }
      
      if(isCodeOKVEDelements.Any())
      {
        var elementValues = isCodeOKVEDelements.Select(x => x.Value).ToList();
        if(person.OKVEDlitiko.Select(x => x.OKVED.ExternalId).Any(x => !elementValues.Contains(x)))
        {
          person.OKVEDlitiko.Clear();
          Logger.DebugFormat("Change OKVEDlitiko: Clear");
        }
        
        foreach (var isCodeOKVED in isCodeOKVEDelements)
        {
          var okved = litiko.NSI.OKVEDs.GetAll().Where(x => x.ExternalId == isCodeOKVED.Value).FirstOrDefault();
          if(okved != null && !person.OKVEDlitiko.Any(x => Equals(x.OKVED, okved)))
          {
            var newRecord = person.OKVEDlitiko.AddNew();
            newRecord.OKVED = okved;
            Logger.DebugFormat("Change OKVEDlitiko: added:{0}", okved.Name);
          }
        }              
      }      
      */
      
      if (!string.IsNullOrEmpty(isCountry))
      {
        var country = Eskhata.Countries.GetAll().Where(x => x.ExternalIdlitiko == isCountry).FirstOrDefault();
        if (country != null && !Equals(person.Citizenship, country))
        {
          Logger.DebugFormat("Change Citizenship: current:{0}, new:{1}", person.Citizenship?.Name, country?.Name);
          person.Citizenship = country;                    
        }
      }

      /*
      if (!string.IsNullOrEmpty(isRegion))
      {
        var region = Eskhata.Regions.GetAll().Where(x => x.ExternalIdlitiko == isRegion).FirstOrDefault();
        if (region != null && !Equals(person.Region, region))
        {
          Logger.DebugFormat("Change Region: current:{0}, new:{1}", person.Region?.Name, region?.Name);
          person.Region = region;                    
        }
      }
*/

      if (!string.IsNullOrEmpty(isCity))
      {
        var city = Eskhata.Cities.GetAll().Where(x => x.ExternalIdlitiko == isCity).FirstOrDefault();
        if (city != null && !Equals(person.City, city))
        {
          Logger.DebugFormat("Change City: current:{0}, new:{1}", person.City?.Name, city?.Name);
          person.City = city;                    
        }
      }      
      
      if(!string.IsNullOrEmpty(isPostAdress) && person.PostalAddress != isPostAdress)
      {
        Logger.DebugFormat("Change PostalAddress: current:{0}, new:{1}", person.PostalAddress, isPostAdress);
        person.PostalAddress = isPostAdress;                        
      }

      if(!string.IsNullOrEmpty(isLegalAdress) && person.LegalAddress != isLegalAdress)
      {
        Logger.DebugFormat("Change LegalAddress: current:{0}, new:{1}", person.LegalAddress, isLegalAdress);
        person.LegalAddress = isLegalAdress;                        
      }

      if(!string.IsNullOrEmpty(isPhone) && person.Phones != isPhone)
      {
        Logger.DebugFormat("Change Phones: current:{0}, new:{1}", person.Phones, isPhone);
        person.Phones = isPhone;                       
      }

      if(!string.IsNullOrEmpty(isEmail) && person.Email != isEmail)
      {
        Logger.DebugFormat("Change Email: current:{0}, new:{1}", person.Email, isEmail);
        person.Email = isEmail;                       
      }

      if(!string.IsNullOrEmpty(isWebSite) && person.Homepage != isWebSite)
      {
        Logger.DebugFormat("Change Homepage: current:{0}, new:{1}", person.Homepage, isWebSite);
        person.Homepage = isWebSite;                        
      }
      
      if(!string.IsNullOrEmpty(isVATApplicable))
      {
        bool VATPayer = isVATApplicable == "1" ? true : false;
        if(person.VATPayerlitiko != VATPayer)
        {
          Logger.DebugFormat("Change VATPayerlitiko: current:{0}, new:{1}", person.VATPayerlitiko.GetValueOrDefault(), VATPayer);
          person.VATPayerlitiko = VATPayer;           
        }               
      }            

      if(!string.IsNullOrEmpty(isIIN) && person.SINlitiko != isIIN)
      {
        Logger.DebugFormat("Change SINlitiko: current:{0}, new:{1}", person.SINlitiko, isIIN);
        person.SINlitiko = isIIN;       
      }

      if(!string.IsNullOrEmpty(isCorrAcc) && person.Account != isCorrAcc)
      {
        Logger.DebugFormat("Change Account: current:{0}, new:{1}", person.Account, isCorrAcc);
        person.Account = isCorrAcc;                        
      } 

      if(!string.IsNullOrEmpty(isInternalAcc) && person.AccountEskhatalitiko != isInternalAcc)
      {
        Logger.DebugFormat("Change AccountEskhatalitiko: current:{0}, new:{1}", person.AccountEskhatalitiko, isInternalAcc);
        person.AccountEskhatalitiko = isInternalAcc;                        
      } 
      
      /* !!! IdentityDocuments !!! */
      if (isIdentityDocument != null)
      {
        var id = isIdentityDocument.Element("ID")?.Value;        
        if (!string.IsNullOrEmpty(id))
        {          
          var identityDocument = Sungero.Parties.IdentityDocumentKinds.GetAll().Where(x => x.SID == id).FirstOrDefault();
          if (identityDocument != null)
          {
            Logger.DebugFormat("IdentityDocument with SID:{0} was found. Id:{1}, Name:{2}", id, identityDocument.Id, identityDocument.Name);
            var isDateBegin = isIdentityDocument.Element("DATE_BEGIN")?.Value;
            var isDateEnd = isIdentityDocument.Element("DATE_END")?.Value;
            var isNum = isIdentityDocument.Element("NUM")?.Value;
            var isSer = isIdentityDocument.Element("SER")?.Value;
            var isWho = isIdentityDocument.Element("WHO")?.Value;
            
            if (!Equals(person.IdentityKind, identityDocument))
            {
              Logger.DebugFormat("Change IdentityKind: current:{0}, new:{1}", person.IdentityKind?.Name, identityDocument?.Name);
              person.IdentityKind = identityDocument;                           
            }
            
            DateTime dateBegin;
            if (!string.IsNullOrEmpty(isDateBegin) && Calendar.TryParseDate(isDateBegin, out dateBegin) && !Equals(person.IdentityDateOfIssue, dateBegin))
            {
              Logger.DebugFormat("Change IdentityDateOfIssue: current:{0}, new:{1}", person.IdentityDateOfIssue?.ToString(dateFormat), dateBegin.ToString(dateFormat));
              person.IdentityDateOfIssue = dateBegin;              
            }
            
            DateTime dateEnd;
            if (!string.IsNullOrEmpty(isDateEnd) && Calendar.TryParseDate(isDateEnd, out dateEnd) && !Equals(person.IdentityExpirationDate, dateEnd))
            {
              Logger.DebugFormat("Change IdentityExpirationDate: current:{0}, new:{1}", person.IdentityExpirationDate?.ToString(dateFormat), dateEnd.ToString(dateFormat));
              person.IdentityExpirationDate = dateEnd;              
            }

            if (!string.IsNullOrEmpty(isNum) && person.IdentityNumber != isNum)
            {
              Logger.DebugFormat("Change IdentityNumber: current:{0}, new:{1}", person.IdentityNumber, isNum);
              person.IdentityNumber = isNum;             
            }
            
            if (!string.IsNullOrEmpty(isSer) && person.IdentitySeries != isSer)
            {
              Logger.DebugFormat("Change IdentitySeries: current:{0}, new:{1}", person.IdentitySeries, isSer);
              person.IdentitySeries = isSer;              
            }            
            
            if (!string.IsNullOrEmpty(isWho) && person.IdentityAuthority != isWho)
            {
              Logger.DebugFormat("Change IdentityAuthority: current:{0}, new:{1}", person.IdentityAuthority, isWho);
              person.IdentityAuthority = isWho;              
            }             
          }
          else
            Logger.ErrorFormat("IdentityDocument with SID:{0} not found.", id);
        }        
      }
      
      var result = Structures.Module.ProcessingPersonResult.Create(person, false);
      if (person.State.IsChanged || person.State.IsInserted)
      {
        if (needSave)
        {
          person.Save();
          Logger.DebugFormat("Person successfully saved. ExternalId:{0}, Id:{1}", isID, person.Id);
        }
        else
          Logger.DebugFormat("Person successfully changed, but not saved. The user can save the changes independently. ExternalId:{0}, Id:{1}", isID, person.Id);
        
        result.isCreatedOrUpdated = true;
      }
      else
      {
        Logger.DebugFormat("There are no changes in Person. ExternalId:{0}, Id:{1}", isID, person.Id);
        result.isCreatedOrUpdated = false;
      }
      
      return result;
    }

    /// <summary>
    /// Создать документ обмена.
    /// </summary>
    /// <returns>Созданный документ обмена.</returns>
    [Public, Remote]
    public IExchangeDocument CreateExchangeDocument()
    {
      return ExchangeDocuments.Create();
    }

    /// <summary>
    /// Получить версию.
    /// </summary>
    /// <returns>Версия.</returns>
    [Public, Remote]
    public Sungero.Content.IElectronicDocumentVersions GetVersion(long exchDocId)
    {
      // var versionFullXML = exchDoc.Versions.Where(v => v.Note == Integration.Resources.VersionRequestToRXFull && v.AssociatedApplication.Extension == "xml" && v.Body.Size > 0).FirstOrDefault();
      var exchDoc = litiko.Integration.ExchangeDocuments.Get(exchDocId);
      if (exchDoc != null)
        return exchDoc.Versions.Where(v => v.Note == Integration.Resources.VersionRequestToRXFull && v.AssociatedApplication.Extension == "xml" && v.Body.Size > 0).FirstOrDefault();
      else
        return null;
    }
    
    public bool IsValidXml(string xml)
    {
      try
      {
        XElement.Parse(xml);
        return true;
      }
      catch
      {
        return false;
      }
    }

    /// <summary>
    /// Получить записи очереди обмена по документу
    /// </summary>
    [Remote]
    public IQueryable<IExchangeQueue> GetExchangeQueueByDoc(IExchangeDocument document)
    {
      return ExchangeQueues.GetAll()
        .Where(x => Equals(x.ExchangeDocument, document));
    }

    /// <summary>
    /// Получить метод интеграции по сущности
    /// </summary>
    /// <param name="entity">Сущность</param>
    [Remote(IsPure = true)]
    public Integration.IIntegrationMethod GetIntegrationMethod(Sungero.Domain.Shared.IEntity entity)
    {
      if (entity == null)
        return null;
            
      var integrationMethod = Integration.IntegrationMethods.Null;
      
      var integrationMethodName = string.Empty;
      if (Eskhata.Companies.Is(entity))
        integrationMethodName = PublicConstants.Module.IntegrationMethods.R_DR_GET_COMPANY;
      else if (Eskhata.Banks.Is(entity))
        integrationMethodName = PublicConstants.Module.IntegrationMethods.R_DR_GET_BANK;
      else if (Eskhata.People.Is(entity))
        integrationMethodName = PublicConstants.Module.IntegrationMethods.R_DR_GET_PERSON;
      else if (Eskhata.Contracts.Is(entity))
        integrationMethodName = PublicConstants.Module.IntegrationMethods.R_DR_SET_CONTRACT;
      else if (Eskhata.SupAgreements.Is(entity))
        integrationMethodName = PublicConstants.Module.IntegrationMethods.R_DR_SET_PAYMENT_DOCUMENT;
      
      if (!string.IsNullOrEmpty(integrationMethodName))
        integrationMethod = Integration.IntegrationMethods.GetAll().FirstOrDefault(x => x.Name == integrationMethodName);
      
      return integrationMethod;
    }    
       
    #endregion
    
    #region Экспорт договоров для интеграции
    
    /// <summary>
    /// Преобразует nullable булевое значение в строковое представление: "true", "false" или "null".
    /// Используется для корректного формирования XML-элементов, где нужно указать значение флага.
    /// </summary>
    /// <param name="value">nullable булевое значение</param>
    /// <returns>строка "true", "false" или "null"</returns>
    private string ToYesNoNull(bool? value)
    {
      if (!value.HasValue)
        return "null";
      return value.Value ? "true" : "false";
    }

    /// <summary>
    /// Формирует XML-структуру <Person> для контрагента-физического лица.
    /// </summary>
    private XElement BuildPersonXml(Sungero.Parties.ICounterparty counterparty)
    {
        const string dateFormat = "dd.MM.yyyy";  
        
        var person = litiko.Eskhata.People.As(counterparty);
        if (person == null)
            return new XElement("Person");
    
        // ==========================
        // Инициализация переменных
        // ==========================
        var id              = person.Id;
        var externalId      = person.ExternalId ?? "";
        var lastName        = person.LastName ?? "";
        var firstName       = person.FirstName ?? "";
        var middleName      = person.MiddleName ?? "";
        var rezident        = ToYesNoNull(!person.Nonresident);
        var nuRezident      = ToYesNoNull(!person.NUNonrezidentlitiko);
        //var iName           = person.Inamelitiko ?? "";
        var datePers        = person.DateOfBirth?.ToString("dd.MM.yyyy") ?? "";
        var sex             = person.Sex.HasValue ? person.Info.Properties.Sex.GetLocalizedValue(person.Sex.Value) : "";
        var marigeSt        = person.FamilyStatuslitiko?.ExternalId ?? "";
        var inn             = person.TIN ?? "";
        var iin             = person.SINlitiko?.ToString() ?? "";
        var country         = litiko.Eskhata.Countries.As(person.Citizenship)?.ExternalIdlitiko ?? "";
        var docBirthPlace   = person.BirthPlace ?? "";
        var addressType     = person.AddressTypelitiko?.ExternalId ?? "";
        var postAddress     = person.PostalAddress ?? "";
        var email           = person.Email ?? "";
        var phone           = person.Phones ?? "";
        var city            = Eskhata.Cities.As(person.City)?.ExternalIdlitiko ?? "";
        var street          = person.Streetlitiko ?? "";
        var buildingNumber  = person.HouseNumberlitiko ?? "";
        var website         = person.Homepage ?? "";
        //var taxNonResident  = ToYesNoNull(person.NUNonrezidentlitiko);
        //var vatPayer        = ToYesNoNull(person.VATPayerlitiko);
        //var reliability     = person.Reliabilitylitiko.HasValue ? person.Info.Properties.Reliabilitylitiko.GetLocalizedValue(person.Reliabilitylitiko.Value) : ""; 
        var corrAcc         = person.Account ?? "";
        var bank            = person.Bank?.ExternalId ?? "";
        var internalAcc     = person.AccountEskhatalitiko ?? "";
    
        var idDocId         = person.IdentityKind?.SID ?? "";        
        var idDocName       = person.IdentityKind?.Name; 
        var idDocBegin      = person.IdentityDateOfIssue?.ToString(dateFormat) ?? "";
        var idDocEnd        = person.IdentityExpirationDate?.ToString(dateFormat) ?? "";
        var idDocNum        = person.IdentityNumber ?? "";
        var idDocSer        = person.IdentitySeries ?? "";
        var idDocWho        = person.IdentityAuthority ?? "";    
    
        // ==========================
        // Формирование XML
        // ==========================
        return new XElement("Person",
            new XElement("ID", id),
            new XElement("ExternalID", externalId),
            new XElement("LastName", lastName),
            new XElement("FirstName", firstName),
            new XElement("MiddleName", middleName),
            new XElement("REZIDENT", rezident),
            new XElement("NU_REZIDENT", nuRezident),
            //new XElement("I_NAME", iName),
            new XElement("DATE_PERS", datePers),
            new XElement("SEX", sex),
            new XElement("MARIGE_ST", marigeSt),
            new XElement("INN", inn),
            new XElement("IIN", iin),
            new XElement("COUNTRY", country),
            new XElement("DOC_BIRTH_PLACE", docBirthPlace),
            new XElement("AdressType", addressType),
            new XElement("PostAdress", postAddress),
            new XElement("Email", email),
            new XElement("Phone", phone),
            new XElement("City", city),
            new XElement("Street", street),
            new XElement("BuildingNumber", buildingNumber),
            new XElement("WebSite", website),
            //new XElement("TaxNonResident", taxNonResident),
            //new XElement("VATPayer", vatPayer),
            //new XElement("Reliability", reliability),
            new XElement("CorrAcc", corrAcc),
            new XElement("Bank", bank),
            new XElement("InternalAcc", internalAcc),
            new XElement("IdentityDocuments",
                new XElement("element",
                    new XElement("ID", idDocId),
                    //new XElement("TYPE", idDocId),
                    new XElement("NAME", idDocName),
                    new XElement("DATE_BEGIN", idDocBegin),
                    new XElement("DATE_END", idDocEnd),
                    new XElement("NUM", idDocNum),
                    new XElement("SER", idDocSer),
                    new XElement("WHO", idDocWho)
                )
            )
        );
    }

    /// <summary>
    /// Формирует XML-структуру <Company> для контрагента-юридического лица.
    /// </summary>    
    private XElement BuildCompanyXml(Sungero.Parties.ICounterparty counterparty)
    {
        var company = litiko.Eskhata.Companies.As(counterparty);
        if (company == null)
            return new XElement("Company");
    
        // ==========================
        // Инициализация переменных
        // ==========================
        var id              = company.Id.ToString();
        var externalId      = company.ExternalId ?? "";
        var name            = company.Name ?? "";
        var longName        = company.LegalName ?? "";
        var iName           = company.Inamelitiko ?? "";
        var rezident        = ToYesNoNull(!company.Nonresident);
        var nuRezident      = ToYesNoNull(!company.NUNonrezidentlitiko);
        var inn             = company.TIN ?? "";
        var kpp             = company.TRRC ?? "";
        var kodOkpo         = company.NCEO ?? "";
        var forma           = company.OKOPFlitiko?.ExternalId ?? "";
        var ownership       = company.OKFSlitiko?.ExternalId ?? "";
        //var iin             = company.SINlitiko?.ToString() ?? "";
        var registNum       = company.RegNumlitiko ?? "";
        var numbers         = company.Numberslitiko?.ToString() ?? "";
        var business        = company.Businesslitiko ?? "";
        var psRef           = company.EnterpriseTypelitiko?.ExternalId ?? "";
        var country         = company.Countrylitiko?.ExternalIdlitiko ?? "";
        //var postAddress     = company.PostalAddress ?? "";
        var addressType     = company.AddressTypelitiko?.ExternalId ?? "";
        var legalAddress    = company.LegalAddress ?? "";
        var phone           = company.Phones ?? "";
        var city            = Eskhata.Cities.As(company.City)?.ExternalIdlitiko ?? "";
        var street          = company.Streetlitiko ?? "";
        var buildingNumber  = company.HouseNumberlitiko ?? "";
        var email           = company.Email ?? "";
        var website         = company.Homepage ?? "";
        //var taxNonResident  = ToYesNoNull(company.NUNonrezidentlitiko);
        var vatPayer        = ToYesNoNull(company.VATPayerlitiko);
        //var reliability     = company.Reliabilitylitiko.HasValue ? company.Info.Properties.Reliabilitylitiko.GetLocalizedValue(company.Reliabilitylitiko.Value) : "";          
        var corrAcc         = company.Account ?? "";
        var bank            = company.Bank?.ExternalId ?? "";
        var internalAcc     = company.AccountEskhatalitiko ?? "";
        var code_OKONH      = company.OKONHlitiko?.ExternalId ?? "";
        var code_OKVED      = company.OKVEDlitiko?.ExternalId ?? "";        
        var ein             = company.EINlitiko ?? "";        
    
        // ==========================
        // Формирование XML
        // ==========================
        return new XElement("Company",
            new XElement("ID", id),
            new XElement("ExternalD", externalId),
            new XElement("Name", name),
            new XElement("LONG_NAME", longName),
            new XElement("I_NAME", iName),
            new XElement("REZIDENT", rezident),
            new XElement("NU_REZIDENT", nuRezident),
            new XElement("INN", inn),
            new XElement("KPP", kpp),
            new XElement("KOD_OKPO", kodOkpo),
            new XElement("FORMA", forma),
            new XElement("OWNERSHIP", ownership),
            new XElement("CODE_OKONH", code_OKONH),
            new XElement("CODE_OKVED", code_OKVED),
            //new XElement("IIN", iin),
            new XElement("REGIST_NUM", registNum),
            new XElement("NUMBERS", numbers),
            new XElement("BUSINESS", business),
            new XElement("PS_REF", psRef),
            new XElement("COUNTRY", country),
            //new XElement("PostAdress", postAddress),
            new XElement("AdressType", addressType),
            new XElement("LegalAdress", legalAddress),
            new XElement("Phone", phone),
            new XElement("City", city),
            new XElement("Street", street),
            new XElement("BuildingNumber", buildingNumber),
            new XElement("Email", email),
            new XElement("WebSite", website),
            //new XElement("TaxNonResident", taxNonResident),
            new XElement("VATPayer", vatPayer),
            //new XElement("Reliability", reliability),
            new XElement("CorrAcc", corrAcc),
            new XElement("Bank", bank),
            new XElement("InternalAcc", internalAcc),
            new XElement("EIN", ein)
        );
    }
    
    /// <summary>
    /// Формирует XML-структуру <Data> для документа типа "Дополнительное соглашение" (SupAgreement).
    /// Включает сведения о документе, без информации о контрагенте (Company/Person).
    /// </summary>
    /// <param name="contractualDocument">Документ SupAgreement</param>
    /// <returns>Элемент XElement с полной информацией о документе</returns>
    private XElement BuildXmlForSupAgreement(litiko.Eskhata.ISupAgreement contractualDocument)
    {
        if (contractualDocument == null)
            return new XElement("Data");
    
        const string dateFormat = "dd.MM.yyyy";
    
        // ==========================
        // Document values
        // ==========================
        var documentId        = contractualDocument.Id.ToString();
        var externalId        = contractualDocument.ExternalId ?? "";
        var contractId        = contractualDocument.LeadingDocument?.Id.ToString() ?? ""; 
        var contractExtId     = contractualDocument.LeadingDocument?.ExternalId ?? "";   
        var documentKind      = litiko.Eskhata.DocumentKinds.As(contractualDocument.DocumentKind)?.ExternalIdlitiko ?? "";
        var subject           = contractualDocument.Subject ?? "";
        var name              = (contractualDocument.Name ?? "").Substring(0, Math.Min((contractualDocument.Name ?? "").Length, 100));
        var registrationNumber= contractualDocument.RegistrationNumber ?? "";
        var registrationDate  = contractualDocument.RegistrationDate?.ToString(dateFormat) ?? "";
        var validFrom         = contractualDocument.ValidFrom?.ToString(dateFormat) ?? "";
        var validTill         = contractualDocument.ValidTill?.ToString(dateFormat) ?? "";
        var totalAmount       = contractualDocument.TotalAmountlitiko?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "";
        var currency          = contractualDocument.CurrencyContractlitiko?.AlphaCode ?? "";
        var operationCurrency = contractualDocument.CurrencyOperationlitiko?.AlphaCode ?? "";
        var currencyRate      = contractualDocument.CurrencyRatelitiko?.Rate is double r
          ? r.ToString(System.Globalization.CultureInfo.InvariantCulture)
          : "";
        var vatApplicable     = ToYesNoNull(contractualDocument.IsVATlitiko);
        var vatRate           = contractualDocument.VatRatelitiko?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "";
        var vatAmount         = contractualDocument.VatAmount?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "";
        var incomeTaxRate     = contractualDocument.IncomeTaxRatelitiko?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "";
        var incomeTaxAmount   = contractualDocument.IncomeTaxAmountlitiko?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? ""; 
        var laborPayment      = ToYesNoNull(contractualDocument.IsIndividualPaymentlitiko); 
        var note              = contractualDocument.Note ?? "Без примечания";
        var isWithinBudget    = ToYesNoNull(contractualDocument.IsWithinBudgetlitiko);
    
        // ==========================
        // Формирование XML
        // ==========================
        var documentElement = new XElement("Document",
            new XElement("ID", documentId),
            new XElement("ExternalD", externalId),
            new XElement("Contract",
                new XElement("ID", contractId),
                new XElement("ExternalD", contractExtId)
            ),
            new XElement("DocumentKind", documentKind),
            new XElement("Subject", subject),
            new XElement("Name", name),
            new XElement("IsWithinBudget", isWithinBudget),
            new XElement("RegistrationNumber", registrationNumber),
            new XElement("RegistrationDate", registrationDate),
            new XElement("ValidFrom", validFrom),
            new XElement("ValidTill", validTill),
            new XElement("TotalAmount", totalAmount),
            new XElement("Currency", currency),
            new XElement("OperationCurrency", operationCurrency),
            new XElement("CurrencyRate", currencyRate),
            new XElement("VATApplicable", vatApplicable),
            new XElement("VATRate", vatRate),
            new XElement("VATAmount", vatAmount),
            new XElement("IncomeTaxRate", incomeTaxRate),
            new XElement("IncomeTaxAmount", incomeTaxAmount),
            new XElement("LaborPayment", laborPayment),
            new XElement("Note", note)
        );
    
        return new XElement("Data", documentElement);
    }
    
    /// <summary>
    /// Формирует XML-структуру <Data> для документа типа "Договор".
    /// Включает в себя сведения о документе, компании и/или физическом лице.
    /// </summary>
    private XElement BuildXmlForContract(litiko.Eskhata.IContract contractualDocument)
    {
        const string dateFormat = "dd.MM.yyyy";
        
        // ==========================
        // Document values
        // ==========================
        var rbo              = contractualDocument.RBOlitiko ?? "";
        var accDebtCredit    = contractualDocument.AccDebtCreditlitiko ?? "";
        var accFutureExpense = contractualDocument.AccFutureExpenselitiko ?? "";
        var paymentRegion    = contractualDocument.PaymentRegionlitiko?.ExternalId ?? "";
        var paymentTaxRegion = contractualDocument.RegionOfRentallitiko?.ExternalId ?? "";        
        var paymentMethod    = contractualDocument.PaymentMethodlitiko.HasValue ? contractualDocument.Info.Properties.PaymentMethodlitiko.GetLocalizedValue(contractualDocument.PaymentMethodlitiko.Value) : "";
        var paymentFrequency = contractualDocument.FrequencyOfPaymentlitiko?.Name ?? "";
    
        var matrix = NSI.PublicFunctions.Module.GetResponsibilityMatrix(contractualDocument);
        var responsibleAccountant =
            litiko.Eskhata.Employees.As(matrix?.ResponsibleAccountant)            
            ?? Roles.As(matrix?.ResponsibleAccountant)?
                   .RecipientLinks
                   .Select(l => litiko.Eskhata.Employees.As(l.Member))
                   .FirstOrDefault(e => e != null);
    
        var responsibleAccountantId = litiko.Eskhata.Employees.As(responsibleAccountant)?.ExternalId ?? string.Empty;
        //var responsibleDepartmentId = litiko.Eskhata.Employees.As(responsibleAccountant)?.Department?.ExternalId ?? string.Empty;
        var batchProcessing = ToYesNoNull(matrix?.BatchProcessing);
    
        // PaymentBasis
        var matrix2 = NSI.PublicFunctions.Module.GetContractsVsPaymentDoc(contractualDocument, contractualDocument.Counterparty);
    
        var isPaymentContract   = ToYesNoNull(matrix2?.PBIsPaymentContract);
        var isPaymentInvoice    = ToYesNoNull(matrix2?.PBIsPaymentInvoice);
        var isPaymentTaxInvoice = ToYesNoNull(matrix2?.PBIsPaymentTaxInvoice);
        var isPaymentAct        = ToYesNoNull(matrix2?.PBIsPaymentAct);
        var isPaymentOrder      = ToYesNoNull(matrix2?.PBIsPaymentOrder);
    
        var isClosureContract   = ToYesNoNull(matrix2?.PCBIsPaymentContract);
        var isClosureInvoice    = ToYesNoNull(matrix2?.PCBIsPaymentInvoice);
        var isClosureTaxInvoice = ToYesNoNull(matrix2?.PCBIsPaymentTaxInvoice);
        var isClosureAct        = ToYesNoNull(matrix2?.PCBIsPaymentAct);
        var isClosureWaybill    = ToYesNoNull(matrix2?.PCBIsPaymentWaybill);                
        
        // ==========================
        // Document XElement
        // ==========================
        var documentId          = contractualDocument.Id.ToString();
        var externalId          = contractualDocument.ExternalId ?? "";
        var documentKind        = litiko.Eskhata.DocumentKinds.As(contractualDocument.DocumentKind)?.ExternalIdlitiko ?? "";
        var documentGroup       = litiko.Eskhata.DocumentGroupBases.As(contractualDocument.DocumentGroup)?.ExternalIdlitiko ?? "";
        var subject             = contractualDocument.Subject ?? "";
        var name                = (contractualDocument.Name ?? "").Substring(0, Math.Min((contractualDocument.Name ?? "").Length, 100));
        var counterpartySign    = litiko.Eskhata.Contacts.As(contractualDocument.CounterpartySignatory)?.ExternalIdlitiko ?? "";
        var department          = litiko.Eskhata.Departments.As(contractualDocument.Department)?.ExternalId ?? "";  
        var responsibleEmployee = litiko.Eskhata.Employees.As(contractualDocument.ResponsibleEmployee)?.ExternalId ?? "";
        var author              = litiko.Eskhata.Employees.As(contractualDocument.Author)?.ExternalId ?? "";
        var validFrom           = contractualDocument.ValidFrom?.ToString(dateFormat) ?? ""; 
        var validTill           = contractualDocument.ValidTill?.ToString(dateFormat) ?? "";
        var changeReason        = contractualDocument.ReasonForChangelitiko;
        var accountDebtCredit   = accDebtCredit;
        var accountFutureExp    = accFutureExpense;
        var totalAmountLitiko   = contractualDocument.TotalAmountlitiko?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "";
        var currencyContract    = contractualDocument.CurrencyContractlitiko?.AlphaCode ?? "";
        var currencyOperation   = contractualDocument.CurrencyOperationlitiko?.AlphaCode ?? "";
        var vatApplicable       = ToYesNoNull(contractualDocument.IsVATlitiko);
        var vatRate             = contractualDocument.VatRatelitiko?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "";
        var vatAmount           = contractualDocument.VatAmount?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "";
        var incomeTaxRate       = contractualDocument.IncomeTaxRatelitiko?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "";
        var amountForPeriod     = contractualDocument.AmountForPeriodlitiko?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "";
        var note                = contractualDocument.Note ?? "Без примечания";
        var registrationNumber  = contractualDocument.RegistrationNumber ?? "";
        var registrationDate    = contractualDocument.RegistrationDate?.ToString(dateFormat) ?? "";
        var isPartialPayment    = ToYesNoNull(contractualDocument.IsPartialPaymentlitiko);
        var isEqualPayment      = ToYesNoNull(contractualDocument.IsEqualPaymentlitiko);
        var laborPayment        = ToYesNoNull(contractualDocument.IsIndividualPaymentlitiko); 
        
        var documentElement = new XElement("Document",
            new XElement("ID", documentId),
            new XElement("ExternalD", externalId),
            new XElement("DocumentKind", documentKind),
            new XElement("DocumentGroup", documentGroup),
            new XElement("Subject", subject),
            new XElement("Name", name),
            new XElement("CounterpartySignatory", counterpartySign),
            new XElement("Department", department),
            new XElement("ResponsibleEmployee", responsibleEmployee),
            new XElement("Author", author),
            new XElement("ResponsibleAccountant", responsibleAccountantId), 
            //new XElement("ResponsibleDepartment", responsibleDepartmentId), 
            new XElement("RBO", rbo),
            new XElement("ValidFrom", validFrom),
            new XElement("ValidTill", validTill),
            new XElement("ChangeReason", changeReason), 
            new XElement("AccountDebtCredt", accountDebtCredit),
            new XElement("AccountFutureExpense", accountFutureExp),
            new XElement("TotalAmount", totalAmountLitiko),
            new XElement("Currency", currencyContract),
            new XElement("OperationCurrency", currencyOperation),
            new XElement("VATApplicable", vatApplicable),
            new XElement("VATRate", vatRate),
            new XElement("VATAmount", vatAmount),
            new XElement("IncomeTaxRate", incomeTaxRate),
            new XElement("PaymentRegion", paymentRegion),
            new XElement("PaymentTaxRegion", paymentTaxRegion),
            new XElement("BatchProcessing", batchProcessing), 
            new XElement("PaymentMethod", paymentMethod),
            new XElement("PaymentFrequency", paymentFrequency),
            new XElement("PaymentBasis",
                new XElement("IsPaymentContract",   isPaymentContract),
                new XElement("IsPaymentInvoice",    isPaymentInvoice),
                new XElement("IsPaymentTaxInvoice", isPaymentTaxInvoice),
                new XElement("IsPaymentAct",        isPaymentAct),
                new XElement("IsPaymentOrder",      isPaymentOrder)
            ),
            new XElement("PaymentClosureBasis",
                new XElement("IsPaymentContract",   isClosureContract),
                new XElement("IsPaymentInvoice",    isClosureInvoice),
                new XElement("IsPaymentTaxInvoice", isClosureTaxInvoice),
                new XElement("IsPaymentAct",        isClosureAct),
                new XElement("IsPaymentWaybill",    isClosureWaybill)
            ),
            new XElement("IsPartialPayment", isPartialPayment),
            new XElement("IsEqualPayment", isEqualPayment),
            new XElement("LaborPayment", laborPayment),
            new XElement("AmountForPeriod", amountForPeriod),
            new XElement("Note", note),
            new XElement("RegistrationNumber", registrationNumber),
            new XElement("RegistrationDate", registrationDate)
        );
    
        // Company
        var companyElement = BuildCompanyXml(contractualDocument.Counterparty);
    
        // Person
        var personElement = BuildPersonXml(contractualDocument.Counterparty);

      // dataElement
      var counterpartyElement = new XElement("Counterparty", companyElement, personElement);
      var dataElement         = new XElement("Data", documentElement, counterpartyElement);
      
      return dataElement;
    }

    /// <summary>
    /// Формирование XML для выгрузки документа
    /// </summary>
    /// <param name="document">Документ</param>
    /// <param name="session_id">ИД сессии = ИД документа обмена</param>
    /// <param name="application_key">Имя call-back функции</param>
    /// <param name="dictionary">Имя протокола интеграции</param>
    /// <param name="lastId">ИД последнего пакета, если информация передается частями</param>
    [Public]
    public string BuildDocumentXml(Sungero.Docflow.IOfficialDocument document, long session_id, string application_key, string dictionary, long lastId = 0)
    {
        if (document == null)
           return string.Empty;
    
        XElement dataElement;
    
        // Определяем тип документа
        bool isContract = litiko.Eskhata.Contracts.Is(document);
        bool isSupAgreement = litiko.Eskhata.SupAgreements.Is(document);
    
        if (isContract)
        {
            // Вызываем функцию построения XML для обычного контракта
            dataElement = BuildXmlForContract(litiko.Eskhata.Contracts.As(document));
        }
        else if (isSupAgreement)
        {
            // Вызываем функцию построения XML для дополнительного соглашения
            dataElement = BuildXmlForSupAgreement(litiko.Eskhata.SupAgreements.As(document));
        }
        else
        {
            // Неизвестный тип документа, возвращаем пустой XML 
            dataElement = new XElement("Data");
        } 

        var xdoc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement("root",
                new XElement("head",
                    new XElement("session_id", session_id.ToString()),
                    new XElement("application_key", application_key)
                ),
                         
                new XElement("request",
                    new XElement("protocol-version", "1.00"),
                    new XElement("request-type", "R_DR_GET_DATA"),
                    new XElement("dictionary", dictionary),
                    new XElement("lastId", lastId.ToString()),
                    dataElement
                )
            )
        );
    
        return xdoc.ToString();
    }
    #endregion
  }
}