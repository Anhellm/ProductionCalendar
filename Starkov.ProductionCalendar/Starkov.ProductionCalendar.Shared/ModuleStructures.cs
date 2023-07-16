using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace Starkov.ProductionCalendar.Structures.Module
{

  /// <summary>
  /// Информация о праздничных днях.
  /// </summary>
  partial class WeekendData
  {
    public string HolidayInfo { get; set; }
    
    public List<DateTime> Weekends { get; set; }
    
    public List<DateTime> Holidays { get; set; }
    
    public List<DateTime> PreHolidays { get; set; }
  }

}