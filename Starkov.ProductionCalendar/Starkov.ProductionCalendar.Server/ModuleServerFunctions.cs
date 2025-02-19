using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using CalendarService;

namespace Starkov.ProductionCalendar.Server
{
  public class ModuleFunctions
  {

    #region Основные календари рабочего времени
    /// <summary>
    /// Получить основной календарь рабочего времени.
    /// </summary>
    /// <param name="year">Год.</param>
    /// <returns>Календарь рабочего времени за год.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IWorkingTimeCalendar GetWorkingTimeCalendar(int year)
    {
      return GetWorkingTimeCalendars().Where(x => x.Year == year).FirstOrDefault();
    }
    
    /// <summary>
    /// Получить основные календари рабочего времени.
    /// </summary>
    /// <returns>Основные календари рабочего времени.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IQueryable<IWorkingTimeCalendar> GetWorkingTimeCalendars()
    {
      return WorkingTime.GetAll(x => !PrivateWorkingTimeCalendars.Is(x));
    }
    
    /// <summary>
    /// Получить основные календари рабочего времени.
    /// </summary>
    /// <returns>Основные календари рабочего времени.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IWorkingTimeCalendar CreateWorkingTimeCalendar()
    {
      if (!WorkingTime.AccessRights.CanCreate())
        throw AppliedCodeException.Create(Resources.AccessRightsCreate_Error);
      
      return WorkingTime.Create();
    }
    #endregion
    
    /// <summary>
    /// Получить структуру с данными по выходным.
    /// </summary>
    /// <param name="year">Год.</param>
    /// <param name="service">Сервис.</param>
    /// <returns>Структура с данными по выходным.</returns>
    [Remote]
    public virtual Structures.Module.IWeekendData GetWeekendData(int year, IService service)
    {
      if (service == null)
      {
        Logger.Error("ProductionCalendar. GetWeekendData(func). Получен пустой сервис.");
        return null;
      }
      
      var requestParams = GetRequestParams(year, service);
      return IsolatedFunctions.ExternalData.GetWeekendData(requestParams, Constants.Module.LoggerPostfix);
    }

    /// <summary>
    /// Получить класс для получения доступа к ресурсам.
    /// </summary>
    /// <param name="year">Год.</param>
    /// <param name="service">Сервис.</param>
    /// <returns>Класс для получения доступа к ресурсам.</returns>
    public virtual Structures.Module.IRequestParams GetRequestParams(int year, IService service)
    {
      if (service == null || !service.DataSource.HasValue)
        return null;
      
      var structure = Structures.Module.RequestParams.Create();
      structure.ServiceName = service.DataSource.Value.Value;
      structure.UrlTemplate = service.Url;
      structure.Year = year;
      structure.ByApi = service.UseApi.GetValueOrDefault();
      
      return structure;
    }
  }
}