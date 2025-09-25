using System;
using Sungero.Core;

namespace litiko.ContractsEskhata.Constants
{
  public static class Module
  {
    public static class DocumentKindGuids
    {
      /// <summary> Юридическое заключение </summary>
      [Sungero.Core.Public]
      public static readonly Guid LegalOpinion = Guid.Parse("7bcb9652-df6a-43f5-bac1-c8a36b934f1f");      
      
    }
    
    public static class DocumentTypeGuids
    {
      /// <summary> Входящий счет на оплату </summary>
      public static readonly Guid IncomingInvoice = Guid.Parse("a523a263-bc00-40f9-810d-f582bae2205d");

      /// <summary> Акт выполненных работ </summary>     
      public static readonly Guid ContractStatement = Guid.Parse("f2f5774d-5ca3-4725-b31d-ac618f6b8850");      
    }    
  }
}