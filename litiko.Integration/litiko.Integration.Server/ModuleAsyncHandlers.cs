using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace litiko.Integration.Server
{
  public class ModuleAsyncHandlers
  {

    /// <summary>
    /// Интеграция. Обновление документа обмена.
    /// </summary>
    /// <param name="args"></param>    
    public virtual void UpdateExchangeDoc(litiko.Integration.Server.AsyncHandlerInvokeArgs.UpdateExchangeDocInvokeArgs args)
    {
      var logPostfix = string.Format("ExchangeDocId = '{0}'", args.DocId);
      var logPrefix = "Integration. Async handler UpdateExchangeDoc.";
      Logger.DebugFormat("{0} Start. {1}", logPrefix, logPostfix);
      
      var exchDoc =  Integration.ExchangeDocuments.GetAll().Where(d => d.Id == args.DocId).FirstOrDefault();      
      if (exchDoc == null)
      {
        Logger.ErrorFormat("{0} ExchangeDocument with id = {1} not found.", logPrefix, args.DocId);
        args.Retry = false;
        return;
      }
      
      if (!Locks.TryLock(exchDoc))
      {
        // При неудачной попытке делаем запись в лог и отправляем обработчик на повтор.
        Logger.ErrorFormat("{0} ExchangeDocument with id = {1} is locked. Sent to retry", logPrefix, args.DocId);
        args.Retry = true;
        return;
      }
      
      try
      {
        if (!string.IsNullOrEmpty(args.StatusRequestToIS) && exchDoc.StatusRequestToIS?.ToString() != args.StatusRequestToIS)
        {
          switch (args.StatusRequestToIS)
          {
            case "Created":
              exchDoc.StatusRequestToIS = Integration.ExchangeDocument.StatusRequestToIS.Created;
              break;
            case "Sent":
              exchDoc.StatusRequestToIS = Integration.ExchangeDocument.StatusRequestToIS.Sent;
              break;              
            case "Error":
              exchDoc.StatusRequestToIS = Integration.ExchangeDocument.StatusRequestToIS.Error;
              break;                
          }
        }
        
        if (!string.IsNullOrEmpty(args.RequestToISInfo) && exchDoc.RequestToISInfo != args.RequestToISInfo)
          exchDoc.RequestToISInfo = args.RequestToISInfo;
        
        if (!string.IsNullOrEmpty(args.StatusRequestToRX) && exchDoc.StatusRequestToRX?.ToString() != args.StatusRequestToRX)
        {
          switch (args.StatusRequestToRX)
          {
            case "Awaiting":
              exchDoc.StatusRequestToRX = Integration.ExchangeDocument.StatusRequestToRX.Awaiting;
              break;
            case "ReceivedPart":
              if (exchDoc.StatusRequestToRX == null || exchDoc.StatusRequestToRX == Integration.ExchangeDocument.StatusRequestToRX.Awaiting)
                exchDoc.StatusRequestToRX = Integration.ExchangeDocument.StatusRequestToRX.ReceivedPart;              
              break;              
            case "ReceivedAll":
              if (exchDoc.StatusRequestToRX == null || exchDoc.StatusRequestToRX == Integration.ExchangeDocument.StatusRequestToRX.Awaiting || exchDoc.StatusRequestToRX == Integration.ExchangeDocument.StatusRequestToRX.ReceivedPart)
                exchDoc.StatusRequestToRX = Integration.ExchangeDocument.StatusRequestToRX.ReceivedFull;
              break;
            case "Error":
              exchDoc.StatusRequestToRX = Integration.ExchangeDocument.StatusRequestToRX.Error;
              break;              
          }          
        }
        
        if (!string.IsNullOrEmpty(args.RequestToRXInfo) && exchDoc.RequestToRXInfo != args.RequestToRXInfo)
          exchDoc.RequestToRXInfo = args.RequestToRXInfo.Length >= 1000 ? args.RequestToRXInfo.Substring(0, 999) : args.RequestToRXInfo;        
        
        if (args.IncreaseNumberOfPackages)
          exchDoc.RequestToRXPacketCount++;
        
        if (exchDoc.State.IsChanged)
          exchDoc.Save();
        
      }
      catch (Exception ex)
      {
        var errorMessage = string.Format("{0}. An error occured. ErrorMessage – {1}, StackTrace - {2}. {3}", logPrefix, ex.Message, ex.StackTrace, logPostfix);
        Logger.Error(errorMessage);
        args.Retry = true;
        //exchDoc.RequestToRXInfo = ex.Message.Length >= 1000 ? ex.Message.Substring(0, 999) : ex.Message;          
      }
      finally
      {
        // Снимаем блокировку с сущности.
        Locks.Unlock(exchDoc);
      }      
      
      // Логируем завершение работы обработчика.
      Logger.DebugFormat("{0} Finish. {1}", logPrefix, logPostfix);       
    }

    /// <summary>
    /// Интеграция. Обработка данных из интегрированной системы.
    /// </summary>
    /// <param name="args"></param>
    public virtual void ImportData(litiko.Integration.Server.AsyncHandlerInvokeArgs.ImportDataInvokeArgs args)
    {      
      var logPostfix = string.Format("ExchangeDocId = '{0}'", args.ExchangeDocId);
      var logPrefix = "Integration. Async handler ImportData.";
      var errorList = new List<string>();
      Logger.DebugFormat("{0} Start. {1}", logPrefix, logPostfix);      
      
      var exchDoc =  Integration.ExchangeDocuments.GetAll().Where(d => d.Id == args.ExchangeDocId).FirstOrDefault();      
      if (exchDoc == null)
      {
        Logger.ErrorFormat("{0} ExchangeDocument with id = {1} not found.", logPrefix, args.ExchangeDocId);
        args.Retry = false;
        return;
      }
      
      if (!Locks.TryLock(exchDoc))
      {
        // При неудачной попытке делаем запись в лог и отправляем обработчик на повтор.
        Logger.ErrorFormat("{0} ExchangeDocument with id = {1} is locked. Sent to retry", logPrefix, args.ExchangeDocId);
        args.Retry = true;
        return;
      }
      
      try
      {
        #region Собрать единый xml из частей
        Sungero.Content.IElectronicDocumentVersions versionFullXML = null;
        versionFullXML = exchDoc.Versions.Where(v => v.Note == Integration.Resources.VersionRequestToRXFull && v.AssociatedApplication.Extension == "xml" && v.Body.Size > 0).FirstOrDefault();
        if (versionFullXML == null)
        {
          var exchQueues = Integration.ExchangeQueues.GetAll().Where(q => Equals(q.ExchangeDocument, exchDoc) && q.Name.StartsWith(Integration.Resources.VersionRequestToRX_));
          if (exchQueues.Count() > 0)
          {
            Logger.DebugFormat("{0} Creating full xml version. Packets count: {1}. {2}", logPrefix, exchDoc.RequestToRXPacketCount, logPostfix);
            List<XElement> dataElements = new List<XElement>();
            XElement headElement = null;
            XElement requestElement = null;
            
            foreach (var exchQueue in exchQueues)
            {
              using (var xmlStream = new MemoryStream(exchQueue.Xml))
              {
                XDocument doc = XDocument.Load(xmlStream);
                var elements = doc.Descendants("Data").Elements("element");
                dataElements.AddRange(elements);
                if (headElement == null && requestElement == null)
                {
                  headElement = doc.Root.Element("head");
                  requestElement = doc.Root.Element("request");
                }                
              }
            }

            // Создание новой версии XML
            if (dataElements.Any())
            {
              using (MemoryStream ms = new MemoryStream())
              {            
                XmlWriterSettings xws = new XmlWriterSettings();
                xws.OmitXmlDeclaration = false;
                xws.Indent = true;
                
                using (XmlWriter xw = XmlWriter.Create(ms, xws))
                {
                  // Копирование структуры request без <Data>
                  XElement newRequestElement = new XElement(requestElement);
                  newRequestElement.Element("Data").Remove();
          
                  // Создание нового элемента <Data> с объединенными элементами
                  XElement newDataElement = new XElement("Data", dataElements);
          
                  // Добавление нового <Data> к запросу
                  newRequestElement.Add(newDataElement);
                  
                  // Создание нового XML документа
                  XDocument newDoc = new XDocument(
                    new XDeclaration("1.0", "UTF-8", null),  
                    new XElement("root", headElement, newRequestElement)
                  );
                  newDoc.WriteTo(xw);
                }
                
                exchDoc.CreateVersionFrom(ms, "xml");
                exchDoc.LastVersion.Note = Integration.Resources.VersionRequestToRXFull;              
                Logger.DebugFormat("{0} Full xml version created. {1}", logPrefix, logPostfix);
                versionFullXML = exchDoc.LastVersion;  
              }                            
            }            
          }          
          
          if (exchDoc.State.IsChanged)
            exchDoc.Save();        
        }        
        #endregion
        
        #region Обработка данных в зависимости от метода интеграции, указанного в xml
        if (versionFullXML == null)
        {
          throw AppliedCodeException.Create("Version with full XML data not found.");
        }
        else
        {
          XDocument xmlDoc = XDocument.Load(versionFullXML.Body.Read());
          var dataElements = xmlDoc.Descendants("Data").Elements("element");                    
          if (!dataElements.Any())
          {
            throw AppliedCodeException.Create("Empty Data node.");
          }
          
          var dictionary = xmlDoc.Root.Element("request").Element("dictionary").Value;
          switch (dictionary)
          {
            case "R_DR_GET_DEPART":
              errorList = Functions.Module.R_DR_GET_DEPART(dataElements);
              break;
            case "R_DR_GET_EMPLOYEES":
              errorList = Functions.Module.R_DR_GET_EMPLOYEES(dataElements);
              break;
            case "R_DR_GET_BUSINESSUNITS":
              errorList = Functions.Module.R_DR_GET_BUSINESSUNITS(dataElements);
              break;
            case "R_DR_GET_COMPANY":
              // Обработка выполняется на клиенте;
              break;
            case "R_DR_GET_BANK":
              // Обработка выполняется на клиенте;
              break;
            case "R_DR_GET_COUNTRIES":
              errorList = Functions.Module.R_DR_GET_COUNTRIES(dataElements);
              break;
            case "R_DR_GET_OKOPF":
              errorList = Functions.Module.R_DR_GET_OKOPF(dataElements);
              break;
            case "R_DR_GET_OKFS":
              errorList = Functions.Module.R_DR_GET_OKFS(dataElements);
              break;
            case "R_DR_GET_OKONH":
              errorList = Functions.Module.R_DR_GET_OKONH(dataElements);
              break;
            case "R_DR_GET_OKVED":
              errorList = Functions.Module.R_DR_GET_OKVED(dataElements);
              break;
            case "R_DR_GET_COMPANYKINDS":
              errorList = Functions.Module.R_DR_GET_COMPANYKINDS(dataElements);
              break;
            case "R_DR_GET_TYPESOFIDCARDS":
              errorList = Functions.Module.R_DR_GET_TYPESOFIDCARDS(dataElements);
              break;
            case "R_DR_GET_ECOLOG":
              errorList = Functions.Module.R_DR_GET_ECOLOG(dataElements);
              break;
            case "R_DR_GET_MARITALSTATUSES":
              errorList = Functions.Module.R_DR_GET_MARITALSTATUSES(dataElements);
              break;
            case "R_DR_GET_CURRENCY_RATES":
              errorList = Functions.Module.R_DR_GET_CURRENCY_RATES(dataElements);
              break;
            case "R_DR_GET_PAYMENT_REGIONS":
              errorList = Functions.Module.R_DR_GET_PAYMENT_REGIONS(dataElements);
              break;
            case "R_DR_GET_TAX_REGIONS":
              errorList = Functions.Module.R_DR_GET_TAX_REGIONS(dataElements);
              break;
            case "R_DR_GET_CONTRACT_VID":
              errorList = Functions.Module.R_DR_GET_CONTRACT_VID(dataElements);
              break;
            case "R_DR_GET_CONTRACT_TYPE":
              errorList = Functions.Module.R_DR_GET_CONTRACT_TYPE(dataElements);
              break;
          }
          
          if (errorList.Any())
          {
            exchDoc.StatusProcessingRx = Integration.ExchangeDocument.StatusProcessingRx.Error;
            var lastError = errorList.Last();            
            exchDoc.RequestToRXInfo = lastError.Length >= 1000 ? lastError.Substring(0, 999) : lastError;
            exchDoc.Save();
          }
          else
          {
            exchDoc.StatusProcessingRx = Integration.ExchangeDocument.StatusProcessingRx.Success;
            exchDoc.RequestToRXInfo = string.Empty;
            exchDoc.Save();
          }
        }
        #endregion
      }
      catch (Exception ex)
      {
        var errorMessage = string.Format("{0}. An error occured. ErrorMessage – {1}, StackTrace - {2}. {3}", logPrefix, ex.Message, ex.StackTrace, logPostfix);
        Logger.Error(errorMessage);
        errorList.Add(errorMessage);
                
        exchDoc.StatusProcessingRx = Integration.ExchangeDocument.StatusProcessingRx.Error;
        exchDoc.RequestToRXInfo = ex.Message.Length >= 1000 ? ex.Message.Substring(0, 999) : ex.Message;
        exchDoc.Save();           
      }
      finally
      {
        // Снимаем блокировку с сущности.
        Locks.Unlock(exchDoc);
      }
      
      if (errorList.Any())
      {
        // Отправка уведомления роли "Ответственные за синхронизацию с учетными системами"
        Logger.Debug("Preparing to send notice about errors");        
        var synchronizationResponsibleRole = Roles.GetAll().Where(r => r.Sid == litiko.Integration.Constants.Module.SynchronizationResponsibleRoleGuid).FirstOrDefault();
        if (synchronizationResponsibleRole == null)
          Logger.ErrorFormat("{0} SynchronizationResponsibleRole not found. Notice was not sent! {1}", logPrefix, logPostfix);
        else
        {
          var newTask = Sungero.Workflow.SimpleTasks.CreateWithNotices(Resources.NoticeSubjectForErrorTask, synchronizationResponsibleRole);
          newTask.NeedsReview = false;
          newTask.ActiveText = string.Join(Environment.NewLine, errorList);;
          newTask.Attachments.Add(exchDoc);
          newTask.Start();
          Logger.DebugFormat("{0} Notice with Id '{1}' has been started. {2}", logPrefix, newTask.Id, logPostfix);
        }
      }
      else
        Logger.Debug("{0} There are no errors. {1}", logPrefix, logPostfix);
      
      // Логируем завершение работы обработчика.
      Logger.DebugFormat("{0} Finish. {1}", logPrefix, logPostfix);      
    }

  }
}