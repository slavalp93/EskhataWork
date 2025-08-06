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