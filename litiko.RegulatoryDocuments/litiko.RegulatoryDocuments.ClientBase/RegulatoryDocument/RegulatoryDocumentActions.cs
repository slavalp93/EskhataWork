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
    public virtual void CreateNormativeOrder(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var roleClerks = Sungero.Docflow.PublicFunctions.DocumentRegister.Remote.GetClerks();
      if (roleClerks != null && Users.Current.IncludedIn(roleClerks))
      {
        var doc = litiko.Eskhata.PublicFunctions.Order.Remote.CreateOrder();
        var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(litiko.RecordManagementEskhata.PublicConstants.Module.DocumentKindGuids.NormativeOrder);
        if (docKind != null)
          doc.DocumentKind = docKind;
        
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