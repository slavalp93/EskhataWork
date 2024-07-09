using System;
using Sungero.Core;

namespace Eskhata.EskhataDocflow.Constants
{
  public static class Module
  {
    public static class DocumentTypeGuids
    {
      [Sungero.Core.Public]
      public static readonly Guid Addendum = Guid.Parse("58b9ed35-9c84-46cd-aa79-9b5ef5a82f5d");
    }
    
    public static class DocumentKindGuids
    {
      [Sungero.Core.Public]
      public static readonly Guid Checklist = Guid.Parse("125a38e1-a9f0-46b9-9103-ed7f9912343d");
    }
  }
}