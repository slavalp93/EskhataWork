using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Addendum;

namespace litiko.Eskhata.Server
{
  partial class AddendumFunctions
  {
    /// <summary>
    /// Пункты решения протокола (RU).
    /// </summary>
    /// <param name="projectSolution">Проект решения.</param>
    /// <returns>Значение табличного реквизита Постановили карточки Проекта решения вкладки Протокол.</returns>
    [Public, Converter("GetProjectSolutionDecidedMinutesRU")]
    public static string GetProjectSolutionDecidedMinutesRU(litiko.Eskhata.IAddendum document)
    {
      if (document.LeadingDocument != null && litiko.CollegiateAgencies.Projectsolutions.Is(document.LeadingDocument) && !litiko.CollegiateAgencies.Projectsolutions.As(document.LeadingDocument).DecidedMinutes.Any())              
        return null;      
      
      return string.Join("\n", litiko.CollegiateAgencies.Projectsolutions.As(document.LeadingDocument).DecidedMinutes
                         .OrderBy(element => element.Number)
                         .Select(element => $"{element.Number}. {element.DecisionRU}")
                        );
    }    

    /// <summary>
    /// Пункты решения протокола (TJ).
    /// </summary>
    /// <param name="projectSolution">Проект решения.</param>
    /// <returns>Значение табличного реквизита Постановили карточки Проекта решения вкладки Протокол.</returns>
    [Public, Converter("GetProjectSolutionDecidedMinutesTJ")]
    public static string GetProjectSolutionDecidedMinutesTJ(litiko.Eskhata.IAddendum document)
    {
      if (document.LeadingDocument != null && litiko.CollegiateAgencies.Projectsolutions.Is(document.LeadingDocument) && !litiko.CollegiateAgencies.Projectsolutions.As(document.LeadingDocument).DecidedMinutes.Any())              
        return null;      
      
      return string.Join("\n", litiko.CollegiateAgencies.Projectsolutions.As(document.LeadingDocument).DecidedMinutes
                         .OrderBy(element => element.Number)
                         .Select(element => $"{element.Number}. {element.DecisionTJ}")
                        );
    }
    
    /// <summary>
    /// Пункты решения протокола (EN).
    /// </summary>
    /// <param name="projectSolution">Проект решения.</param>
    /// <returns>Значение табличного реквизита Постановили карточки Проекта решения вкладки Протокол.</returns>
    [Public, Converter("GetProjectSolutionDecidedMinutesEN")]
    public static string GetProjectSolutionDecidedMinutesEN(litiko.Eskhata.IAddendum document)
    {
      if (document.LeadingDocument != null && litiko.CollegiateAgencies.Projectsolutions.Is(document.LeadingDocument) && !litiko.CollegiateAgencies.Projectsolutions.As(document.LeadingDocument).DecidedMinutes.Any())              
        return null;       
      
      return string.Join("\n", litiko.CollegiateAgencies.Projectsolutions.As(document.LeadingDocument).DecidedMinutes
                         .OrderBy(element => element.Number)
                         .Select(element => $"{element.Number}. {element.DecisionEN}")
                        );
    } 
  }
}