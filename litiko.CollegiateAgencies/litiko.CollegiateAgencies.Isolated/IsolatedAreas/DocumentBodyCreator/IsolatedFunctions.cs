using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sungero.Core;
using litiko.CollegiateAgencies.Structures.Module;
using System.IO;
using Aspose.Words;
using System.Reflection;

namespace litiko.CollegiateAgencies.Isolated.DocumentBodyCreator
{
  public class IsolatedFunctions
  {

    /// <summary>
    /// Формирование тела протокола совещания по шаблону
    /// </summary>
    /// <param name="inputStream">Входящий поток тела шаблона</param>
    /// <param name="minutesTemplateInfo">Значения параметров для замены в шаблоне</param>
    /// <returns>Исходящий поток заполненного шаблона</returns>
    [Public]
    public Stream FillMinutesBodyByTemplate(Stream inputStream, Dictionary<string, string> replacebleFields, List<litiko.CollegiateAgencies.Structures.Module.IMeetingResolutionInfo> meetingResolutions)
    {
      try
      {                        
        var document = new Document(inputStream);       

        foreach (var item in replacebleFields)
          document.Range.Replace(item.Key, item.Value);
       
        DocumentBuilder builder = new DocumentBuilder(document);
        bool found = false;
        foreach (Run run in document.GetChildNodes(NodeType.Run, true))
        {                
          var t = run.GetText();
          if (run.Text.Contains("ResolutionList"))
          {
            found = true;
            builder.MoveTo(run);
            run.Remove();
            break;
          }
        }

        if (found)
        {
          string nameForTemplate = string.Empty;
          replacebleFields.TryGetValue("<CategoryNameForTemplate>", out nameForTemplate);
          
          foreach (var resolutionInfo in meetingResolutions)
          {
            builder.ParagraphFormat.LeftIndent = 7.0866;
            builder.Font.Bold = true;
            builder.Writeln(resolutionInfo.ProjectSolutionTittle);
                                        
            builder.ParagraphFormat.LeftIndent = 5.102352;
            builder.ParagraphFormat.FirstLineIndent = 35.433;
            builder.Write("Слушали: ");
            builder.Font.Bold = false;
            builder.Writeln(resolutionInfo.ListenedRU);
            builder.Writeln("После рассмотрения представленного предложения члены " + nameForTemplate);

            builder.ParagraphFormat.LeftIndent = 7.0866;
            builder.ParagraphFormat.FirstLineIndent = 0;
            builder.Font.Bold = true;
            builder.Writeln("ПОСТАНОВИЛИ:");
                    
            builder.Font.Bold = false;
            builder.ParagraphFormat.LeftIndent = 14.1732;
            builder.ParagraphFormat.FirstLineIndent = 28.3464;                    
            builder.Writeln(string.Join(Environment.NewLine, resolutionInfo.Decigions));
            builder.InsertParagraph();                    

            builder.ParagraphFormat.LeftIndent = 40.251888;
            builder.ParagraphFormat.FirstLineIndent = 0;
            builder.Font.Bold = true;
            builder.Writeln("Результат голосования:");
            builder.Writeln(string.Format("«За» - {0} голосов", resolutionInfo.VoutingYes));
            builder.Writeln(string.Format("«Против» - {0} голосов", resolutionInfo.VoutingNo));
            builder.Writeln(string.Format("«Воздержавшихся» - {0} голосов", resolutionInfo.VoutingAbstained));
            string isAccepted = resolutionInfo.VoutingAccepted.HasValue && resolutionInfo.VoutingAccepted.Value ? "Да" : "Нет";
            builder.Writeln("Решение принято - " + isAccepted);
            builder.InsertParagraph();                    
          }
        }        
        
        
        var resultStream = new MemoryStream();
        document.Save(resultStream, SaveFormat.Docx);        
        return resultStream;
      }
      catch (Exception e)
      {
        Logger.Error(string.Format("Cannot FillMinutesBodyByTemplate: {0}. {1}", e.Message, e.StackTrace));
        throw new AppliedCodeException(string.Format("FillMinutesBodyByTemplate: {0}.", e.Message));
      }
    }

  }
}