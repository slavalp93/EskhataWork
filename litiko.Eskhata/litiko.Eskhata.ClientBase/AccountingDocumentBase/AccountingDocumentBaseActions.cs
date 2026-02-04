using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.AccountingDocumentBase;

namespace litiko.Eskhata.Client
{
  partial class AccountingDocumentBaseActions
  {
    public override void ExportToABSlitiko(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.ExportToABSlitiko(e);
    }

    public override bool CanExportToABSlitiko(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Доступно роли «Администраторы» и «Ответственные за синхронизацию с учетными системами» и «Менеджеры модуля "Договоры"»
      return Users.Current.IncludedIn(Roles.Administrators) || 
        Users.Current.IncludedIn(Integration.PublicConstants.Module.RoleGuid.SynchronizationResponsibleRoleGuid) ||
        Users.Current.IncludedIn(ContractsEskhata.PublicConstants.Module.RoleGuid.ContractsManagers);
    }

  }

}