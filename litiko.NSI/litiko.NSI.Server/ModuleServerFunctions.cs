using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace litiko.NSI.Server
{
  public class ModuleFunctions
  {            
    /// <summary>
    /// Экспорт справочника "Маппинг" для переноса в прод. среду
    /// </summary>
    [Remote]
    public IZip ExportMapping()
    {
      var root = new XElement("Data",
                              NSI.Mappings.GetAll().Select(x =>
                                                           new XElement("element",
                                                                        new XElement("Type", x.EntityType.HasValue ? x.EntityType.Value.Value : string.Empty),
                                                                        new XElement("Id", (long)x.SourceId),
                                                                        new XElement("Name", x.SourceName ?? string.Empty),
                                                                        new XElement("ExternalId", x.SourceExternalId ?? string.Empty)
                                                                       )));
      var xmlData = new XDocument(root);
      return CreateZipFromXml(xmlData, "export.zip", "data");
    }
    
    /// <summary>
    /// Импорт справочника "Маппинг"
    /// </summary>
    [Remote]
    public Structures.Module.IResultImportXml ImportMappingFromXml(string fileBase64)
    {
      var result = Structures.Module.ResultImportXml.Create();
      result.Errors = new List<string>();

      byte[] fileBytes;
      try
      {
        if (string.IsNullOrEmpty(fileBase64))
          throw new Exception("Переданы пустые данные файла.");

        fileBytes = Convert.FromBase64String(fileBase64);
      }
      catch (Exception ex)
      {
        result.Errors.Add($"Ошибка чтения файла (Base64): {ex.Message}");
        return result;
      }

      try
      {
        XDocument xDoc;
        using (var stream = new MemoryStream(fileBytes))
          xDoc = XDocument.Load(stream);

        var dataElements = xDoc.Element("Data");
        if (dataElements == null)
        {
          result.Errors.Add("Корневой элемент <Data> отсутствует в XML.");
          return result;
        }

        var typeMap = new Dictionary<string, Enumeration>(StringComparer.OrdinalIgnoreCase);
        typeMap.Add(NSI.Mapping.EntityType.DocumentKind.Value, NSI.Mapping.EntityType.DocumentKind);
        typeMap.Add(NSI.Mapping.EntityType.ContrCategory.Value, NSI.Mapping.EntityType.ContrCategory);
        
        var items = new List<Structures.Module.MappingRecordInfo>();
        foreach (var element in dataElements.Elements("element"))
        {
          result.TotalCount++;

          var xmlType = element.Element("Type")?.Value;
          var xmlId = element.Element("Id")?.Value;
          var xmlName = element.Element("Name")?.Value;
          var xmlExternalId = element.Element("ExternalId")?.Value;
          
          if (string.IsNullOrWhiteSpace(xmlType))
          {
            result.Errors.Add($"Тип справочника (Type) отсутствует. Id={xmlId}, Name={xmlName}");
            continue;
          }

          if (string.IsNullOrWhiteSpace(xmlId))
          {
            result.Errors.Add($"Id отсутствует. Type={xmlType}, Name={xmlName}");
            continue;
          }

          Enumeration type;
          if (!typeMap.TryGetValue(xmlType, out type))
          {
            result.Errors.Add($"Неизвестный тип справочника: {xmlType}. Id={xmlId}, Name={xmlName}");
            continue;
          }

          long id;
          if (!long.TryParse(xmlId, out id))
          {
            result.Errors.Add($"Некорректный Id: {xmlId}. Type={xmlType}, Name={xmlName}");
            continue;
          }

          items.Add(Structures.Module.MappingRecordInfo.Create(
            type.Value,
            id,
            xmlName,
            xmlExternalId));
        }

        var existingByType = new Dictionary<string, Dictionary<long, NSI.IMapping>>(StringComparer.OrdinalIgnoreCase);

        foreach (var grp in items.GroupBy(x => x.Type))
        {
          var ids = grp.Select(x => x.Id).Distinct().ToList();

          Enumeration entityType;
          if (!typeMap.TryGetValue(grp.Key ?? "", out entityType))
            continue;
          
          var existing = NSI.Mappings.GetAll()
            .Where(x => x.EntityType == entityType)
            .Where(x => ids.Contains((long)x.SourceId))
            .ToList();
          
          existingByType[grp.Key] = existing.ToDictionary(x => (long)x.SourceId, x => x);
        }

        foreach (var item in items)
        {
          Dictionary<long, NSI.IMapping> dict;
          if (!existingByType.TryGetValue(item.Type ?? "", out dict))
            dict = new Dictionary<long, NSI.IMapping>();;

          var entityType = typeMap[item.Type];
          
          NSI.IMapping record;
          if (!dict.TryGetValue(item.Id, out record))
          {
            record = NSI.Mappings.Create();
            record.SourceId = item.Id;
            record.EntityType = entityType;

            dict[item.Id] = record;
            existingByType[item.Type] = dict;
          }

          if (record.SourceName != item.Name)
            record.SourceName = item.Name;

          if (record.SourceExternalId != item.ExternalId)
            record.SourceExternalId = item.ExternalId;

          if (record.State.IsInserted)
          {
            record.Save();
            result.ImportedCount++;
          }
          else if (record.State.IsChanged)
          {
            record.Save();
            result.ChangedCount++;
          }
          else
            result.SkippedCount++;
        }
      }
      catch (Exception ex)
      {
        result.Errors.Add($"Критическая ошибка: {ex.Message}");
      }

      return result;
    }    
    
    /// <summary>
    /// Экспорт справочника "Матрица ответственности" для переноса в прод. среду
    /// </summary>
    [Remote]
    public IZip ExportResponsibilityMatrix()
    {
      var dataBookRecords = NSI.ResponsibilityMatrices.GetAll()
        .Where(x => x.DocumentKind != null)
        .ToList();

      var root = new XElement("Data");
      foreach (var x in dataBookRecords)
      {
        var contractCategoriesEl = new XElement("ContractCategories");
        
        foreach (var c in x.ContractCategories)
        {
          if (c.Category != null)
            contractCategoriesEl.Add(new XElement("ContractCategoryId", c.Category.Id));
        }

        root.Add(new XElement("element",
                              new XElement("DocumentKindId", x.DocumentKind.Id),
                              contractCategoriesEl,
                              new XElement("ResponsibleLawyer", x.ResponsibleLawyer != null ? x.ResponsibleLawyer.DisplayValue : string.Empty),
                              new XElement("ResponsibleAccountant", x.ResponsibleAccountant != null ? x.ResponsibleAccountant.DisplayValue : string.Empty),
                              new XElement("ResponsibleAHD", x.ResponsibleAHD != null ? x.ResponsibleAHD.DisplayValue : string.Empty),
                              new XElement("ConclusionDKR", x.ConclusionDKR.GetValueOrDefault()),
                              new XElement("BatchProcessing", x.BatchProcessing.GetValueOrDefault())
                             ));
      }
      
      var xmlData = new XDocument(root);
      return CreateZipFromXml(xmlData, "export.zip", "data");
    }
    
    /// <summary>
    /// Экспорт справочника "Соответствие видов договоров и документов на оплату" для переноса в прод. среду
    /// </summary>
    [Remote]
    public IZip ExportContractsVsPaymentDoc()
    {
      var dataBookRecords = NSI.ContractsVsPaymentDocs.GetAll()
        .Where(x => x.DocumentKind != null)
        .ToList();
      
      var root = new XElement("Data",
                              dataBookRecords.Select(x =>
                                                     new XElement("element",
                                                                  new XElement("DocumentKindId", x.DocumentKind.Id),
                                                                  new XElement("CategoryId", x.Category != null ? (long?)x.Category.Id : null),
                                                                  new XElement("CounterpartyType", x.CounterpartyType.HasValue ? x.CounterpartyType.Value.Value : string.Empty),
                                                                  new XElement("PBIsPaymentContract", x.PBIsPaymentContract.GetValueOrDefault()),
                                                                  new XElement("PBIsPaymentInvoice", x.PBIsPaymentInvoice.GetValueOrDefault()),
                                                                  new XElement("PBIsPaymentTaxInvoice", x.PBIsPaymentTaxInvoice.GetValueOrDefault()),
                                                                  new XElement("PBIsPaymentAct", x.PBIsPaymentAct.GetValueOrDefault()),
                                                                  new XElement("PBIsPaymentOrder", x.PBIsPaymentOrder.GetValueOrDefault()),
                                                                  new XElement("PCBIsPaymentContract", x.PCBIsPaymentContract.GetValueOrDefault()),
                                                                  new XElement("PCBIsPaymentInvoice", x.PCBIsPaymentInvoice.GetValueOrDefault()),
                                                                  new XElement("PCBIsPaymentTaxInvoice", x.PCBIsPaymentTaxInvoice.GetValueOrDefault()),
                                                                  new XElement("PCBIsPaymentAct", x.PCBIsPaymentAct.GetValueOrDefault()),
                                                                  new XElement("PCBIsPaymentWaybill", x.PCBIsPaymentWaybill.GetValueOrDefault()),
                                                                  new XElement("PCBIsPaymentInsurance", x.PCBIsPaymentInsurance.GetValueOrDefault())
                                                                  
                                                                 )));
      var xmlData = new XDocument(root);
      return CreateZipFromXml(xmlData, "export.zip", "data");
    }
    
    /// <summary>
    /// Экспорт справочника "Ставка налогов" для переноса в прод. среду
    /// </summary>
    [Remote]
    public IZip ExportTaxRate()
    {
      var dataBookRecords = NSI.TaxRates.GetAll().ToList();
      var root = new XElement("Data",
                              dataBookRecords.Select(x =>
                                              new XElement("element",
                                                           new XElement("Name", x.Name),
                                                           new XElement("DocumentKindId", x.DocumentKind != null ? (long?)x.DocumentKind.Id : null),
                                                           new XElement("CategoryId", x.Category != null ? (long?)x.Category.Id : null),
                                                           new XElement("CounterpartyType", x.CounterpartyType.HasValue ? x.CounterpartyType.Value.Value : string.Empty),
                                                           new XElement("TaxResident", x.TaxResident.GetValueOrDefault()),
                                                           new XElement("VAT", x.VAT.GetValueOrDefault()),
                                                           new XElement("VATmethod", x.VATMethod.HasValue ? x.VATMethod.Value.Value : string.Empty),
                                                           new XElement("IncomeTax", x.IncomeTax.GetValueOrDefault()),
                                                           new XElement("IncomeTaxMethod", x.IncomeTaxMethod.HasValue ? x.IncomeTaxMethod.Value.Value : string.Empty),
                                                           new XElement("IncomeTaxLimit", x.IncomeTaxLimit.GetValueOrDefault()),
                                                           new XElement("PensionContribution", x.PensionContribution.GetValueOrDefault()),
                                                           new XElement("PensionContributionMethod", x.PensionContributionMethod.HasValue ? x.PensionContributionMethod.Value.Value : string.Empty),
                                                           new XElement("FSZN", x.FSZN.GetValueOrDefault()),
                                                           new XElement("FSZNMethod", x.FSZNMethod.HasValue ? x.FSZNMethod.Value.Value : string.Empty)
                                                          )));
      var xmlData = new XDocument(root);
      return CreateZipFromXml(xmlData, "export.zip", "data");
    }
    
    /// <summary>
    /// Упаковать XDocument в IZip
    /// </summary>
    /// <returns></returns>
    public IZip CreateZipFromXml(System.Xml.Linq.XDocument xmlData, string zipFileName, string xmlEntryName)
    {
      byte[] xmlBytes;
      using (var ms = new MemoryStream())
      {
        using (var xw = XmlWriter.Create(ms, new XmlWriterSettings
                                         {
                                           Encoding = new UTF8Encoding(false),
                                           Indent = true,
                                           OmitXmlDeclaration = false
                                         }))
        {
          xmlData.WriteTo(xw);
          xw.Flush();
        }
        xmlBytes = ms.ToArray();
      }

      var zip = Sungero.Core.Zip.Create();
      zip.Add(xmlBytes, xmlEntryName, "xml");
      zip.Save(zipFileName);
      return zip;
    }
    
    /// <summary>
    /// Получить запись матрицы ответственности по договору
    /// </summary>
    [Public]
    public NSI.IResponsibilityMatrix GetResponsibilityMatrix(litiko.Eskhata.IContract contract)
    {
      return NSI.ResponsibilityMatrices.GetAll()
        .Where(x => Equals(x.DocumentKind, contract.DocumentKind))
        .Where(x => x.ContractCategories.Any(c => Equals(c.Category, contract.DocumentGroup)))
        .FirstOrDefault();
    }    
    
    /// <summary>
    /// Получить запись справочника "Соответствие видов договоров и документов на оплату"
    /// </summary>
    [Public]
    public NSI.IContractsVsPaymentDoc GetContractsVsPaymentDoc(litiko.Eskhata.IContract contract, Sungero.Parties.ICounterparty counterparty)
    {
      Sungero.Core.Enumeration? counterpartyType;
      if (Sungero.Parties.Companies.Is(counterparty))
        counterpartyType = NSI.TaxRate.CounterpartyType.Company;
      else if (Sungero.Parties.People.Is(counterparty))
        counterpartyType = NSI.TaxRate.CounterpartyType.Person;
      else if (Sungero.Parties.Banks.Is(counterparty))
        counterpartyType = NSI.TaxRate.CounterpartyType.Bank;
      else
        counterpartyType = null;
      
      return NSI.ContractsVsPaymentDocs.GetAll()
        .Where(x => Equals(x.DocumentKind, contract.DocumentKind) && Equals(x.Category, contract.DocumentGroup) && Equals(x.CounterpartyType, counterpartyType))        
        .FirstOrDefault();
    }
    
    /// <summary>
    /// Добавление записей справочника в маппинг
    /// </summary>
    /// <param name="entityType">Тип справочника</param>
    /// <param name="objIds">Список ИД-ов</param>
    [Public, Remote]
    public string AddToMapping(string entityType, List<long> objIds)
    {                  
      Uri uri = new Uri(Hyperlinks.Get(Users.Current));
      if (uri.Host == "sed.eskhata.com")
        return "Действие доступно только в тестовой среде";
      
      int created = 0;
      int modified = 0;
      string result = $"Создано: {created}{Environment.NewLine}Обновлено: {modified}";
      
      if (objIds == null || !objIds.Any())
        return result;
            
      var typeMap = new Dictionary<string, Enumeration>(StringComparer.OrdinalIgnoreCase);
		  typeMap.Add(NSI.Mapping.EntityType.DocumentKind.Value, NSI.Mapping.EntityType.DocumentKind);
		  typeMap.Add(NSI.Mapping.EntityType.ContrCategory.Value, NSI.Mapping.EntityType.ContrCategory);
      
      Enumeration type;
      if (!typeMap.TryGetValue(entityType ?? "", out type))
        return $"Неизвестный тип справочника: {entityType}";
      
      var ids = objIds.Distinct().ToList();
      var mappingRecordsToAdd = new List<litiko.NSI.Structures.Module.MappingRecordInfo>();
      
      var existing = NSI.Mappings.GetAll()
        .Where(x => x.EntityType == type)
        .Where(x => ids.Contains(x.SourceId.Value))
        .ToList()
        .ToDictionary(x => x.SourceId.Value);
      
      if (type == NSI.Mapping.EntityType.DocumentKind)
      {
        mappingRecordsToAdd = Eskhata.DocumentKinds.GetAll()
          .Where(x => ids.Contains(x.Id))
          .Select(x => new Structures.Module.MappingRecordInfo
                  {
                    Id = x.Id,
                    Name = x.Name,
                    ExternalId = x.ExternalIdlitiko
                  })
          .ToList();
      }
      else if (type == NSI.Mapping.EntityType.ContrCategory)
      {
        mappingRecordsToAdd = Sungero.Contracts.ContractCategories.GetAll()
          .Where(x => ids.Contains(x.Id))
          .Select(x => new Structures.Module.MappingRecordInfo
                  {
                    Id = x.Id,
                    Name = x.Name,
                    ExternalId = x.ExternalIdlitiko
                  })
          .ToList();
      }

      foreach (var item in mappingRecordsToAdd)
      {
        NSI.IMapping record;
        if (!existing.TryGetValue(item.Id, out record))
        {
          record = NSI.Mappings.Create();
          record.EntityType = type;
          record.SourceId = item.Id;
          existing[item.Id] = record;
        }
        
        if (record.SourceId != item.Id)
          record.SourceId = item.Id;
        
        if (record.SourceName != item.Name)
          record.SourceName = item.Name;
        
        if (record.SourceExternalId != item.ExternalId)
          record.SourceExternalId = item.ExternalId;        
        
        if (record.State.IsInserted)
        {
          record.Save();
          created++;
        }
        else if (record.State.IsChanged)
        {
          record.Save();
          modified++;
        }
        
      }
      
      result = $"Создано: {created}{Environment.NewLine}Обновлено: {modified}";
      return result;
    } 
  }
}