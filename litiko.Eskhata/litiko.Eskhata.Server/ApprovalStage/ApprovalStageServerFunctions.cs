using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ApprovalStage;

namespace litiko.Eskhata.Server
{
  partial class ApprovalStageFunctions
  {
    // Получить исполнителей этапа без раскрытия групп и ролей.
    // <param name="task">Задача.</param>
    // <param name="additionalApprovers">Доп.согласующие.</param>
    // <returns>Исполнители.</returns>
    [Remote(IsPure = true), Public]
    public override List<IRecipient> GetStageRecipients(Sungero.Docflow.IApprovalTask task, List<IRecipient> additionalApprovers)
    {
      var recipients = base.GetStageRecipients(task, additionalApprovers);

      var roleMeetingMembers = _obj.ApprovalRoles.Where(x => x.ApprovalRole.Type == litiko.CollegiateAgencies.ApprovalRole.Type.MeetingMembers)
        .Select(x => litiko.CollegiateAgencies.ApprovalRoles.As(x.ApprovalRole))
        .Where(x => x != null).SingleOrDefault();
      if (roleMeetingMembers != null)
      {
        recipients.AddRange(litiko.CollegiateAgencies.PublicFunctions.ApprovalRole.Remote.GetRolePerformers(roleMeetingMembers, task));
      }

      var roleMeetingInvited = _obj.ApprovalRoles.Where(x => x.ApprovalRole.Type == litiko.CollegiateAgencies.ApprovalRole.Type.MeetingInvited)
        .Select(x => litiko.CollegiateAgencies.ApprovalRoles.As(x.ApprovalRole))
        .Where(x => x != null).SingleOrDefault();
      if (roleMeetingInvited != null)
      {
        recipients.AddRange(litiko.CollegiateAgencies.PublicFunctions.ApprovalRole.Remote.GetRolePerformers(roleMeetingInvited, task));
      }
      
      var roleMeetingPresentKOU = _obj.ApprovalRoles.Where(x => x.ApprovalRole.Type == litiko.CollegiateAgencies.ApprovalRole.Type.MeetingPresentKOU)
        .Select(x => litiko.CollegiateAgencies.ApprovalRoles.As(x.ApprovalRole))
        .Where(x => x != null).SingleOrDefault();
      if (roleMeetingPresentKOU != null)
      {
        recipients.AddRange(litiko.CollegiateAgencies.PublicFunctions.ApprovalRole.Remote.GetRolePerformers(roleMeetingPresentKOU, task));
      }      
      
      return recipients;
    }
  }
}