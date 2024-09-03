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
      
    }
    
    public static class DocumentKindGuids
    {
      /// <summary> Проект решения </summary>
      [Sungero.Core.Public]
      public static readonly Guid ProjectSolution = Guid.Parse("1e104576-8dd4-45be-ad53-e0c7ccabd9f0");
      
      /// <summary> Пояснительная записка </summary>
      [Sungero.Core.Public]
      public static readonly Guid ExplanatoryNote = Guid.Parse("83c31ba0-0c56-4738-8450-a2a401bc9a2e");      
     
      
    }
  }
}