using System;
using Sungero.Core;

namespace litiko.RecordManagementEskhata.Constants
{
  public static class Module
  {
    public static class DocumentTypeGuids
    {
      [Sungero.Core.Public]
      public static readonly Guid CompanyDirective = Guid.Parse("264ada4e-b272-4ecc-a115-1246c9556bfa");
      
      [Sungero.Core.Public]
      public static readonly Guid Order = Guid.Parse("9570e517-7ab7-4f23-a959-3652715efad3");
    }
    
    public static class DocumentKindGuids
    {
      /// <summary> Приказ от имени Председателя Правления </summary>
      [Sungero.Core.Public]
      public static readonly Guid CharmanOrder = Guid.Parse("98ec419e-d7c1-4f62-8ab8-d0dfc0632c6b");
      
      /// <summary> Приказ по утверждению нормативного документа </summary>
      [Sungero.Core.Public]
      public static readonly Guid NormativeOrder = Guid.Parse("aad5fbea-26d6-42fb-a938-6f2b3c1952d8");
      
      /// <summary> Распоряжение по департаменту </summary>
      [Sungero.Core.Public]
      public static readonly Guid DepartmentalDirective = Guid.Parse("812ac7b2-f762-492a-9045-f97f364dc9f9");
      
      /// <summary> Приказ по филиалу </summary>
      [Sungero.Core.Public]
      public static readonly Guid BranchOrder = Guid.Parse("9018466b-9bdc-4179-8046-af55bbf1d624");
      
      /// <summary> Приказ по филиалу (ЭЦП) </summary>
      [Sungero.Core.Public]
      public static readonly Guid BranchOrderEds = Guid.Parse("9018466b-9bdc-4179-8046-af55bbf1d624");
    }
  }
}