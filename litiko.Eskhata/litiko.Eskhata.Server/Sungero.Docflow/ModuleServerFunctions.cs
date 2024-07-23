using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.Eskhata.Module.Docflow.Server
{
  partial class ModuleFunctions
  {
    [Public]
    public virtual Structures.Module.IConversionToPdfResult GeneratePublicBodyWithSignatureMarkEskhata(Sungero.Docflow.IOfficialDocument document, long versionId, string signatureMark)
    {
      var baseResult = this.GeneratePublicBodyWithSignatureMark(document, versionId, signatureMark);
      var result = Structures.Module.ConversionToPdfResult.Create();
      result.ErrorMessage = baseResult.ErrorMessage;
      result.ErrorTitle = baseResult.ErrorTitle;
      result.HasConvertionError = baseResult.HasConvertionError;
      result.HasErrors = baseResult.HasErrors;
      result.HasLockError = baseResult.HasLockError;
      result.IsFastConvertion = baseResult.IsFastConvertion;
      result.IsOnConvertion = baseResult.IsOnConvertion;
      return result;
    }
    
    public override Sungero.Docflow.Structures.OfficialDocument.ConversionToPdfResult GeneratePublicBodyWithSignatureMark(Sungero.Docflow.IOfficialDocument document, long versionId, string signatureMark)
    {
      return base.GeneratePublicBodyWithSignatureMark(document, versionId, signatureMark);
    }
  }
}