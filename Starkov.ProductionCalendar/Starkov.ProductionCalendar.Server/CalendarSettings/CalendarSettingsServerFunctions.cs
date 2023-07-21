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
    /// <param name="settings">Структура с настройками.</param>
    /// <returns>Структура с настройками обновления.</returns>
    [Public, Remote(IsPure = true)]
    public static Structures.CalendarSettings.IUpdateSettings GetUpdateSettings(ICalendarSettings settings)
    {
      var structure = Structures.CalendarSettings.UpdateSettings.Create();
      
      if (settings == null)
        return structure;
      
      structure.DefaultService = settings.DefaultService;
      structure.NeedSetPreHolidays = settings.NeedSetPreHolidays;
      structure.DayBeginning = settings.DayBeginning;
      structure.DayEnding = settings.DayEnding;
      structure.LunchBreakBeginning = settings.LunchBreakBeginning;
      structure.LunchBreakEnding = settings.LunchBreakEnding;
      
      return structure;
    }
    
    /// <summary>
    /// Получить структуру с настройками обновления.
    /// </summary>
    /// <returns>Структура с настройками обновления.</returns>
    [Public, Remote(IsPure = true)]
    public static Structures.CalendarSettings.IUpdateSettings GetUpdateSettings()
    {
      var settings = GetSettings();
      return GetUpdateSettings(settings);
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
      
      settings.NeedSetPreHolidays = true;
      settings.DefaultService = Functions.Service.GetServices(Starkov.ProductionCalendar.Service.DataSource.Consultant).FirstOrDefault();
      Functions.CalendarSettings.UpdateSettings(settings, 9, 17, null, null);
      
      return settings;
    }

  }
}