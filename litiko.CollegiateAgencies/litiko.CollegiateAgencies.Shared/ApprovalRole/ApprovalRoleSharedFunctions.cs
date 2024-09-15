using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.CollegiateAgencies.ApprovalRole;

namespace litiko.CollegiateAgencies.Shared
{
  partial class ApprovalRoleFunctions
  {
    public override List<Sungero.Docflow.IDocumentKind> Filter(List<Sungero.Docflow.IDocumentKind> kinds)
    {
      var query = base.Filter(kinds);
      
      #region Докладчик
      if (_obj.Type == litiko.CollegiateAgencies.ApprovalRole.Type.Speaker)
        query = query.Where(k => k.DocumentType.DocumentTypeGuid == PublicConstants.Module.DocumentTypeGuids.ProjectSolution.ToString()).ToList();
      #endregion
      
      #region Секретарь совещания
      if (_obj.Type == litiko.CollegiateAgencies.ApprovalRole.Type.MeetingSecretary)
      {
        query = query.Where(k => k.DocumentType.DocumentTypeGuid == PublicConstants.Module.DocumentTypeGuids.ProjectSolution.ToString() || 
                            k.DocumentType.DocumentTypeGuid == PublicConstants.Module.DocumentTypeGuids.Agenda.ToString() ||
                            k.DocumentType.DocumentTypeGuid == PublicConstants.Module.DocumentTypeGuids.Minutes.ToString() ||
                            k.DocumentType.DocumentTypeGuid == PublicConstants.Module.DocumentTypeGuids.Addendum.ToString()
                           )
          .ToList();
      }
      #endregion

      #region Председатель совещания
      if (_obj.Type == litiko.CollegiateAgencies.ApprovalRole.Type.MeetingPresident)
      {
        query = query.Where(k => k.DocumentType.DocumentTypeGuid == PublicConstants.Module.DocumentTypeGuids.ProjectSolution.ToString() || 
                            k.DocumentType.DocumentTypeGuid == PublicConstants.Module.DocumentTypeGuids.Agenda.ToString() ||
                            k.DocumentType.DocumentTypeGuid == PublicConstants.Module.DocumentTypeGuids.Minutes.ToString() ||
                            k.DocumentType.DocumentTypeGuid == PublicConstants.Module.DocumentTypeGuids.Addendum.ToString()
                           )
          .ToList();
      }
      #endregion
      
      #region Участники совещания
      if (_obj.Type == litiko.CollegiateAgencies.ApprovalRole.Type.MeetingMembers)
      {
        query = query.Where(k => k.DocumentType.DocumentTypeGuid == PublicConstants.Module.DocumentTypeGuids.Agenda.ToString() ||
                            k.DocumentType.DocumentTypeGuid == PublicConstants.Module.DocumentTypeGuids.Minutes.ToString())
          .ToList();
      }
      #endregion      

      #region Приглашенные сотрудники
      if (_obj.Type == litiko.CollegiateAgencies.ApprovalRole.Type.MeetingInvited)
      {
        query = query.Where(k => k.DocumentType.DocumentTypeGuid == PublicConstants.Module.DocumentTypeGuids.Agenda.ToString() ||
                            k.DocumentType.DocumentTypeGuid == PublicConstants.Module.DocumentTypeGuids.Minutes.ToString())
          .ToList();
      }
      #endregion             
      
      return query;
    }
  }
}