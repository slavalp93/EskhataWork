using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ApprovalStage;

namespace litiko.Eskhata.Shared
{
  partial class ApprovalStageFunctions
  {
    public override List<Enumeration?> GetPossibleRoles()
    {
      var baseRoles = base.GetPossibleRoles();
      baseRoles.Add(DocflowEskhata.UnitManagerApprovalRole.Type.UnitManager);
      
      #region Согласование
      if (_obj.StageType == Sungero.Docflow.ApprovalStage.StageType.Approvers)
      {
        baseRoles.Add(DocflowEskhata.UnitManagerApprovalRole.Type.Signatory);
        baseRoles.Add(Archive.ApprovalRole.Type.Archivist);
        baseRoles.Add(CollegiateAgencies.ApprovalRole.Type.Speaker);
        baseRoles.Add(CollegiateAgencies.ApprovalRole.Type.MeetingSecretary);
        baseRoles.Add(CollegiateAgencies.ApprovalRole.Type.MeetingPresident);
        baseRoles.Add(CollegiateAgencies.ApprovalRole.Type.MeetingMembers);
        baseRoles.Add(CollegiateAgencies.ApprovalRole.Type.MeetingInvited);
        baseRoles.Add(CollegiateAgencies.ApprovalRole.Type.MeetingPresentKOU);
        baseRoles.Add(CollegiateAgencies.ApprovalRole.Type.MeetingPresentDOP);
        baseRoles.Add(RegulatoryDocuments.ApprovalRole.Type.ProcessManager);
        
        baseRoles.Add(Sungero.Docflow.ApprovalRole.Type.Initiator);
      }
      #endregion
      
      #region Задание
      if (_obj.StageType == Sungero.Docflow.ApprovalStage.StageType.SimpleAgr)
      {
        baseRoles.Add(Archive.ApprovalRole.Type.Archivist);
        baseRoles.Add(CollegiateAgencies.ApprovalRole.Type.MeetingSecretary);
        baseRoles.Add(CollegiateAgencies.ApprovalRole.Type.MeetingPresentKOU);
        baseRoles.Add(CollegiateAgencies.ApprovalRole.Type.MeetingPresentDOP);
      }
      #endregion
      
      #region Уведомление
      if (_obj.StageType == Sungero.Docflow.ApprovalStage.StageType.Notice)
      {
        baseRoles.Add(CollegiateAgencies.ApprovalRole.Type.MeetingMembers);
        baseRoles.Add(CollegiateAgencies.ApprovalRole.Type.MeetingInvited);      
        baseRoles.Add(CollegiateAgencies.ApprovalRole.Type.MeetingSecretary);
        baseRoles.Add(CollegiateAgencies.ApprovalRole.Type.MeetingPresident);
        baseRoles.Add(CollegiateAgencies.ApprovalRole.Type.MeetingPresentKOU);
        baseRoles.Add(CollegiateAgencies.ApprovalRole.Type.MeetingPresentDOP);
      }
      #endregion
      
      #region Регистрация
      if (_obj.StageType == Sungero.Docflow.ApprovalStage.StageType.Register)
      {
        baseRoles.Add(CollegiateAgencies.ApprovalRole.Type.MeetingSecretary);
      }
      #endregion      
      
      #region Печать
      if (_obj.StageType == Sungero.Docflow.ApprovalStage.StageType.Print)
      {
        baseRoles.Add(CollegiateAgencies.ApprovalRole.Type.MeetingSecretary);
      }
      #endregion       
      
      return baseRoles;
    }
    
    /// <summary>
    /// Установить видимость свойств.
    /// </summary>
    public override void SetPropertiesVisibility()
    {
      base.SetPropertiesVisibility();
            
      var properties = _obj.State.Properties;
      var type = _obj.StageType;            
      var isApprovers = type == StageType.Approvers;
      properties.AllowResultNotAgreelitiko.IsVisible = isApprovers;
    }
  }
}