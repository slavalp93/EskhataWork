using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.IncomingLetter;

namespace litiko.Eskhata.Server
{
  partial class IncomingLetterFunctions
  {
    public override string GetRegistrationStampAsHtml()
    {
      var regNumber = _obj.RegistrationNumber;
      var regDate = _obj.RegistrationDate;
      
      if (string.IsNullOrWhiteSpace(regNumber) || regDate == null)
        return string.Empty;
      
      string html;
      using (Sungero.Core.CultureInfoExtensions.SwitchTo(TenantInfo.Culture))
      {
        html = litiko.Eskhata.IncomingLetters.Resources.RegistrationStampEskhata;
        html = html.Replace("{RegNumber}", regNumber.ToString());
        html = html.Replace("{RegDate}", regDate.Value.ToShortDateString());
      }
      
      return html;
    }
  }
}