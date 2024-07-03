using System;
using Sungero.Core;

namespace litiko.Integration.Constants
{
  public static class Module
  {
    /// <summary>
    /// GUID для роли "Ответственные за синхронизацию с учетными системами".
    /// </summary>
    [Public]
    public static readonly Guid SynchronizationResponsibleRoleGuid = Guid.Parse("6F98BA36-3B7F-4767-8369-88A65578DC5A");
  }
}