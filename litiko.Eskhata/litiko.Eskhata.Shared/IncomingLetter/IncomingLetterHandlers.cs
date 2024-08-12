using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.IncomingLetter;

namespace litiko.Eskhata
{
  partial class IncomingLetterAddresseeDepartmentSharedHandlers
  {

    public virtual void AddresseeDepartmentDepartmentChanged(litiko.Eskhata.Shared.IncomingLetterAddresseeDepartmentDepartmentChangedEventArgs e)
    {
      if (e.NewValue == null)
      {
        var groups = Sungero.Docflow.RegistrationGroups.GetAll(x => x.Departments.Any(d => d.Department == e.OldValue)).ToList();
        var responsiblesList = groups.Select(x => x.ResponsibleEmployee).ToList();
        List<Sungero.Docflow.IIncomingDocumentBaseAddressees> addresseesToDelete = new List<Sungero.Docflow.IIncomingDocumentBaseAddressees>();
        foreach (var employee in responsiblesList)
        {
          addresseesToDelete.AddRange(_obj.IncomingLetter.Addressees.Where(x => x.Addressee == employee).ToList());
        }
        if (addresseesToDelete.Any())
        {
          foreach (var addressee in addresseesToDelete.Distinct().ToList())
            _obj.IncomingLetter.Addressees.Remove(addressee);
        }
      }
      else
      {
        var groups = Sungero.Docflow.RegistrationGroups.GetAll(x => x.Departments.Any(d => d.Department.Id == e.NewValue.Id)).ToList();
        var responsiblesList = groups.Select(x => x.ResponsibleEmployee).ToList();
        List<Sungero.Docflow.IIncomingDocumentBaseAddressees> addresseesToAdd = new List<Sungero.Docflow.IIncomingDocumentBaseAddressees>();
        foreach (var employee in responsiblesList)
        {
          if (!_obj.IncomingLetter.Addressees.Any(x => x.Addressee == employee))
            _obj.IncomingLetter.Addressees.AddNew().Addressee = employee;
        }
      }
    }
  }

  partial class IncomingLetterAddresseeDepartmentSharedCollectionHandlers
  {

    public virtual void AddresseeDepartmentDeleted(Sungero.Domain.Shared.CollectionPropertyDeletedEventArgs e)
    {
      if (_deleted.Department == null)
        return;
      var groups = Sungero.Docflow.RegistrationGroups.GetAll(x => x.Departments.Any(d => d.Department.Id == _deleted.Department.Id)).ToList();
      var responsiblesList = groups.Select(x => x.ResponsibleEmployee).ToList();
      List<Sungero.Docflow.IIncomingDocumentBaseAddressees> addresseesToDelete = new List<Sungero.Docflow.IIncomingDocumentBaseAddressees>();
      foreach (var employee in responsiblesList)
      {
        addresseesToDelete.AddRange(_obj.Addressees.Where(x => x.Addressee == employee).ToList());
      }
      if (addresseesToDelete.Any())
      {
        foreach (var addressee in addresseesToDelete.Distinct().ToList())
          _obj.Addressees.Remove(addressee);
      }
    }
  }

}