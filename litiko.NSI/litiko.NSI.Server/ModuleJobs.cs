using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.NSI.Server
{
  public class ModuleJobs
  {

    /// <summary>
    /// Администрирование. Удаление лишних населенных пунктов, регионов, стран и банков.
    /// </summary>
    public virtual void ClearCitiesAndRegions()
    {
      var logPrefix = "ClearCitiesAndRegions.";      
      Logger.DebugFormat("{0} Start.", logPrefix);
      
      #region Удаляем банки с пустым "ИД во внешней системе"      
      var banksToDelete = Eskhata.Banks.GetAll()
        .Where(x => x.ExternalId == null || x.ExternalId == "")
        .Select(x => x.Id)
        .ToList();
      
      Logger.DebugFormat("{0} Banks to delete count: {1}.", logPrefix, banksToDelete.Count);
      int deletedBanksCount = 0;
      int notDeletedBanksCount = 0;
      foreach (var bankId in banksToDelete)
      {        
        Transactions.Execute(() =>
                             {
                               try
                               {
                                 var bank = Eskhata.Banks.Get(bankId);
                                 Eskhata.Banks.Delete(bank);
                                 Logger.DebugFormat("{0} Deleted Id:{1}.", logPrefix, bankId);
                                 deletedBanksCount++;
                               }
                               catch (Exception ex)
                               {
                                 Logger.ErrorFormat("{0} Bank Id:{1} not deleted:{2}.", logPrefix, bankId, ex.Message);
                                 notDeletedBanksCount++;
                               }
                             });
      }
      Logger.DebugFormat("{0} deletedBanksCount:{1}. notDeletedBanksCount:{2}.", logPrefix, deletedBanksCount, notDeletedBanksCount);      
      #endregion      
      
      #region Удаляем населенные пункты с пустым "ИД во внешней системе"
      var citiesToDelete = Eskhata.Cities.GetAll()
        .Where(x => x.ExternalIdlitiko == null || x.ExternalIdlitiko == "")
        .Select(x => x.Id)
        .ToList();
      
      Logger.DebugFormat("{0} Cities to delete count: {1}.", logPrefix, citiesToDelete.Count);
      int deletedCitiesCount = 0;
      int notDeletedCitiesCount = 0;
      foreach (var cityId in citiesToDelete)
      {        
        Transactions.Execute(() =>
                             {
                               try
                               {
                                 var city = Eskhata.Cities.Get(cityId);
                                 Eskhata.Cities.Delete(city);
                                 Logger.DebugFormat("{0} Deleted Id:{1}.", logPrefix, cityId);
                                 deletedCitiesCount++;
                               }
                               catch (Exception ex)
                               {
                                 Logger.ErrorFormat("{0} City Id:{1} not deleted:{2}.", logPrefix, cityId, ex.Message);
                                 notDeletedCitiesCount++;
                               }
                             });
      }      
      Logger.DebugFormat("{0} deletedCitiesCount:{1}. notDeletedCitiesCount:{2}.", logPrefix, deletedCitiesCount, notDeletedCitiesCount);
      #endregion
     
      #region Удаляем регионы с пустым "ИД во внешней системе"      
      var regionsToDelete = Eskhata.Regions.GetAll()
        .Where(x => x.ExternalIdlitiko == null || x.ExternalIdlitiko == "")
        .Select(x => x.Id)
        .ToList();
      
      Logger.DebugFormat("{0} Regions to delete count: {1}.", logPrefix, regionsToDelete.Count);
      int deletedRegionsCount = 0;
      int notDeletedRegionsCount = 0;
      foreach (var regionId in regionsToDelete)
      {        
        Transactions.Execute(() =>
                             {
                               try
                               {
                                 var region = Eskhata.Regions.Get(regionId);
                                 Eskhata.Regions.Delete(region);
                                 Logger.DebugFormat("{0} Deleted Id:{1}.", logPrefix, regionId);
                                 deletedRegionsCount++;
                               }
                               catch (Exception ex)
                               {
                                 Logger.ErrorFormat("{0} Region Id:{1} not deleted:{2}.", logPrefix, regionId, ex.Message);
                                 notDeletedRegionsCount++;
                               }
                             });
      }
      Logger.DebugFormat("{0} deletedRegionsCount:{1}. notDeletedRegionsCount:{2}.", logPrefix, deletedRegionsCount, notDeletedRegionsCount);      
      #endregion
      
      #region Удаляем страны с пустым "ИД во внешней системе"      
      var countriesToDelete = Eskhata.Countries.GetAll()
        .Where(x => x.ExternalIdlitiko == null || x.ExternalIdlitiko == "")
        .Select(x => x.Id)
        .ToList();
      
      Logger.DebugFormat("{0} Countries to delete count: {1}.", logPrefix, countriesToDelete.Count);
      int deletedCountriesCount = 0;
      int notDeletedCountriesCount = 0;
      foreach (var countryId in countriesToDelete)
      {        
        Transactions.Execute(() =>
                             {
                               try
                               {
                                 var country = Eskhata.Countries.Get(countryId);
                                 Eskhata.Countries.Delete(country);
                                 Logger.DebugFormat("{0} Deleted Id:{1}.", logPrefix, countryId);
                                 deletedCountriesCount++;
                               }
                               catch (Exception ex)
                               {
                                 Logger.ErrorFormat("{0} Country Id:{1} not deleted:{2}.", logPrefix, countryId, ex.Message);
                                 notDeletedCountriesCount++;
                               }
                             });
      }      
      Logger.DebugFormat("{0} deletedCountriesCount:{1}. notDeletedCountriesCount:{2}.", logPrefix, deletedCountriesCount, notDeletedCountriesCount);      
      #endregion                   
      
      Logger.DebugFormat("{0} Finish.", logPrefix);
    }

  }
}