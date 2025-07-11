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
    public const int BottomIndent = 20;
    
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
    
    public Stream AddHtmlMark(Stream inputStream, string htmlMark, string anchorSymbol)
    {
      try
      {
        var document = new Aspose.Pdf.Document(inputStream);
        var mark = this.CreateStampFromHtml(htmlMark);
        var pageLimit = 5;
        var pagesCount = document.Pages.Count > pageLimit ? pageLimit : document.Pages.Count;
        // Поиск символа производится постранично с конца документа.
        for (var pageNumber = 1; pageNumber <= pagesCount; pageNumber++)
        {
          var page = document.Pages[pageNumber];
          var lastAnchorEntry = GetLastAnchorEntry(page, anchorSymbol);
          if (lastAnchorEntry == null)
            continue;
          
          lastAnchorEntry.Text = "";
          
          // Установить центры символа-якоря и отметки об ЭП на одной линии по горизонтали.
          mark.XIndent = lastAnchorEntry.Position.XIndent;
          mark.YIndent = lastAnchorEntry.Position.YIndent - (mark.Height / 2) + (lastAnchorEntry.Rectangle.Height / 2);
          
          var updatedStream = new MemoryStream();
          document.Save(updatedStream);
          updatedStream.Position = 0;

          // Передаём в метод установки штампа уже изменённый поток
          return AddStampToDocumentPage(updatedStream, page.Number, mark);
        }

        return inputStream;
      }
      catch (Exception ex)
      {
        Logger.Error("Cannot add stamp", ex);
        throw new AppliedCodeException("Cannot add stamp");
      }
    }
    
    public virtual TextFragment GetLastAnchorEntry(Aspose.Pdf.Page page, string anchor)
    {
      var absorber = new TextFragmentAbsorber(anchor);
      page.Accept(absorber);
      if (absorber.TextFragments.Count == 0)
        return null;

      // Найти последнее вхождение символа-якоря на странице.
      // Условное самое первое вхождение будет иметь координаты левого верхнего угла.
      // https://forum.aspose.com/t/textfragment-at-top-of-page/64774.
      // Ось X - горизонтальная.
      // Ось Y - вертикальная.
      // Начало координат - левый нижний угол.
      var lastEntry = new TextFragment();
      var rectConsiderRotation = page.GetPageRect(true);
      lastEntry.Position.XIndent = 0;
      lastEntry.Position.YIndent = rectConsiderRotation.Height;
      foreach (TextFragment textFragment in absorber.TextFragments)
      {
        if (textFragment.Position.YIndent < lastEntry.Position.YIndent ||
            textFragment.Position.YIndent == lastEntry.Position.YIndent &&
            textFragment.Position.XIndent > lastEntry.Position.XIndent)
          lastEntry = textFragment;
      }

      return lastEntry;
    }
    
    public virtual Aspose.Pdf.PdfPageStamp CreateStampFromHtml(string html)
    {
      try
      {
        Aspose.Pdf.HtmlLoadOptions objLoadOptions = new Aspose.Pdf.HtmlLoadOptions();
        
        objLoadOptions.PageInfo.Margin = new Aspose.Pdf.MarginInfo(0, 0, 0, 0);
        
        Aspose.Pdf.Document stampDoc;
        using (var htmlStamp = new MemoryStream(Encoding.UTF8.GetBytes(html)))
          stampDoc = new Aspose.Pdf.Document(htmlStamp, objLoadOptions);
        var firstPage = stampDoc.Pages[1];
        var contentBox = firstPage.CalculateContentBBox();
        objLoadOptions.PageInfo.Width = contentBox.Width;
        objLoadOptions.PageInfo.Height = contentBox.Height;
        using (var htmlStamp = new MemoryStream(Encoding.UTF8.GetBytes(html)))
          stampDoc = new Aspose.Pdf.Document(htmlStamp, objLoadOptions);
        if (stampDoc.Pages.Count > 0)
        {
          var mark = new Aspose.Pdf.PdfPageStamp(stampDoc.Pages[1]);
          mark.Background = false;
          return mark;
        }
        return null;
      }
      catch (Exception ex)
      {
        Logger.Error("Cannot create stamp from html", ex);
        throw new AppliedCodeException("Cannot create stamp from html");
      }
    }
    
    public virtual Stream AddStampToDocumentPage(Stream inputStream, int pageNumber, Aspose.Pdf.PdfPageStamp stamp)
    {
      try
      {
        // Создание нового потока, в который будет записан документ с отметкой (во входной поток записывать нельзя).
        var outputStream = new MemoryStream();
        var document = new Aspose.Pdf.Document(inputStream);
        // Поднимаем версию и переполучаем документ из потока,
        // чтобы гарантировать читаемость штампа после вставки.
        using (var documentStream = this.GetUpgradedPdf(document))
        {
          document = new Aspose.Pdf.Document(documentStream);

          var documentPage = document.Pages[pageNumber];
          var rectConsiderRotation = documentPage.GetPageRect(true);
          if (stamp.Width > rectConsiderRotation.Width || stamp.Width > (rectConsiderRotation.Height - BottomIndent))
          {
            inputStream.CopyTo(outputStream);
          }
          else
          {
            documentPage.AddStamp(stamp);
            document.Save(outputStream);
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
    
    public int GetLastSearchablePage(int docPagesCount, int searchablePagesNumber, string extension)
    {
      var excelFormats = new List<string>() { "xls", "xlsx", "ods" };
      
      return docPagesCount > searchablePagesNumber && excelFormats.Contains(extension) ?
        docPagesCount - searchablePagesNumber :
        0;
    }
    
    public virtual TextFragment GetLastAnchorEntry(Aspose.Pdf.Page page, string anchor)
    {
      var absorber = new TextFragmentAbsorber(anchor);
      page.Accept(absorber);
      if (absorber.TextFragments.Count == 0)
        return null;

      // Найти последнее вхождение символа-якоря на странице.
      // Условное самое первое вхождение будет иметь координаты левого верхнего угла.
      // https://forum.aspose.com/t/textfragment-at-top-of-page/64774.
      // Ось X - горизонтальная.
      // Ось Y - вертикальная.
      // Начало координат - левый нижний угол.
      var lastEntry = new TextFragment();
      var rectConsiderRotation = page.GetPageRect(true);
      lastEntry.Position.XIndent = 0;
      lastEntry.Position.YIndent = rectConsiderRotation.Height;
      foreach (TextFragment textFragment in absorber.TextFragments)
      {
        if (textFragment.Position.YIndent < lastEntry.Position.YIndent ||
            textFragment.Position.YIndent == lastEntry.Position.YIndent &&
            textFragment.Position.XIndent > lastEntry.Position.XIndent)
          lastEntry = textFragment;
      }

      return lastEntry;
    }
    
    public virtual Stream AddStampToDocumentPage(Stream inputStream, int pageNumber, Aspose.Pdf.PdfPageStamp stamp)
    {
      try
      {
        // Создание нового потока, в который будет записан документ с отметкой (во входной поток записывать нельзя).
        var outputStream = new MemoryStream();
        var document = new Aspose.Pdf.Document(inputStream);
        // Поднимаем версию и переполучаем документ из потока,
        // чтобы гарантировать читаемость штампа после вставки.
        using (var documentStream = this.GetUpgradedPdf(document))
        {
          document = new Aspose.Pdf.Document(documentStream);

          var documentPage = document.Pages[pageNumber];
          var rectConsiderRotation = documentPage.GetPageRect(true);
          if (stamp.Width > rectConsiderRotation.Width || stamp.Width > (rectConsiderRotation.Height - BottomIndent))
          {
            inputStream.CopyTo(outputStream);
          }
          else
          {
            documentPage.AddStamp(stamp);
            document.Save(outputStream);
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
  }
}