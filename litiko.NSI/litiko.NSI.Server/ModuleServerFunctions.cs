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
    #region Перенос справичников модуля "Договоры" в продуктивную среду    
    
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

      var dataElements = GetDataElements(fileBase64, result);
      if (dataElements == null)
        return result;

      try
      {
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
    /// Импорт справочника "Матрица ответственности"
    /// </summary>
    [Remote]
    public Structures.Module.IResultImportXml ImportResponsibilityMatrix(string fileBase64)
    {
      var result = Structures.Module.ResultImportXml.Create();
      result.Errors = new List<string>();

      var dataElements = GetDataElements(fileBase64, result);
      if (dataElements == null)
        return result;

      try
      {                
        var xmlSourceDocKindIds = dataElements.Elements("element")
          .Select(e => (long?)e.Element("DocumentKindId"))
          .Where(id => id.HasValue)
          .Select(id => id.Value)
          .Distinct()
          .ToList();

        var xmlSourceCategoryIds = dataElements.Elements("element")
          .SelectMany(e => e.Element("ContractCategories")?.Elements("ContractCategoryId") ?? Enumerable.Empty<XElement>())
          .Select(x => (long?)x)
          .Where(id => id.HasValue)
          .Select(id => id.Value)
          .Distinct()
          .ToList();
        
        var docKindMap = NSI.Mappings.GetAll()
          .Where(m => m.EntityType == NSI.Mapping.EntityType.DocumentKind)
          .Where(m => m.SourceId.HasValue && m.DestId.HasValue)
          .Where(m => xmlSourceDocKindIds.Contains(m.SourceId.Value))
          .ToDictionary(m => m.SourceId.Value, m => m.DestId.Value);

        var categoryMap = NSI.Mappings.GetAll()
          .Where(m => m.EntityType == NSI.Mapping.EntityType.ContrCategory)
          .Where(m => m.SourceId.HasValue && m.DestId.HasValue)
          .Where(m => xmlSourceCategoryIds.Contains(m.SourceId.Value))
          .ToDictionary(m => m.SourceId.Value, m => m.DestId.Value);
        
        var destDocKindIds = xmlSourceDocKindIds
          .Where(src => docKindMap.ContainsKey(src))
          .Select(src => docKindMap[src])
          .Distinct()
          .ToList();

        var existingByDestDocKindId = NSI.ResponsibilityMatrices.GetAll()
          .Where(x => x.DocumentKind != null && destDocKindIds.Contains(x.DocumentKind.Id))
          .ToList()
          .ToDictionary(x => x.DocumentKind.Id);
        
        var docKindsById = litiko.Eskhata.DocumentKinds.GetAll()
          .Where(dk => destDocKindIds.Contains(dk.Id))
          .ToList()
          .ToDictionary(dk => dk.Id);
        
        var activeRecipientsByName = Recipients.GetAll()
          .Where(x => x.Status == Sungero.CoreEntities.Recipient.Status.Active)
          .ToList()
          .GroupBy(x => x.DisplayValue)
          .ToDictionary(g => g.Key, g => g.FirstOrDefault());        

        var notFoundRecipients = new List<string>();
        foreach (var element in dataElements.Elements("element"))
        {
          result.TotalCount++;

          var srcDocKindStr = element.Element("DocumentKindId")?.Value;
          long srcDocKindId;
          if (!long.TryParse(srcDocKindStr, out srcDocKindId))
          {
            result.Errors.Add($"Некорректный DocumentKindId: {srcDocKindStr}.");
            continue;
          }
          
          long destDocKindId;
          if (!docKindMap.TryGetValue(srcDocKindId, out destDocKindId))
          {
            result.Errors.Add($"Не найден mapping DocumentKind: SourceId={srcDocKindId}.");
            continue;
          }

          NSI.IResponsibilityMatrix record;
          if (!existingByDestDocKindId.TryGetValue(destDocKindId, out record))
          {
            record = NSI.ResponsibilityMatrices.Create();

            litiko.Eskhata.IDocumentKind docKind;
            if (!docKindsById.TryGetValue(destDocKindId, out docKind))
            {
              result.Errors.Add($"Не найден DocumentKind по DestId={destDocKindId} (из mapping).");
              continue;
            }
            record.DocumentKind = docKind;
            
            existingByDestDocKindId[destDocKindId] = record;
          }
          
          #region Категории
          var srcCategoryIds = (element.Element("ContractCategories")?.Elements("ContractCategoryId")
                                ?? Enumerable.Empty<XElement>())
            .Select(x => (long?)x)
            .Where(x => x.HasValue)
            .Select(x => x.Value)
            .Distinct()
            .ToList();

          var destCategoryIds = new List<long>();
          foreach (var srcCatId in srcCategoryIds)
          {
            long destCatId;
            if (categoryMap.TryGetValue(srcCatId, out destCatId))
              destCategoryIds.Add(destCatId);
            else
              result.Errors.Add($"Не найден mapping ContractCategory: SourceId={srcCatId} (DocumentKind={srcDocKindId}).");
          }

          destCategoryIds = destCategoryIds.Distinct().ToList();

          // удалить лишние
          var toRemove = record.ContractCategories
            .Where(cc => cc.Category != null && !destCategoryIds.Contains(cc.Category.Id))
            .ToList();
          foreach (var cc in toRemove)
            record.ContractCategories.Remove(cc);

          var currentIds = record.ContractCategories
            .Where(cc => cc.Category != null)
            .Select(cc => cc.Category.Id)
            .ToList();

          // добавить недостающие
          var toAdd = destCategoryIds.Where(id => !currentIds.Contains(id)).ToList();
          foreach (var categoryId in toAdd)
            record.ContractCategories.AddNew().Category = Sungero.Contracts.ContractCategories.Get(categoryId);

          #endregion
                    
          #region Остальные поля          
          var xmlResponsibleLawyer = element.Element("ResponsibleLawyer")?.Value?.Trim();
          var xmlResponsibleAccountant = element.Element("ResponsibleAccountant")?.Value?.Trim();
          var xmlResponsibleAHD = element.Element("ResponsibleAHD")?.Value?.Trim();
          
          if (!string.IsNullOrEmpty(xmlResponsibleLawyer))
          {
            Sungero.CoreEntities.IRecipient responsibleLawyer;

            if (!activeRecipientsByName.TryGetValue(xmlResponsibleLawyer, out responsibleLawyer))
              notFoundRecipients.Add(xmlResponsibleLawyer);            
            
            if (!Equals(record.ResponsibleLawyer, responsibleLawyer))
              record.ResponsibleLawyer = responsibleLawyer;
          }

          if (!string.IsNullOrEmpty(xmlResponsibleAccountant))
          {
            Sungero.CoreEntities.IRecipient responsibleAccountant;
            if (!activeRecipientsByName.TryGetValue(xmlResponsibleAccountant, out responsibleAccountant))
              notFoundRecipients.Add(xmlResponsibleAccountant);

            if (!Equals(record.ResponsibleAccountant, responsibleAccountant))
              record.ResponsibleAccountant = responsibleAccountant;
          }

          if (!string.IsNullOrEmpty(xmlResponsibleAHD))
          {
            Sungero.CoreEntities.IRecipient responsibleAHD;
            if (!activeRecipientsByName.TryGetValue(xmlResponsibleAHD, out responsibleAHD))
              notFoundRecipients.Add(xmlResponsibleAHD);

            if (!Equals(record.ResponsibleAHD, responsibleAHD))
              record.ResponsibleAHD = responsibleAHD;
          }

          var xmlConclusionDKR = (bool?)element.Element("ConclusionDKR") ?? false;
          if (record.ConclusionDKR.GetValueOrDefault() != xmlConclusionDKR)
            record.ConclusionDKR = xmlConclusionDKR;

          var xmlBatchProcessing = (bool?)element.Element("BatchProcessing") ?? false;
          if (record.BatchProcessing.GetValueOrDefault() != xmlBatchProcessing)
            record.BatchProcessing = xmlBatchProcessing;
          
          #endregion

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
        
        if (notFoundRecipients.Any())
          result.Errors.Add("Не найдены сотрудники/роли: " + string.Join(Environment.NewLine, notFoundRecipients.Distinct().OrderBy(x => x)));
        
      }
      catch (Exception ex)
      {
        result.Errors.Add($"Критическая ошибка: {ex.Message}");
      }

      return result;
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
    /// Импорт справочника "Соответствие видов договоров и документов на оплату"
    /// </summary>
    [Remote]
    public Structures.Module.IResultImportXml ImportContractsVsPaymentDoc(string fileBase64)
    {
      var result = Structures.Module.ResultImportXml.Create();
      result.Errors = new List<string>();

      var dataElements = GetDataElements(fileBase64, result);
      if (dataElements == null)
        return result;

      try
      {
        var xmlSourceDocKindIds = dataElements.Elements("element")
          .Select(e => (long?)e.Element("DocumentKindId"))
          .Where(id => id.HasValue)
          .Select(id => id.Value)
          .Distinct()
          .ToList();

        var xmlSourceCategoryIds = dataElements.Elements("element")
          .Select(e => (long?)e.Element("CategoryId"))
          .Where(id => id.HasValue)
          .Select(id => id.Value)
          .Distinct()
          .ToList();

        var docKindMap = NSI.Mappings.GetAll()
          .Where(m => m.EntityType == NSI.Mapping.EntityType.DocumentKind)
          .Where(m => m.SourceId.HasValue && m.DestId.HasValue)
          .Where(m => xmlSourceDocKindIds.Contains(m.SourceId.Value))
          .ToDictionary(m => m.SourceId.Value, m => m.DestId.Value);

        var categoryMap = NSI.Mappings.GetAll()
          .Where(m => m.EntityType == NSI.Mapping.EntityType.ContrCategory)
          .Where(m => m.SourceId.HasValue && m.DestId.HasValue)
          .Where(m => xmlSourceCategoryIds.Contains(m.SourceId.Value))
          .ToDictionary(m => m.SourceId.Value, m => m.DestId.Value);

        var destDocKindIds = xmlSourceDocKindIds
          .Where(src => docKindMap.ContainsKey(src))
          .Select(src => docKindMap[src])
          .Distinct()
          .ToList();

        var destCategoriesIds = xmlSourceCategoryIds
          .Where(src => categoryMap.ContainsKey(src))
          .Select(src => categoryMap[src])
          .Distinct()
          .ToList();

        var docKindsById = litiko.Eskhata.DocumentKinds.GetAll()
          .Where(dk => destDocKindIds.Contains(dk.Id))
          .ToList()
          .ToDictionary(dk => dk.Id);

        var categoriesById = Sungero.Contracts.ContractCategories.GetAll()
          .Where(c => destCategoriesIds.Contains(c.Id))
          .ToList()
          .ToDictionary(c => c.Id);

        var typeMap = new Dictionary<string, Enumeration?>(StringComparer.OrdinalIgnoreCase);
        typeMap.Add(NSI.ContractsVsPaymentDoc.CounterpartyType.Company.Value, NSI.ContractsVsPaymentDoc.CounterpartyType.Company);
        typeMap.Add(NSI.ContractsVsPaymentDoc.CounterpartyType.Person.Value, NSI.ContractsVsPaymentDoc.CounterpartyType.Person);
        typeMap.Add(NSI.ContractsVsPaymentDoc.CounterpartyType.Bank.Value, NSI.ContractsVsPaymentDoc.CounterpartyType.Bank);

        // ключ: "docKindId|categoryId|null|counterpartyTypeValue|null"
        Func<long, long?, Enumeration?, string> makeKey = (dkId, catId, cpt) =>
          dkId.ToString() + "|" +
          (catId.HasValue ? catId.Value.ToString() : "null") + "|" +
          (cpt.HasValue ? cpt.Value.Value : "null");

        // Существующие записи словарём по ключу (в памяти)
        var existingByKey = NSI.ContractsVsPaymentDocs.GetAll()
          .Where(x => x.DocumentKind != null)
          .ToList()
          .ToDictionary(x => makeKey(x.DocumentKind.Id,
                                     x.Category != null ? (long?)x.Category.Id : null,
                                     x.CounterpartyType),
                        x => x);

        foreach (var element in dataElements.Elements("element"))
        {
          result.TotalCount++;

          // --------- DocumentKind: Source -> Dest -> Entity ---------
          var srcDocKindStr = element.Element("DocumentKindId")?.Value;
          long srcDocKindId;
          if (!long.TryParse(srcDocKindStr, out srcDocKindId))
          {
            result.Errors.Add($"Некорректный DocumentKindId: {srcDocKindStr}.");
            continue;
          }

          long destDocKindId;
          if (!docKindMap.TryGetValue(srcDocKindId, out destDocKindId))
          {
            result.Errors.Add($"Не найден mapping DocumentKind: SourceId={srcDocKindId}.");
            continue;
          }

          litiko.Eskhata.IDocumentKind docKind;
          if (!docKindsById.TryGetValue(destDocKindId, out docKind))
          {
            result.Errors.Add($"Не найден DocumentKind по DestId={destDocKindId} (из mapping).");
            continue;
          }

          // --------- Category: Source -> Dest -> Entity (может быть null) ---------
          Sungero.Contracts.IContractCategory category = null;
          long? destCategoryIdNullable = null;

          var srcCategoryStr = element.Element("CategoryId")?.Value;
          if (!string.IsNullOrEmpty(srcCategoryStr))
          {
            long srcCategoryId;
            if (!long.TryParse(srcCategoryStr, out srcCategoryId))
            {
              result.Errors.Add($"Некорректный CategoryId: {srcCategoryStr}.");
              continue;
            }

            long destCategoryId;
            if (!categoryMap.TryGetValue(srcCategoryId, out destCategoryId))
            {
              result.Errors.Add($"Не найден mapping Category: SourceId={srcCategoryId}.");
              continue;
            }

            if (!categoriesById.TryGetValue(destCategoryId, out category))
            {
              result.Errors.Add($"Не найден Category по DestId={destCategoryId} (из mapping).");
              continue;
            }

            destCategoryIdNullable = destCategoryId;
          }

          // --------- CounterpartyType (может быть null) ---------
          var xmlCounterpartyType = element.Element("CounterpartyType")?.Value?.Trim();
          Enumeration? counterpartyType = null;

          if (!string.IsNullOrEmpty(xmlCounterpartyType))
          {
            if (!typeMap.TryGetValue(xmlCounterpartyType, out counterpartyType))
            {
              result.Errors.Add($"Неизвестный тип контрагента CounterpartyType={xmlCounterpartyType}.");
              continue;
            }
          }

          // --------- Find/Create by composite key ---------
          var key = makeKey(destDocKindId, destCategoryIdNullable, counterpartyType);

          NSI.IContractsVsPaymentDoc record;
          if (!existingByKey.TryGetValue(key, out record))
          {
            record = NSI.ContractsVsPaymentDocs.Create();
            record.DocumentKind = docKind;
            record.Category = category;
            record.CounterpartyType = counterpartyType;

            existingByKey[key] = record;
          }
          else
          {
            // синхронизация ключевых полей (на всякий случай)
            if (!Equals(record.DocumentKind, docKind))
              record.DocumentKind = docKind;

            if (!Equals(record.Category, category))
              record.Category = category;

            if (record.CounterpartyType != counterpartyType)
              record.CounterpartyType = counterpartyType;
          }

          // --------- Остальные флаги ---------
          var xmlPBIsPaymentContract = (bool?)element.Element("PBIsPaymentContract") ?? false;
          if (record.PBIsPaymentContract.GetValueOrDefault() != xmlPBIsPaymentContract)
            record.PBIsPaymentContract = xmlPBIsPaymentContract;

          var xmlPBIsPaymentInvoice = (bool?)element.Element("PBIsPaymentInvoice") ?? false;
          if (record.PBIsPaymentInvoice.GetValueOrDefault() != xmlPBIsPaymentInvoice)
            record.PBIsPaymentInvoice = xmlPBIsPaymentInvoice;

          var xmlPBIsPaymentTaxInvoice = (bool?)element.Element("PBIsPaymentTaxInvoice") ?? false;
          if (record.PBIsPaymentTaxInvoice.GetValueOrDefault() != xmlPBIsPaymentTaxInvoice)
            record.PBIsPaymentTaxInvoice = xmlPBIsPaymentTaxInvoice;

          var xmlPBIsPaymentAct = (bool?)element.Element("PBIsPaymentAct") ?? false;
          if (record.PBIsPaymentAct.GetValueOrDefault() != xmlPBIsPaymentAct)
            record.PBIsPaymentAct = xmlPBIsPaymentAct;

          var xmlPBIsPaymentOrder = (bool?)element.Element("PBIsPaymentOrder") ?? false;
          if (record.PBIsPaymentOrder.GetValueOrDefault() != xmlPBIsPaymentOrder)
            record.PBIsPaymentOrder = xmlPBIsPaymentOrder;

          var xmlPCBIsPaymentContract = (bool?)element.Element("PCBIsPaymentContract") ?? false;
          if (record.PCBIsPaymentContract.GetValueOrDefault() != xmlPCBIsPaymentContract)
            record.PCBIsPaymentContract = xmlPCBIsPaymentContract;

          var xmlPCBIsPaymentInvoice = (bool?)element.Element("PCBIsPaymentInvoice") ?? false;
          if (record.PCBIsPaymentInvoice.GetValueOrDefault() != xmlPCBIsPaymentInvoice)
            record.PCBIsPaymentInvoice = xmlPCBIsPaymentInvoice;

          var xmlPCBIsPaymentTaxInvoice = (bool?)element.Element("PCBIsPaymentTaxInvoice") ?? false;
          if (record.PCBIsPaymentTaxInvoice.GetValueOrDefault() != xmlPCBIsPaymentTaxInvoice)
            record.PCBIsPaymentTaxInvoice = xmlPCBIsPaymentTaxInvoice;

          var xmlPCBIsPaymentAct = (bool?)element.Element("PCBIsPaymentAct") ?? false;
          if (record.PCBIsPaymentAct.GetValueOrDefault() != xmlPCBIsPaymentAct)
            record.PCBIsPaymentAct = xmlPCBIsPaymentAct;

          var xmlPCBIsPaymentWaybill = (bool?)element.Element("PCBIsPaymentWaybill") ?? false;
          if (record.PCBIsPaymentWaybill.GetValueOrDefault() != xmlPCBIsPaymentWaybill)
            record.PCBIsPaymentWaybill = xmlPCBIsPaymentWaybill;

          var xmlPCBIsPaymentInsurance = (bool?)element.Element("PCBIsPaymentInsurance") ?? false;
          if (record.PCBIsPaymentInsurance.GetValueOrDefault() != xmlPCBIsPaymentInsurance)
            record.PCBIsPaymentInsurance = xmlPCBIsPaymentInsurance;

          // --------- Save + counters ---------
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
    /// Экспорт справочника "Ставки налогов" для переноса в прод. среду
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
    /// Импорт справочника "Ставки налогов"
    /// </summary>
    [Remote]
    public Structures.Module.IResultImportXml ImportTaxRate(string fileBase64)
    {
      var result = Structures.Module.ResultImportXml.Create();
      result.Errors = new List<string>();

      var dataElements = GetDataElements(fileBase64, result);
      if (dataElements == null)
        return result;

      try
      {
        var xmlSourceDocKindIds = dataElements.Elements("element")
          .Select(e => (long?)e.Element("DocumentKindId"))
          .Where(id => id.HasValue)
          .Select(id => id.Value)
          .Distinct()
          .ToList();

        var xmlSourceCategoryIds = dataElements.Elements("element")
          .Select(e => (long?)e.Element("CategoryId"))
          .Where(id => id.HasValue)
          .Select(id => id.Value)
          .Distinct()
          .ToList();

        var docKindMap = NSI.Mappings.GetAll()
          .Where(m => m.EntityType == NSI.Mapping.EntityType.DocumentKind)
          .Where(m => m.SourceId.HasValue && m.DestId.HasValue)
          .Where(m => xmlSourceDocKindIds.Contains(m.SourceId.Value))
          .ToDictionary(m => m.SourceId.Value, m => m.DestId.Value);

        var categoryMap = NSI.Mappings.GetAll()
          .Where(m => m.EntityType == NSI.Mapping.EntityType.ContrCategory)
          .Where(m => m.SourceId.HasValue && m.DestId.HasValue)
          .Where(m => xmlSourceCategoryIds.Contains(m.SourceId.Value))
          .ToDictionary(m => m.SourceId.Value, m => m.DestId.Value);

        var destDocKindIds = xmlSourceDocKindIds
          .Where(src => docKindMap.ContainsKey(src))
          .Select(src => docKindMap[src])
          .Distinct()
          .ToList();

        var destCategoriesIds = xmlSourceCategoryIds
          .Where(src => categoryMap.ContainsKey(src))
          .Select(src => categoryMap[src])
          .Distinct()
          .ToList();

        var docKindsById = litiko.Eskhata.DocumentKinds.GetAll()
          .Where(dk => destDocKindIds.Contains(dk.Id))
          .ToList()
          .ToDictionary(dk => dk.Id);

        var categoriesById = Sungero.Contracts.ContractCategories.GetAll()
          .Where(c => destCategoriesIds.Contains(c.Id))
          .ToList()
          .ToDictionary(c => c.Id);

        var typeMap = new Dictionary<string, Enumeration?>(StringComparer.OrdinalIgnoreCase);
        typeMap.Add(NSI.TaxRate.CounterpartyType.Company.Value, NSI.TaxRate.CounterpartyType.Company);
        typeMap.Add(NSI.TaxRate.CounterpartyType.Person.Value, NSI.TaxRate.CounterpartyType.Person);
        typeMap.Add(NSI.TaxRate.CounterpartyType.Bank.Value, NSI.TaxRate.CounterpartyType.Bank);
        
        var vatMethodMap = new Dictionary<string, Enumeration?>(StringComparer.OrdinalIgnoreCase);
        vatMethodMap.Add(NSI.TaxRate.VATMethod.DuringCustoms.Value, NSI.TaxRate.VATMethod.DuringCustoms);
        vatMethodMap.Add(NSI.TaxRate.VATMethod.Included.Value, NSI.TaxRate.VATMethod.Included);
        vatMethodMap.Add(NSI.TaxRate.VATMethod.OnTop.Value, NSI.TaxRate.VATMethod.OnTop);
        
        var incomeTaxMethodMap = new Dictionary<string, Enumeration?>(StringComparer.OrdinalIgnoreCase);
        incomeTaxMethodMap.Add(NSI.TaxRate.IncomeTaxMethod.FromAmount1.Value, NSI.TaxRate.IncomeTaxMethod.FromAmount1);
        incomeTaxMethodMap.Add(NSI.TaxRate.IncomeTaxMethod.FromAmount2.Value, NSI.TaxRate.IncomeTaxMethod.FromAmount2);
        incomeTaxMethodMap.Add(NSI.TaxRate.IncomeTaxMethod.FromAmount3.Value, NSI.TaxRate.IncomeTaxMethod.FromAmount3);
        incomeTaxMethodMap.Add(NSI.TaxRate.IncomeTaxMethod.NotHold.Value, NSI.TaxRate.IncomeTaxMethod.NotHold);

        var fsznMethodMap = new Dictionary<string, Enumeration?>(StringComparer.OrdinalIgnoreCase);
        fsznMethodMap.Add(NSI.TaxRate.FSZNMethod.AccruedOnTop.Value, NSI.TaxRate.FSZNMethod.AccruedOnTop);
        fsznMethodMap.Add(NSI.TaxRate.FSZNMethod.NotAccrued.Value, NSI.TaxRate.FSZNMethod.NotAccrued);
        
        var pensionContributionMethodMap = new Dictionary<string, Enumeration?>(StringComparer.OrdinalIgnoreCase);
        pensionContributionMethodMap.Add(NSI.TaxRate.PensionContributionMethod.FromAmount.Value, NSI.TaxRate.PensionContributionMethod.FromAmount);
        pensionContributionMethodMap.Add(NSI.TaxRate.PensionContributionMethod.NotHold.Value, NSI.TaxRate.PensionContributionMethod.NotHold);
        
        // ключ: "docKindId|categoryId|counterpartyType|taxResident"
        Func<long, long?, Enumeration?, bool, string> makeKey = (dkId, catId, cpt, taxResident) =>
          dkId.ToString() + "|" +
          (catId.HasValue ? catId.Value.ToString() : "null") + "|" +
          (cpt.HasValue ? cpt.Value.Value : "null") + "|" +
          (taxResident ? "true" : "false");

        var existingList = NSI.TaxRates.GetAll()
          .Where(x => x.DocumentKind != null)
          .ToList();

        var duplicates = existingList
          .GroupBy(x => makeKey(x.DocumentKind.Id,
                                x.Category != null ? (long?)x.Category.Id : null,
                                x.CounterpartyType,
                                x.TaxResident.GetValueOrDefault()))
          .Where(g => g.Count() > 1)
          .ToList();

        if (duplicates.Any())
          result.Errors.Add("В системе найдены дубли TaxRate по ключу (DocumentKind|Category|CounterpartyType|TaxResident): " +
                            string.Join(", ", duplicates.Select(g => g.Key).Take(20)) +
                            (duplicates.Count > 20 ? " ..." : ""));

        var existingByKey = existingList
          .GroupBy(x => makeKey(x.DocumentKind.Id,
                                x.Category != null ? (long?)x.Category.Id : null,
                                x.CounterpartyType,
                                x.TaxResident.GetValueOrDefault()))
          .ToDictionary(g => g.Key, g => g.First());

        foreach (var element in dataElements.Elements("element"))
        {
          result.TotalCount++;
          
          var xmlName = element.Element("Name")?.Value?.Trim();
          if (string.IsNullOrEmpty(xmlName))
          {
            result.Errors.Add($"Не заполнено Name.");
            continue;
          }
          
          // --------- DocumentKind: Source -> Dest -> Entity ---------
          var srcDocKindStr = element.Element("DocumentKindId")?.Value;
          long srcDocKindId;
          if (!long.TryParse(srcDocKindStr, out srcDocKindId))
          {
            result.Errors.Add($"Некорректный DocumentKindId: {srcDocKindStr}.");
            continue;
          }

          long destDocKindId;
          if (!docKindMap.TryGetValue(srcDocKindId, out destDocKindId))
          {
            result.Errors.Add($"Не найден mapping DocumentKind: SourceId={srcDocKindId}.");
            continue;
          }

          litiko.Eskhata.IDocumentKind docKind;
          if (!docKindsById.TryGetValue(destDocKindId, out docKind))
          {
            result.Errors.Add($"Не найден DocumentKind по DestId={destDocKindId} (из mapping).");
            continue;
          }

          // --------- Category: Source -> Dest -> Entity (может быть null) ---------
          Sungero.Contracts.IContractCategory category = null;
          long? destCategoryIdNullable = null;

          var srcCategoryStr = element.Element("CategoryId")?.Value;
          if (!string.IsNullOrEmpty(srcCategoryStr))
          {
            long srcCategoryId;
            if (!long.TryParse(srcCategoryStr, out srcCategoryId))
            {
              result.Errors.Add($"Некорректный CategoryId: {srcCategoryStr}.");
              continue;
            }

            long destCategoryId;
            if (!categoryMap.TryGetValue(srcCategoryId, out destCategoryId))
            {
              result.Errors.Add($"Не найден mapping Category: SourceId={srcCategoryId}.");
              continue;
            }

            if (!categoriesById.TryGetValue(destCategoryId, out category))
            {
              result.Errors.Add($"Не найден Category по DestId={destCategoryId} (из mapping).");
              continue;
            }

            destCategoryIdNullable = destCategoryId;
          }

          // --------- CounterpartyType (может быть null) ---------
          var xmlCounterpartyType = element.Element("CounterpartyType")?.Value?.Trim();
          Enumeration? counterpartyType = null;

          if (!string.IsNullOrEmpty(xmlCounterpartyType))
          {
            if (!typeMap.TryGetValue(xmlCounterpartyType, out counterpartyType))
            {
              result.Errors.Add($"Неизвестный тип контрагента CounterpartyType={xmlCounterpartyType}.");
              continue;
            }
          }
          
          var xmlTaxResident = (bool?)element.Element("TaxResident") ?? false;

          // --------- Find/Create by composite key ---------
          var key = makeKey(destDocKindId, destCategoryIdNullable, counterpartyType, xmlTaxResident);

          NSI.ITaxRate record;
          if (!existingByKey.TryGetValue(key, out record))
          {
            record = NSI.TaxRates.Create();
            record.Name = xmlName;
            record.DocumentKind = docKind;
            record.Category = category;
            record.CounterpartyType = counterpartyType;
            record.TaxResident = xmlTaxResident;
            existingByKey[key] = record;
          }
          else
          {
            // синхронизация ключевых полей (на всякий случай)
            if (record.Name != xmlName)
              record.Name = xmlName;
            
            if (!Equals(record.DocumentKind, docKind))
              record.DocumentKind = docKind;

            if (!Equals(record.Category, category))
              record.Category = category;

            if (record.CounterpartyType != counterpartyType)
              record.CounterpartyType = counterpartyType;
            
            if (record.TaxResident.GetValueOrDefault() != xmlTaxResident)
              record.TaxResident = xmlTaxResident;            
          }

          // --------- Остальные поля ---------                    
          
          var xmlVat = (double?)element.Element("VAT");
          if (xmlVat.HasValue && xmlVat.Value == 0)
            xmlVat = null;
          if (record.VAT != xmlVat)
            record.VAT = xmlVat;

          var xmlVATmethod = element.Element("VATmethod")?.Value?.Trim();
          Enumeration? vatMethod = null;
          if (!string.IsNullOrEmpty(xmlVATmethod) && !vatMethodMap.TryGetValue(xmlVATmethod, out vatMethod))
          {
            result.Errors.Add($"Неизвестный VATmethod={xmlVATmethod}.");
            continue;
          }
          if (record.VATMethod != vatMethod)
            record.VATMethod = vatMethod;

          var xmlIncomeTax = (double?)element.Element("IncomeTax");
          if (xmlIncomeTax.HasValue && xmlIncomeTax.Value == 0)
            xmlIncomeTax = null;
          if (record.IncomeTax != xmlIncomeTax)
            record.IncomeTax = xmlIncomeTax;

          var xmlIncomeTaxLimit = (double?)element.Element("IncomeTaxLimit");
          if (xmlIncomeTaxLimit.HasValue && xmlIncomeTaxLimit.Value == 0)
            xmlIncomeTaxLimit = null;
          if (record.IncomeTaxLimit != xmlIncomeTaxLimit)
            record.IncomeTaxLimit = xmlIncomeTaxLimit;

          var xmlIncomeTaxMethod = element.Element("IncomeTaxMethod")?.Value?.Trim();
          Enumeration? incomeTaxMethod = null;
          if (!string.IsNullOrEmpty(xmlIncomeTaxMethod) && !incomeTaxMethodMap.TryGetValue(xmlIncomeTaxMethod, out incomeTaxMethod))
          {
            result.Errors.Add($"Неизвестный IncomeTaxMethod={xmlIncomeTaxMethod}.");
            continue;
          }
          if (record.IncomeTaxMethod != incomeTaxMethod)
            record.IncomeTaxMethod = incomeTaxMethod;
          
          var xmlPensionContribution = (double?)element.Element("PensionContribution");
          if (xmlPensionContribution.HasValue && xmlPensionContribution.Value == 0)
            xmlPensionContribution = null;
          if (record.PensionContribution != xmlPensionContribution)
            record.PensionContribution = xmlPensionContribution;
          
          var xmlPensionContributionMethod = element.Element("PensionContributionMethod")?.Value?.Trim();
          Enumeration? pensionContributionMethod = null;
          if (!string.IsNullOrEmpty(xmlPensionContributionMethod) && !pensionContributionMethodMap.TryGetValue(xmlPensionContributionMethod, out pensionContributionMethod))
          {
            result.Errors.Add($"Неизвестный PensionContributionMethod={xmlPensionContributionMethod}.");
            continue;
          }
          if (record.PensionContributionMethod != pensionContributionMethod)
            record.PensionContributionMethod = pensionContributionMethod;
          
          var xmlFSZN = (double?)element.Element("FSZN");
          if (xmlFSZN.HasValue && xmlFSZN.Value == 0)
            xmlFSZN = null;
          if (record.FSZN != xmlFSZN)
            record.FSZN = xmlFSZN;
          
          var xmlFSZNMethod = element.Element("FSZNMethod")?.Value?.Trim();
          Enumeration? fsznMethod = null;
          if (!string.IsNullOrEmpty(xmlFSZNMethod) && !fsznMethodMap.TryGetValue(xmlFSZNMethod, out fsznMethod))
          {
            result.Errors.Add($"Неизвестный FSZNMethod={xmlFSZNMethod}.");
            continue;
          }
          if (record.FSZNMethod != fsznMethod)
            record.FSZNMethod = fsznMethod;          
          
          // --------- Save + counters ---------
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
    /// Упаковать XDocument в IZip
    /// </summary>
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
    /// Чтение данных из xml
    /// </summary>
    public System.Xml.Linq.XElement GetDataElements(string fileBase64, Structures.Module.IResultImportXml result)
    {
      if (result.Errors == null)
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
        return null;
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
          return null;
        }

        return dataElements;
      }
      catch (Exception ex)
      {
        result.Errors.Add($"Критическая ошибка: {ex.Message}");
        return null;
      }
    }
    
    
    #endregion
    
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
    
  }
}