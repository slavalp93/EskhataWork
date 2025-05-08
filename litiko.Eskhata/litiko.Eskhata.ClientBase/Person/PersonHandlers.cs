using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Person;

namespace litiko.Eskhata
{
  partial class PersonClientHandlers
  {
    /// <summary>
    /// Если пользователь не входит в роль Администраторы, то рабочий телефон не будет отображаться в карточке сотрудника
    /// </summary>
    /// <param name="e"></param>
    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);
      var allRoles = Roles.AllUsers;
      
      if(allRoles.IncludedIn(Roles.Administrators))
      {
        _obj.AccessRights.Grant(allRoles, DefaultAccessRightsTypes.FullAccess);
      }
      else
      {
        _obj.State.Properties.Phones.IsVisible = false;
      }
    }
    public override void TINValueInput(Sungero.Presentation.StringValueInputEventArgs e)
    {
      var len = !string.IsNullOrWhiteSpace(e.NewValue) ? e.NewValue.Length : 0;
      if (_obj.Nonresident == true)
      {
        if (len > 12)
          e.AddError(Resources.INNNonRezidentPersonError);
      }
      else
      {
        if (len > 9)
          e.AddError(Resources.INNRezidentPersonError);
      }
    }
  }
}