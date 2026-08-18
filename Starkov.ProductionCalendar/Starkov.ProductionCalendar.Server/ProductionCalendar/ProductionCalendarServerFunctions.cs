using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ProductionCalendar.ProductionCalendar;

namespace Starkov.ProductionCalendar.Server
{
  partial class ProductionCalendarFunctions
  {
    #region Получение данных.
    /// <summary>
    /// Получить основные производственные календари.
    /// </summary>
    /// <returns>Производственные календари.</returns>
    [Public, Remote(IsPure = true)]
    public static IQueryable<IProductionCalendar> GetCalendars()
    {
      return ProductionCalendars.GetAll(x => x.IsPrivate != true);
    }
    
    /// <summary>
    /// Получить основной производственный календарь.
    /// </summary>
    /// <param name="year">Год.</param>
    /// <returns>Производственный календарь на конкретный год.</returns>
    [Public, Remote(IsPure = true)]
    public static IProductionCalendar GetCalendar(int? year)
    {
      if (!year.HasValue)
        return ProductionCalendars.Null;
      
      return GetCalendars().Where(x => x.Year == year).FirstOrDefault();
    }
    
    /// <summary>
    /// Получить основной производственный календарь.
    /// </summary>
    /// <param name="workingTimeCalendar">Календарь рабочего времени.</param>
    /// <returns>Получить производственный календарь на основе календаря рабочего времени.</returns>
    [Public, Remote(IsPure = true)]
    public static IProductionCalendar GetCalendar(IWorkingTimeCalendar workingTimeCalendar)
    {
      if (workingTimeCalendar == null)
        return ProductionCalendars.Null;
      
      return GetCalendars().Where(x => Equals(x.WorkingTimeCalendar, workingTimeCalendar)).FirstOrDefault();
    }
    
    #region Частные календари.
    /// <summary>
    /// Получить частные производственные календари.
    /// </summary>
    /// <returns>Производственные календари.</returns>
    [Public, Remote(IsPure = true)]
    public static IQueryable<IProductionCalendar> GetPrivateCalendars()
    {
      return ProductionCalendars.GetAll(x => x.IsPrivate == true);
    }
    
    /// <summary>
    /// Получить частные производственные календари.
    /// </summary>
    /// <param name="year">Год.</param>
    /// <returns>Производственные календари.</returns>
    [Public, Remote(IsPure = true)]
    public static IQueryable<IProductionCalendar> GetPrivateCalendars(int? year)
    {
      return GetPrivateCalendars().Where(x => x.Year == year);
    }
    #endregion
    #endregion
    
    #region Создание
    /// <summary>
    /// Создать производственный календарь.
    /// </summary>
    /// <param name="workingTimeCalendar">Календарь рабочего времени.</param>
    /// <returns>Производственный календарь.</returns>
    [Public, Remote]
    public static IProductionCalendar CreateCalendar(IWorkingTimeCalendar workingTimeCalendar)
    {
      if (workingTimeCalendar == null)
        return ProductionCalendars.Null;
      
      var calendar = ProductionCalendars.Create();
      calendar.WorkingTimeCalendar = workingTimeCalendar;
      return calendar;
    }
    #endregion
    
    /// <summary>
    /// Необходимость обновления сериализованных данных календаря.
    /// </summary>
    /// <returns>True/False.</returns>
    [Remote(IsPure = true)]
    public virtual bool NeedSerialize()
    {
      var wtc = _obj.WorkingTimeCalendar;
      return wtc == null || GetLastDateChange(wtc) > GetLastDateChange(_obj);
    }
    
    /// <summary>
    /// Получить последнюю дату изменения сущности.
    /// </summary>
    /// <param name="entity">Сущность.</param>
    /// <returns>Дата.</returns>
    [Remote(IsPure = true)]
    public static DateTime? GetLastDateChange(Sungero.Domain.Shared.IEntity entity)
    {
      if (entity == null)
        return null;
      
      return entity.History.GetAll()
        .Where(x => x.Action == Sungero.CoreEntities.History.Action.Update && x.HistoryDate.HasValue)
        .Select(x => x.HistoryDate.Value)
        .OrderByDescending(x => x)
        .FirstOrDefault();
    }
    
    /// <summary>
    /// Сериализовать календаря в json.
    /// </summary>
    /// <param name="withSave">Необходимость сохранения.</param>
    [Remote]
    public virtual void SerializeCalendar(bool withSave)
    {
      var data = _obj.WorkingTimeCalendar?.Day
        ?.Select(x => Starkov.ProductionCalendar.Structures.Module.DateInfo.Create(x.Day, x.Kind.HasValue ? x.Kind.Value.Value : string.Empty))
        ?.ToList();
      var preHolidays = _obj.PreHolidays.Where(x => x.Date.HasValue).Select(x => x.Date.Value).ToList();
      
      _obj.JsonInfo = IsolatedFunctions.Serialize.SerializeCalendar(data, preHolidays, Constants.Module.LoggerPostfix);
      if (withSave)
        _obj.Save();
    }
  }
}