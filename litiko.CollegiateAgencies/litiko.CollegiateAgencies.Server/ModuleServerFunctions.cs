using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Shared;

namespace litiko.CollegiateAgencies.Server
{
  public class ModuleFunctions
  {
    
    /// <summary>
    /// Проверка изменения свойства
    /// </summary>
    /// <param name="entity">Сущность или строка коллекции</param>
    /// <param name="propertyMetadata">Метаданные свойства</param>
    /// <returns>Сообщение об изменении свойства, иначе string.Empty</returns>
    private static string CheckRequisite(Sungero.Domain.Shared.IEntity entity, Sungero.Metadata.PropertyMetadata propertyMetadata)
    {
      var stateProperties = entity.State.Properties;
      var propertyState = stateProperties.GetType().GetProperty(propertyMetadata.Name).GetValue(stateProperties);
      
      //Получим текущее значение свойства
      var newValue = propertyMetadata.GetValue(entity);
      //Получим предыдущее значение свойства, до сохранения
      var originalValue = propertyState.GetType().GetProperty("OriginalValue").GetValue(propertyState);
      
      if (Equals(newValue, originalValue))
        return string.Empty;
      
      //Если тип свойства Enumeration, то получим локализированные значения
      if (propertyMetadata.PropertyType == Sungero.Metadata.PropertyType.Enumeration)
      {
        var infoProperties = entity.Info.Properties;
        var propertyInfo = infoProperties.GetType().GetProperty(propertyMetadata.Name).GetValue(infoProperties) as Sungero.Domain.Shared.EnumPropertyInfo;
        
        newValue = propertyInfo.GetLocalizedValue(newValue as Sungero.Core.Enumeration?);
        originalValue = propertyInfo.GetLocalizedValue(originalValue as Sungero.Core.Enumeration?);
      }
      
      var formatedNewValue = string.Format(" Новое значение: {0}.", newValue == null || string.IsNullOrEmpty(newValue.ToString()) ? "Пусто" : newValue);
      var formatedOldValue = string.Format(" Прежнее значение: {0}.", originalValue == null || string.IsNullOrEmpty(originalValue.ToString()) ? "Пусто" : originalValue);
      return string.Format("\"{0}\".{1}{2}", propertyMetadata.GetLocalizedName(), formatedNewValue, formatedOldValue);
    }
    
    /// <summary>
    /// Получить список измененных свойств объекта
    /// </summary>
    /// <param name="entity">Сущность</param>
    /// <returns>Список измененных свойств</returns>
    [Public]
    public static List<string> ChangeRequisites(Sungero.Domain.Shared.IEntity entity)
    {
      var changeList = new List<string>();
      
      //Получаем "Тип" объекта
      var objType = entity.GetType().GetFinalType();
      
      //Получаем "Метаданные" объекта
      var objMetadata = objType.GetEntityMetadata();      
      
      // Список отслеживаемых свойств - все с вкладки Протокол.
      var requsitesList = new List<string>
      {
          "Speaker",
          "ListenedRUMinutes",
          "ListenedTJMinutes",
          "ListenedENMinutes",
          "DecidedMinutes"
      };
      
      //Получаем свойства объекта
      var properties = objMetadata.Properties.Where(p => requsitesList.Contains(p.Name));      
      foreach (var propertyMetadata in properties)
      {
        //Если текущее свойство это коллекция, то обработает ее отдельно
        if (propertyMetadata.PropertyType == Sungero.Metadata.PropertyType.Collection)
        {
          //Получим значение коллекции
          var collectionValue = (Sungero.Domain.Shared.IChildEntityCollection<Sungero.Domain.Shared.IChildEntity>)propertyMetadata.GetValue(entity);
          
          //Переберем строки коллекции
          foreach (Sungero.Domain.Shared.IChildEntity line in collectionValue)
          {
            System.Type lineType = line.GetType();
            var lineMetadata = lineType.GetEntityMetadata();            
            
            var collectionRequsitesList = new List<string>
            {
              "Number",
              "DecisionRU",
              "DecisionTJ",
              "DecisionEN",
              "Responsible",
              "Date"
            };
            
            //Получаем свойства строки коллекции
            var lineProperties = lineMetadata.Properties.Where(p => collectionRequsitesList.Contains(p.Name));
            foreach (var linePropertyMetadata in lineProperties)
            {
              //Если значение свойства это RootEntity, то пропустим обработку этого свойства
              if (Equals(entity, linePropertyMetadata.GetValue(line)))
                continue;
              
              var checkResult = CheckRequisite(line, linePropertyMetadata);
              if (!string.IsNullOrEmpty(checkResult))
                changeList.Add(string.Format("Коллекция {0}. {1}", propertyMetadata.GetLocalizedName(), checkResult));
              }
          }
        }
        else
        {
          var checkResult = CheckRequisite(entity, propertyMetadata);
          if (!string.IsNullOrEmpty(checkResult))
            changeList.Add(checkResult);
        }
      }
      
      return changeList;
    }
    
