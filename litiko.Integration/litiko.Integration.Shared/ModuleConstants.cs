using System;
using Sungero.Core;

namespace litiko.Integration.Constants
{
  public static class Module
  {
    /// <summary>
    /// GUID для роли "Ответственные за синхронизацию с учетными системами".
    /// </summary>
    [Public]
    public static readonly Guid SynchronizationResponsibleRoleGuid = Guid.Parse("6F98BA36-3B7F-4767-8369-88A65578DC5A");
    
    /// <summary>
    /// Протоколы (методы) интеграции
    /// </summary>    
    public static class IntegrationMethods
    {
      /// <summary> Подразделения </summary>
      [Sungero.Core.Public]
      public const string R_DR_GET_DEPART = "R_DR_GET_DEPART";

      /// <summary> Сотрудники </summary>
      [Sungero.Core.Public]
      public const string R_DR_GET_EMPLOYEES = "R_DR_GET_EMPLOYEES";

      /// <summary> Наши организации </summary>
      [Sungero.Core.Public]
      public const string R_DR_GET_BUSINESSUNITS = "R_DR_GET_BUSINESSUNITS";      
      
      /// <summary> Организации </summary>
      [Sungero.Core.Public]
      public const string R_DR_GET_COMPANY = "R_DR_GET_COMPANY";

      /// <summary> Банки </summary>
      [Sungero.Core.Public]
      public const string R_DR_GET_BANK = "R_DR_GET_BANK";

      /// <summary> Персоны </summary>
      [Sungero.Core.Public]
      public const string R_DR_GET_PERSON = "R_DR_GET_PERSON";

      /// <summary> Страны </summary>
      [Sungero.Core.Public]
      public const string R_DR_GET_COUNTRIES = "R_DR_GET_COUNTRIES";

      /// <summary> ОКОПФ </summary>
      [Sungero.Core.Public]
      public const string R_DR_GET_OKOPF = "R_DR_GET_OKOPF";

      /// <summary> ОКФС </summary>
      [Sungero.Core.Public]
      public const string R_DR_GET_OKFS = "R_DR_GET_OKFS";

      /// <summary> ОКОНХ </summary>
      [Sungero.Core.Public]
      public const string R_DR_GET_OKONH = "R_DR_GET_OKONH";

      /// <summary> ОКВЕД </summary>
      [Sungero.Core.Public]
      public const string R_DR_GET_OKVED = "R_DR_GET_OKVED";

      /// <summary> Виды предприятий </summary>
      [Sungero.Core.Public]
      public const string R_DR_GET_COMPANYKINDS = "R_DR_GET_COMPANYKINDS";

      /// <summary> Типы удостоверений личности </summary>
      [Sungero.Core.Public]
      public const string R_DR_GET_TYPESOFIDCARDS = "R_DR_GET_TYPESOFIDCARDS";

      /// <summary> Экологические риски </summary>
      [Sungero.Core.Public]
      public const string R_DR_GET_ECOLOG = "R_DR_GET_ECOLOG";

      /// <summary> Семейное положение </summary>
      [Sungero.Core.Public]
      public const string R_DR_GET_MARITALSTATUSES = "R_DR_GET_MARITALSTATUSES";
      
      /// <summary> Курсы валют </summary>
      [Sungero.Core.Public]
      public const string R_DR_GET_CURRENCY_RATES = "R_DR_GET_CURRENCY_RATES";

      /// <summary> Регионы оплаты </summary>
      [Sungero.Core.Public]
      public const string R_DR_GET_PAYMENT_REGIONS = "R_DR_GET_PAYMENT_REGIONS";

      /// <summary> Регионы объектов аренды </summary>
      [Sungero.Core.Public]
      public const string R_DR_GET_TAX_REGIONS = "R_DR_GET_TAX_REGIONS";

      /// <summary> Виды договоров </summary>
      [Sungero.Core.Public]
      public const string R_DR_GET_CONTRACT_VID = "R_DR_GET_CONTRACT_VID";

      /// <summary> Типы договоров </summary>
      [Sungero.Core.Public]
      public const string R_DR_GET_CONTRACT_TYPE = "R_DR_GET_CONTRACT_TYPE";

      /// <summary> Договор </summary>
      [Sungero.Core.Public]
      public const string R_DR_SET_CONTRACT = "R_DR_SET_CONTRACT";      
      
      /// <summary> Дополнительное соглашение, Счет, Акт </summary>
      [Sungero.Core.Public]
      public const string R_DR_SET_PAYMENT_DOCUMENT = "R_DR_SET_PAYMENT_DOCUMENT";      
    }    
  }
}