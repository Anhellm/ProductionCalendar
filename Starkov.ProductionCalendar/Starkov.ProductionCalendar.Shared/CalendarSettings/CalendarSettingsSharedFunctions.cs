using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ProductionCalendar.CalendarSettings;

namespace Starkov.ProductionCalendar.Shared
{
  partial class CalendarSettingsFunctions
  {
    /// <summary>
    /// Обновить настройки.
    /// </summary>
    /// <param name="dayBeginning">Начало дня.</param>
    /// <param name="dayEnding">Конец дня.</param>
    /// <param name="lunchBreakBeginning">Начало обеда.</param>
    /// <param name="lunchBreakEnding">Конец обеда.</param>
    public virtual void UpdateSettings(Structures.CalendarSettings.IUpdateSettings settings)
    {
      UpdateSettings(settings.DayBeginning, settings.DayEnding, settings.LunchBreakBeginning, settings.LunchBreakEnding);
    }
    
    /// <summary>
    /// Обновить настройки.
    /// </summary>
    /// <param name="dayBeginning">Начало дня.</param>
    /// <param name="dayEnding">Конец дня.</param>
    /// <param name="lunchBreakBeginning">Начало обеда.</param>
    /// <param name="lunchBreakEnding">Конец обеда.</param>
    public virtual void UpdateSettings(double? dayBeginning, double? dayEnding, double? lunchBreakBeginning, double? lunchBreakEnding)
    {
      _obj.DayBeginning = dayBeginning;
      _obj.DayEnding = dayEnding;
      _obj.LunchBreakBeginning = lunchBreakBeginning;
      _obj.LunchBreakEnding = lunchBreakEnding;
      
      _obj.Save();
    }

    /// <summary>
    /// Задать состояние свойств.
    /// </summary>
    public virtual void SetPropertiesState()
    {
      var properties = _obj.State.Properties;
      
      properties.LunchBreakBeginning.IsRequired = _obj.LunchBreakEnding.HasValue;
      properties.LunchBreakEnding.IsRequired = _obj.LunchBreakBeginning.HasValue;
    }

  }
}