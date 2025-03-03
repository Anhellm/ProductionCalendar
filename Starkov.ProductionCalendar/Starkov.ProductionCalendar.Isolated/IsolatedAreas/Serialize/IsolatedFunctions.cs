using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sungero.Core;
using Starkov.ProductionCalendar.Structures.Module;

namespace Starkov.ProductionCalendar.Isolated.Serialize
{
  public class IsolatedFunctions
  {
    /// <summary>
    /// Сериализовать данные по датам в json.
    /// </summary>
    /// <param name="dateInfos">Список структур с датой и типом из календаря рабочего времени.</param>
    /// <param name="preHolidays">Список предпраздничных дат.</param>
    /// <param name="loggerPostfix">Постфикс логгера.</param>
    /// <returns></returns>
    [Public]
    public string SerializeCalendar(List<Structures.Module.IDateInfo> dateInfos, List<DateTime> preHolidays, string loggerPostfix)
    {
      var logger = Logger.WithLogger(loggerPostfix);
      
      var info = PrepareDates(dateInfos, preHolidays);
      try
      {
        return Serializer.Serialize(info, false);
      }
      catch (Exception ex)
      {
        logger.Error(ex, "Ошибка сериализации данных по датам.");
        return string.Empty;
      }
    }
    
    /// <summary>
    /// Получить список структур с информацией о дате и типе.
    /// </summary>
    /// <param name="dateInfos">Список структур из календаря рабочего времени.</param>
    /// <param name="preHolidays">Список предпраздничных дат.</param>
    /// <returns>Список структур с информацией о дате и типе.</returns>
    public List<DateInfo> PrepareDates(List<Structures.Module.IDateInfo> dateInfos, List<DateTime> preHolidays)
    {
      var info = new List<DateInfo>();
      if (dateInfos == null)
        return info;
      
      foreach (var date in dateInfos)
      {
        DateType type = 0;
        if (string.IsNullOrEmpty(date.Type))
          type = (preHolidays?.Contains(date.Date) ?? false) ? DateType.Preholiday : DateType.Work;
        else if (date.Type == "Weekend")
          type = DateType.Weekend;
        else if (date.Type == "Holiday")
          type = DateType.Holiday;
        
        info.Add(new DateInfo(date.Date, type));
      }
      
      return info;
    }

  }
}