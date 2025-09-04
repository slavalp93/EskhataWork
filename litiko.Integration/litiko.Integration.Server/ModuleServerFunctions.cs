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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace litiko.Integration.Server
{
  public class ModuleFunctions
  {
    #region Отправка / получение ответа от внешней информационной системы
    
    /// <summary>
    /// Отправка запроса во внешнюю информационную систему.
    /// </summary>
    /// <param name="method">Метод интеграции</param>
    /// <param name="exchDoc">Документ обмена</param>
    /// <param name="lastId">0 - новый запрос, >0 - запрос очередной части предыдущего запроса</param></param>
    /// <returns>Строка с ошибкой или пустая строка</returns>
    [Public, Remote]
    public string SendRequestToIS(IIntegrationMethod method, IExchangeDocument exchDoc, long lastId)
    {
      var logPostfix = string.Format("ExchangeDocId = '{0}'", exchDoc.Id);
      var logPrefix = "Integration. SendRequestToIS.";
      Logger.DebugFormat("{0} Start. {1}", logPrefix, logPostfix);
      
      var errorMessage = string.Empty;
      
      var statusRequestToIS = exchDoc.StatusRequestToIS;
      var requestToISInfo = exchDoc.RequestToISInfo;
      
      try
      {
        //string application_key = "10.10.202.171/Integration/odata/Integration/ProcessResponseFromIS##";
        Uri uri = new Uri(Hyperlinks.Get(exchDoc));
        string ipAdress = Dns.GetHostAddresses(uri.Host).FirstOrDefault().ToString();
        
        //string application_key = $"{uri.Scheme}://{uri.Host}/" +"Integration/odata/Integration/ProcessResponseFromIS##";
        //string application_key = $"{ipAdress}/Integration/odata/Integration/ProcessResponseFromIS##";
        
        string application_key = $"{uri.Host}/Integration/odata/Integration/ProcessResponseFromIS##";
        
        string url = method.IntegrationSystem.ServiceUrl;
        var xmlRequestBody = string.Empty;
        
        if (method.Name == "R_DR_GET_COMPANY")
          xmlRequestBody = Integration.Resources.RequestXMLTemplateForCompanyFormat(exchDoc.Id, application_key, method.Name, lastId, exchDoc.Counterparty.TIN);
        else if (method.Name == "R_DR_GET_BANK")
          xmlRequestBody = Integration.Resources.RequestXMLTemplateForBankFormat(exchDoc.Id, application_key, method.Name, lastId, Eskhata.Banks.As(exchDoc.Counterparty).BIC);
        else
          xmlRequestBody = Integration.Resources.RequestXMLTemplateFormat(exchDoc.Id, application_key, method.Name, lastId);

        if (method.SaveRequestToIS.Value)
        {
          /*
          using (var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlRequestBody)))
          {
            exchDoc.CreateVersionFrom(xmlStream, "xml");
            exchDoc.LastVersion.Note = Integration.Resources.VersionRequestToISFormat(lastId);
            // exchDoc.Save();
          }
           */
          
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
              //if (exchDoc.StatusRequestToIS != Integration.ExchangeDocument.StatusRequestToIS.Sent)
              //  exchDoc.StatusRequestToIS = Integration.ExchangeDocument.StatusRequestToIS.Sent;
            }
            else
            {
              Logger.DebugFormat("Response failed. State message: {0}", stateMsg);
              statusRequestToIS = Integration.ExchangeDocument.StatusRequestToIS.Error;
              //if (exchDoc.StatusRequestToIS != Integration.ExchangeDocument.StatusRequestToIS.Error)
              //  exchDoc.StatusRequestToIS = Integration.ExchangeDocument.StatusRequestToIS.Error;
            }
            
            //if (exchDoc.RequestToISInfo != stateMsg)
            //  exchDoc.RequestToISInfo = stateMsg;
            requestToISInfo = stateMsg;
            
            // Сохранение ответа
            if (method.SaveResponseFromIS.Value)
            {
              /*
              using (var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(responseBody)))
              {
                exchDoc.CreateVersionFrom(xmlStream, "xml");
                exchDoc.LastVersion.Note = Integration.Resources.VersionResponseFromISFormat(lastId);
              }
               */
              
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
            /*
            if (exchDoc.RequestToISInfo != errorMessage)
              exchDoc.RequestToISInfo = errorMessage;
            if (exchDoc.StatusRequestToIS != Integration.ExchangeDocument.StatusRequestToIS.Error)
              exchDoc.StatusRequestToIS = Integration.ExchangeDocument.StatusRequestToIS.Error;
             */
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
      
      if (exchDoc.StatusRequestToIS != statusRequestToIS || exchDoc.RequestToISInfo != requestToISInfo)
      {
        // Обновляем exchDoc асинхронным обработчиком
        var asyncHandler = Integration.AsyncHandlers.UpdateExchangeDoc.Create();
        asyncHandler.DocId = exchDoc.Id;
        asyncHandler.RequestToISInfo = requestToISInfo;
        asyncHandler.StatusRequestToIS = statusRequestToIS.ToString();
        asyncHandler.IncreaseNumberOfPackages = false;
        asyncHandler.ExecuteAsync();
      }
      
      /*
      if (exchDoc.State.IsChanged)
      {
        try
        {
          exchDoc.Save();
        }
        catch (Exception ex)
        {
          errorMessage = string.Format("{0} Error saving. Message: {2} {1}", logPrefix, logPostfix, ex.Message);
          Logger.Error(errorMessage);
        }
      }
       */
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
      using (var xmlStream = new MemoryStream(xmlData))
      {
        var logPrefix = "Integration. ProcessResponseFromIS.";
        Logger.DebugFormat("{0} Start.", logPrefix);
        
        var session_id_str = string.Empty;
        long session_id;
        var dictionary = string.Empty;
        var lastId_str = string.Empty;
        long lastId;
        var invalidParamValue = "???";
        bool increaseNumberOfPackages = false;
        string errorMessage = string.Empty;
        
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(xmlStream);
        XmlElement root = xmlDoc.DocumentElement;
        
        #region Предпроверки
        XmlNode session_id_Node = root.SelectSingleNode("//head/session_id");
        if (session_id_Node == null)
        {
          errorMessage = "session_id node is absent";
          Logger.ErrorFormat("{0} ErrorMessage: {1}.", logPrefix, errorMessage);
          return Encoding.UTF8.GetBytes(Integration.Resources.ResponseXMLTemplateFormat(invalidParamValue, invalidParamValue, 2, errorMessage));
        }
        else
          session_id_str = session_id_Node.InnerText;

        if (!long.TryParse(session_id_str, out session_id))
        {
          errorMessage = "Invalid value in session_id";
          Logger.ErrorFormat("{0} ErrorMessage: {1}.", logPrefix, errorMessage);
          return Encoding.UTF8.GetBytes(Integration.Resources.ResponseXMLTemplateFormat(invalidParamValue, invalidParamValue, 2, errorMessage));
        }
        var logPostfix = string.Format("ExchangeDocId = '{0}'", session_id);
        
        XmlNode dictionary_Node = root.SelectSingleNode("//request/dictionary");
        if (dictionary_Node == null)
        {
          errorMessage = "dictionary node is absent";
          Logger.ErrorFormat("{0} ErrorMessage: {1}.", logPrefix, errorMessage);
          return Encoding.UTF8.GetBytes(Integration.Resources.ResponseXMLTemplateFormat(session_id, invalidParamValue, 2, errorMessage));
        }
        else
          dictionary = dictionary_Node.InnerText;
        
        if (string.IsNullOrEmpty(dictionary) || string.IsNullOrWhiteSpace(dictionary))
        {
          errorMessage = "Invalid value in dictionary";
          Logger.ErrorFormat("{0} ErrorMessage: {1}.", logPrefix, errorMessage);
          return Encoding.UTF8.GetBytes(Integration.Resources.ResponseXMLTemplateFormat(session_id, invalidParamValue, 2, errorMessage));
        }
        
        XmlNode lastId_Node = root.SelectSingleNode("//request/lastId");
        if (lastId_Node == null)
        {
          errorMessage = "lastId node is absent";
          Logger.ErrorFormat("{0} ErrorMessage: {1}.", logPrefix, errorMessage);
          return Encoding.UTF8.GetBytes(Integration.Resources.ResponseXMLTemplateFormat(session_id, dictionary, 2, errorMessage));
        }
        else
          lastId_str = lastId_Node.InnerText;
        
        if (!long.TryParse(lastId_str, out lastId))
        {
          errorMessage = "Invalid value in lastId";
          Logger.ErrorFormat("{0} ErrorMessage: {1}.", logPrefix, errorMessage);
          return Encoding.UTF8.GetBytes(Integration.Resources.ResponseXMLTemplateFormat(session_id, dictionary, 2, errorMessage));
        }
        
        XmlNode data_Node = root.SelectSingleNode("//request/Data");
        if (data_Node == null)
        {
          errorMessage = "Data node is absent";
          Logger.ErrorFormat("{0} ErrorMessage: {1}.", logPrefix, errorMessage);
          return Encoding.UTF8.GetBytes(Integration.Resources.ResponseXMLTemplateFormat(session_id, dictionary, 2, errorMessage));
        }
        
        var exchDoc = ExchangeDocuments.GetAll().Where(d => d.Id == session_id).FirstOrDefault();
        if (exchDoc == null)
        {
          errorMessage = "Request for session id not found. Session Id=" +session_id.ToString();
          Logger.ErrorFormat("{0} ErrorMessage: {1}.", logPrefix, errorMessage);
          return Encoding.UTF8.GetBytes(Integration.Resources.ResponseXMLTemplateFormat(session_id, dictionary, 2, errorMessage));
        }
        
        #endregion
        
        var statusRequestToRX = exchDoc.StatusRequestToRX;
        var requestToRXInfo = exchDoc.RequestToRXInfo;
        
        try
        {
          
          // сохранить xml в новую версию
          //exchDoc.CreateVersionFrom(xmlStream, "xml");
          //exchDoc.LastVersion.Note = Integration.Resources.VersionRequestToRXFormat(lastId);
          
          var exchQueue = ExchangeQueues.Create();
          exchQueue.ExchangeDocument = exchDoc;
          exchQueue.Xml = xmlData;
          exchQueue.Name = Integration.Resources.VersionRequestToRXFormat(lastId);
          exchQueue.Save();
          increaseNumberOfPackages = true;
          
          // обновить статусы в exchDoc
          if (lastId > 0)
            statusRequestToRX = Integration.ExchangeDocument.StatusRequestToRX.ReceivedPart;
          else
            statusRequestToRX = Integration.ExchangeDocument.StatusRequestToRX.ReceivedFull;
          
          //exchDoc.RequestToRXPacketCount++;
          //if (exchDoc.RequestToRXInfo != "Saved")
          //  exchDoc.RequestToRXInfo = "Saved";
          if (requestToRXInfo != "Saved")
            requestToRXInfo = "Saved";
        }
        catch (Exception ex)
        {
          errorMessage = ex.Message;
          Logger.ErrorFormat("{0} ErrorMessage: {1}.{2}", logPrefix, errorMessage, logPostfix);
          statusRequestToRX = Integration.ExchangeDocument.StatusRequestToRX.Error;
          requestToRXInfo = ex.Message;
          return Encoding.UTF8.GetBytes(Integration.Resources.ResponseXMLTemplateFormat(session_id, dictionary, 2, ex.Message));
        }
        
        // Обновляем exchDoc асинхронным обработчиком
        var asyncHandler = Integration.AsyncHandlers.UpdateExchangeDoc.Create();
        asyncHandler.DocId = exchDoc.Id;
        asyncHandler.StatusRequestToRX = statusRequestToRX.ToString();
        asyncHandler.RequestToRXInfo = requestToRXInfo;
        asyncHandler.IncreaseNumberOfPackages = increaseNumberOfPackages;
        asyncHandler.ExecuteAsync();
        
        if (string.IsNullOrEmpty(errorMessage))
        {
          if (lastId > 0)
            // вызвать получение остальной части пакета
            SendRequestToIS(exchDoc.IntegrationMethod, exchDoc, lastId);
          else
          {
            Thread.Sleep(5000); // Пауза на 5 сек.
            
            // запустить обработчик пакета
            var asyncHandlerImportData = Integration.AsyncHandlers.ImportData.Create();
            asyncHandlerImportData.ExchangeDocId = exchDoc.Id;
            asyncHandlerImportData.ExecuteAsync();
          }
        }
        
        Logger.DebugFormat("{0} Finish. {1}", logPrefix, logPostfix);
        return Encoding.UTF8.GetBytes(Integration.Resources.ResponseXMLTemplateFormat(session_id, dictionary, 1, "Saved"));
      }
    }

    [Public, Remote(IsPure = true)]
    public static bool WaitForGettingDataFromIS(Integration.IExchangeDocument exchDoc, int intervalMilliseconds, int maxAttempts)
    {
      for (int attempt = 0; attempt < maxAttempts; attempt++)
      {
        if (Equals(exchDoc.StatusRequestToRX, litiko.Integration.ExchangeDocument.StatusRequestToRX.ReceivedFull))
        {
          return true;
        }
        
        Thread.Sleep(intervalMilliseconds); // Ожидание
      }
      
      return false; // Условие не выполнено за maxAttempts попыток
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
      
      // Проверить, есть ли документы обмена, на которые еще не получен ответ
      /*
      var exchDocs = ExchangeDocuments.GetAll().Where(d => d.StatusRequestToRX == Integration.ExchangeDocument.StatusRequestToRX.Awaiting || d.StatusRequestToRX == Integration.ExchangeDocument.StatusRequestToRX.ReceivedPart
                                                     && Equals(d.IntegrationMethod, integrationMethod)).Select(d => d.Id);
      if (exchDocs.Any())
      {
        Logger.DebugFormat("Pending requests found: {0}", exchDocs.ToString());
        return;
      }
       */
      var exchDoc = Integration.ExchangeDocuments.Create();
      exchDoc.IntegrationMethod = integrationMethod;
      exchDoc.Save();
      
      var errorMessage = Functions.Module.SendRequestToIS(integrationMethod, exchDoc, lastId);
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
                               
                               var isPersonnelNumber = element.Element("PersonnelNumber")?.Value;
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
                                   var personResult = ProcessingPerson(isPerson, fioInfo);
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
                                 
                                 var entity = NSI.IDTypes.GetAll().Where(x => x.ExternalId == isId).FirstOrDefault();
                                 if (entity != null)
                                   Logger.DebugFormat("IDType with ExternalId:{0} was found. Id:{1}, Name:{2}", isId, entity.Id, entity.Name);
                                 else
                                 {
                                   entity = NSI.IDTypes.Create();
                                   entity.ExternalId = isId;
                                   Logger.DebugFormat("Create new IDType with ExternalId:{0}. Id:{1}", isId, entity.Id);
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
                                   Logger.DebugFormat("IDType successfully saved. ExternalId:{0}, Id:{1}", isId, entity.Id);
                                   countChanged++;
                                 }
                                 else
                                 {
                                   Logger.DebugFormat("There are no changes in IDType. ExternalId:{0}, Id:{1}", isId, entity.Id);
                                   countNotChanged++;
                                 }
                               }
                               catch (Exception ex)
                               {
                                 var errorMessage = string.Format("Error when processing IDType with ExternalId:{0}. Description: {1}. StackTrace: {2}", isId, ex.Message, ex.StackTrace);
                                 Logger.Error(errorMessage);
                                 errorList.Add(errorMessage);
                                 countErrors++;
                               }
                             });
      }
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
    /// <returns>Список ошибок (List<string>)</returns>
    [Public, Remote]
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
      var isName = element.Element("NAME")?.Value;
      var isINN = element.Element("INN")?.Value;
      
      var isLongName = element.Element("LONG_NAME")?.Value;
      var isIName = element.Element("I_NAME")?.Value;
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
          var elementValues = isCodeOKONHelements.Select(x => x.Value).ToList();
          if(company.OKONHlitiko.Select(x => x.OKONH.ExternalId).Any(x => !elementValues.Contains(x)))
          {
            company.OKONHlitiko.Clear();
            Logger.DebugFormat("Change OKONHlitiko: Clear");
          }
          
          foreach (var isCodeOKONH in isCodeOKONHelements)
          {
            var okonh = litiko.NSI.OKONHs.GetAll().Where(x => x.ExternalId == isCodeOKONH.Value).FirstOrDefault();
            if(okonh != null && !company.OKONHlitiko.Any(x => Equals(x.OKONH, okonh)))
            {
              var newRecord = company.OKONHlitiko.AddNew();
              newRecord.OKONH = okonh;
              Logger.DebugFormat("Change OKONHlitiko: added:{0}", okonh.Name);
            }
          }
        }
        
        if(isCodeOKVEDelements.Any())
        {
          var elementValues = isCodeOKVEDelements.Select(x => x.Value).ToList();
          if(company.OKVEDlitiko.Select(x => x.OKVED.ExternalId).Any(x => !elementValues.Contains(x)))
          {
            company.OKVEDlitiko.Clear();
            Logger.DebugFormat("Change OKVEDlitiko: Clear");
          }
          
          foreach (var isCodeOKVED in isCodeOKVEDelements)
          {
            var okved = litiko.NSI.OKVEDs.GetAll().Where(x => x.ExternalId == isCodeOKVED.Value).FirstOrDefault();
            if(okved != null && !company.OKVEDlitiko.Any(x => Equals(x.OKVED, okved)))
            {
              var newRecord = company.OKVEDlitiko.AddNew();
              newRecord.OKVED = okved;
              Logger.DebugFormat("Change OKVEDlitiko: added:{0}", okved.Name);
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
              
              var personResult = ProcessingPerson(isPerson, null);
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
    [Public, Remote]
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
        
        if(!string.IsNullOrEmpty(isCorrAcc) && bank.CorrespondentAccount != isCorrAcc)
        {
          Logger.DebugFormat("Change CorrespondentAccount: current:{0}, new:{1}", bank.CorrespondentAccount, isCorrAcc);
          bank.CorrespondentAccount = isCorrAcc;
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
              
              var personResult = ProcessingPerson(isPerson, null);
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
    public Structures.Module.ProcessingPersonResult ProcessingPerson(System.Xml.Linq.XElement personData, Structures.Module.FIOInfo fioInfo)
    {
      var isID = personData.Element("ID")?.Value;
      var isName = personData.Element("NAME")?.Value;
      var isSex = personData.Element("SEX")?.Value;
      
      var isIName = personData.Element("I_NAME")?.Value;
      var isRezident = personData.Element("REZIDENT")?.Value;
      var isNuRezident = personData.Element("NU_REZIDENT")?.Value;
      var isDateOfBirth = personData.Element("DATE_PERS")?.Value;
      var isFamilyStatus = personData.Element("MARIGE_ST")?.Value;
      var isINN = personData.Element("INN")?.Value;
      var isCodeOKONHelements = personData.Element("CODE_OKONH").Elements("element");
      var isCodeOKVEDelements = personData.Element("CODE_OKVED").Elements("element");
      var isCountry = personData.Element("COUNTRY")?.Value;
      var isLegalAdress = personData.Element("DOC_BIRTH_PLACE")?.Value;
      var isPostAdress = personData.Element("PostAdress")?.Value;
      var isPhone = personData.Element("Phone")?.Value;
      var isEmail = personData.Element("Email")?.Value;
      var isWebSite = personData.Element("WebSite")?.Value;
      
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
      
      var person = Eskhata.People.GetAll().Where(x => x.ExternalId == isID).FirstOrDefault();
      if (person == null)
      {
        person = Eskhata.People.Create();
        person.ExternalId = isID;
        Logger.DebugFormat("Create new Person with ExternalId:{0}. Id:{1}", isID, person.Id);
      }
      else
        Logger.DebugFormat("Person with ExternalId:{0} was found. Id:{1}, Name:{2}", isID, person.Id, person.Name);
      
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
      
      if (!string.IsNullOrEmpty(isCountry))
      {
        var country = Eskhata.Countries.GetAll().Where(x => x.ExternalIdlitiko == isCountry).FirstOrDefault();
        if (country != null && !Equals(person.Citizenship, country))
        {
          Logger.DebugFormat("Change Citizenship: current:{0}, new:{1}", person.Citizenship?.Name, country?.Name);
          person.Citizenship = country;
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
      
      /* !!! IdentityDocuments !!! */
      
      var result = Structures.Module.ProcessingPersonResult.Create(person, false);
      if (person.State.IsChanged || person.State.IsInserted)
      {
        person.Save();
        Logger.DebugFormat("Person successfully saved. ExternalId:{0}, Id:{1}", isID, person.Id);
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
    
    #endregion
  }
}