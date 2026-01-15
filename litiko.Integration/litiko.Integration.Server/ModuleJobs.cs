using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.Integration.Server
{
  public class ModuleJobs
  {
    /// <summary>
    /// Интеграция. Экспорт обновлений по действующим договорам
    /// </summary>
    public virtual void ExportUpdatesForActiveContracts()      
    {
      var logPrefix = "Integration. ExportUpdatesForActiveContracts.";      
      Logger.DebugFormat("{0} Start.", logPrefix);
      
      var documents = litiko.Eskhata.Contracts.GetAll()
        .Where(d => Equals(d.LifeCycleState, litiko.Eskhata.Contract.LifeCycleState.Active))
        .Where(d => d.UpdateRquiredlitiko.GetValueOrDefault());
      
      if (documents.Any())
      {
        var integrationMethod = IntegrationMethods.GetAll().Where(x => x.Name == Constants.Module.IntegrationMethods.R_DR_SET_CONTRACT).FirstOrDefault();
        if (integrationMethod == null)
          throw new AppliedCodeException(SendDocumentStages.Resources.IntegrationMethodNotFoundFormat(Constants.Module.IntegrationMethods.R_DR_SET_CONTRACT));
      
        Logger.DebugFormat("{0} Found {1} documents.", logPrefix, documents.Count());
        foreach (var document in documents)
        {
          Transactions.Execute(
            () =>
            {
              if (!Locks.TryLock(document))              
                Logger.ErrorFormat("{0} Document with Id:{1} is locked.", logPrefix, document.Id);
              else
              {
                try
                {
                  var exchDoc = Integration.ExchangeDocuments.Create();
                  exchDoc.IntegrationMethod = integrationMethod;
                  exchDoc.IsOnline = false;
                  exchDoc.Save();                  
                  
                  var errorMessage = Functions.Module.SendRequestToIS(exchDoc, 0, document);
                  if (!string.IsNullOrEmpty(errorMessage))
                    Logger.ErrorFormat("{0} Document with Id:{1} not sent. Error: {2}", logPrefix, document.Id, errorMessage);                  
                  else
                  {                    
                    document.IntegrationStatuslitiko = litiko.Eskhata.OfficialDocument.IntegrationStatuslitiko.Send;
                    document.UpdateRquiredlitiko = false;
                    document.Save();
                    
                    Logger.DebugFormat("{0} Document successfully sent. Id:{1}.", logPrefix, document.Id);
                  }                                    
                }
                catch (Exception ex)
                {
                  Logger.ErrorFormat("{0} Error when processing document with Id:{1}. Error: {2}", logPrefix, document.Id, ex.Message);
                  //throw ex;
                }
                finally
                {                  
                  Locks.Unlock(document);
                }                
              }
            }
          );
        }        
      }
      else
        Logger.DebugFormat("{0} Documents not found.", logPrefix);
      
      Logger.DebugFormat("{0} Finish.", logPrefix);      
    }

    /// <summary>
    /// Интеграция. Виды документов, удостоверяющих личность
    /// </summary>
    public virtual void GetIdentityDocumentKinds()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart(Constants.Module.IntegrationMethods.R_DR_GET_TYPESOFIDCARDS);
    }

    /// <summary>
    /// Интеграция. Типы договоров.
    /// </summary>
    public virtual void GetContractType()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart(Constants.Module.IntegrationMethods.R_DR_GET_CONTRACT_TYPE);
    }

    /// <summary>
    /// Интеграция. Виды договоров.
    /// </summary>
    public virtual void GetContractVid()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart(Constants.Module.IntegrationMethods.R_DR_GET_CONTRACT_VID);
    }

    /// <summary>
    /// Интеграция. Регионы объектов аренды.
    /// </summary>
    public virtual void GetRegionOfRental()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart(Constants.Module.IntegrationMethods.R_DR_GET_TAX_REGIONS);
    }

    /// <summary>
    /// Интеграция. Регионы оплаты
    /// </summary>
    public virtual void GetPaymentRegions()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart(Constants.Module.IntegrationMethods.R_DR_GET_PAYMENT_REGIONS);
    }

    /// <summary>
    /// Интеграция. Курсы валют
    /// </summary>
    public virtual void GetCurrencyRates()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart(Constants.Module.IntegrationMethods.R_DR_GET_CURRENCY_RATES);
    }

    /// <summary>
    /// Интеграция. Удаление старых документов обмена
    /// </summary>
    public virtual void RemovingOldExchangeDocs()
    {
      var logPrefix = "Integration. RemovingOldExchangeDocs.";
      Logger.DebugFormat("{0} Start.", logPrefix);
      
      var createdNinetyDaysAgo = Calendar.Now.AddDays(-90).Date;
      var exchangeDocIdsToDelete = ExchangeDocuments.GetAll()
        .Where(x => x.Created.HasValue && x.Created.Value.Date <= createdNinetyDaysAgo)
        .Select(x => x.Id)
        .ToList();
      
      if (exchangeDocIdsToDelete.Any())
      {
        Logger.DebugFormat("{0} Number of documents to delete: {1}.", logPrefix, exchangeDocIdsToDelete.Count);
        foreach (var exchangeDocId in exchangeDocIdsToDelete)
        {
          Transactions.Execute(
            () =>
            {
              try
              {
                var exchangeDoc = ExchangeDocuments.Get(exchangeDocId);
                
                // Удаляем записи из очереди обмена
                var exchangeQueue = ExchangeQueues.GetAll()
                  .Where(x => x.ExchangeDocument.Id == exchangeDocId)
                  .ToList();
                
                foreach (var item in exchangeQueue)
                  ExchangeQueues.Delete(item);
                
                // Удаляем сам документ обмена
                ExchangeDocuments.Delete(exchangeDoc);
                
                Logger.DebugFormat("{0} Document successfully deleted Id:{1}.", logPrefix, exchangeDocId);
              }
              catch (Exception ex)
              {
                Logger.Error(string.Format("{0} Failed to delete document. Id:{1}.", logPrefix, exchangeDocId), ex);
                throw;
              }
            }
           );
        }
      }
      else
        Logger.DebugFormat("{0} No documents available for deletion.", logPrefix);
      
      Logger.DebugFormat("{0} Finish.", logPrefix);
    }    
   
    /// <summary>
    /// 
    /// </summary>
    public virtual void GetOKVED()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart(Constants.Module.IntegrationMethods.R_DR_GET_OKVED);        
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual void GetOKOPF()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart(Constants.Module.IntegrationMethods.R_DR_GET_OKOPF);       
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual void GetOKONH()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart(Constants.Module.IntegrationMethods.R_DR_GET_OKONH);         
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual void GetOKFS()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart(Constants.Module.IntegrationMethods.R_DR_GET_OKFS);         
    }

    /// <summary>
    /// Интеграция. Семейное положение
    /// </summary>
    public virtual void GetMaterialStatuses()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart(Constants.Module.IntegrationMethods.R_DR_GET_MARITALSTATUSES);       
    }

    /// <summary>
    /// Интеграция. Экологические риски
    /// </summary>
    public virtual void GetEcolog()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart(Constants.Module.IntegrationMethods.R_DR_GET_ECOLOG);       
    }

    /// <summary>
    /// Интеграция. Страны
    /// </summary>
    public virtual void GetCountries()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart(Constants.Module.IntegrationMethods.R_DR_GET_COUNTRIES);      
    }

    /// <summary>
    /// Интеграция. Виды предприятий
    /// </summary>
    public virtual void GetCompanyKinds()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart(Constants.Module.IntegrationMethods.R_DR_GET_COMPANYKINDS);
    }

    /// <summary>
    /// Интеграция. Сотрудники
    /// </summary>
    public virtual void GetEmployees()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart(Constants.Module.IntegrationMethods.R_DR_GET_EMPLOYEES);            
    }

    /// <summary>
    /// Интеграция. Наши организации
    /// </summary>
    public virtual void GetBusinessUnits()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart(Constants.Module.IntegrationMethods.R_DR_GET_BUSINESSUNITS);                  
    }

    /// <summary>
    /// Интеграция. Подразделения
    /// </summary>
    public virtual void GetDepartments()
    {                
      litiko.Integration.Functions.Module.BackgroundProcessStart(Constants.Module.IntegrationMethods.R_DR_GET_DEPART);
    }
    
    /// <summary>
    /// Интеграция. Населенные пункты
    /// </summary>
    public virtual void GetCities()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart(Constants.Module.IntegrationMethods.R_DR_GET_CITIES); 
    }

    /// <summary>
    /// Интеграция. Регионы
    /// </summary>
    public virtual void GetRegions()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart(Constants.Module.IntegrationMethods.R_DR_GET_REGIONS); 
    }    

  }
}