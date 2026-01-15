using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.NSI.ResponsibilityMatrix;

namespace litiko.NSI.Client
{
  internal static class ResponsibilityMatrixStaticActions
  {

    public static bool CanExport(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return Users.Current.IncludedIn(Roles.Administrators);
    }

    public static void Export(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var zip = NSI.Functions.Module.Remote.ExportResponsibilityMatrix();
      zip.Export();      
    }

    public static bool CanImport(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return Users.Current.IncludedIn(Roles.Administrators);
    }

    public static void Import(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Functions.Module.ImportClientAction(Constants.Module.ImportEntityTypes.ResponsibilityMatrix);
    }
  }

  partial class ResponsibilityMatrixActions
  {


    public virtual void ShowDuplicates(Sungero.Domain.Client.ExecuteActionArgs e)
    {      
      var duplicates = Functions.ResponsibilityMatrix.Remote.GetDuplicates(_obj);
      if (duplicates.Any())
        duplicates.Show();
      else
        Dialogs.NotifyMessage(Sungero.Parties.Counterparties.Resources.DuplicateNotFound);
    }

    public virtual bool CanShowDuplicates(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

}