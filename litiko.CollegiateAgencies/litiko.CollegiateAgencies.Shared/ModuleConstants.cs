using System;
using Sungero.Core;

namespace litiko.CollegiateAgencies.Constants
{
  public static class Module
  {
    public static class DocumentTypeGuids
    {
      /// <summary> Проект решения </summary>
      [Sungero.Core.Public]
      public static readonly Guid ProjectSolution = Guid.Parse("38450693-6600-4c7b-a2a6-5ca6aa9edd21");

      /// <summary> Приложение к документу </summary>
      [Sungero.Core.Public]
      public static readonly Guid Addendum = Guid.Parse("58b9ed35-9c84-46cd-aa79-9b5ef5a82f5d");
      
      /// <summary> Повестка совещания </summary>
      [Sungero.Core.Public]
      public static readonly Guid Agenda = Guid.Parse("5261da93-7879-4210-b3db-c92fa894ab4d");      

      /// <summary> Протокол совещания </summary>
      [Sungero.Core.Public]
      public static readonly Guid Minutes = Guid.Parse("bb4780ff-b2c3-4044-a390-e9e110791bf6");     
    }
    
    public static class DocumentKindGuids
    {
      /// <summary> Проект решения </summary>
      [Sungero.Core.Public]
      public static readonly Guid ProjectSolution = Guid.Parse("1e104576-8dd4-45be-ad53-e0c7ccabd9f0");
      
      /// <summary> Пояснительная записка </summary>
      [Sungero.Core.Public]
      public static readonly Guid ExplanatoryNote = Guid.Parse("83c31ba0-0c56-4738-8450-a2a401bc9a2e");
     
      /// <summary> Выписка из протокола </summary>
      [Sungero.Core.Public]
      public static readonly Guid ExtractProtocol = Guid.Parse("7925bb14-ab32-40e7-96cf-656547d5f6a3");

      /// <summary> Постановление </summary>
      [Sungero.Core.Public]
      public static readonly Guid Resolution = Guid.Parse("a145af0a-35bc-40ac-b616-8eb7418cf785");
    }
    
    public static class RoleGuid
    {
      /// <summary> Секретари КОУ </summary>
      [Sungero.Core.Public]
      public static readonly Guid Secretaries = Guid.Parse("56618558-3348-4293-9f89-69907ece3fc9");
      
      /// <summary> Председатели КОУ </summary>
      [Sungero.Core.Public]
      public static readonly Guid Presidents = Guid.Parse("b576b6af-c159-416d-bb37-bc8bfed3210b");      
    }
    
    public static class ParamNames
    {
      /// <summary> Не обновлять Проект решения </summary>
      [Sungero.Core.Public]
      public const string DontUpdateProjectSolution = "DontUpdateProjectSolution";
    }
    
   /// <summary> Наименование шаблона Протокола совещания </summary>
   [Sungero.Core.Public]
   public const string MinutesTemplateName = "Шаблон протокола заседания КОУ (RU)";    

   /// <summary> Наименование шаблона Выписки из протокола совещания </summary>
   [Sungero.Core.Public]
   public const string ExtractTemplateName = "Шаблон выписки из протокола заседания КОУ (RU)";  
   
  }
}