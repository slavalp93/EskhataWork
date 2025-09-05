using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.SignatureSetting;

namespace litiko.Eskhata.Shared
{
  partial class SignatureSettingFunctions
  {
    /// <summary>
    /// Получить роли, которым могут быть назначены права подписи.
    /// </summary>
    /// <returns>Список Sid ролей.</returns>
    public override List<Guid> GetPossibleSignatureRoles()
    {
      var result = base.GetPossibleSignatureRoles();
      result.Add(CollegiateAgencies.PublicConstants.Module.RoleGuid.Secretaries);
      result.Add(CollegiateAgencies.PublicConstants.Module.RoleGuid.Presidents);
      result.Add(CollegiateAgencies.PublicConstants.Module.RoleGuid.ResponsibleEmployeeAHD);
      return result;            
    }
  }
}