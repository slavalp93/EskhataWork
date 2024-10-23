using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.RegulatoryDocuments.RegulatoryDocument;

namespace litiko.RegulatoryDocuments.Client
{
  partial class RegulatoryDocumentActions
  {
    public virtual void ShowApprovalSheetIRDReport(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var report = Reports.GetApprovalSheetIRD();
      report.Entity = _obj;
      report.Open();
    }

    public virtual bool CanShowApprovalSheetIRDReport(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !_obj.State.IsChanged && !_obj.State.IsInserted;
    }

    public virtual void ShowActOnTheRelevanceReport(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var report = Reports.GetActOnTheRelevance();
      report.Entity = _obj;
      report.Open();
    }

    public virtual bool CanShowActOnTheRelevanceReport(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !_obj.State.IsChanged && !_obj.State.IsInserted;
    }

    public virtual void CreateProjectSolution(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var newDoc = litiko.CollegiateAgencies.PublicFunctions.Projectsolution.Remote.CreateProjectsolution();
      var docId = newDoc.Id;
      
      newDoc.ShowModal();
      
      var createdDoc = litiko.CollegiateAgencies.Projectsolutions.GetAll(x => x.Id == docId).FirstOrDefault();
      if (createdDoc != null)
      {          
        // Связать с текщим документом
        if (!_obj.Relations.GetRelated(Sungero.Docflow.PublicConstants.Module.SimpleRelationName).Contains(createdDoc))
          _obj.Relations.Add(Sungero.Docflow.PublicConstants.Module.SimpleRelationName, createdDoc);
      }
    }

    public virtual bool CanCreateProjectSolution(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
       return !_obj.State.IsInserted && !_obj.State.IsChanged;
    }

    public virtual void CreateUpdate(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var newDoc = litiko.RegulatoryDocuments.PublicFunctions.RegulatoryDocument.Remote.CreateRegulatoryDocument();
      
      newDoc.Type = litiko.RegulatoryDocuments.RegulatoryDocument.Type.IsUpdate;
      newDoc.LeadingDocument = _obj;
      
      newDoc.Show();      
    }

    public virtual bool CanCreateUpdate(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !_obj.State.IsInserted && !_obj.State.IsChanged && _obj.AccessRights.CanUpdate();
    }

    public virtual void CreateChange(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var newDoc = litiko.RegulatoryDocuments.PublicFunctions.RegulatoryDocument.Remote.CreateRegulatoryDocument();            
      
      newDoc.Type = litiko.RegulatoryDocuments.RegulatoryDocument.Type.IsChange;
      newDoc.LeadingDocument = _obj;      
      
      newDoc.Show();
    }

    public virtual bool CanCreateChange(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !_obj.State.IsInserted && !_obj.State.IsChanged && _obj.AccessRights.CanUpdate();
    }


    public virtual void CopyVersionOnRU(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var version = _obj.Versions.Where(x => x.Note == litiko.RegulatoryDocuments.RegulatoryDocuments.Resources.VersionNoteOnRU).FirstOrDefault();
      if (version == null)
        Functions.Module.CreateFromFileDialog(_obj, litiko.RegulatoryDocuments.RegulatoryDocuments.Resources.VersionNoteOnRU.ToString());
      else
        version.Open();       
    }

    public virtual bool CanCopyVersionOnRU(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !_obj.State.IsInserted && !_obj.State.IsChanged && _obj.AccessRights.CanUpdateBody() && _obj.Language.HasValue && _obj.Language.Value == litiko.RegulatoryDocuments.RegulatoryDocument.Language.Tj;
    }

    public virtual void CopyVersionOnTJ(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var version = _obj.Versions.Where(x => x.Note == litiko.RegulatoryDocuments.RegulatoryDocuments.Resources.VersionNoteOnTJ).FirstOrDefault();
      if (version == null)
        Functions.Module.CreateFromFileDialog(_obj, litiko.RegulatoryDocuments.RegulatoryDocuments.Resources.VersionNoteOnTJ.ToString());
      else
        version.Open();      
    }

    public virtual bool CanCopyVersionOnTJ(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !_obj.State.IsInserted && !_obj.State.IsChanged && _obj.AccessRights.CanUpdateBody() && _obj.Language.HasValue && _obj.Language.Value == litiko.RegulatoryDocuments.RegulatoryDocument.Language.Ru;
    }

    public virtual void CreateNormativeOrder(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var roleClerks = Sungero.Docflow.PublicFunctions.DocumentRegister.Remote.GetClerks();
      if (roleClerks != null && Users.Current.IncludedIn(roleClerks))
      {
        var doc = litiko.Eskhata.PublicFunctions.Order.Remote.CreateOrder();
        var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(litiko.RecordManagementEskhata.PublicConstants.Module.DocumentKindGuids.NormativeOrder);
        if (docKind != null)
        {
          doc.DocumentKind = docKind;
          doc.LeadingDocument = _obj;
        }
        
        var docId = doc.Id;
        doc.ShowModal();
                
        var createdDoc = litiko.Eskhata.Orders.GetAll(x => x.Id == docId).FirstOrDefault();
        if (createdDoc != null && !Equals(_obj.LegalAct, createdDoc))
        {          
          _obj.LegalAct = createdDoc;
          _obj.Save();
        }
      }
      else      
      {
        e.AddWarning(litiko.RegulatoryDocuments.RegulatoryDocuments.Resources.ActionAvailableToClerks);
        return;      
      }
    }

    public virtual bool CanCreateNormativeOrder(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !_obj.State.IsInserted && !_obj.State.IsChanged;
    }

    public override void SendForApproval(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if ((_obj.Type.GetValueOrDefault() == litiko.RegulatoryDocuments.RegulatoryDocument.Type.IsChange || _obj.Type.GetValueOrDefault() == litiko.RegulatoryDocuments.RegulatoryDocument.Type.IsNew) &&
          _obj.OrganForApproving == null)
      {
        e.AddWarning(litiko.RegulatoryDocuments.RegulatoryDocuments.Resources.NeedFillOrganForApproving);
        return;      
      }
          
      
      base.SendForApproval(e);
    }

    public override bool CanSendForApproval(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanSendForApproval(e);            
    }

  }

}