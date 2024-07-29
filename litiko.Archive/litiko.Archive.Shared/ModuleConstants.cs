using System;
using Sungero.Core;

namespace litiko.Archive.Constants
{
  public static class Module
  {
    public static class DocumentTypeGuids
    {
      /// <summary> Приказ </summary>
      [Sungero.Core.Public]
      public static readonly Guid Order = Guid.Parse("9570e517-7ab7-4f23-a959-3652715efad3");
      
      /// <summary> Простой документ </summary>
      [Sungero.Core.Public]
      public static readonly Guid SimpleDocument = Guid.Parse("09584896-81e2-4c83-8f6c-70eb8321e1d0");
      
      /// <summary> Список окументов </summary>
      [Sungero.Core.Public]
      public static readonly Guid ArchiveList = Guid.Parse("926182a5-94ef-4bbb-a6c5-cf670038da1d");           
      
    }
    
    public static class DocumentKindGuids
    {
      /// <summary> Приказ о сдаче документов в архив </summary>
      [Sungero.Core.Public]
      public static readonly Guid OrderArchive = Guid.Parse("f4ecaaa2-a115-4866-a95d-0b10701d0720");
      
      /// <summary> График сдачи документов в архив </summary>
      [Sungero.Core.Public]
      public static readonly Guid ArchiveShedule = Guid.Parse("c1d06c86-f923-40ec-b853-39773cfbbddc");   

      /// <summary> Список документов для передачи в архив </summary>
      [Sungero.Core.Public]
      public static readonly Guid ArchiveList = Guid.Parse("50adebff-f5e8-4765-9e5f-4ed8994ced87");        
      
    }
  }
}