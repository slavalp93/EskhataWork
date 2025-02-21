using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Person;

namespace litiko.Eskhata.Shared
{
  partial class PersonFunctions
  {
    /// <summary>
    /// Получить фамилию и инициалы на тадж. языке
    /// </summary>        
    /// <returns>Фамилия и инициалы.</returns>
    [Public]
    public virtual string GetShortNameTJ()
    {      
      var lastName = _obj.LastNameTGlitiko;
      var firstName = _obj.FirstNameTGlitiko;
      var middleName = _obj.MiddleNameTGlitiko;
      
      // ФИО в формате "Фамилия И.О." или "Фамилия Имя". Аналогично платформенной логике построения краткого имени в переписке.
      if (string.IsNullOrEmpty(middleName))
      {
        using (TenantInfo.Culture.SwitchTo())
          return Sungero.Parties.People.Resources.FullNameWithoutMiddleFormat(firstName, lastName);
      }
      
      return Sungero.Parties.PublicFunctions.Module.GetSurnameAndInitialsInTenantCulture(firstName, middleName, lastName);
    }
  }
}