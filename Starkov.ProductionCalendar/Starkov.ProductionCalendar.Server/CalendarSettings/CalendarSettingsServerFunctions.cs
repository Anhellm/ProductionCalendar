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
    /// Получить настройки.
    /// </summary>
    /// <returns>Запись справочника Настройки.</returns>
    [Public, Remote(IsPure = true)]
    public static ICalendarSettings GetSettings()
    {
      return CalendarSettingses.GetAll().FirstOrDefault();
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