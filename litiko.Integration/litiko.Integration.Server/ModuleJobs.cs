using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.Integration.Server
{
  public class ModuleJobs
  {

    /// <summary>
    /// 
    /// </summary>
    public virtual void GetOKVED()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart("R_DR_GET_OKVED");        
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual void GetOKOPF()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart("R_DR_GET_OKOPF");       
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual void GetOKONH()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart("R_DR_GET_OKONH");         
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual void GetOKFS()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart("R_DR_GET_OKFS");         
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual void GetMaterialStatuses()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart("R_DR_GET_MARITALSTATUSES");       
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual void GetEcolog()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart("R_DR_GET_ECOLOG");       
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual void GetCountries()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart("R_DR_GET_COUNTRIES");      
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual void GetCompanyKinds()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart("R_DR_GET_COMPANYKINDS");
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual void GetEmployees()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart("R_DR_GET_EMPLOYEES");            
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual void GetBusinessUnits()
    {
      litiko.Integration.Functions.Module.BackgroundProcessStart("R_DR_GET_BUSINESSUNITS");                  
    }

    /// <summary>
    /// Запрос подразделений из интегрируемой системы
    /// </summary>
    public virtual void GetDepartments()
    {                
      litiko.Integration.Functions.Module.BackgroundProcessStart("R_DR_GET_DEPART");
    }

  }
}