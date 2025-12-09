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
    
    /// <summary>
    /// Получить ФИО на тадж. языке
    /// </summary>        
    /// <returns>ФИО.</returns>
    [Public]
    public virtual string GetNameTJ()
    {      
      var lastName = _obj.LastNameTGlitiko;
      var firstName = _obj.FirstNameTGlitiko;
      var middleName = _obj.MiddleNameTGlitiko;            
      
      using (TenantInfo.Culture.SwitchTo())
      {
        if (string.IsNullOrEmpty(middleName))
          return Sungero.Parties.People.Resources.FullNameWithoutMiddleFormat(firstName, lastName);
        else
          return Sungero.Parties.People.Resources.FullNameFormat(firstName, middleName, lastName);
      }
    }    
    
    /// <summary>
    /// Проверяет, есть ли у текущего пользователя право на редактирование документов, удостоверяющих личность.
    /// </summary>
    /// <returns>True, если у пользователя есть права на редактирование документов, удостоверяющих личность.</returns>
    public override bool CheckCanEditIdentityDocuments()
    {            
      var canEditIdentityBase = base.CheckCanEditIdentityDocuments();
            
      var personParams = ((Sungero.Domain.Shared.IExtendedEntity)_obj).Params;
      if (personParams.ContainsKey(Constants.Parties.Person.PersonIsEmployeeParamName))
        return canEditIdentityBase
          && !((bool)personParams[Constants.Parties.Person.PersonIsEmployeeParamName] && Users.Current.IncludedIn(Sungero.Docflow.PublicConstants.Module.RoleGuid.CounterpartiesResponsibleRole));
      
      var personIsEmployee = Functions.Person.Remote.IsPesonEmployee(_obj);
      var canEditIdentity = canEditIdentityBase && !(personIsEmployee && Users.Current.IncludedIn(Sungero.Docflow.PublicConstants.Module.RoleGuid.CounterpartiesResponsibleRole));
      personParams.Add(Constants.Parties.Person.PersonIsEmployeeParamName, canEditIdentity);
      
      return canEditIdentity;
    }    
  }
}