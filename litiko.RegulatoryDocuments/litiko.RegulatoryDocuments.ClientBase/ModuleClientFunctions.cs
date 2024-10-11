using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.RegulatoryDocuments.Client
{
  public class ModuleFunctions
  {

    /// <summary>
    /// 
    /// </summary>
    public virtual void Function4()
    {
      Dialogs.NotifyMessage("В разработке...");
    }

    /// <summary>
    /// Создать нормативный документ
    /// </summary>
    public virtual void CreateRegulatoryDocument()
    {
      litiko.RegulatoryDocuments.PublicFunctions.RegulatoryDocument.Remote.CreateRegulatoryDocument().Show();
    }

  }
}