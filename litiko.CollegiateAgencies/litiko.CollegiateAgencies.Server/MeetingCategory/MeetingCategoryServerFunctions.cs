using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.CollegiateAgencies.MeetingCategory;

namespace litiko.CollegiateAgencies.Server
{
  partial class MeetingCategoryFunctions
  {
    /// <summary>
    /// Синхронизировать секретаря в роль "Секретари КОУ".
    /// </summary>
    public virtual void SynchronizeSecretaryInRole()
    {
      var originalSecretary = _obj.State.Properties.Secretary.OriginalValue;
      var secretary = _obj.Secretary;
      
      var role = Roles.GetAll(r => r.Sid == Constants.Module.RoleGuid.Secretaries).SingleOrDefault();
      
      if (role == null || (secretary != null && secretary.IncludedIn(role) && originalSecretary != null &&
                                  Equals(originalSecretary, secretary) && _obj.State.Properties.Status.OriginalValue == _obj.Status))
        return;
      
      var roleRecipients = role.RecipientLinks;
      
      if (_obj.Status != Sungero.CoreEntities.DatabookEntry.Status.Closed && secretary != null && !secretary.IncludedIn(role))
        roleRecipients.AddNew().Member = secretary;

    }
    
    /// <summary>
    /// Синхронизировать председателя в роль "Председатели КОУ".
    /// </summary>
    public virtual void SynchronizePresidentInRole()
    {
      var originalPresident = _obj.State.Properties.President.OriginalValue;
      var president = _obj.President;
      
      var role = Roles.GetAll(r => r.Sid == Constants.Module.RoleGuid.Presidents).SingleOrDefault();
      
      if (role == null || (president != null && president.IncludedIn(role) && originalPresident != null &&
                                  Equals(originalPresident, president) && _obj.State.Properties.Status.OriginalValue == _obj.Status))
        return;
      
      var roleRecipients = role.RecipientLinks;
      
      if (_obj.Status != Sungero.CoreEntities.DatabookEntry.Status.Closed && president != null && !president.IncludedIn(role))
        roleRecipients.AddNew().Member = president;

    }    
  }
}