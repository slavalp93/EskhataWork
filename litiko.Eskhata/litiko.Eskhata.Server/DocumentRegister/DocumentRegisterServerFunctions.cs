using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.DocumentRegister;

namespace litiko.Eskhata.Server
{
  partial class DocumentRegisterFunctions
  {
    /// <summary>
    /// Получить текущий порядковый номер для журнала.
    /// </summary>
    /// <param name="date">Дата.</param>
    /// <param name="leadDocumentId">ID ведущего документа.</param>
    /// <param name="departmentId">ID подразделения.</param>
    /// <param name="businessUnitId">ID НОР.</param>
    /// <returns>Порядковый номер.</returns>
    [Remote(IsPure = true)]
    public override int GetCurrentNumber(DateTime date, long leadDocumentId, long departmentId, long businessUnitId)
    {
      return base.GetCurrentNumber(date, leadDocumentId, departmentId, businessUnitId);
    }
  }
}