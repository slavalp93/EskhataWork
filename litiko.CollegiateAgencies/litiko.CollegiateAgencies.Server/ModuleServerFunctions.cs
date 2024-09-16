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
      
      // Наличие "Шаблон протокола заседания КОУ"
      var templateDoc = Sungero.Content.ElectronicDocuments.Null;
      if (!isExtract)      
        templateDoc = Sungero.Content.ElectronicDocuments.GetAll().Where(d => Sungero.Docflow.DocumentTemplates.Is(d) && d.Name == Constants.Module.MinutesTemplateName).FirstOrDefault();
      else
        templateDoc = Sungero.Content.ElectronicDocuments.GetAll().Where(d => Sungero.Docflow.DocumentTemplates.Is(d) && d.Name == Constants.Module.ExtractTemplateName).FirstOrDefault();

      if (templateDoc == null)
        throw AppliedCodeException.Create(Resources.MinutesTemplateNotFoundFormat(isExtract == true ? Constants.Module.ExtractTemplateName : Constants.Module.MinutesTemplateName));
            
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
      
      #endregion
      
      #region Формирование данных для шаблона            
      Dictionary<string, string> replacebleFields = new Dictionary<string, string>();
      var meetingResolutions = new List<litiko.CollegiateAgencies.Structures.Module.IMeetingResolutionInfo>();
      
      string nameForTemplate = !string.IsNullOrEmpty(meetingCategory.NameForTemplate) ? meetingCategory.NameForTemplate : string.Empty;      
      replacebleFields.Add("<CategoryNameForTemplateUpper>", nameForTemplate.ToUpper());
      replacebleFields.Add("<CategoryNameForTemplate>", nameForTemplate);
      if (meeting.MeetingTypelitiko.HasValue)
      {        
        replacebleFields.Add("<Type>", meeting.MeetingTypelitiko == litiko.Eskhata.Meeting.MeetingTypelitiko.Regular ? "ОЧЕРЕДНОГО" : "ВНЕОЧЕРЕДНОГО");
        replacebleFields.Add("<Type2>", meeting.Info.Properties.MeetingTypelitiko.GetLocalizedValue(meeting.MeetingTypelitiko).ToLower());
      }            
      replacebleFields.Add("<Forma>", meeting.Formalitiko.HasValue ? meeting.Info.Properties.Formalitiko.GetLocalizedValue(meeting.Formalitiko) : string.Empty);            
      replacebleFields.Add("<Method>", meeting.MeetingMethodlitiko != null ? meeting.MeetingMethodlitiko.Name : string.Empty);            
      replacebleFields.Add("<Location>", !string.IsNullOrEmpty(meeting.Location) ? meeting.Location : string.Empty);            
      replacebleFields.Add("<PresidentJobTittle>", meeting.President != null ? meeting.President.JobTitle?.Name : string.Empty);            
      replacebleFields.Add("<PresidentFIO>", meeting.President != null ? Sungero.Company.PublicFunctions.Employee.GetShortName(meeting.President, true) : string.Empty);            
      replacebleFields.Add("<SecretaryFIO>", meeting.Secretary!= null ? Sungero.Company.PublicFunctions.Employee.GetShortName(meeting.Secretary, true) : string.Empty);            
      replacebleFields.Add("<PresentFIOList>", litiko.Eskhata.PublicFunctions.Meeting.GetMeetingPresentNumberedList(meeting, false));            
      replacebleFields.Add("<AbsentFIOList>", litiko.Eskhata.PublicFunctions.Meeting.GetMeetingAbsentNumberedList(meeting, false, true));            
      replacebleFields.Add("<InvitedFIOList>", litiko.Eskhata.PublicFunctions.Meeting.GetMeetingInvitedNumberedList(meeting, false));                  
      replacebleFields.Add("<TextForMinutesRU>", !string.IsNullOrEmpty(meetingCategory.TextForTemplate) ? meetingCategory.TextForTemplate : string.Empty);            
      replacebleFields.Add("<Quorum>", meeting.Quorumlitiko.HasValue ? meeting.Info.Properties.Quorumlitiko.GetLocalizedValue(meeting.Quorumlitiko).ToLower() : string.Empty);
      
      if (!isExtract)
      {
        replacebleFields.Add("<DocDate>", document.RegistrationDate.HasValue ? document.RegistrationDate.Value.ToString("dd.MM.yyyy") : string.Empty);            
        replacebleFields.Add("<DocNumber>", !string.IsNullOrEmpty(document.RegistrationNumber) ? document.RegistrationNumber : string.Empty);                  
        replacebleFields.Add("<AgendaList>", litiko.Eskhata.PublicFunctions.Meeting.GetMeetingProjectSolutionsNumberedList(meeting));
        // Все решения по совещанию
        foreach (var element in meeting.ProjectSolutionslitiko.Where(x => x.ProjectSolution != null))
        {
          var projectSolution = element.ProjectSolution;
          
          var meetingResolutionInfo = new Structures.Module.MeetingResolutionInfo();
          meetingResolutionInfo.ProjectSolutionTittle = string.Format("{0}. Рассмотрение {1}", element.Number, projectSolution.Subject);
          meetingResolutionInfo.ListenedRU = !string.IsNullOrEmpty(projectSolution.ListenedRUMinutes) ? projectSolution.ListenedRUMinutes : string.Empty;
          meetingResolutionInfo.Decigions = litiko.CollegiateAgencies.PublicFunctions.Projectsolution.GetProjectSolutionDecidedMinutesRU(projectSolution);
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
        var regDate = string.Empty;
        var regnumber = string.Empty;
        var minutes = litiko.Eskhata.Minuteses.GetAll().Where(x => Equals(x.Meeting, meeting)).FirstOrDefault();
        if (minutes != null)
        {
          regDate = minutes.RegistrationDate.HasValue ? minutes.RegistrationDate.Value.ToString("dd.MM.yyyy") : string.Empty;
          regnumber = !string.IsNullOrEmpty(minutes.RegistrationNumber) ? minutes.RegistrationNumber : string.Empty;
        }
        replacebleFields.Add("<DocDate>", regDate);
        replacebleFields.Add("<DocNumber>", regnumber);
        replacebleFields.Add("<AgendaList>", !string.IsNullOrEmpty(projectSolution.Subject) ? projectSolution.Subject : string.Empty);
        
        // Решения только по конкретному Проекту решения                  
        var meetingResolutionInfo = new Structures.Module.MeetingResolutionInfo();
        meetingResolutionInfo.ProjectSolutionTittle = string.Format("1. Рассмотрение {0}", projectSolution.Subject);
        meetingResolutionInfo.ListenedRU = !string.IsNullOrEmpty(projectSolution.ListenedRUMinutes) ? projectSolution.ListenedRUMinutes : string.Empty;
        meetingResolutionInfo.Decigions = litiko.CollegiateAgencies.PublicFunctions.Projectsolution.GetProjectSolutionDecidedMinutesRU(projectSolution);
        meetingResolutionInfo.VoutingYes = projectSolution.Voting.Count(x => x.Yes.Value);
        meetingResolutionInfo.VoutingNo = projectSolution.Voting.Count(x => x.No.Value);
        meetingResolutionInfo.VoutingAbstained = projectSolution.Voting.Count(x => x.Abstained.Value);
        meetingResolutionInfo.VoutingAccepted = meetingResolutionInfo.VoutingYes > meetingResolutionInfo.VoutingNo ? true : false;
                  
        meetingResolutions.Add(meetingResolutionInfo);                
      }

      #endregion
      
      #region Формирование тела по шаблону
      
      var resultStream = litiko.CollegiateAgencies.IsolatedFunctions.DocumentBodyCreator.FillMinutesBodyByTemplate(templateDoc.LastVersion.Body.Read() , replacebleFields, meetingResolutions);
      
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
  }
}