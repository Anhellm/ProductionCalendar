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
  [Public(Isolated = true)]
  partial class WeekendData
  {
    public string HolidayInfo { get; set; }
    
    public List<DateTime> Weekends { get; set; }
    
    public List<DateTime> Holidays { get; set; }
    
    public List<DateTime> PreHolidays { get; set; }
  }
  
  /// <summary>
  /// Параметры запроса.
  /// </summary>
  [Public(Isolated = true)]
  partial class RequestParams
  {
    /// <summary>
    /// Год.
    /// </summary>
    public int Year { get; set; }
    
    /// <summary>
    /// Шаблон Url.
    /// </summary>
    public string UrlTemplate { get; set; }
    
    /// <summary>
    /// Название сервиса.
    /// </summary>
    public string ServiceName { get; set; }
    
    /// <summary>
    /// Запрос к Api.
    /// </summary>
    public bool ByApi { get; set; }
  }

}