using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.RegulatoryDocuments.RegulatoryDocument;

namespace litiko.RegulatoryDocuments.Server
{
  partial class RegulatoryDocumentFunctions
  {
    /// <summary>
    /// Создать нормативный документ.
    /// </summary>
    /// <returns>Нормативный документ.</returns>
    [Remote, Public]
    public static IRegulatoryDocument CreateRegulatoryDocument()
    {
      return RegulatoryDocuments.Create();
    }
  }
}