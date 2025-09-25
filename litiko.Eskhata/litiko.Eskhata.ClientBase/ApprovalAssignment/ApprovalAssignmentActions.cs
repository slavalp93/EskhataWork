using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ApprovalAssignment;

namespace litiko.Eskhata.Client
{
  partial class ApprovalAssignmentActions
  {
    public override void WithSuggestions(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      base.WithSuggestions(e);

      var CustomStage = litiko.Eskhata.ApprovalStages.As(_obj.Stage);
      var document = _obj.DocumentGroup.OfficialDocuments.FirstOrDefault();
        
      #region Контроль бюджета
      if (CustomStage.CustomStageTypelitiko == litiko.Eskhata.ApprovalStage.CustomStageTypelitiko.BudgetCheck && document != null && CollegiateAgencies.Projectsolutions.Is(document))
      {
        var projectSolution = CollegiateAgencies.Projectsolutions.As(document);
        if (!projectSolution.Budget.HasValue || !projectSolution.BudgetRemaining.HasValue)
        {
          e.AddError(litiko.Eskhata.ApprovalAssignments.Resources.RequiredToFillBudget);
          return;          
        }
      }        
      #endregion      
      
      #region Договора. Согласование с бюджетным контролером
      if (CustomStage.CustomStageTypelitiko == litiko.Eskhata.ApprovalStage.CustomStageTypelitiko.BudgetCheckCont && document != null && ContractualDocuments.Is(document))
      {
        var contractualDocument = ContractualDocuments.As(document);
        if (!contractualDocument.IsWithinBudgetlitiko.HasValue)
        {
          e.AddError(litiko.ContractsEskhata.Resources.RequiredToFillIsWithinBudget);
          return;          
        }
      }        
      #endregion       
    }

    public override bool CanWithSuggestions(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return base.CanWithSuggestions(e);
    }

    public override void Approved(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      base.Approved(e);
      
      var CustomStage = litiko.Eskhata.ApprovalStages.As(_obj.Stage);
      var document = _obj.DocumentGroup.OfficialDocuments.FirstOrDefault();
        
      #region Тендера. Контроль бюджета
      if (CustomStage.CustomStageTypelitiko == litiko.Eskhata.ApprovalStage.CustomStageTypelitiko.BudgetCheck && document != null && CollegiateAgencies.Projectsolutions.Is(document))
      {
        var projectSolution = CollegiateAgencies.Projectsolutions.As(document);
        if (!projectSolution.Budget.HasValue || !projectSolution.BudgetRemaining.HasValue)
        {
          e.AddError(litiko.Eskhata.ApprovalAssignments.Resources.RequiredToFillBudget);
          return;          
        }
        // Vals 20250915
        var dialogMessage = litiko.Eskhata.ApprovalAssignments.Resources.ConfirmBudgetAndBalance;
        var dialogDescription = litiko.Eskhata.ApprovalAssignments.Resources.BudgetControl;
        var dialog = Dialogs.CreateTaskDialog(dialogMessage, dialogDescription, MessageType.Question);
        dialog.Buttons.AddYesNo();
        dialog.Buttons.Default = DialogButtons.Yes;
        var result = dialog.Show();
        
        if (result == DialogButtons.No)
        {
          e.AddError(litiko.Eskhata.ApprovalAssignments.Resources.BudgetNotConfirmed);
          return;          
        }        
      }        
      #endregion
      
      #region Договора. Согласование с бюджетным контролером
      if (CustomStage.CustomStageTypelitiko == litiko.Eskhata.ApprovalStage.CustomStageTypelitiko.BudgetCheckCont && document != null && ContractualDocuments.Is(document))
      {
        var contractualDocument = ContractualDocuments.As(document);
        if (!contractualDocument.IsWithinBudgetlitiko.HasValue)
        {
          e.AddError(litiko.ContractsEskhata.Resources.RequiredToFillIsWithinBudget);
          return;          
        }
      }        
      #endregion      
      
      #region Договора. Согласование с бухгалтером
      if (CustomStage.CustomStageTypelitiko == litiko.Eskhata.ApprovalStage.CustomStageTypelitiko.AccountantAppr && document != null && ContractualDocuments.Is(document))
      {
        var contract = Contracts.Null;                       
        if (Contracts.Is(document))
          contract = Contracts.As(document);
        else if (SupAgreements.Is(document))
          contract = Contracts.As(SupAgreements.As(document).LeadingDocument);
        
        if (contract != null)
        {
          var notFilledFields = new List<string>();
          if (string.IsNullOrEmpty(contract.AccDebtCreditlitiko))
            notFilledFields.Add(contract.Info.Properties.AccDebtCreditlitiko.LocalizedName);
          
          if (string.IsNullOrEmpty(contract.AccFutureExpenselitiko))
            notFilledFields.Add(contract.Info.Properties.AccFutureExpenselitiko.LocalizedName);
          
          if (contract.PaymentRegionlitiko == null)
            notFilledFields.Add(contract.Info.Properties.PaymentRegionlitiko.LocalizedName);
          
          if (notFilledFields.Any())
          {
            e.AddError(litiko.ContractsEskhata.Resources.RequiredToFillFieldsFormat(string.Join(Environment.NewLine, notFilledFields)));
            return;            
          }          
        }
      }        
      #endregion      
    }

    public override bool CanApproved(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return base.CanApproved(e);
    }


    public virtual void NotAgreelitiko(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (string.IsNullOrWhiteSpace(_obj.ActiveText))
      {
        e.AddError(litiko.Eskhata.ApprovalAssignments.Resources.CommentNeeded);
        return;
      }      
      
      var action = new Sungero.Workflow.Client.ExecuteResultActionArgs(e.FormType, e.Entity, e.Action);
      base.Approved(action);
      _obj.IsNotAgreeResultlitiko = true;
      e.CloseFormAfterAction = true;
      _obj.Complete(Result.Approved);      
    }

    public virtual bool CanNotAgreelitiko(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.Addressee == null && _obj.DocumentGroup.OfficialDocuments.Any() && _obj.Status == Sungero.Docflow.ApprovalAssignment.Status.InProcess;
    }

  }

}