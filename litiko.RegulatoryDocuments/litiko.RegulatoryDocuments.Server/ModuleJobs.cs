using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.RegulatoryDocuments.Server
{
  public class ModuleJobs
  {

    /// <summary>
    /// Рассылка уведомлений о необходимости пересмотра ВНД
    /// </summary>
    public virtual void SendNoticeAboutRevisionIRD()
    {
      Logger.Debug("Start SendNoticeAboutRevisionIRD");
           
      var inTwelveMonth = Calendar.Today.AddMonths(12).Date;
      var inNineMonth   = Calendar.Today.AddMonths(9).Date;
      var inSixMonth    = Calendar.Today.AddMonths(6).Date;
      var inThreeMonth  = Calendar.Today.AddMonths(3).Date;
      
      var documents = litiko.RegulatoryDocuments.RegulatoryDocuments.GetAll()
        .Where(d => d.LifeCycleState == litiko.RegulatoryDocuments.RegulatoryDocument.LifeCycleState.Active)
        .Where(d => d.RegistrationState == litiko.RegulatoryDocuments.RegulatoryDocument.RegistrationState.Registered)
        .Where(d => d.InternalApprovalState == litiko.RegulatoryDocuments.RegulatoryDocument.InternalApprovalState.Signed)
        .Where(d => d.DateRevision.HasValue)
        .Where(d => inTwelveMonth.CompareTo(d.DateRevision.Value.Date) == 0);
           
      Logger.DebugFormat("{0} matching documents found", documents.Count().ToString());      
      
      foreach(var document in documents)
      {                
        string subject = Resources.RevisionNoticeSubjectFormat(12, document.Name);
        List<IRecipient> addressees = new List<IRecipient>();
        if (document.PreparedBy != null)
          addressees.Add(document.PreparedBy);
        if (document.ProcessManager != null && !addressees.Contains(document.ProcessManager))
          addressees.Add(document.ProcessManager);        
        
        var notice = Sungero.Workflow.SimpleTasks.CreateWithNotices(subject, addressees.ToArray());
        notice.ActiveText = Resources.RegulatoryDocumentFormat(Hyperlinks.Get(document));
        notice.Attachments.Add(document);
        notice.Start();
        Logger.DebugFormat("Notice Task ID:{0} was sent succsesfully for document ID:{1}", notice.Id, document.Id);        
      }

      Logger.Debug("Finish SendNoticeAboutRevisionIRD");      
    }

  }
}