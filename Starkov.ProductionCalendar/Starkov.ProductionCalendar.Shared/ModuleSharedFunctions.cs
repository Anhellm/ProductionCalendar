using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace Starkov.ProductionCalendar.Shared
{
  public class ModuleFunctions
  {

    /// <summary>
    /// Получить список дат.
    /// </summary>
    /// <param name="dates">Массив дней.</param>
    /// <param name="month">Номер месяца.</param>
    /// <param name="year">Год.</param>
    /// <returns>Список дат.</returns>
    public List<DateTime> GetListDates(string[] dates, int month, int year)
    {
      if (dates == null)
        return new List<DateTime>();
      
      return dates
        .Select(x =>
                {
                  int result;
                  return int.TryParse(x, out result) ? result : -1;
                })
        .Where(x => x > -1)
        .Select(x =>
                {
                  DateTime date;
                  string stringDate = string.Format("{0}.{1}.{2}", x, month, year);
                  return DateTime.TryParseExact(stringDate, "d.M.yyyy", TenantInfo.Culture, System.Globalization.DateTimeStyles.None, out date) ?
                    date :
                    DateTime.MinValue;
                })
        .Where(x => x > DateTime.MinValue)
        .ToList();
    }

    /// <summary>
    /// Обновить календарь рабочего времени.
    /// </summary>
    /// <param name="calendar">Календарь.</param>
    /// <param name="data">Данные по выходным.</param>
    /// <param name="settings">Настройки обновления.</param>
    public virtual void UpdateCalendar(IWorkingTimeCalendar calendar, Structures.Module.WeekendData data, Structures.CalendarSettings.IUpdateSettings settings)
    {
      if (calendar == null || data == null)
      {
        Logger.Error("ProductionCalendar. UpdateCalendar(func). Переданы пустые параметры.");
        return;
      }
      
      Logger.DebugFormat("ProductionCalendar. UpdateCalendar(func). Обновление данных календаря с ИД {0}.", calendar.Id);
      
      var days = calendar.Day;
      
      // Выходные.
      foreach (var weekend in days.Where(x => data.Weekends.Contains(x.Day)))
        weekend.Kind = Sungero.CoreEntities.WorkingTimeCalendarDay.Kind.Weekend;
      
      // Праздники.
      foreach (var holiday in days.Where(x => data.Holidays.Contains(x.Day)))
        holiday.Kind = Sungero.CoreEntities.WorkingTimeCalendarDay.Kind.Holiday;
      
      // Рабочие дни.
      foreach (var workingDay in days.Where(x => !data.Weekends.Contains(x.Day) && !data.Holidays.Contains(x.Day)))
        workingDay.Kind = null;
      
      // Заполняем время для рабочих дней.
      foreach (var workingDay in days.Where(x => !x.Duration.HasValue && !x.Kind.HasValue))
      {
        workingDay.DayBeginning = settings.DayBeginning;
        workingDay.DayEnding = settings.DayEnding;
        workingDay.LunchBreakBeginning = settings.LunchBreakBeginning;
        workingDay.LunchBreakEnding = settings.LunchBreakEnding;
      }
      
      // Предпраздничные дни.
      if (settings.NeedSetPreHolidays.GetValueOrDefault())
        foreach (var preHoliday in days.Where(x => data.PreHolidays.Contains(x.Day)))
          preHoliday.DayEnding -= 1;
      
      calendar.Save();
    }
  }
}