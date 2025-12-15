using System;
using Sungero.Core;

namespace litiko.Eskhata.Module.Docflow.Constants
{
  public static class Module
  {
    public static class RoleGuid
    {
      /// <summary> Доступ к модулю "Документооборот" </summary>
      [Sungero.Core.Public]
      public static readonly Guid DocflowAccessAllowed = Guid.Parse("97EB6298-9867-4020-AFED-5249675AA703");      
    }
  }
}