using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.Integration.Structures.Module
{

  /// <summary>
  /// Результат обработки должности
  /// </summary>
  partial class ProcessingJobTittleResult
  {        
    /// <summary>
    /// Должность.
    /// </summary>
    public Eskhata.IJobTitle jobTittle { get; set; }
    
    /// <summary>
    /// Признак, было ли изменение или создание.
    /// </summary>
    public bool isCreatedOrUpdated { get; set; }    
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