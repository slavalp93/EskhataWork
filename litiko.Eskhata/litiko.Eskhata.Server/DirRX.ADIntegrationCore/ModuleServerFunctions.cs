using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using ADServer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace litiko.Eskhata.Module.ADIntegrationCore.Server
{
  partial class ModuleFunctions
  {
    /// <summary>
    /// Получить сотрудника по ExternalID.
    /// </summary>
    /// <param name="importContext">Контекст импорта.</param>
    /// <param name="jEmployee">Словарь со свойствами сотрудника.</param>
    /// <param name="isCreated">Признак того, что сотружник был создан (не используется).</param>
    /// <param name="changedExternalLinks">Список измененных ExternalLink-ов (не используется).</param>
    /// <returns>Сущность сотрудника.</returns>
    protected override Sungero.Company.IEmployee GetOrCreateEmployee(ImportContext importContext, JObject jEmployee, out bool isCreated, List<Sungero.Domain.Shared.IExternalLink> changedExternalLinks)
    {
      isCreated = false;
      var employee = Sungero.Company.Employees.Null;
      
      var employeeID = (string)jEmployee["employeeID"];
      if (!string.IsNullOrEmpty(employeeID))
        employee = Sungero.Company.Employees.GetAll().SingleOrDefault(e => e.ExternalId == employeeID);
            
      return employee;
    }
    
    /// <summary>
    /// Обновить свойства сотрудника.
    /// </summary>
    /// <param name="importContext">Контекст импорта.</param>
    /// <param name="jEmployee">Информация о сотрудника, из которой выполняется синхронизация.</param>
    /// <param name="employee">Запись сотрудника, в которую выполняется синхронизация.</param>
    /// <param name="isCreated">Признак того, что запись сотрудника создана.</param>
    /// <param name="changedExternalLinks">Список измененных ExternalLink-ов.</param>
    /// <param name="importResult">Результат импорта.</param>
    protected override void UpdateEmployeeProperties(ImportContext importContext, JObject jEmployee, Sungero.Company.IEmployee employee, bool isCreated, List<Sungero.Domain.Shared.IExternalLink> changedExternalLinks, DirRX.ADIntegrationCore.Structures.Module.ImportResult importResult)
    {
      ILogin login;
      //IPerson person;

      UpdateLogin(importContext, jEmployee, employee, importResult, out login);
      //UpdatePerson(importContext, jEmployee, employee, changedExternalLinks, out person);
      //UpdateJobTitle(importContext, jEmployee, employee);
      
      //if (UpdateStringRequisite(importContext, (string)jEmployee["EmployeePhones"], "Phone", employee))
      //  AddToChangedEmployees(importContext, employee);

      //UpdateDepartment(importContext, jEmployee, employee, isCreated, importResult);
      UpdateEmail(importContext, jEmployee, employee);
      //UpdateStatus(importContext, jEmployee, employee, login, person);
      DisableRequiredPropertyCheck(employee);    
      //CloseEmployeeIfNeeded(importContext, employee);

      if (employee.State.IsInserted)
        HandleEmployeeCreated(employee, jEmployee);
      else if (employee.State.IsChanged)
        HandleEmployeeChanged(employee, jEmployee);
    }   

    /// <summary>
    /// Отключить поверку заполненности обязательных свойств.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    private void DisableRequiredPropertyCheck(Sungero.Company.IEmployee employee)
    {
      employee.State.Properties.Department.IsRequired = false;
      employee.State.Properties.Email.IsRequired = false;
      employee.State.Properties.Person.IsRequired = false;
      employee.State.Properties.Status.IsRequired = false;
    }    
  }
}