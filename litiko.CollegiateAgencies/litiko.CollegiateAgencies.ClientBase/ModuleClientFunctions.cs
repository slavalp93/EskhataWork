using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.CollegiateAgencies.Client
{
  public class ModuleFunctions
  {
    /// <summary>
    /// 
    /// </summary>
    public virtual void Resolutions()
    {
      Dialogs.ShowMessage("В разработке");
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual void Extracts()
    {
      Dialogs.ShowMessage("В разработке");
    }

    /// <summary>
    /// Создать проект решения.
    /// </summary>
    public virtual void BringQuestion()
    {
      CollegiateAgencies.PublicFunctions.Projectsolution.Remote.CreateProjectsolution().Show();
    }

  }
}