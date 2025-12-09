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
    public virtual void CreateExplanatoryNote(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var addendum = litiko.CollegiateAgencies.PublicFunctions.Projectsolution.Remote.CreateExplanatoryNote();
      if (addendum != null)
      {
        addendum.LeadingDocument = _obj;
        addendum.Show();      
      }
    }

    public virtual bool CanCreateExplanatoryNote(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !_obj.State.IsChanged && (Equals(Users.Current, _obj.Author) || Equals(Users.Current, Users.As(_obj.PreparedBy)) || Users.Current.IncludedIn(Roles.Administrators));
    }

    public virtual void ShowControlApprovingIRDReport(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var report = Reports.GetControlApprovingIRD();
      report.DocumentId = _obj.Id;
      report.Open();
    }

    public virtual bool CanShowControlApprovingIRDReport(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !_obj.State.IsChanged && !_obj.State.IsInserted;
    }

    public override void ApprovalForm(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.ApprovalForm(e);
    }

    public override bool CanApprovalForm(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return false;
    }

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
      
      // Через параметр передаем ИД текущего ВНД. При сохранении ПР будет связан с ВНД
      ((Sungero.Domain.Shared.IExtendedEntity)newDoc).Params[Constants.Module.CreatedFromIRD_ID] = _obj.Id;
      newDoc.Show();            
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
      {
        if (_obj.AccessRights.CanUpdate())
          Functions.Module.CreateFromFileDialog(_obj, litiko.RegulatoryDocuments.RegulatoryDocuments.Resources.VersionNoteOnRU.ToString(), litiko.RegulatoryDocuments.RegulatoryDocuments.Resources.VersionNoteOnTJ.ToString());
        else
          e.AddInformation(litiko.RegulatoryDocuments.RegulatoryDocuments.Resources.NoAccessRightsToCreateVersion);
      }        
      else
        version.Open();       
    }

    public virtual bool CanCopyVersionOnRU(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !_obj.State.IsInserted && !_obj.State.IsChanged;
    }

    public virtual void CopyVersionOnTJ(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var version = _obj.Versions.Where(x => x.Note == litiko.RegulatoryDocuments.RegulatoryDocuments.Resources.VersionNoteOnTJ).FirstOrDefault();
      if (version == null)
      {
        if (_obj.AccessRights.CanUpdate())
          Functions.Module.CreateFromFileDialog(_obj, litiko.RegulatoryDocuments.RegulatoryDocuments.Resources.VersionNoteOnTJ.ToString(), litiko.RegulatoryDocuments.RegulatoryDocuments.Resources.VersionNoteOnRU.ToString());
        else
          e.AddInformation(litiko.RegulatoryDocuments.RegulatoryDocuments.Resources.NoAccessRightsToCreateVersion);      
      }        
      else
        version.Open();      
    }

    public virtual bool CanCopyVersionOnTJ(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !_obj.State.IsInserted && !_obj.State.IsChanged;
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
          
      // Vals added 25.09.10 
      var createdTasks = Sungero.Docflow.PublicFunctions.Module.Remote.GetApprovalTasks(_obj);
      if (createdTasks.Any()) {
        Dialogs.ShowMessage(litiko.RegulatoryDocuments.RegulatoryDocuments.Resources.DocumentHasIncompleteTasks);
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