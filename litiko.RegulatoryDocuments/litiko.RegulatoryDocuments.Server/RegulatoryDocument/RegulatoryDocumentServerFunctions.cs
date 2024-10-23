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
    /// Отобразить кол-во дней до актуализации
    /// </summary>       
    [Remote]
    public StateView GetDaysUntilUpdate()
    {
      var daysCount = 0;
      if (_obj.DateUpdate.HasValue)
      {
        TimeSpan diff = _obj.DateUpdate.Value - Calendar.Today;
        daysCount = diff.Days;
      }
      var stateView = StateView.Create();
      var block = stateView.AddBlock();
      block.AddLabel(litiko.RegulatoryDocuments.RegulatoryDocuments.Resources.DaysUntilUpdateFormat(daysCount));
      
      return stateView;
    }
    
    /// <summary>
    /// Создать нормативный документ.
    /// </summary>
    /// <returns>Нормативный документ.</returns>
    [Remote, Public]
    public static IRegulatoryDocument CreateRegulatoryDocument()
    {
      return RegulatoryDocuments.Create();
    }
    
    /*
    [Public]
    public void Test(){
      return;
    }
    */
  }
}