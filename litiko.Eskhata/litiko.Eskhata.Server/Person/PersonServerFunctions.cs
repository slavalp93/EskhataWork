using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Person;

namespace litiko.Eskhata.Server
{
  partial class PersonFunctions
  {

    /// <summary>
    /// Является ли персона сотрудником
    /// </summary>
    [Remote(IsPure = true)]
    public bool IsPesonEmployee()
    {
      return Employees.GetAll().Any(x => x.Person != null && x.Person.Id == _obj.Id);
    }

  }
}