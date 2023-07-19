using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace Starkov.ProductionCalendar.Structures.CalendarSettings
{

  /// <summary>
  /// Настройки обновления.
  /// </summary>
  [Public]
  partial class UpdateSettings
  {
    public IService DefaultService { get; set; }
    public bool? NeedSetPreHolidays { get; set; }
    public double? DayBeginning { get; set; }
    public double? DayEnding { get; set; }
    public double? LunchBreakBeginning { get; set; }
    public double? LunchBreakEnding { get; set; }
  }

}