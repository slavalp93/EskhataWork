using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ActionItemExecutionTask;

namespace litiko.Eskhata.Server
{
  partial class ActionItemExecutionTaskFunctions
  {
    /// <summary>
    /// Выдать права субъекту прав на вложения поручения.
    /// </summary>
    /// <param name="attachmentGroup"> Группа вложения.</param>
    /// <param name="recipient"> Субъект прав.</param>
    public override void GrantAccessRightsToRecipient(List<Sungero.Domain.Shared.IEntity> attachmentGroup, IRecipient recipient)
    {
      foreach (var item in attachmentGroup)
      {
        // Не выдавать права на Протокол
        if (Sungero.Content.ElectronicDocuments.Is(item) && !litiko.Eskhata.Minuteses.Is(item))
          Sungero.Docflow.PublicFunctions.Module.GrantAccessRightsOnEntity(item, recipient, DefaultAccessRightsTypes.Read);
      }
    }
  }
}