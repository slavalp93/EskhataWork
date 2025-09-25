using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ApprovalStage;

namespace litiko.Eskhata
{
  partial class ApprovalStageClientHandlers
  {

    public virtual IEnumerable<Enumeration> CustomStageTypelitikoFiltering(IEnumerable<Enumeration> query)
    {     
      #region Согласование
      if (_obj.StageType == StageType.Approvers)
      {        
        var allowedValues = new List<Enumeration>
        {
          CustomStageTypelitiko.BudgetCheck,
          CustomStageTypelitiko.BudgetCheckCont,
          CustomStageTypelitiko.AccountantAppr
        };
        
        return query.Where(q => allowedValues.Contains(q));
      }      
      #endregion

      #region Задание
      if (_obj.StageType == StageType.SimpleAgr)
      {
        var allowedValues = new List<Enumeration>
        {
          CustomStageTypelitiko.Voting,
          CustomStageTypelitiko.IncludeInMeet,
          CustomStageTypelitiko.ControlIRD,
          CustomStageTypelitiko.ScanReceivedCon,
          CustomStageTypelitiko.SubmitIssueKou
        };
        
        if (_obj.AllowSendToRework.GetValueOrDefault())
          allowedValues.Remove(CustomStageTypelitiko.Voting);

        return query.Where(q => allowedValues.Contains(q));
      }      
      #endregion

      #region Контроль возврата
      if (_obj.StageType == StageType.CheckReturn)
      {
        var allowedValues = new List<Enumeration>
        {
          CustomStageTypelitiko.OrigReceivedCon
        };        

        return query.Where(q => allowedValues.Contains(q));
      }      
      #endregion
      
      // Для всех остальных — недоступны никакие значения
      return Enumerable.Empty<Enumeration>();
    }
  }

}