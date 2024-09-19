using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.ApprovalSimpleAssignment;

namespace litiko.Eskhata
{
  partial class ApprovalSimpleAssignmentVotinglitikoClientHandlers
  {

    public virtual void VotinglitikoAbstainedValueInput(Sungero.Presentation.BooleanValueInputEventArgs e)
    {
      if (e.NewValue == true)
      {
        _obj.No = false;
        _obj.Yes = false;
        
        _obj.State.Properties.Comment.IsRequired = true;
      }      
    }

    public virtual void VotinglitikoNoValueInput(Sungero.Presentation.BooleanValueInputEventArgs e)
    {
      if (e.NewValue == true)
      {
        _obj.Yes = false;
        _obj.Abstained = false;
        
        _obj.State.Properties.Comment.IsRequired = true;
      }      
    }

    public virtual void VotinglitikoYesValueInput(Sungero.Presentation.BooleanValueInputEventArgs e)
    {
      if (e.NewValue == true)
      {
        _obj.No = false;
        _obj.Abstained = false;
        
        _obj.State.Properties.Comment.IsRequired = false;
      }                    
    }
  }

  partial class ApprovalSimpleAssignmentClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);
      
      //_obj.State.Properties.Votinglitiko.IsVisible = _obj.CustomStageTypelitiko == litiko.Eskhata.ApprovalSimpleAssignment.CustomStageTypelitiko.Voting;
    }

  }
}