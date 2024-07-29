using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using Sungero.Core;
using litiko.DocflowEskhata.Structures.Module;

namespace litiko.DocflowEskhata.Isolated.PdfConverter
{
  public class IsolatedFunctions
  {
    [Public]
    public Stream AddRegistrationData(Stream inputStream, string registrationNumber, DateTime? registrationDate)
    {
      var pdfStamper = this.CreatePdfStamper();
      try
      {
        var outputStream = pdfStamper.ReplacePhraseInPdf(inputStream, "⚓№", registrationNumber);
        
        var year = registrationDate != null ? registrationDate.Value.ToString("yyyy") : string.Empty;
        var month = registrationDate != null ? registrationDate.Value.ToString("MM") : string.Empty;
        var day = registrationDate != null ? registrationDate.Value.ToString("dd") : string.Empty;
        
        outputStream = pdfStamper.ReplacePhraseInPdf(outputStream, "⚓ гггг", year);
        outputStream = pdfStamper.ReplacePhraseInPdf(outputStream, "⚓ мм", month);
        outputStream = pdfStamper.ReplacePhraseInPdf(outputStream, "⚓ дд", day);
        
        return outputStream;
        
      }
      catch (Exception e)
      {
        Logger.Error("Cannot add AddRegistrationData", e);
        throw new AppliedCodeException("Cannot add AddRegistrationData");
      }
    }
    
    public virtual litiko.DocflowEskhata.Isolated.PdfConverter.Eskhata_PdfStamper CreatePdfStamper()
    {
      return new PdfConverter.Eskhata_PdfStamper();
    }
  }
}