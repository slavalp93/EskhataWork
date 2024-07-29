using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Archive.ArchiveList;

namespace litiko.Archive
{
  partial class ArchiveListCaseFilesSharedHandlers
  {

    public virtual void CaseFilesCaseFileChanged(litiko.Archive.Shared.ArchiveListCaseFilesCaseFileChangedEventArgs e)
    {
      var selectedCaseFile = e.NewValue;
      if (selectedCaseFile != null)
      {
        _obj.DepartmentIndex = selectedCaseFile.Department != null ? selectedCaseFile.Department.Code : null;
        _obj.DateEndYear = selectedCaseFile.EndDate.HasValue ? selectedCaseFile.EndDate.Value.Year.ToString() : null;
        _obj.RetentionPeriod = selectedCaseFile.RetentionPeriod;
        _obj.DocCount = litiko.Eskhata.PublicFunctions.CaseFile.Remote.CaseFileDocumentsCount(selectedCaseFile);
      }
      else
      {
        _obj.DepartmentIndex = null;
        _obj.DateEndYear = null;
        _obj.RetentionPeriod = null;
        _obj.DocCount = null;        
      }
    }
  }



  partial class ArchiveListSharedHandlers
  {

  }
}