    /// <summary>
    /// Сформировать тело протокола по документу-шаблону
    /// </summary>        
    /// <param name="document">Протокол совещания / Выдержка из протокола совещания</param>
    /// <param name="isExtract">true - ормирование выдержки из протокола</param>
    /// <returns>Текст ошибки или пустая строка</returns>
    [Public, Remote]
    public void CreateMinutesBody(Sungero.Docflow.IOfficialDocument document, bool isExtract)
    {
      #region Предпроверки
            
      var templateDoc = Sungero.Docflow.DocumentTemplates.GetAll().Where(d => d.DocumentKinds.Any(k => Equals(k.DocumentKind, document.DocumentKind))).FirstOrDefault();      
      
      /*
      // Наличие "Шаблон протокола заседания КОУ" или "Шаблон выписки из протокола заседания КОУ (RU)"      
      if (!isExtract)
        templateDoc = Sungero.Content.ElectronicDocuments.GetAll().Where(d => Sungero.Docflow.DocumentTemplates.Is(d) && d.Name == Constants.Module.MinutesTemplateName).FirstOrDefault();
      else
        templateDoc = Sungero.Content.ElectronicDocuments.GetAll().Where(d => Sungero.Docflow.DocumentTemplates.Is(d) && d.Name == Constants.Module.ExtractTemplateName).FirstOrDefault();
      */

      if (document.DocumentKind == null)
        throw AppliedCodeException.Create(Resources.DocumentKindIsEmpty);     

      if (templateDoc == null)
        throw AppliedCodeException.Create(Resources.MinutesTemplateNotFoundFormat(document.DocumentKind.Name));            
      
      var meeting = litiko.Eskhata.Meetings.Null;
      if (litiko.Eskhata.Minuteses.Is(document))
        meeting = litiko.Eskhata.Meetings.As(litiko.Eskhata.Minuteses.As(document).Meeting);

      if (litiko.Eskhata.Addendums.Is(document) && document.LeadingDocument != null)
        meeting = litiko.CollegiateAgencies.Projectsolutions.As(document.LeadingDocument).Meeting;
      
      if (meeting == null)
        throw AppliedCodeException.Create(Resources.MeetingIsEmpty);
      
      var meetingCategory = meeting.MeetingCategorylitiko;
      if (meetingCategory == null)
        throw AppliedCodeException.Create(Resources.MeetingCategoryIsEmpty);

      if (document.DocumentKind == null)
        throw AppliedCodeException.Create(Resources.DocumentKindIsEmpty);
      
      #endregion
      
      #region Формирование данных для шаблона            
      Dictionary<string, string> replacebleFields = new Dictionary<string, string>();
      var meetingResolutions = new List<litiko.CollegiateAgencies.Structures.Module.IMeetingResolutionInfo>();
      
      string nameForTemplate = !string.IsNullOrEmpty(meetingCategory.NameForTemplate) ? meetingCategory.NameForTemplate : string.Empty;      
      replacebleFields.Add("<CategoryNameForTemplateUpper>", nameForTemplate.ToUpper());
      replacebleFields.Add("<CategoryNameForTemplate>", nameForTemplate);
      string nameForTemplateTJ = !string.IsNullOrEmpty(meetingCategory.NameForTemplateTJ) ? meetingCategory.NameForTemplateTJ : string.Empty;      
      replacebleFields.Add("<CategoryNameForTemplateTJUpper>", nameForTemplateTJ.ToUpper());
      replacebleFields.Add("<CategoryNameForTemplateTJ>", nameForTemplateTJ);
      
      if (meeting.MeetingTypelitiko.HasValue)
      {        
        replacebleFields.Add("<Type>", meeting.MeetingTypelitiko == litiko.Eskhata.Meeting.MeetingTypelitiko.Regular ? "ОЧЕРЕДНОГО" : "ВНЕОЧЕРЕДНОГО");
        replacebleFields.Add("<Type2>", meeting.Info.Properties.MeetingTypelitiko.GetLocalizedValue(meeting.MeetingTypelitiko).ToLower());
        replacebleFields.Add("<Type3>", meeting.Info.Properties.MeetingTypelitiko.GetLocalizedValue(meeting.MeetingTypelitiko));
        replacebleFields.Add("<TypeTJ>", meeting.MeetingTypelitiko == litiko.Eskhata.Meeting.MeetingTypelitiko.Regular ? "НАВБАТЙ" : "ГАЙРИНАВБАТЙ");
        replacebleFields.Add("<TypeTJ2>", meeting.MeetingTypelitiko == litiko.Eskhata.Meeting.MeetingTypelitiko.Regular ? "НАВБАТЙ".ToLower() : "ГАЙРИНАВБАТЙ".ToLower());
        replacebleFields.Add("<TypeTJ3>", meeting.MeetingTypelitiko == litiko.Eskhata.Meeting.MeetingTypelitiko.Regular ? "Навбатй" : "Гайринавбатй");
      }            
      if (meeting.Formalitiko.HasValue)
      {
        replacebleFields.Add("<Forma>", meeting.Formalitiko.HasValue ? meeting.Info.Properties.Formalitiko.GetLocalizedValue(meeting.Formalitiko) : string.Empty);  
        replacebleFields.Add("<FormaTJ>", meeting.Formalitiko == litiko.Eskhata.Meeting.Formalitiko.Intramural ? "Шахсан" : "Мукотиба");       
      }            
      
      replacebleFields.Add("<Method>", meeting.MeetingMethodlitiko != null ? meeting.MeetingMethodlitiko.Name : string.Empty);
      replacebleFields.Add("<MethodTJ>", meeting.MeetingMethodlitiko != null ? !string.IsNullOrEmpty(meeting.MeetingMethodlitiko.NameTJ) ? meeting.MeetingMethodlitiko.NameTJ : string.Empty : string.Empty);
      
      replacebleFields.Add("<Location>", !string.IsNullOrEmpty(meeting.Location) ? meeting.Location : string.Empty);            
      replacebleFields.Add("<PresidentJobTittle>", meeting.President != null && meeting.President.JobTitle != null ? meeting.President.JobTitle.Name : string.Empty);            
      string presidentJobTittleTJ = meeting.President != null && meeting.President.JobTitle != null ? litiko.Eskhata.JobTitles.As(meeting.President.JobTitle).NameTGlitiko : string.Empty;      
      replacebleFields.Add("<PresidentJobTittleTJ>", !string.IsNullOrEmpty(presidentJobTittleTJ) ? presidentJobTittleTJ : string.Empty);
      replacebleFields.Add("<PresidentFIO>", meeting.President != null ? Sungero.Company.PublicFunctions.Employee.GetShortName(meeting.President, true) : string.Empty);      
      replacebleFields.Add("<PresidentFIOlong>", meeting.President != null ? meeting.President.Name : string.Empty);      
      string presidentFIOTJ = meeting.President != null ? litiko.Eskhata.PublicFunctions.Person.GetShortNameTJ(litiko.Eskhata.People.As(meeting.President.Person)) : string.Empty;
      replacebleFields.Add("<PresidentFIOTJ>", !string.IsNullOrEmpty(presidentFIOTJ) ? presidentFIOTJ : string.Empty);
      string presidentFIOTJlong = meeting.President != null ? litiko.Eskhata.PublicFunctions.Person.GetNameTJ(litiko.Eskhata.People.As(meeting.President.Person)) : string.Empty;
      replacebleFields.Add("<PresidentFIOTJlong>", !string.IsNullOrEmpty(presidentFIOTJlong) ? presidentFIOTJlong : string.Empty);
      
      replacebleFields.Add("<SecretaryFIO>", meeting.Secretary!= null ? Sungero.Company.PublicFunctions.Employee.GetShortName(meeting.Secretary, true) : string.Empty);
      replacebleFields.Add("<SecretaryFIOlong>", meeting.Secretary!= null ? meeting.Secretary.Name : string.Empty);
      string secretaryFIOTJ = meeting.Secretary!= null ? litiko.Eskhata.PublicFunctions.Person.GetShortNameTJ(litiko.Eskhata.People.As(meeting.Secretary.Person)) : string.Empty;
      replacebleFields.Add("<SecretaryFIOTJ>", !string.IsNullOrEmpty(secretaryFIOTJ) ? secretaryFIOTJ : string.Empty);
      string secretaryFIOTJlong = meeting.Secretary!= null ? litiko.Eskhata.PublicFunctions.Person.GetNameTJ(litiko.Eskhata.People.As(meeting.Secretary.Person)) : string.Empty;
      replacebleFields.Add("<SecretaryFIOTJlong>", !string.IsNullOrEmpty(secretaryFIOTJlong) ? secretaryFIOTJlong : string.Empty);
      
      //replacebleFields.Add("<PresentFIOList>", litiko.Eskhata.PublicFunctions.Meeting.GetMeetingPresentNumberedList(meeting, false, false, false));       
      List<string> presentFIOList = meeting.Presentlitiko
        .Where(x => x?.Employee != null)
        .OrderBy(x => x.Employee.Name)
        .Select(x => x.Employee.Name)
        .ToList();
      
      //replacebleFields.Add("<PresentFIOListTJ>", litiko.Eskhata.PublicFunctions.Meeting.GetMeetingPresentNumberedList(meeting, false, true, false));
      List<string> presentFIOListTJ = meeting.Presentlitiko
        .Where(x => x?.Employee != null)
        .Select(x => litiko.Eskhata.PublicFunctions.Person.GetNameTJ(litiko.Eskhata.People.As(x.Employee.Person)))
        .OrderBy(name => name)
        .ToList();                 
      
      //replacebleFields.Add("<AbsentFIOList>", litiko.Eskhata.PublicFunctions.Meeting.GetMeetingAbsentNumberedList(meeting, false, true, false, false));
      List<string> absentFIOList = meeting.Absentlitiko
        .Where(x => x?.Employee != null)
        .OrderBy(x => x.Employee.Name)
        .Select(x => $"{x.Employee.Name} ({x.AbsentReason?.Name})")
        .ToList();
      
      //replacebleFields.Add("<AbsentFIOListTJ>", litiko.Eskhata.PublicFunctions.Meeting.GetMeetingAbsentNumberedList(meeting, false, true, true, false));
      List<string> absentFIOListTJ = meeting.Absentlitiko
        .Where(x => x?.Employee != null)
        .Select(x => $"{litiko.Eskhata.PublicFunctions.Person.GetNameTJ(litiko.Eskhata.People.As(x.Employee.Person))} ({x.AbsentReason?.Name})")
        .OrderBy(name => name)
        .ToList();
      
      //replacebleFields.Add("<InvitedFIOList>", litiko.Eskhata.PublicFunctions.Meeting.GetMeetingInvitedNumberedList(meeting, false, false, true));                  
      var invitedFIOList = meeting.InvitedEmployeeslitiko
        .Where(x => x?.Employee != null)
        .Select(x =>
        {
          var employee = Sungero.Company.Employees.As(x.Employee);
          var shortName = employee != null
              ? Sungero.Company.PublicFunctions.Employee.GetShortName(employee, true)
              : string.Empty;
          var jobTitle = x.Employee.JobTitle?.Name ?? string.Empty;
            
          return $"{shortName} - {jobTitle}";
        })
      .OrderBy(name => name)
      .ToList();     
      
      //replacebleFields.Add("<InvitedFIOListTJ>", litiko.Eskhata.PublicFunctions.Meeting.GetMeetingInvitedNumberedList(meeting, false, true, true));      
      var invitedFIOListTJ = meeting.InvitedEmployeeslitiko
        .Where(x => x?.Employee != null)
        .Select(x =>
        {
          var employee = litiko.Eskhata.People.As(x.Employee.Person);
          var shortName = employee != null
              ? litiko.Eskhata.PublicFunctions.Person.GetNameTJ(employee)
              : string.Empty;
          var jobTitle = litiko.Eskhata.JobTitles.As(x.Employee.JobTitle)?.NameTGlitiko ?? string.Empty;
  
          return $"{shortName} - {jobTitle}";
        })
      .OrderBy(name => name)
      .ToList();
      
      replacebleFields.Add("<TextForMinutesRU>", !string.IsNullOrEmpty(meetingCategory.TextForTemplate) ? meetingCategory.TextForTemplate : string.Empty);
      replacebleFields.Add("<TextForMinutesTJ>", !string.IsNullOrEmpty(meetingCategory.TextForTemplateTJ) ? meetingCategory.TextForTemplateTJ : string.Empty);
      replacebleFields.Add("<Quorum>", meeting.Quorumlitiko.HasValue ? meeting.Info.Properties.Quorumlitiko.GetLocalizedValue(meeting.Quorumlitiko).ToLower() : string.Empty);
      List<string> agendaList = new List<string>();
      List<string> agendaListTJ = new List<string>();
      if (!isExtract)
      {
        replacebleFields.Add("<DocDate>", litiko.Eskhata.Minuteses.As(document).Meeting.DateTime.HasValue ? litiko.Eskhata.Minuteses.As(document).Meeting.DateTime.Value.ToString("dd.MM.yyyy") : string.Empty);
        replacebleFields.Add("<DocNumber>", !string.IsNullOrEmpty(document.RegistrationNumber) ? document.RegistrationNumber : string.Empty);                  
        //replacebleFields.Add("<AgendaList>", litiko.Eskhata.PublicFunctions.Meeting.GetMeetingProjectSolutionsNumberedList(meeting));
        // Все решения по совещанию
        foreach (var element in meeting.ProjectSolutionslitiko.Where(x => x.ProjectSolution != null).OrderBy(x => x.Number))
        {
          var projectSolution = element.ProjectSolution;
          
          agendaList.Add($"Рассмотрение вопроса: {projectSolution.Subject}");
          agendaListTJ.Add($"Баррасии масъала: {projectSolution.Subject}");
          
          var meetingResolutionInfo = new Structures.Module.MeetingResolutionInfo();
          meetingResolutionInfo.ProjectSolutionTittle = string.Format("Рассмотрение вопроса: {1}", element.Number, projectSolution.Subject);
          meetingResolutionInfo.ProjectSolutionTittleTJ = string.Format("Баррасии: {1}", element.Number, projectSolution.Subject);
          
          string speaker = string.Empty;
          if (projectSolution.Speaker != null)
          {
            var fio = Sungero.Company.PublicFunctions.Employee.GetShortName(projectSolution.Speaker, DeclensionCase.Accusative, true);
            var title = Sungero.Company.PublicFunctions.Employee.GetJobTitle(projectSolution.Speaker, DeclensionCase.Accusative);
            speaker = string.Format("{0} - {1}", fio, title);                        
          }          
          meetingResolutionInfo.SpeakerRU = speaker;
          meetingResolutionInfo.SpeakerTJ = projectSolution.Speaker != null ? litiko.Eskhata.PublicFunctions.Person.GetShortNameTJ(litiko.Eskhata.People.As(projectSolution.Speaker.Person)) : string.Empty;
          
          meetingResolutionInfo.ListenedRU = !string.IsNullOrEmpty(projectSolution.ListenedRUMinutes) ? projectSolution.ListenedRUMinutes : string.Empty;
          meetingResolutionInfo.ListenedTJ = !string.IsNullOrEmpty(projectSolution.ListenedTJMinutes) ? projectSolution.ListenedTJMinutes : string.Empty;
/*                    
          meetingResolutionInfo.Decigions = string.Join(
            Environment.NewLine,
            projectSolution.DecidedMinutes
              .OrderBy(decided => decided.Number)
              .Select(decided => $"{element.Number}.{decided.Number}. {decided.DecisionRU}")
          );
*/
          meetingResolutionInfo.Decigions = string.Join(
            "##DECISION##",
            projectSolution.DecidedMinutes
              .OrderBy(decided => decided.Number)
              .Select(decided => decided.DecisionRU)
          );
/*          
          meetingResolutionInfo.DecigionsTJ = string.Join(
            Environment.NewLine, 
            projectSolution.DecidedMinutes
              .OrderBy(decided => decided.Number)
              .Select(decided => $"{element.Number}.{decided.Number}. {decided.DecisionTJ}"
            ));          
*/
          meetingResolutionInfo.DecigionsTJ = string.Join(
            "##DECISION##", 
            projectSolution.DecidedMinutes
              .OrderBy(decided => decided.Number)
              .Select(decided => decided.DecisionTJ
            )); 

          meetingResolutionInfo.WithVoting = element.VotingType.HasValue && element.VotingType != litiko.Eskhata.MeetingProjectSolutionslitiko.VotingType.NoVoting;
          meetingResolutionInfo.VoutingYes = element.Yes.HasValue ? element.Yes.Value : 0;
          meetingResolutionInfo.VoutingNo = element.No.HasValue ? element.No.Value : 0;
          meetingResolutionInfo.VoutingAbstained = element.Abstained.HasValue ? element.Abstained.Value : 0;
          meetingResolutionInfo.VoutingAccepted = element.Accepted.HasValue ? element.Accepted.Value : false;
                  
          meetingResolutions.Add(meetingResolutionInfo);
        }
        
      }
      else
      {                
        var projectSolution = litiko.CollegiateAgencies.Projectsolutions.As(document.LeadingDocument);
        var meetingProjectSolutionNumber = projectSolution?.Meeting.ProjectSolutionslitiko.Where(ps => Equals(ps.ProjectSolution, projectSolution))
          .Select(ps => ps.Number)
          .FirstOrDefault();        
        agendaList.Add($"##{meetingProjectSolutionNumber}##Рассмотрение вопроса: {projectSolution.Subject}");
        agendaListTJ.Add($"##{meetingProjectSolutionNumber}##Баррасии масъала: {projectSolution.Subject}");
        
        var regDate = string.Empty;
        var regnumber = string.Empty;
        var minutes = litiko.Eskhata.Minuteses.GetAll().Where(x => Equals(x.Meeting, meeting)).FirstOrDefault();
        if (minutes != null)
        {
          //regDate = minutes.RegistrationDate.HasValue ? minutes.RegistrationDate.Value.ToString("dd.MM.yyyy") : string.Empty;
          regDate = meeting.DateTime.HasValue ? meeting.DateTime.Value.ToString("dd.MM.yyyy") : string.Empty;
          regnumber = !string.IsNullOrEmpty(minutes.RegistrationNumber) ? minutes.RegistrationNumber : string.Empty;
        }
        replacebleFields.Add("<DocDate>", regDate);
        replacebleFields.Add("<DocNumber>", regnumber);        
        
        // Решения только по конкретному Проекту решения                  
        var meetingResolutionInfo = new Structures.Module.MeetingResolutionInfo();        

        meetingResolutionInfo.Number = meetingProjectSolutionNumber;
        //replacebleFields.Add("<AgendaList>", !string.IsNullOrEmpty(projectSolution.Subject) ? string.Format("... {0}. {1}", meetingProjectSolutionNumber, projectSolution.Subject) : string.Empty);
        
        string speaker = string.Empty;
        if (projectSolution.Speaker != null)
        {
          var fio = Sungero.Company.PublicFunctions.Employee.GetShortName(projectSolution.Speaker, DeclensionCase.Accusative, true);
          var title = Sungero.Company.PublicFunctions.Employee.GetJobTitle(projectSolution.Speaker, DeclensionCase.Accusative);
          speaker = string.Format("{0} - {1}", fio, title);                        
        }          
        meetingResolutionInfo.SpeakerRU = speaker;
        meetingResolutionInfo.SpeakerTJ = projectSolution.Speaker != null ? litiko.Eskhata.PublicFunctions.Person.GetShortNameTJ(litiko.Eskhata.People.As(projectSolution.Speaker.Person)) : string.Empty;        
        
        meetingResolutionInfo.ProjectSolutionTittle = string.Format("Рассмотрение вопроса: {0}", projectSolution.Subject);
        meetingResolutionInfo.ListenedRU = !string.IsNullOrEmpty(projectSolution.ListenedRUMinutes) ? projectSolution.ListenedRUMinutes : string.Empty;
        
        //string originalResult = litiko.CollegiateAgencies.PublicFunctions.Projectsolution.GetProjectSolutionDecidedMinutesRU(projectSolution);
        //meetingResolutionInfo.Decigions = string.Join("\n", originalResult.Split(new[] { '\n' }, StringSplitOptions.None)
        //                                              .Select((line, index) => $"{meetingProjectSolutionNumber}.{line}"));
        
        meetingResolutionInfo.Decigions = string.Join(
            "##DECISION##",
            projectSolution.DecidedMinutes
              .OrderBy(decided => decided.Number)
              .Select(decided => decided.DecisionRU)
          );
        
        var votingrecord = meeting.ProjectSolutionslitiko.Where(x => Equals(x.ProjectSolution, projectSolution)).FirstOrDefault();
        if (votingrecord != null)
        {
          meetingResolutionInfo.WithVoting = votingrecord.VotingType.HasValue && votingrecord.VotingType != litiko.Eskhata.MeetingProjectSolutionslitiko.VotingType.NoVoting;
          meetingResolutionInfo.VoutingYes = votingrecord.Yes.HasValue ? votingrecord.Yes.Value : 0;
          meetingResolutionInfo.VoutingNo = votingrecord.No.HasValue ? votingrecord.No.Value : 0;
          meetingResolutionInfo.VoutingAbstained = votingrecord.Abstained.HasValue ? votingrecord.Abstained.Value : 0;                    
        }
        else
        {
          meetingResolutionInfo.WithVoting = false;
          meetingResolutionInfo.VoutingYes = 0;
          meetingResolutionInfo.VoutingNo = 0;
          meetingResolutionInfo.VoutingAbstained = 0;
        }
                  
        meetingResolutionInfo.VoutingAccepted = meetingResolutionInfo.VoutingYes > meetingResolutionInfo.VoutingNo ? true : false;
        meetingResolutions.Add(meetingResolutionInfo);
      }

      // Vals 20250915
      if (meeting.MeetingMethodlitiko?.Name == "Электронное голосование")
      {
        replacebleFields.Add("<MeetingOpeningGreeting_RU>", "");
        replacebleFields.Add("<MeetingOpeningGreeting_TJ>", "");
      }
      else
      {
        var replacePresidentFIOlong = replacebleFields.ContainsKey("<PresidentFIOlong>") ? replacebleFields["<PresidentFIOlong>"] : "";
        var replaceCategoryNameForTemplate = replacebleFields.ContainsKey("<CategoryNameForTemplate>") ? replacebleFields["<CategoryNameForTemplate>"] : "";
        var replaceType2 = replacebleFields.ContainsKey("<Type2>") ? replacebleFields["<Type2>"] : "";
        
        string meetingOpeningGreeting_RU = litiko.CollegiateAgencies.Resources.MeetingOpeningGreeting_RU;
        meetingOpeningGreeting_RU = meetingOpeningGreeting_RU
          .Replace("<PresidentFIOlong>", replacePresidentFIOlong)
          .Replace("<CategoryNameForTemplate>", replaceCategoryNameForTemplate)
          .Replace("<Type2>", replaceType2);
        
        replacebleFields.Add("<MeetingOpeningGreeting_RU>", meetingOpeningGreeting_RU); 
        
        var replacePresidentFIOlong_TJ = replacebleFields.ContainsKey("<PresidentFIOTJ>") ? replacebleFields["<PresidentFIOTJ>"] : "";
        var replaceCategoryNameForTemplate_TJ = replacebleFields.ContainsKey("<CategoryNameForTemplateTJ>") ? replacebleFields["<CategoryNameForTemplateTJ>"] : "";
        var replaceType2_TJ = replacebleFields.ContainsKey("<TypeTJ2>") ? replacebleFields["<TypeTJ2>"] : "";

        string meetingOpeningGreeting_TJ = litiko.CollegiateAgencies.Resources.MeetingOpeningGreeting_TJ;
        meetingOpeningGreeting_TJ = meetingOpeningGreeting_TJ
          .Replace("<PresidentFIOTJ>", replacePresidentFIOlong_TJ)
          .Replace("<CategoryNameForTemplateTJ>", replaceCategoryNameForTemplate_TJ)
          .Replace("<TypeTJ2>", replaceType2_TJ);
        
        replacebleFields.Add("<MeetingOpeningGreeting_TJ>", meetingOpeningGreeting_TJ);
      }
         
      
      #endregion
      
      #region Формирование тела по шаблону
            
      var resultStream = litiko.CollegiateAgencies.IsolatedFunctions.DocumentBodyCreator.FillMinutesBodyByTemplate(templateDoc.LastVersion.Body.Read(),
                                                                                                                   replacebleFields,
                                                                                                                   presentFIOList,
                                                                                                                   presentFIOListTJ,
                                                                                                                   absentFIOList,
                                                                                                                   absentFIOListTJ,
                                                                                                                   invitedFIOList,
                                                                                                                   invitedFIOListTJ,
                                                                                                                   agendaList,
                                                                                                                   agendaListTJ,
                                                                                                                   meetingResolutions);
      
      // Выключить error-логирование при доступе к зашифрованным бинарным данным/версии.
      AccessRights.SuppressSecurityEvents(
        () =>
        {
          document.CreateVersionFrom(resultStream, "docx");                                   
        });      
      
      resultStream.Close();
      document.Save();      
      Sungero.Docflow.PublicFunctions.OfficialDocument.PreparePreview(document);
      
      #endregion
    }
    
