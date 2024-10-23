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
    /// Рассылка уведомлений о необходимости актуализации ВНД
    /// </summary>
    public virtual void SendNoticeAboutUpdateIRD()
    {
      Logger.Debug("Start SendNoticeAboutUpdateIRD");
                 
      var inThreeMonth  = Calendar.Today.AddMonths(3).Date;            
      var inOneMonth  = Calendar.Today.AddMonths(1).Date;
      
      var documents = litiko.RegulatoryDocuments.RegulatoryDocuments.GetAll()
        .Where(d => d.LifeCycleState == litiko.RegulatoryDocuments.RegulatoryDocument.LifeCycleState.Active)
        .Where(d => d.RegistrationState == litiko.RegulatoryDocuments.RegulatoryDocument.RegistrationState.Registered)
        .Where(d => d.InternalApprovalState == litiko.RegulatoryDocuments.RegulatoryDocument.InternalApprovalState.Signed)
        .Where(d => d.ProcessManager != null)
        .Where(d => d.DateUpdate.HasValue)
        .Where(d => 
               inThreeMonth.CompareTo(d.DateUpdate.Value.Date) == 0 ||
               inOneMonth.CompareTo(d.DateUpdate.Value.Date) == 0
              );
           
      Logger.DebugFormat("{0} matching documents found", documents.Count().ToString());      
      
      foreach(var document in documents)
      {                

        string subject = string.Empty;
        // Через {0} месяцев наступает дата актуализации нормативного документа: {1}
        if (inThreeMonth.CompareTo(document.DateUpdate.Value.Date) == 0)
          subject = Resources.UpdateNoticeSubjectFormat(3, document.Name);
        else if (inOneMonth.CompareTo(document.DateUpdate.Value.Date) == 0)
          subject = Resources.UpdateNoticeSubjectFormat(1, document.Name);                                     
        
        List<IRecipient> addressees = new List<IRecipient>();
        if (document.ProcessManager != null)
          addressees.Add(document.ProcessManager);
        
        if (addressees.Any() && !string.IsNullOrEmpty(subject))
        {
          var notice = Sungero.Workflow.SimpleTasks.CreateWithNotices(subject, addressees.ToArray());
          notice.ActiveText = Resources.RegulatoryDocumentFormat(Hyperlinks.Get(document));
          notice.Attachments.Add(document);
          notice.Start();
          Logger.DebugFormat("Notice Task ID:{0} was sent succsesfully for document ID:{1}", notice.Id, document.Id);                  
        }
        else
          Logger.DebugFormat("No data to send notice for document ID:{0}", document.Id);                  
      }           

      Logger.Debug("Finish SendNoticeAboutUpdateIRD");       
    }

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
      var inYesterday   = Calendar.Today.AddDays(-1).Date;
      var roleHeadOfGeneralDepartment = Roles.GetAll(x => x.Name == "Руководитель Общего отдела").FirstOrDefault();
      
      var documents = litiko.RegulatoryDocuments.RegulatoryDocuments.GetAll()
        .Where(d => d.LifeCycleState == litiko.RegulatoryDocuments.RegulatoryDocument.LifeCycleState.Active)
        .Where(d => d.RegistrationState == litiko.RegulatoryDocuments.RegulatoryDocument.RegistrationState.Registered)
        .Where(d => d.InternalApprovalState == litiko.RegulatoryDocuments.RegulatoryDocument.InternalApprovalState.Signed)
        .Where(d => d.DateRevision.HasValue)
        .Where(d => 
               inTwelveMonth.CompareTo(d.DateRevision.Value.Date) == 0 ||
               inNineMonth.CompareTo(d.DateRevision.Value.Date) == 0 ||
               inSixMonth.CompareTo(d.DateRevision.Value.Date) == 0 ||
               inThreeMonth.CompareTo(d.DateRevision.Value.Date) == 0 ||
               inYesterday.CompareTo(d.DateRevision.Value.Date) == 0
              );
           
      Logger.DebugFormat("{0} matching documents found", documents.Count().ToString());      
      
      foreach(var document in documents)
      {                

        string subject = string.Empty;
        // Через {0} месяцев наступает дата пересмотра нормативного документа: {1}
        if (inTwelveMonth.CompareTo(document.DateRevision.Value.Date) == 0)
          subject = Resources.RevisionNoticeSubjectFormat(12, document.Name);
        else if (inNineMonth.CompareTo(document.DateRevision.Value.Date) == 0)
          subject = Resources.RevisionNoticeSubjectFormat(9, document.Name);
        else if (inSixMonth.CompareTo(document.DateRevision.Value.Date) == 0)
          subject = Resources.RevisionNoticeSubjectFormat(6, document.Name);
        else if (inThreeMonth.CompareTo(document.DateRevision.Value.Date) == 0)
          subject = Resources.RevisionNoticeSubjectFormat(3, document.Name);                
        
        // Просрочен пересмотр нормативного документа: {0}
        else if (inYesterday.CompareTo(document.DateRevision.Value.Date) == 0)
          subject = Resources.RevisionOverdueNoticeSubjectFormat(document.Name);        
        
        List<IRecipient> addressees = new List<IRecipient>();
        if (document.PreparedBy != null)
          addressees.Add(document.PreparedBy);
        if (document.ProcessManager != null && !addressees.Contains(document.ProcessManager))
          addressees.Add(document.ProcessManager);
        if (roleHeadOfGeneralDepartment != null && inYesterday.CompareTo(document.DateRevision.Value.Date) == 0 && !addressees.Contains(roleHeadOfGeneralDepartment))
          addressees.Add(roleHeadOfGeneralDepartment);
        
        if (addressees.Any() && !string.IsNullOrEmpty(subject))
        {
          var notice = Sungero.Workflow.SimpleTasks.CreateWithNotices(subject, addressees.ToArray());
          notice.ActiveText = Resources.RegulatoryDocumentFormat(Hyperlinks.Get(document));
          notice.Attachments.Add(document);
          notice.Start();
          Logger.DebugFormat("Notice Task ID:{0} was sent succsesfully for document ID:{1}", notice.Id, document.Id);                  
        }
        else
          Logger.DebugFormat("No data to send notice for document ID:{0}", document.Id);                  
      }           

      Logger.Debug("Finish SendNoticeAboutRevisionIRD");      
    }

  }
}