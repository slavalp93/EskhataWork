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
    /// Проект решения - заголовок
    /// </summary>
    public string ProjectSolutionTittle { get; set; }
    
    /// <summary>
    /// Слушали (RU)
    /// </summary>
    public string ListenedRU { get; set; }    
    
    /// <summary>
    /// Решения
    /// </summary>
    public string Decigions { get; set; }
    
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