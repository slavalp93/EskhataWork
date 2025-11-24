using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.Eskhata.Module.Contracts.Structures.Module
{
  [Public]
  partial class ResultImportXmlUI
  {
    public List<string> Errors { get; set; }
    public int ImportedCount { get; set; }
    public int TotalCount { get; set; }
    public int DuplicateCount {get; set;}
  }
  
  /// <summary>
  /// Результат обработки персоны
  /// </summary>
  partial class ProcessingPersonResult
  {
    /// <summary>
    /// Персона.
    /// </summary>
    public Eskhata.IPerson person { get; set; }
    
    /// <summary>
    /// Признак, было ли изменение или создание.
    /// </summary>
    public bool isCreatedOrUpdated { get; set; }

//    public static ProcessingPersonResult Create(litiko.Eskhata.IPerson person, System.Boolean isCreatedOrUpdated)
//    {
//      return new ProcessingPersonResult
//      {
//        person = person,
//        isCreatedOrUpdated = isCreatedOrUpdated
//      };
//    }
  }
  
  /// <summary>
  /// Информация о ФИО
  /// </summary>
  partial class FIOInfo
  {
    /// <summary>
    /// Фамилия.
    /// </summary>
    public string LastNameRU { get; set; }
    
    /// <summary>
    /// Имя.
    /// </summary>
    public string FirstNameRU { get; set; }
    
    /// <summary>
    /// Отчество.
    /// </summary>
    public string MiddleNameRU { get; set; }
    
    /// <summary>
    /// Фамилия тадж.
    /// </summary>
    public string LastNameTG { get; set; }
    
    /// <summary>
    /// Имя тадж.
    /// </summary>
    public string FirstNameTG { get; set; }
    
    /// <summary>
    /// Отчество тадж.
    /// </summary>
    public string MiddleNameTG { get; set; }
    
  }
}