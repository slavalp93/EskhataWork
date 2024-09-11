using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Shared;

namespace litiko.CollegiateAgencies.Server
{
  public class ModuleFunctions
  {
    /// <summary>
    /// Проверка изменения свойства
    /// </summary>
    /// <param name="entity">Сущность или строка коллекции</param>
    /// <param name="propertyMetadata">Метаданные свойства</param>
    /// <returns>Сообщение об изменении свойства, иначе string.Empty</returns>
    private static string CheckRequisite(Sungero.Domain.Shared.IEntity entity, Sungero.Metadata.PropertyMetadata propertyMetadata)
    {
      var stateProperties = entity.State.Properties;
      var propertyState = stateProperties.GetType().GetProperty(propertyMetadata.Name).GetValue(stateProperties);
      
      //Получим текущее значение свойства
      var newValue = propertyMetadata.GetValue(entity);
      //Получим предыдущее значение свойства, до сохранения
      var originalValue = propertyState.GetType().GetProperty("OriginalValue").GetValue(propertyState);
      
      if (Equals(newValue, originalValue))
        return string.Empty;
      
      //Если тип свойства Enumeration, то получим локализированные значения
      if (propertyMetadata.PropertyType == Sungero.Metadata.PropertyType.Enumeration)
      {
        var infoProperties = entity.Info.Properties;
        var propertyInfo = infoProperties.GetType().GetProperty(propertyMetadata.Name).GetValue(infoProperties) as Sungero.Domain.Shared.EnumPropertyInfo;
        
        newValue = propertyInfo.GetLocalizedValue(newValue as Sungero.Core.Enumeration?);
        originalValue = propertyInfo.GetLocalizedValue(originalValue as Sungero.Core.Enumeration?);
      }
      
      var formatedNewValue = string.Format(" Новое значение: {0}.", newValue == null || string.IsNullOrEmpty(newValue.ToString()) ? "Пусто" : newValue);
      var formatedOldValue = string.Format(" Прежнее значение: {0}.", originalValue == null || string.IsNullOrEmpty(originalValue.ToString()) ? "Пусто" : originalValue);
      return string.Format("\"{0}\".{1}{2}", propertyMetadata.GetLocalizedName(), formatedNewValue, formatedOldValue);
    }
    
    /// <summary>
    /// Получить список измененных свойств объекта
    /// </summary>
    /// <param name="entity">Сущность</param>
    /// <returns>Список измененных свойств</returns>
    [Public]
    public static List<string> ChangeRequisites(Sungero.Domain.Shared.IEntity entity)
    {
      var changeList = new List<string>();
      
      //Получаем "Тип" объекта
      var objType = entity.GetType().GetFinalType();
      
      //Получаем "Метаданные" объекта
      var objMetadata = objType.GetEntityMetadata();      
      
      // Список отслеживаемых свойств - все с вкладки Протокол.
      var requsitesList = new List<string>
      {
          "Speaker",
          "ListenedRUMinutes",
          "ListenedTJMinutes",
          "ListenedENMinutes",
          "DecidedMinutes"
      };
      
      //Получаем свойства объекта
      var properties = objMetadata.Properties.Where(p => requsitesList.Contains(p.Name));      
      foreach (var propertyMetadata in properties)
      {
        //Если текущее свойство это коллекция, то обработает ее отдельно
        if (propertyMetadata.PropertyType == Sungero.Metadata.PropertyType.Collection)
        {
          //Получим значение коллекции
          var collectionValue = (Sungero.Domain.Shared.IChildEntityCollection<Sungero.Domain.Shared.IChildEntity>)propertyMetadata.GetValue(entity);
          
          //Переберем строки коллекции
          foreach (Sungero.Domain.Shared.IChildEntity line in collectionValue)
          {
            System.Type lineType = line.GetType();
            var lineMetadata = lineType.GetEntityMetadata();            
            
            var collectionRequsitesList = new List<string>
            {
              "Number",
              "DecisionRU",
              "DecisionTJ",
              "DecisionEN",
              "Responsible",
              "Date"
            };
            
            //Получаем свойства строки коллекции
            var lineProperties = lineMetadata.Properties.Where(p => collectionRequsitesList.Contains(p.Name));
            foreach (var linePropertyMetadata in lineProperties)
            {
              //Если значение свойства это RootEntity, то пропустим обработку этого свойства
              if (Equals(entity, linePropertyMetadata.GetValue(line)))
                continue;
              
              var checkResult = CheckRequisite(line, linePropertyMetadata);
              if (!string.IsNullOrEmpty(checkResult))
                changeList.Add(string.Format("Коллекция {0}. {1}", propertyMetadata.GetLocalizedName(), checkResult));
              }
          }
        }
        else
        {
          var checkResult = CheckRequisite(entity, propertyMetadata);
          if (!string.IsNullOrEmpty(checkResult))
            changeList.Add(checkResult);
        }
      }
      
      return changeList;
    }    
  }
}