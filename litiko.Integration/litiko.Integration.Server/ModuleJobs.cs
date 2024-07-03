using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.Integration.Server
{
  public class ModuleJobs
  {

    /// <summary>
    /// Запрос подразделений из интегрируемой системы
    /// </summary>
    public virtual void GetDepartments()
    {                
      string integrationMethodName = "R_DR_GET_DEPART";
      int lastId = 0;
            
      var integrationMethod = IntegrationMethods.GetAll().Where(x => x.Name == integrationMethodName).FirstOrDefault();
      if (integrationMethod == null)
        throw AppliedCodeException.Create(string.Format("Integration method {0} not found", integrationMethodName));
      
      // Проверить, есть ли документы обмена, на которые еще не получен ответ
      var exchDocs = ExchangeDocuments.GetAll().Where(d => d.StatusRequestToRX == Integration.ExchangeDocument.StatusRequestToRX.Awaiting || d.StatusRequestToRX == Integration.ExchangeDocument.StatusRequestToRX.ReceivedPart
                                                     && Equals(d.IntegrationMethod, integrationMethod)).Select(d => d.Id);
      if (exchDocs.Any())
      {
        Logger.DebugFormat("Pending requests found: {0}", exchDocs.ToString());
        return;
      }
            
      var exchDoc = Integration.ExchangeDocuments.Create();
      exchDoc.IntegrationMethod = integrationMethod;      
      exchDoc.Save();
      
      var errorMessage = Functions.Module.SendRequestToIS(integrationMethod, exchDoc, lastId);            
      if (!string.IsNullOrEmpty(errorMessage))
        throw AppliedCodeException.Create(errorMessage);
    }

  }
}