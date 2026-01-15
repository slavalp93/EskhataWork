using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.NSI.Mapping;

namespace litiko.NSI.Client
{


  internal static class MappingStaticActions
  {

    public static bool CanExport(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return Users.Current.IncludedIn(Roles.Administrators);
    }

    public static void Export(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var zip = NSI.Functions.Module.Remote.ExportMapping();
      zip.Export();
    }

    public static bool CanImport(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return Users.Current.IncludedIn(Roles.Administrators);
    }

    public static void Import(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Functions.Module.ImportClientAction(Constants.Module.ImportEntityTypes.Mapping);
    }
  }

}