    /// <summary>
    /// Есть ли активные задачи с этапом голосование по документам.
    /// </summary>
    /// <param name="documentIds">Список ИД документов</param>
    [Public, Remote(IsPure = true)]
    public bool AnyVoitingTasks(List<long> documentIds)
    {      
      var documentsAddendaGroupGuid = litiko.CollegiateAgencies.PublicConstants.Module.ApprovalTaskAddendaGroupGuid;
      var votingStage = litiko.Eskhata.ApprovalStage.CustomStageTypelitiko.Voting;
      var hasActiveTaskWithVotingStage = false;
      
      // Получить данные без учета прав доступа.
      AccessRights.AllowRead(
      () =>
      {
        var activeTasks = litiko.Eskhata.ApprovalTasks.GetAll()
          .Where(t => t.Status == Sungero.Docflow.ApprovalTask.Status.InProcess)
          .Where(t => t.AttachmentDetails.Any(g => g.GroupId == documentsAddendaGroupGuid && documentIds.Contains(g.AttachmentId.GetValueOrDefault())));
              
        foreach (var task in activeTasks)
        {
          if (litiko.Eskhata.PublicFunctions.ApprovalTask.HasCustomStage(task, votingStage))
          {
            hasActiveTaskWithVotingStage = true;
            break;
          }
        }      
      });
    
      return hasActiveTaskWithVotingStage;           
    }

    
  }
}