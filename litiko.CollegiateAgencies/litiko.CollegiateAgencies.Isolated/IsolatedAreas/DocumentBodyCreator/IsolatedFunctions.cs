using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sungero.Core;
using litiko.CollegiateAgencies.Structures.Module;
using System.IO;
using Aspose.Words;
using Aspose.Words.Lists;
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
    /// <param name="isVoting">Необходимость добавления результатов голосования</param>
    /// <returns>Исходящий поток заполненного шаблона</returns>
    [Public]
    public Stream FillMinutesBodyByTemplate(Stream inputStream, 
                                            Dictionary<string, string> replacebleFields, 
                                            List<string> presentFIOList,
                                            List<string> presentFIOListTJ,
                                            List<string> absentFIOList,
                                            List<string> absentFIOListTJ,
                                            List<string> invitedFIOList,
                                            List<string> invitedFIOListTJ,
                                            List<string> agendaList,
                                            List<string> agendaListTJ,
                                            List<litiko.CollegiateAgencies.Structures.Module.IMeetingResolutionInfo> meetingResolutions
                                           )
    {
      try
      {                        
        var document = new Document(inputStream);       

        foreach (var item in replacebleFields)
          document.Range.Replace(item.Key, item.Value);
       
        DocumentBuilder builder = new DocumentBuilder(document);
        bool found = false;
        
        #region presentFIOList
        if (MoveTopaPagraphContainsText(document, "PresentFIOList", builder))
          AddStringsToNumberedList(builder, presentFIOList);
        
        if (MoveTopaPagraphContainsText(document, "PresentFIOListTJ", builder))
          AddStringsToNumberedList(builder, presentFIOListTJ);
        #endregion
        
        #region absentFIOList
        if (MoveTopaPagraphContainsText(document, "AbsentFIOList", builder))
          AddStringsToNumberedList(builder, absentFIOList);
        
        if (MoveTopaPagraphContainsText(document, "AbsentFIOListTJ", builder))
          AddStringsToNumberedList(builder, absentFIOListTJ);
        #endregion        

        #region invitedFIOList
        if (MoveTopaPagraphContainsText(document, "InvitedFIOList", builder))
          AddStringsToNumberedList(builder, invitedFIOList);
        
        if (MoveTopaPagraphContainsText(document, "InvitedFIOListTJ", builder))
          AddStringsToNumberedList(builder, invitedFIOListTJ);
        #endregion  
        
        #region AgendaList
        if (MoveTopaPagraphContainsText(document, "AgendaList", builder))
        {          
          // Добавляем нумерованный список
          builder.ListFormat.ApplyNumberDefault();
          var numberedList = builder.ListFormat.List;          
          // ОБЩАЯ текстовая позиция и табуляция
          const double textIndentPt = 36.0; // 1.27 см
          
          // Настройка уровня 0 (1., 2.)
          var level0 = numberedList.ListLevels[0];
          level0.NumberFormat = "\u0000.";
          level0.NumberStyle = NumberStyle.Arabic;
          level0.NumberPosition = 0;
          level0.TextPosition = textIndentPt;
          level0.TabPosition = textIndentPt;
          level0.Alignment = ListLevelAlignment.Left;
          if (agendaList.Count == 1)
          {
            var firstAgenda = agendaList.FirstOrDefault();
            var parts = firstAgenda.Split(new[] { "##" }, StringSplitOptions.None);
            if (parts.Length >= 3)
            {
              if (int.TryParse(parts[1], out int number))
              {
                if (number > 1)
                  level0.StartAt = number;              
              }
              
              builder.Writeln(parts[2]);
            }
            else
              builder.Writeln(firstAgenda);
          }
          else
          {
            foreach (var agendaText in agendaList)
            {
              builder.Writeln(agendaText);
            }          
          }
          builder.ListFormat.RemoveNumbers();
            
        }        
        #endregion
        
        #region AgendaListTJ
        if (MoveTopaPagraphContainsText(document, "AgendaListTJ", builder))
        {          
            AddStringsToNumberedList(builder, agendaListTJ);                                                                
        }        
        #endregion        
        
        #region ResolutionList v2       
        if (MoveTopaPagraphContainsText(document, "ResolutionList", builder))
        {                  
          string nameForTemplate = string.Empty;
          replacebleFields.TryGetValue("<CategoryNameForTemplate>", out nameForTemplate);          
          
          // Добавляем нумерованный список
          builder.ListFormat.ApplyNumberDefault();
          var numberedList = builder.ListFormat.List;          
          // ОБЩАЯ текстовая позиция и табуляция
          const double textIndentPt = 36.0; // 1.27 см
          
          // Настройка уровня 0 (1., 2.)
          var level0 = numberedList.ListLevels[0];
          level0.NumberFormat = "\u0000.";
          level0.NumberStyle = NumberStyle.Arabic;
          level0.NumberPosition = 0;
          level0.TextPosition = textIndentPt;
          level0.TabPosition = textIndentPt;
          level0.Alignment = ListLevelAlignment.Left;
          
          // Настройка уровня 1 (1.1., 1.2.)
          var level1 = numberedList.ListLevels[1];
          level1.NumberFormat = "\u0000.\u0001.";
          level1.NumberStyle = NumberStyle.Arabic;
          level1.NumberPosition = 0;
          level1.TextPosition = textIndentPt;
          level1.TabPosition = textIndentPt;
          level1.Alignment = ListLevelAlignment.Left;
          
          foreach (var resolutionInfo in meetingResolutions)
          {            
            // Установка отступов
            builder.ParagraphFormat.LeftIndent = 0;
            builder.ParagraphFormat.FirstLineIndent = -textIndentPt;            
            // Межстрочный интервал
            builder.ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
            builder.ParagraphFormat.LineSpacing = 1.15 * 12;            
            // Интервалы до/после абзаца
            builder.ParagraphFormat.SpaceBefore = 0;
            builder.ParagraphFormat.SpaceAfter = 0;            
            // Выравнивание
            builder.ParagraphFormat.Alignment = ParagraphAlignment.Justify;            
            
            if (resolutionInfo.Number.HasValue && resolutionInfo.Number.Value > 1)
              level0.StartAt = resolutionInfo.Number.Value;
            
            builder.ListFormat.List = numberedList;
            builder.ListFormat.ListLevelNumber = 0;
            builder.Font.Bold = true;
            builder.Writeln(resolutionInfo.ProjectSolutionTittle);
            builder.ListFormat.RemoveNumbers();            
                        
            builder.ParagraphFormat.LeftIndent = textIndentPt;
            builder.ParagraphFormat.FirstLineIndent = 0;
            builder.ParagraphFormat.RightIndent = 0;            
            builder.ParagraphFormat.SpaceBefore = 0;
            builder.ParagraphFormat.SpaceAfter = 0;            
            builder.ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
            builder.ParagraphFormat.LineSpacing = 1.15 * 12;            
            builder.ParagraphFormat.Alignment = ParagraphAlignment.Justify;            
            builder.Write("Слушали: ");
            builder.Font.Bold = false;
            builder.Writeln(resolutionInfo.SpeakerRU);
            
            builder.ParagraphFormat.LeftIndent = 0;
            builder.Writeln(resolutionInfo.ListenedRU);
            builder.Writeln("После рассмотрения представленного предложения члены " + nameForTemplate);
            
            builder.Font.Bold = true;
            builder.Writeln("ПОСТАНОВИЛИ:");
                    
            builder.Font.Bold = false;
            foreach (var decisionText in resolutionInfo.Decigions.Split(new[] { "##DECISION##" }, StringSplitOptions.RemoveEmptyEntries))
            {
              builder.ListFormat.List = numberedList;
              builder.ListFormat.ListLevelNumber = 1;
              builder.ParagraphFormat.LeftIndent = textIndentPt;              
              builder.ParagraphFormat.FirstLineIndent = -textIndentPt;              
              
              var lines = decisionText
                .Replace("\r\n", "\n") // normalize all endings
                .Split(new[] { "\n" }, StringSplitOptions.None);
              bool firstLine = true;
              foreach (var line in lines)
              {                
                if (firstLine)
                {
                  builder.Writeln(line);
                  firstLine = false;
                }
                else
                {
                    builder.ListFormat.RemoveNumbers();
                    builder.ParagraphFormat.FirstLineIndent = 0;
                    builder.Writeln(line);        
                }                
              }                            
              builder.ListFormat.RemoveNumbers();
            }
            builder.ListFormat.RemoveNumbers();
            builder.InsertParagraph();                    

            if (resolutionInfo.WithVoting)
            {
              builder.Font.Bold = true;
              builder.ParagraphFormat.LeftIndent = textIndentPt;
              builder.ParagraphFormat.FirstLineIndent = 0;
              builder.Writeln("Результат голосования:");
              builder.Writeln(string.Format("«За» - {0} голосов", resolutionInfo.VoutingYes));
              builder.Writeln(string.Format("«Против» - {0} голосов", resolutionInfo.VoutingNo));
              builder.Writeln(string.Format("«Воздержавшихся» - {0} голосов", resolutionInfo.VoutingAbstained));
              string isAccepted = string.Empty;
              if (resolutionInfo.VoutingAccepted.HasValue && resolutionInfo.VoutingAccepted.Value)
              {
                if (resolutionInfo.VoutingNo == 0 && resolutionInfo.VoutingAbstained == 0)
                  isAccepted = "единогласно";
                else
                  isAccepted = "большинством голосов";
              }
              else
                isAccepted = "Нет";
              
              builder.Writeln("Решение принято - " + isAccepted);
              builder.InsertParagraph();
            }
          }
        }

        if (MoveTopaPagraphContainsText(document, "ResolutionListTJ", builder))
        {                                  
          string nameForTemplateTJ = string.Empty;
          replacebleFields.TryGetValue("<CategoryNameForTemplateTJ>", out nameForTemplateTJ);          
          
          // Добавляем нумерованный список
          builder.ListFormat.ApplyNumberDefault();
          var numberedList = builder.ListFormat.List;          
          // ОБЩАЯ текстовая позиция и табуляция
          const double textIndentPt = 36.0; // 1.27 см
          
          // Настройка уровня 0 (1., 2.)
          var level0 = numberedList.ListLevels[0];
          level0.NumberFormat = "\u0000.";
          level0.NumberStyle = NumberStyle.Arabic;
          level0.NumberPosition = 0;
          level0.TextPosition = textIndentPt;
          level0.TabPosition = textIndentPt;
          level0.Alignment = ListLevelAlignment.Left;
          
          // Настройка уровня 1 (1.1., 1.2.)
          var level1 = numberedList.ListLevels[1];
          level1.NumberFormat = "\u0000.\u0001.";
          level1.NumberStyle = NumberStyle.Arabic;
          level1.NumberPosition = 0;
          level1.TextPosition = textIndentPt;
          level1.TabPosition = textIndentPt;
          level1.Alignment = ListLevelAlignment.Left;
          
          foreach (var resolutionInfo in meetingResolutions)
          {            
            // Установка отступов
            builder.ParagraphFormat.LeftIndent = 0;
            builder.ParagraphFormat.FirstLineIndent = -textIndentPt;            
            // Межстрочный интервал
            builder.ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
            builder.ParagraphFormat.LineSpacing = 1.15 * 12;            
            // Интервалы до/после абзаца
            builder.ParagraphFormat.SpaceBefore = 0;
            builder.ParagraphFormat.SpaceAfter = 0;            
            // Выравнивание
            builder.ParagraphFormat.Alignment = ParagraphAlignment.Justify;            
            
            builder.ListFormat.List = numberedList;
            builder.ListFormat.ListLevelNumber = 0;
            builder.Font.Bold = true;
            builder.Writeln(resolutionInfo.ProjectSolutionTittle);
            builder.ListFormat.RemoveNumbers();            
                        
            builder.ParagraphFormat.LeftIndent = textIndentPt;
            builder.ParagraphFormat.FirstLineIndent = 0;
            builder.ParagraphFormat.RightIndent = 0;            
            builder.ParagraphFormat.SpaceBefore = 0;
            builder.ParagraphFormat.SpaceAfter = 0;            
            builder.ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
            builder.ParagraphFormat.LineSpacing = 1.15 * 12;            
            builder.ParagraphFormat.Alignment = ParagraphAlignment.Justify;            
            builder.Write("Шунида шуд: ");
            builder.Font.Bold = false;
            builder.Writeln(resolutionInfo.SpeakerRU);
            
            builder.ParagraphFormat.LeftIndent = 0;
            builder.Writeln(resolutionInfo.ListenedRU);
            builder.Writeln("Пас аз баррасии пешниҳоди муаррифишуда аъзоёни " + nameForTemplateTJ);
            
            builder.Font.Bold = true;
            builder.Writeln("ҚАРОР КАРДАНД:");
                    
            builder.Font.Bold = false;
            foreach (var decisionText in resolutionInfo.DecigionsTJ.Split(new[] { "##DECISION##" }, StringSplitOptions.RemoveEmptyEntries))
            {              
              builder.ListFormat.List = numberedList;
              builder.ListFormat.ListLevelNumber = 1;
              builder.ParagraphFormat.LeftIndent = textIndentPt;              
              builder.ParagraphFormat.FirstLineIndent = -textIndentPt;              
              
              var lines = decisionText
                .Replace("\r\n", "\n") // normalize all endings
                .Split(new[] { "\n" }, StringSplitOptions.None);
              bool firstLine = true;
              foreach (var line in lines)
              {                
                if (firstLine)
                {
                  builder.Writeln(line);
                  firstLine = false;
                }
                else
                {
                    builder.ListFormat.RemoveNumbers();
                    builder.ParagraphFormat.FirstLineIndent = 0;
                    builder.Writeln(line);        
                }                
              }                            
              builder.ListFormat.RemoveNumbers();
            }
            builder.ListFormat.RemoveNumbers();
            builder.InsertParagraph();                    

            if (resolutionInfo.WithVoting)
            {
              builder.Font.Bold = true;
              builder.ParagraphFormat.LeftIndent = textIndentPt;
              builder.ParagraphFormat.FirstLineIndent = 0;
              builder.Writeln("Натиҷаи овоздиҳӣ:");
              builder.Writeln(string.Format("«Тарафдор» - {0} овоз", resolutionInfo.VoutingYes));
              builder.Writeln(string.Format("«Муқобил» - {0} овоз", resolutionInfo.VoutingNo));
              builder.Writeln(string.Format("«Худдорӣ кард» - {0} овоз", resolutionInfo.VoutingAbstained));
              string isAccepted = string.Empty;
              if (resolutionInfo.VoutingAccepted.HasValue && resolutionInfo.VoutingAccepted.Value)
              {
                if (resolutionInfo.VoutingNo == 0 && resolutionInfo.VoutingAbstained == 0)
                  isAccepted = "якдилона";
                else
                  isAccepted = "бо аксарияти овоз";
              }
              else
                isAccepted = "Не";
              
              builder.Writeln("Карор кабул карда шуд - " + isAccepted);
              builder.InsertParagraph();
            }
          }
        }                
        #endregion        
        
        #region ResolutionList        
        /*
        if (MoveTopaPagraphContainsText(document, "ResolutionList", builder))
        {                  
          string nameForTemplate = string.Empty;
          replacebleFields.TryGetValue("<CategoryNameForTemplate>", out nameForTemplate);
          
          foreach (var resolutionInfo in meetingResolutions)
          {
            builder.ParagraphFormat.LeftIndent = 7.0866;
            builder.ParagraphFormat.FirstLineIndent = 0;
            builder.Font.Bold = true;
            builder.Writeln(resolutionInfo.ProjectSolutionTittle);
                                        
            builder.ParagraphFormat.LeftIndent = 5.102352;
            builder.ParagraphFormat.FirstLineIndent = 35.433;
            builder.Write("Слушали: ");
            builder.Font.Bold = false;
            builder.Writeln(resolutionInfo.SpeakerRU);
            builder.Writeln(resolutionInfo.ListenedRU);
            builder.Writeln("После рассмотрения представленного предложения члены " + nameForTemplate);
            
            builder.ParagraphFormat.LeftIndent = 7.0866;
            builder.ParagraphFormat.FirstLineIndent = 0;
            builder.Font.Bold = true;
            builder.InsertParagraph();
            builder.Writeln("ПОСТАНОВИЛИ:");
                    
            builder.Font.Bold = false;
            builder.ParagraphFormat.LeftIndent = 14.1732;
            builder.ParagraphFormat.FirstLineIndent = 28.3464;                    
            builder.Writeln(string.Join(Environment.NewLine, resolutionInfo.Decigions));
            builder.InsertParagraph();                    

            if (resolutionInfo.WithVoting)
            {
              builder.ParagraphFormat.LeftIndent = 40.251888;
              builder.ParagraphFormat.FirstLineIndent = 0;
              builder.Font.Bold = true;
              builder.Writeln("Результат голосования:");
              builder.Writeln(string.Format("«За» - {0} голосов", resolutionInfo.VoutingYes));
              builder.Writeln(string.Format("«Против» - {0} голосов", resolutionInfo.VoutingNo));
              builder.Writeln(string.Format("«Воздержавшихся» - {0} голосов", resolutionInfo.VoutingAbstained));
              string isAccepted = string.Empty;
              if (resolutionInfo.VoutingAccepted.HasValue && resolutionInfo.VoutingAccepted.Value)
              {
                if (resolutionInfo.VoutingNo == 0 && resolutionInfo.VoutingAbstained == 0)
                  isAccepted = "единогласно";
                else
                  isAccepted = "большинством голосов";
              }
              else
                isAccepted = "Нет";
              
              builder.Writeln("Решение принято - " + isAccepted);
              builder.InsertParagraph();            
            }
          }
        }        
        
        found = false;
        foreach (Run run in document.GetChildNodes(NodeType.Run, true))
        {                
          var t = run.GetText();
          if (run.Text.Contains("ResolutionListTJ"))
          {
            found = true;
            builder.MoveTo(run);
            run.Remove();
            break;
          }
        }

        if (found)
        {
          string nameForTemplateTJ = string.Empty;
          replacebleFields.TryGetValue("<CategoryNameForTemplateTJ>", out nameForTemplateTJ);
          
          foreach (var resolutionInfo in meetingResolutions)
          {
            builder.ParagraphFormat.LeftIndent = 7.0866;
            builder.ParagraphFormat.FirstLineIndent = 0;
            builder.Font.Bold = true;
            builder.Writeln(resolutionInfo.ProjectSolutionTittleTJ);
                                        
            builder.ParagraphFormat.LeftIndent = 5.102352;
            builder.ParagraphFormat.FirstLineIndent = 35.433;
            builder.Write("Шунида шуд: ");
            builder.Font.Bold = false;
            builder.Writeln(resolutionInfo.SpeakerTJ);
            builder.Writeln(resolutionInfo.ListenedTJ);
            builder.Writeln("Пас аз баррасии пешниҳоди муаррифишуда аъзоёни " + nameForTemplateTJ);
            
            builder.ParagraphFormat.LeftIndent = 7.0866;
            builder.ParagraphFormat.FirstLineIndent = 0;
            builder.Font.Bold = true;
            builder.InsertParagraph();
            builder.Writeln("ҚАРОР КАРДАНД:");
                    
            builder.Font.Bold = false;
            builder.ParagraphFormat.LeftIndent = 14.1732;
            builder.ParagraphFormat.FirstLineIndent = 28.3464;                    
            builder.Writeln(string.Join(Environment.NewLine, resolutionInfo.DecigionsTJ));
            builder.InsertParagraph();                    

            if (resolutionInfo.WithVoting)
            {
              builder.ParagraphFormat.LeftIndent = 40.251888;
              builder.ParagraphFormat.FirstLineIndent = 0;
              builder.Font.Bold = true;
              builder.Writeln("Натиҷаи овоздиҳӣ:");
              builder.Writeln(string.Format("«Тарафдор» - {0} овоз", resolutionInfo.VoutingYes));
              builder.Writeln(string.Format("«Муқобил» - {0} овоз", resolutionInfo.VoutingNo));
              builder.Writeln(string.Format("«Худдорӣ кард» - {0} овоз", resolutionInfo.VoutingAbstained));              
              string isAccepted = string.Empty;
              if (resolutionInfo.VoutingAccepted.HasValue && resolutionInfo.VoutingAccepted.Value)
              {
                if (resolutionInfo.VoutingNo == 0 && resolutionInfo.VoutingAbstained == 0)
                  isAccepted = "якдилона";
                else
                  isAccepted = "бо аксарияти овоз";
              }
              else
                isAccepted = "Не";              
              builder.Writeln("Карор кабул карда шуд - " + isAccepted);
              builder.InsertParagraph();            
            }
          }
        }
        */
        #endregion        
        
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

    private bool MoveTopaPagraphContainsText(Document document, string searchText, DocumentBuilder builder)
    {
        var paragraphs = document.GetChildNodes(NodeType.Paragraph, true).Cast<Paragraph>().ToList();
    
        for (int i = 0; i < paragraphs.Count; i++)
        {
            var paragraph = paragraphs[i];
            if (paragraph.GetText().Contains(searchText))
            {
                foreach (var run in paragraph.GetChildNodes(NodeType.Run, true).Cast<Run>().ToList())
                  run.Remove();
                
                if (i < paragraphs.Count - 1)
                    builder.MoveTo(paragraphs[i]);
                else
                    builder.MoveToDocumentEnd();
    
                return true;
            }
        }
    
        return false;
    }

    private void AddStringsToNumberedList(DocumentBuilder builder, IList<string> strings)
    {
        if (strings == null || strings.Count == 0)
            return;
    
        for (int i = 0; i < strings.Count; i++)
        {
            if (i < strings.Count - 1)
            {
                builder.Writeln(strings[i]);
            }
            else
            {
                // Вставляем строку в текущий Run
                builder.Write(strings[i]);                
            }
        }
    }


  }
}