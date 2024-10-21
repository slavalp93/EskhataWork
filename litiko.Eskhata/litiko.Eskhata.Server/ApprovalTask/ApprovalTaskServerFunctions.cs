using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ApprovalTask;

namespace litiko.Eskhata.Server
{
  partial class ApprovalTaskFunctions
  {
    /// <summary>
    /// Получить значение параметра "Разрешить вариант выполнения "Не согласен"" из настроек этапа.
    /// </summary>
    /// <param name="stageNumber">Номер этапа.</param>
    [Remote(IsPure = true)]
    public virtual bool GetApprovalWithResultNotAgreeParameter(int stageNumber)
    {
      var item = _obj.ApprovalRule.Stages.Where(s => s.Number == stageNumber).FirstOrDefault();
      if (item == null)
        return false;
      
      return litiko.Eskhata.ApprovalStages.As(item.Stage).AllowResultNotAgreelitiko ?? false;
    }
  }
}