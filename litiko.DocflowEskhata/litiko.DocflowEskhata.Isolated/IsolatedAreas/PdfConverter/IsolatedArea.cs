using System;
using System.Collections.Generic;
using System.Drawing;
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
using Newtonsoft.Json;
using Sungero.Core;
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
  }
}