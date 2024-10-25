using System;
using Sungero.Core;

namespace litiko.RegulatoryDocuments.Constants
{
  public static class Module
  {

    /// <summary>
    /// Наименование группы регистрации "Общий отдел"
    /// </summary>
    public const string RegGroupGeneralDepartment = "Общий отдел";

    /// <summary>
    /// Таблица для отчетов по согласованию ВНД
    /// </summary>    
    public const string SourceTableName = "litiko_ApprSheetIRD";
    
    public static class DocumentTypeGuids
    {
      /// <summary> Нормативный документ </summary>
      [Sungero.Core.Public]
      public static readonly Guid RegulatoryDocument = Guid.Parse("9151081e-29d5-4c68-9204-68f04ff4d7e5");

    }
    
    public static class DocumentKindGuids
    {
      /// <summary> Акт об актуальности ВНД </summary>
      [Sungero.Core.Public]
      public static readonly Guid ActOnTheRelevance = Guid.Parse("a5844c56-6930-4850-8964-1e64caed94ef");
      
      /// <summary> Лист согласования ВНД </summary>
      [Sungero.Core.Public]
      public static readonly Guid ApprovalSheetIRD = Guid.Parse("475db1f5-2cb8-494d-8b4e-4faf8ba2a675");      
      
    }
  }
}