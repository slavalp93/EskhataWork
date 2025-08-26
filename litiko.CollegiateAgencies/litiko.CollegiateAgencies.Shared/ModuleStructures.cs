using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.CollegiateAgencies.Structures.Module
{

  /// <summary>
  /// Информация о принятом решении заседания.
  /// </summary>
  [Public(Isolated=true)]
  partial class MeetingResolutionInfo
  {    
    /// <summary>
    /// Номер вопроса в совещании и протоколе
    /// </summary>
    public int? Number { get; set; }    
    
    /// <summary>
    /// Проект решения - заголовок
    /// </summary>
    public string ProjectSolutionTittle { get; set; }
    
    /// <summary>
    /// Проект решения - заголовок TJ
    /// </summary>
    public string ProjectSolutionTittleTJ { get; set; }    
    
    /// <summary>
    /// Фамилия, иницалы и должность Докладчика в винительном падеже (RU)
    /// </summary>
    public string SpeakerRU { get; set; }
    
    /// <summary>
    /// Фамилия, иницалы Докладчика (TJ)
    /// </summary>
    public string SpeakerTJ { get; set; }    
    
    /// <summary>
    /// Слушали (RU)
    /// </summary>
    public string ListenedRU { get; set; }
    
    /// <summary>
    /// Слушали (TJ)
    /// </summary>
    public string ListenedTJ { get; set; }     
    
    /// <summary>
    /// Решения
    /// </summary>
    public string Decigions { get; set; }
    
    /// <summary>
    /// Решения
    /// </summary>
    public string DecigionsTJ { get; set; }    
    
    /// <summary>
    /// С голосованием?
    /// </summary>
    public bool WithVoting { get; set; }    
    
    /// <summary>
    /// Кол-во голосов - «За»
    /// </summary>
    public int? VoutingYes { get; set; }
    
    /// <summary>
    /// Кол-во голосов - «Против»
    /// </summary>
    public int? VoutingNo { get; set; }

    /// <summary>
    /// Кол-во голосов - «Воздержавшихся»
    /// </summary>
    public int? VoutingAbstained { get; set; }    

    /// <summary>
    /// Голосование - решение принято?
    /// </summary>
    public bool? VoutingAccepted { get; set; }    
  }

}