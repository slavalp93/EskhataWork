using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.Eskhata.Module.RecordManagementUI.Server
{
  partial class ModuleFunctions
  {
    /// <summary>
    /// Получить задания по типу этапа согласования, в том числе схлопнутые.
    /// </summary>
    /// <param name="query">Фильтруемые задания.</param>
    /// <param name="stageType">Тип этапа согласования.</param>
    /// <returns>Задания.</returns>
    public IQueryable<Sungero.Workflow.IAssignmentBase> GetSpecificAssignmentsWithCollapsed(IQueryable<Sungero.Workflow.IAssignmentBase> query,
                                                                                            Enumeration stageType)
    {
      var needCheckSending = stageType == Sungero.Docflow.ApprovalReviewAssignmentCollapsedStagesTypesRe.StageType.Sending;
      var needCheckPrint = needCheckSending || stageType == Sungero.Docflow.ApprovalReviewAssignmentCollapsedStagesTypesRe.StageType.Print;
      var needCheckRegister = needCheckPrint || stageType == Sungero.Docflow.ApprovalReviewAssignmentCollapsedStagesTypesRe.StageType.Register;
      var isCheckExecution = stageType == Sungero.Docflow.ApprovalReviewAssignmentCollapsedStagesTypesRe.StageType.Execution;
      var needCheckExecution = needCheckRegister || isCheckExecution;
      var needCheckConfirmSign = stageType == Sungero.Docflow.ApprovalReviewAssignmentCollapsedStagesTypesRe.StageType.ConfirmSign;
      var needCheckSign = needCheckExecution && !needCheckConfirmSign;
      var needCheckReview = needCheckSign;
      
      query = query.Where(q => needCheckReview && ApprovalReviewAssignments.Is(q) && ApprovalReviewAssignments.As(q).CollapsedStagesTypesRe.Any(s => s.StageType == stageType) ||
                          needCheckSign && ApprovalSigningAssignments.Is(q) && ApprovalSigningAssignments.As(q).CollapsedStagesTypesSig.Any(s => s.StageType == stageType) ||
                          needCheckConfirmSign && ApprovalSigningAssignments.Is(q) && ApprovalSigningAssignments.As(q).CollapsedStagesTypesSig.Any(s => s.StageType == stageType) &&
                          ApprovalSigningAssignments.As(q).IsConfirmSigning == true ||
                          needCheckExecution && (ApprovalExecutionAssignments.Is(q) && ApprovalExecutionAssignments.As(q).CollapsedStagesTypesExe.Any(s => s.StageType == stageType)) ||
                          needCheckRegister && ApprovalRegistrationAssignments.Is(q) && ApprovalRegistrationAssignments.As(q).CollapsedStagesTypesReg.Any(s => s.StageType == stageType) ||
                          needCheckPrint && ApprovalPrintingAssignments.Is(q) && ApprovalPrintingAssignments.As(q).CollapsedStagesTypesPr.Any(s => s.StageType == stageType) ||
                          needCheckSending && ApprovalSendingAssignments.Is(q) && ApprovalSendingAssignments.As(q).CollapsedStagesTypesSen.Any(s => s.StageType == stageType));
      
      return query;
    }
  }
}