using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.Integration.Client
{
  public class ModuleFunctions
  {

    /// <summary>
    /// Запуск интеграции с АБС по кнопке с карточки объекта
    /// </summary>    
    /// <param name="entity">Сущность, из которой запущен процесс</param></param>
    /// <returns>Строка с ошибкой или пустая строка</returns>
    [Public]
    public string IntegrationClientAction(Sungero.Domain.Shared.IEntity entity)
    {
      string errorMessage = string.Empty;
      
      #region Предпроверки
      var company = Eskhata.Companies.As(entity);
      var bank = Eskhata.Banks.As(entity);
      var person = Eskhata.People.As(entity);
      var contract = Eskhata.Contracts.As(entity);
      var supAgreement = Eskhata.SupAgreements.As(entity);
      
      if ((company != null || person != null) && string.IsNullOrEmpty(Eskhata.Counterparties.As(entity).TIN))
        return Eskhata.Companies.Resources.ErrorNeedFillTin;
            
      if (bank != null && string.IsNullOrEmpty(bank.BIC))      
        return Eskhata.Banks.Resources.ErrorNeedFillBIC;
            
      if (entity.State.IsInserted)
      {        
        if (company != null && string.IsNullOrEmpty(company.Name))
          company.Name = Constants.Module.UndefinedString;
        
        if (bank != null && string.IsNullOrEmpty(bank.Name))
          bank.Name = Constants.Module.UndefinedString;
        
        if (person != null)
        {
          if (string.IsNullOrEmpty(person.LastName))
            person.LastName = Constants.Module.UndefinedString;
          
          if (string.IsNullOrEmpty(person.FirstName))
            person.FirstName = Constants.Module.UndefinedString;          
        }          
        
        entity.Save();
      }     
      
      var integrationMethod = Functions.Module.Remote.GetIntegrationMethod(entity);
      if (integrationMethod == null)        
        return litiko.Integration.Resources.IntegrationMethodNotFound;
                  
      #endregion
            
      var exchDoc = Integration.PublicFunctions.Module.Remote.CreateExchangeDocument();
      exchDoc.IntegrationMethod = integrationMethod;
      exchDoc.IsOnline = true;
      exchDoc.Save();
      
      errorMessage =  Integration.PublicFunctions.Module.Remote.SendRequestToIS(exchDoc, 0, entity);
      if (!string.IsNullOrEmpty(errorMessage))
      {
        exchDoc.StatusRequestToIS = Integration.ExchangeDocument.StatusRequestToIS.Error;
        exchDoc.RequestToISInfo = errorMessage.Length >= 1000 ? errorMessage.Substring(0, 999) : errorMessage;
        exchDoc.Save();        
        return errorMessage;
      }
      else
      {
        exchDoc.StatusRequestToIS = Integration.ExchangeDocument.StatusRequestToIS.Sent;        
        exchDoc.Save();
      }
            
      long exchangeQueueId = Integration.PublicFunctions.Module.Remote.WaitForGettingDataFromIS(exchDoc.Id, 1000, 10);
      if (exchangeQueueId > 0)
      {                
        
        #region Создать версию из xml
        var exchQueue = litiko.Integration.ExchangeQueues.Get(exchangeQueueId);
        using (var xmlStream = new System.IO.MemoryStream(exchQueue.Xml))
        {
          exchDoc.CreateVersionFrom(xmlStream, "xml");
          exchDoc.LastVersion.Note = Integration.Resources.VersionRequestToRXFull;                                    
          exchDoc.StatusRequestToRX = Integration.ExchangeDocument.StatusRequestToRX.ReceivedFull;
          exchDoc.RequestToRXInfo = "Saved";
          exchDoc.RequestToRXPacketCount = 1;
          exchDoc.Save();
        }        
        #endregion
        
        var errorList = new List<string>();        
        if (company != null)
          errorList = litiko.Integration.Functions.Module.Remote.R_DR_GET_COMPANY(exchDoc.Id, company);
        else if (bank != null)
          errorList = litiko.Integration.Functions.Module.Remote.R_DR_GET_BANK(exchDoc.Id, bank);
        else if (person != null)
          errorList = litiko.Integration.Functions.Module.Remote.R_DR_GET_PERSON(exchDoc.Id, person);
        else if (contract != null)
          errorList = litiko.Integration.Functions.Module.Remote.R_DR_SET_CONTRACT_Online(exchDoc, contract);
        else if (supAgreement != null)
          errorList = litiko.Integration.Functions.Module.Remote.R_DR_SET_CONTRACT_Online(exchDoc, supAgreement);
                
        if (errorList.Any())
        {
          var lastError = errorList.LastOrDefault();          
          exchDoc.RequestToRXInfo = lastError.Length >= 1000 ? lastError.Substring(0, 999) : lastError;
          exchDoc.StatusProcessingRx = Integration.ExchangeDocument.StatusProcessingRx.Error;          
          exchDoc.Save();
          
          return lastError;
        }
        else
        {
          exchDoc.StatusProcessingRx = Integration.ExchangeDocument.StatusProcessingRx.Success;          
          exchDoc.Save();
        }        
      }
      else
        return litiko.Integration.Resources.ResponseNotReceived;
      
      return errorMessage;
      
    }

  }
}