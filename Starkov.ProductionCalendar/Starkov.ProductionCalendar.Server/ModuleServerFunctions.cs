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
    public virtual Structures.Module.WeekendData GetWeekendData(int year, IService service)
    {
      if (service == null)
      {
        return null;
      }
      
      var dataAccess = GetGataAccess(year, service);
      var data = CalendarService.CalendarService.GetWeekendData(dataAccess, service.UseApi.GetValueOrDefault());
      
      return GetWeekendData(data);
    }
    
    /// <summary>
    /// Получить структуру с данными по выходным.
    /// </summary>
    /// <param name="data">Данные из внещней системы.</param>
    /// <returns>Структура с данными по выходным.</returns>
    public virtual Structures.Module.WeekendData GetWeekendData(CalendarService.WeekendData data)
    {
      if (data == null)
        return null;
      
      var year = data.Year;
      
      var holidays = new List<DateTime>();
      var weekends = new List<DateTime>();
      var preHolidays = new List<DateTime>();
      
      foreach (var month in data.Months)
      {
        holidays.AddRange(Functions.Module.GetListDates(month.Holidays, month.Number, year));
        weekends.AddRange(Functions.Module.GetListDates(month.Weekends, month.Number, year));
        preHolidays.AddRange(Functions.Module.GetListDates(month.PreHolidays, month.Number, year));
      }
      
      var weekendData = Structures.Module.WeekendData.Create();
      weekendData.HolidayInfo = data.HolidayInfo;
      weekendData.Holidays = holidays;
      weekendData.Weekends = weekends;
      weekendData.PreHolidays = preHolidays;
      
      return weekendData;
    }
    
    /// <summary>
    /// Получить класс для получения доступа к ресурсам.
    /// </summary>
    /// <param name="year">Год.</param>
    /// <param name="service">Сервис.</param>
    /// <returns>Класс для получения доступа к ресурсам.</returns>
    public virtual CalendarService.DataAccess GetGataAccess(int year, IService service)
    {
      if (service == null || !service.DataSource.HasValue)
        return null;
      
      var dataSource = GetDataSource(service.DataSource.Value.ToString());
      return new DataAccess(year, service.Url, dataSource);
    }
    
    /// <summary>
    /// Получить значение перечисления источника данных.
    /// </summary>
    /// <param name="dataSource">Наименование источника данных.</param>
    /// <returns>Значение перечисления источника данных.</returns>
    public virtual CalendarService.DataSource GetDataSource(string dataSourceName)
    {
      DataSource dataSource = DataSource.Undefined;
      if (DataSource.TryParse(dataSourceName, out dataSource))
        return dataSource;
      
      return DataSource.Undefined;
    }
  }
}