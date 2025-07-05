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
    public Stream AddRegistrationData(Stream inputStream, string registrationNumber, DateTime? registrationDate, DateTime? outgoingLetterDate, string outgoingLetterNo)
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
        
        string stamp = GenerateRegistrationStampHtml(registrationNumber, registrationDate, outgoingLetterNo, outgoingLetterDate);
        outputStream = pdfStamper.AddHtmlMark(outputStream, stamp, "⚓&");
        
        return outputStream;
        
      }
      catch (Exception e)
      {
        Logger.Error("Cannot add AddRegistrationData", e);
        throw new AppliedCodeException("Cannot add AddRegistrationData");
      }
    }
    
    private string GenerateRegistrationStampHtml(string registrationNumber, DateTime? registrationDate, string outgoingLetterNo, DateTime? outgoingLetterDate)
    {
      string regDay = registrationDate?.ToString("dd") ?? "";
      string regMonth = registrationDate?.ToString("MM") ?? "";
      string regYear = registrationDate?.ToString("yyyy") ?? "";

      string outDay = outgoingLetterDate?.ToString("dd") ?? "";
      string outMonth = outgoingLetterDate?.ToString("MM") ?? "";
      string outYear = outgoingLetterDate?.ToString("yyyy") ?? "";

      bool hasOutgoing = !string.IsNullOrWhiteSpace(outgoingLetterNo) || outgoingLetterDate != null;

      var sb = new System.Text.StringBuilder();
      sb.AppendLine("<table style='border-collapse: collapse; font-family: Arial; font-size: 12px;'>");

      // Перша строка
      sb.AppendLine("  <tr>");
      sb.AppendLine("    <td style='font-weight: bold; padding-right: 5px;'>Сод. №</td>");
      sb.AppendLine($"    <td style='padding-right: 10px;'>{registrationNumber}</td>");
      sb.AppendLine("    <td style='font-weight: bold; padding-right: 5px;'>аз</td>");
      sb.AppendLine($"    <td>«{regDay}» {regMonth} {regYear}</td>");
      sb.AppendLine("  </tr>");

      if (hasOutgoing)
      {
        // Друга строка
        sb.AppendLine("  <tr>");
        sb.AppendLine("    <td style='font-weight: bold; padding-right: 5px;'>Ба №</td>");
        sb.AppendLine($"    <td style='padding-right: 10px;'>{outgoingLetterNo}</td>");
        sb.AppendLine("    <td style='font-weight: bold; padding-right: 5px;'>аз</td>");
        sb.AppendLine($"    <td>«{outDay}» {outMonth} {outYear}</td>");
        sb.AppendLine("  </tr>");
      }

      sb.AppendLine("</table>");

      return sb.ToString();
    }

    [Public]
    public Stream AddSignatureQRStamp(Stream inputStream, string qrText, string phrase)
    {
      var pdfStamper = CreatePdfStamper();
      try
      {
        return pdfStamper.AddQRStampToDocument(inputStream, qrText, phrase, 6, 80);
      }
      catch (Exception e)
      {
        Logger.Error("Cannot add stamp", e);
        throw new AppliedCodeException("Cannot add stamp");
      }
    }
    
    public virtual litiko.DocflowEskhata.Isolated.PdfConverter.Eskhata_PdfStamper CreatePdfStamper()
    {
      return new PdfConverter.Eskhata_PdfStamper();
    }
  }
}