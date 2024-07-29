using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.CaseFile;

namespace litiko.Eskhata.Client
{
  partial class CaseFileActions
  {
    public virtual void TransferToArchivelitiko(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      e.AddInformation("Создание нового документа с видом Список документов для передачи в архив.");
    }

    public virtual bool CanTransferToArchivelitiko(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !_obj.State.IsChanged && !_obj.State.IsInserted && Equals(_obj.RegistrationGroup?.ResponsibleEmployee, Users.Current);
    }

  }

}