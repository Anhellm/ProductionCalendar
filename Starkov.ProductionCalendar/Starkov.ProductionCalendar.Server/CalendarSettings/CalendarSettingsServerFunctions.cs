using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ProductionCalendar.CalendarSettings;

namespace Starkov.ProductionCalendar.Server
{
  partial class CalendarSettingsFunctions
  {

    /// <summary>
    /// Получить структуру с настройками обновления.
    /// </summary>
    /// <returns>Структура с настройками обновления.</returns>
    [Public, Remote(IsPure = true)]
    public static Structures.CalendarSettings.IUpdateSettings GetUpdateSettings()
    {
      var structure = Structures.CalendarSettings.UpdateSettings.Create();
      
      var setting = GetSettings();
      if (setting == null)
        return structure;
      
      structure.DefaultService = setting.DefaultService;
      structure.NeedSetPreHolidays = setting.NeedSetPreHolidays;
      structure.DayBeginning = setting.DayBeginning;
      structure.DayEnding = setting.DayEnding;
      structure.LunchBreakBeginning = setting.LunchBreakBeginning;
      structure.LunchBreakEnding = setting.LunchBreakEnding;
      
      return structure;
    }
    
    /// <summary>
    /// Получить настройки.
    /// </summary>
    /// <returns>Запись справочника Настройки.</returns>
    [Public, Remote(IsPure = true)]
    public static ICalendarSettings GetSettings()
    {
      return CalendarSettingses.GetAllCached().FirstOrDefault();
    }
    
    /// <summary>
    /// Создать настройки по умолчанию.
    /// </summary>
    /// <returns>Запись справочника Настройки.</returns>
    public static ICalendarSettings CreateDefaultSettings()
    {
      var settings = CalendarSettingses.Create();
      
      settings.DayBeginning = 9;
      settings.DayEnding = 17;
      settings.NeedSetPreHolidays = true;
      settings.DefaultService = Functions.Service.GetServices(Starkov.ProductionCalendar.Service.DataSource.Consultant).FirstOrDefault();
      settings.Save();
      
      return settings;
    }

  }
}