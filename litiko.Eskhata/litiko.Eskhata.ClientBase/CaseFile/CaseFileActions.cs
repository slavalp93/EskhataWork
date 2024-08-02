using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.CaseFile;

namespace litiko.Eskhata.Client
{
  partial class CaseFileCollectionActions
  {

    public virtual bool CanTransferToArchivelitiko(Sungero.Domain.Client.CanExecuteActionArgs e)
    {      
      return !_objs.Any(x => !Equals(x.RegistrationGroup?.ResponsibleEmployee, Users.Current))
        && !_objs.Any(x => x.State.IsInserted || x.State.IsChanged)
        && !_objs.Any(x => x.Archivelitiko == null || !x.TransferredToArchivelitiko.HasValue);
    }

    public virtual void TransferToArchivelitiko(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var doc = litiko.Eskhata.PublicFunctions.Module.Remote.CreateArchiveListByCaseFiles(_objs.ToList());
      doc.Show();
    }
  }

}