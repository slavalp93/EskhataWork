using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Aspose.Cells;
using Aspose.Imaging;
using Aspose.Pdf;
using Aspose.Pdf.Text;
using Aspose.Slides;
using Aspose.Words;
using Aspose.Words.Shaping;
using Aspose.BarCode;
using Newtonsoft.Json;
using Sungero.Core;
using Aspose.BarCode.Generation;
using litiko.DocflowEskhata.Structures.Module;

namespace litiko.DocflowEskhata.Isolated.PdfConverter
{
  public class Eskhata_PdfStamper
  {
    public Stream ReplacePhraseInPdf(System.IO.Stream inputStream, string searchPhrase, string replacingText)
    {
      var outputStream = new MemoryStream();
      try
      {
        var pdfDocument = new Aspose.Pdf.Document(inputStream);
        TextFragmentAbsorber textFragmentAbsorber = new TextFragmentAbsorber(searchPhrase);
        pdfDocument.Pages.Accept(textFragmentAbsorber);
        TextFragmentCollection textFragmentCollection = textFragmentAbsorber.TextFragments;
        foreach (TextFragment textFragment in textFragmentCollection)
        {
          textFragment.Text = replacingText;
        }
        pdfDocument.Save(outputStream);
      }
      catch (Exception ex)
      {
        Logger.Error("Cannot replace phrase in Pdf", ex);
        throw new AppliedCodeException("Cannot replace phrase in Pdf");
      }
      return outputStream;
    }
    
    public Stream AddStampToDocument(Stream inputStream, Aspose.Pdf.PdfPageStamp stamp, string phrase, int x, int y)
    {
      try
      {
        // Создание нового потока, в который будет записан документ с отметкой (во входной поток записывать нельзя).
        var outputStream = new MemoryStream();
        var document = new Aspose.Pdf.Document(inputStream);
        // Поднимаем версию и переполучаем документ из потока, чтобы гарантировать читаемость штампа после вставки.
        using (var documentStream = this.GetUpgradedPdf(document))
        {
          document = new Aspose.Pdf.Document(documentStream);
          
          var textFragmentAbsorber = new Aspose.Pdf.Text.TextFragmentAbsorber(phrase);
          document.Pages.Accept(textFragmentAbsorber);
          var textFragmentCollection = textFragmentAbsorber.TextFragments;
          foreach (var textFragment in textFragmentCollection)
          {
            stamp.XIndent = textFragment.Position.XIndent - x;
            stamp.YIndent = textFragment.Position.YIndent - y;
            textFragment.Page.AddStamp(stamp);
          }
          document.Save(outputStream);
        }
        return outputStream;
      }
      catch (Exception ex)
      {
        Logger.Error("Cannot add stamp to document page", ex);
        throw new AppliedCodeException("Cannot add stamp to document page");
      }
      finally
      {
        inputStream.Close();
      }
    }
    
    public Stream GetUpgradedPdf(Aspose.Pdf.Document document)
    {
      
      // Минимальная совместимая версия PDF для корректного отображения отметки.
      string MinCompatibleVersion = "1.4.0.0";
      
      if (!document.IsPdfaCompliant)
      {
        // Получить версию стандарта PDF из свойств документа. Достаточно первых двух чисел, разделённых точкой.
        var versionRegex = new Regex(@"^\d{1,2}\.\d{1,2}");
        var pdfVersionAsString = versionRegex.Match(document.Version).Value;
        var minCompatibleVersion = Version.Parse(MinCompatibleVersion);

        if (Version.TryParse(pdfVersionAsString, out Version version) && version < minCompatibleVersion)
          document.Convert(new Aspose.Pdf.PdfFormatConversionOptions(Aspose.Pdf.PdfFormat.v_1_4));
      }
      // Необходимо пересохранить документ в поток, чтобы изменение версии применилось до простановки отметки, а не после.
      var docStream = new MemoryStream();
      document.Save(docStream);
      return docStream;
    }
    
    public Stream AddQRStampToDocument(Stream inputStream, string text, string phrase, int x, int y)
    {
      try
      {
        var outputStream = new MemoryStream();
        var document = new Aspose.Pdf.Document(inputStream);
        using (var documentStream = this.GetUpgradedPdf(document))
        {
          document = new Aspose.Pdf.Document(documentStream);
          var textFragmentAbsorber = new Aspose.Pdf.Text.TextFragmentAbsorber(phrase);
          document.Pages.Accept(textFragmentAbsorber);
          var textFragmentCollection = textFragmentAbsorber.TextFragments;
          using (var imageStream = GetQRImage(text))
          {
            Aspose.Pdf.Facades.PdfFileMend mender = new Aspose.Pdf.Facades.PdfFileMend();
            mender.BindPdf(document);
            foreach (var textFragment in textFragmentCollection)
            {
              var upperRightX = textFragment.Rectangle.LLX + 50;
              var upperRightY = textFragment.Rectangle.URY;
              var lowerLeftX = textFragment.Rectangle.LLX;
              var lowerLeftY = upperRightY - 50;
              mender.AddImage(imageStream, textFragment.Page.Number, (float)lowerLeftX, (float)lowerLeftY, (float)upperRightX, (float)upperRightY);
            }
            mender.Save(outputStream);
          }
        }
        return outputStream;
      }
      catch (Exception ex)
      {
        Logger.Error("Cannot add stamp to document page", ex);
        throw new AppliedCodeException("Cannot add stamp to document page");
      }
      finally
      {
        inputStream.Close();
      }
    }
    
    public Stream GetQRImage(string qrText)
    {
      var barCodeBuilder = new BarcodeGenerator(EncodeTypes.QR)
      {
        CodeText = qrText
      };
      var barCodeBitmap = barCodeBuilder.GenerateBarCodeImage();
      MemoryStream imageStream = new MemoryStream();
      barCodeBitmap.Save(imageStream, ImageFormat.Png);
      return imageStream;
    }
  }
}