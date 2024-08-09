using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.CaseFile;

namespace litiko.Eskhata
{
  partial class CaseFileCreatingFromServerHandler
  {

    public override void CreatingFrom(Sungero.Domain.CreatingFromEventArgs e)
    {
      base.CreatingFrom(e);
      
      // Без Архива
      e.Without(_info.Properties.Archivelitiko);
      e.Without(_info.Properties.TransferredToArchivelitiko);
    }
  }

}