using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.RegulatoryDocuments.RegulatoryDocument;

namespace litiko.RegulatoryDocuments
{
  partial class RegulatoryDocumentSharedHandlers
  {

    public override void LeadingDocumentChanged(Sungero.Docflow.Shared.OfficialDocumentLeadingDocumentChangedEventArgs e)
    {
      base.LeadingDocumentChanged(e);
      
      if (e.NewValue != null && !Equals(e.NewValue, e.OldValue))
      {
        var leadingDoc = litiko.RegulatoryDocuments.RegulatoryDocuments.As(e.NewValue);
        
        if (leadingDoc != null)
        {
          if (_obj.Type == litiko.RegulatoryDocuments.RegulatoryDocument.Type.IsChange)
          {
            if (_obj.DocumentKind == null)
              _obj.DocumentKind = leadingDoc.DocumentKind;
            
            if (string.IsNullOrWhiteSpace(_obj.Subject))
              _obj.Subject = leadingDoc.Subject;
            
            if (_obj.ProcessManager == null)
              _obj.ProcessManager = leadingDoc.ProcessManager;
            
            if (!_obj.Language.HasValue)
              _obj.Language = leadingDoc.Language;                     
            
            if (!Equals(_obj.IsRequirements, leadingDoc.IsRequirements))
              _obj.IsRequirements = leadingDoc.IsRequirements;
            
            if (!Equals(_obj.IsRelatedToStructure, leadingDoc.IsRelatedToStructure))
              _obj.IsRelatedToStructure = leadingDoc.IsRelatedToStructure;
            
            if (!Equals(_obj.IsRecommendations, leadingDoc.IsRecommendations))
              _obj.IsRecommendations = leadingDoc.IsRecommendations;
            
            if (_obj.OrganForApproving == null)
              _obj.OrganForApproving = leadingDoc.OrganForApproving;
            
            if (_obj.ForWhom == null)
              _obj.ForWhom = leadingDoc.ForWhom;            
          }
          
          if (_obj.Type == litiko.RegulatoryDocuments.RegulatoryDocument.Type.IsUpdate)
          {
            if (_obj.DocumentKind == null)
              _obj.DocumentKind = leadingDoc.DocumentKind;
            
            if (string.IsNullOrWhiteSpace(_obj.Subject))
              _obj.Subject = leadingDoc.Subject;
            
            if (_obj.ProcessManager == null)
              _obj.ProcessManager = leadingDoc.ProcessManager;
            
            if (!_obj.Language.HasValue)
              _obj.Language = leadingDoc.Language;                     
            
            if (!Equals(_obj.IsRequirements, leadingDoc.IsRequirements))
              _obj.IsRequirements = leadingDoc.IsRequirements;
            
            if (!Equals(_obj.IsRelatedToStructure, leadingDoc.IsRelatedToStructure))
              _obj.IsRelatedToStructure = leadingDoc.IsRelatedToStructure;
            
            if (!Equals(_obj.IsRecommendations, leadingDoc.IsRecommendations))
              _obj.IsRecommendations = leadingDoc.IsRecommendations;
            
            if (_obj.OrganForApproving == null)
              _obj.OrganForApproving = leadingDoc.OrganForApproving;
            
            if (_obj.ForWhom == null)
              _obj.ForWhom = leadingDoc.ForWhom;    

            if (_obj.LegalAct == null)
              _obj.LegalAct = leadingDoc.LegalAct;

            if (string.IsNullOrEmpty(_obj.VersionNumber))
              _obj.VersionNumber = leadingDoc.VersionNumber;
            
            if (!_obj.DateBegin.HasValue)
              _obj.DateBegin = leadingDoc.DateBegin;
            
            if (!_obj.DateRevision.HasValue)
              _obj.DateRevision = leadingDoc.DateRevision;
          }          
          

          if (leadingDoc.HasVersions && !_obj.HasVersions)
          {            
            _obj.CreateVersionFrom(leadingDoc.LastVersion.Body.Read(), leadingDoc.LastVersion.BodyAssociatedApplication.Extension);
            
            if (leadingDoc.Language.GetValueOrDefault() == RegulatoryDocument.Language.Ru)
            {
              var versionOnTJ = leadingDoc.Versions.Where(x => x.Note == litiko.RegulatoryDocuments.RegulatoryDocuments.Resources.VersionNoteOnTJ).FirstOrDefault();
              if (versionOnTJ != null)
              {
                var version = _obj.CreateVersionFrom(versionOnTJ.Body.Read(), versionOnTJ.BodyAssociatedApplication.Extension);
                version.Note = litiko.RegulatoryDocuments.RegulatoryDocuments.Resources.VersionNoteOnTJ;
              }            
            }            
            
            if (leadingDoc.Language.GetValueOrDefault() == RegulatoryDocument.Language.Tj)
            {
              var versionOnRU = leadingDoc.Versions.Where(x => x.Note == litiko.RegulatoryDocuments.RegulatoryDocuments.Resources.VersionNoteOnRU).FirstOrDefault();
              if (versionOnRU != null)
              {
                var version = _obj.CreateVersionFrom(versionOnRU.Body.Read(), versionOnRU.BodyAssociatedApplication.Extension);
                version.Note = litiko.RegulatoryDocuments.RegulatoryDocuments.Resources.VersionNoteOnRU;
              }                        
            }
          }
        }        
      }                  
    }

    public virtual void VersionNumberChanged(Sungero.Domain.Shared.StringPropertyChangedEventArgs e)
    {
      FillName();
    }

    public virtual void LanguageChanged(Sungero.Domain.Shared.EnumerationPropertyChangedEventArgs e)
    {
      FillName();
    }

    public virtual void DateBeginChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      var dateBegin = e.NewValue;
      if (dateBegin.HasValue && _obj.DocumentKind != null)
      {
        var deadLine = litiko.RegulatoryDocuments.DeadlineForRevisions.GetAll(x => x.Status == litiko.RegulatoryDocuments.DeadlineForRevision.Status.Active &&
                                                                 Equals(x.DocumentKind, _obj.DocumentKind) &&
                                                                x.Deadline.HasValue)
          .FirstOrDefault();        
          
        if (deadLine != null)
        {
          var newRevisionDate = Calendar.GetDate(dateBegin.Value.Year + deadLine.Deadline.Value, dateBegin.Value.Month, dateBegin.Value.Day);
          if (_obj.DateRevision != newRevisionDate)
            _obj.DateRevision = newRevisionDate;
          
          if (_obj.DateUpdate != newRevisionDate)
            _obj.DateUpdate = newRevisionDate;
        }      
      }
      else
        _obj.DateRevision = null;
    }

  }
}