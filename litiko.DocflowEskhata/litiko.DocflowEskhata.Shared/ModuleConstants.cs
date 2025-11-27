using System;
using Sungero.Core;

namespace litiko.DocflowEskhata.Constants
{
  public static class Module
  {

    /// <summary>
    /// 
    /// </summary>
    public const string ApiKey = "ApiKes";
    public const string ApiKey2 = "ApiKey";
    
    
    public static class DocumentTypeGuids
    {
      [Sungero.Core.Public]
      public static readonly Guid IncomingLetter = Guid.Parse("8dd00491-8fd0-4a7a-9cf3-8b6dc2e6455d");
      
      [Sungero.Core.Public]
      public static readonly Guid OutgoingLetter = Guid.Parse("d1d2a452-7732-4ba8-b199-0a4dc78898ac");
      
      [Sungero.Core.Public]
      public static readonly Guid Addendum = Guid.Parse("58b9ed35-9c84-46cd-aa79-9b5ef5a82f5d");
    }
    
    public static class DocumentKindGuids
    {
      /// <summary> Входящая корреспонденция </summary>
      [Sungero.Core.Public]
      public static readonly Guid IncomingCorrespondence = Guid.Parse("75f32cd3-feb2-43a4-bc7e-f30da69b3faf");
      
      /// <summary> Входящая корреспонденция от исполнительных органов </summary>
      [Sungero.Core.Public]
      public static readonly Guid IncomingCorrespondenceExecutive = Guid.Parse("e5132953-354c-42c6-ab11-ec08a7fb9925");
      
      /// <summary> Входящая корреспонденция от налогового комитета </summary>
      [Sungero.Core.Public]
      public static readonly Guid IncomingCorrespondenceTax = Guid.Parse("5f2e8635-3a32-4475-9318-662073e94af9");
      
      /// <summary> Входящая корреспонденция от НБТ </summary>
      [Sungero.Core.Public]
      public static readonly Guid IncomingCorrespondenceNBT = Guid.Parse("314610fe-29b5-4603-8fb1-b8e1eda4fa1a");
      
      /// <summary> Входящая корреспонденция от НБТ </summary>
      [Sungero.Core.Public]
      public static readonly Guid IncomingCorrespondenceBranches = Guid.Parse("3603821a-143f-4aa1-ab49-4876ace03ee6");
      
      /// <summary> Входящие письма/ обращения граждан </summary>
      [Sungero.Core.Public]
      public static readonly Guid IncomingLettersCitizens = Guid.Parse("8a5f87ec-2fc4-4d85-83c9-77d8eaf9ac6e");
      
      /// <summary> Входящие письма/ обращения организаций </summary>
      [Sungero.Core.Public]
      public static readonly Guid IncomingLettersOrganisations = Guid.Parse("a70557c1-f6da-474d-890d-dbc0e1e57064");
      
      /// <summary> Входящая корреспонденция адресованная в ГО </summary>
      [Sungero.Core.Public]
      public static readonly Guid IncomingCorrespondenceHeadOffice = Guid.Parse("b4fc7d0f-468f-4341-bc8f-2f8b9e279f30");
      
      /// <summary> Исходящая корреспонденция </summary>
      [Sungero.Core.Public]
      public static readonly Guid OutgoingCorrespondence = Guid.Parse("3613e9ac-8ffa-4d8b-9dbc-cc8a7f7ed1de");
      
      /// <summary> Исходящая корреспонденция по доверенности </summary>
      [Sungero.Core.Public]
      public static readonly Guid OutgoingPoACorrespondence = Guid.Parse("4a530f9c-593b-4b7e-a8af-2ef8b6d33029");
      
      /// <summary> Исходящая корреспонденция в НБТ </summary>
      [Sungero.Core.Public]
      public static readonly Guid OutgoingCorrespondenceNBT = Guid.Parse("fe13dbab-cdca-49df-b39d-b0ed5fab0465");
      
      /// <summary> Регистрационно-контрольный лист </summary>
      [Sungero.Core.Public]
      public static readonly Guid Checklist = Guid.Parse("125a38e1-a9f0-46b9-9103-ed7f9912343d");
    }
    [Sungero.Core.Public]
    public const string ConvertingVersionId = "convertingVersionId";
  }
}