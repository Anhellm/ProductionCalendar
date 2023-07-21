using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace Starkov.ProductionCalendar.Shared
{
  public class ModuleFunctions
  {
    #region Валидация значений времени.
    /// <summary>
    /// Валидация структуры настроек.
    /// </summary>
    /// <param name="settings">Структура с настройками.</param>
    /// <returns>Строка с ошибкой или пустая строка.</returns>
    public virtual string Validate(Structures.CalendarSettings.IUpdateSettings settings)
    {
      var propertiesInfo = WorkingTime.Info.Properties.Day.Properties;
      
      var stringBuilder = new System.Text.StringBuilder();
      stringBuilder.AppendLine(TimeRangeValidate(settings.DayBeginning, propertiesInfo.DayBeginning.LocalizedName));
      stringBuilder.AppendLine(TimeRangeValidate(settings.DayEnding, propertiesInfo.DayEnding.LocalizedName));
      stringBuilder.AppendLine(TimeRangeValidate(settings.LunchBreakBeginning, propertiesInfo.LunchBreakBeginning.LocalizedName));
      stringBuilder.AppendLine(TimeRangeValidate(settings.LunchBreakEnding, propertiesInfo.LunchBreakEnding.LocalizedName));
      
      stringBuilder.AppendLine(TimeRangeValidate(settings.DayBeginning, settings.DayEnding,
                                                 propertiesInfo.DayBeginning.LocalizedName, propertiesInfo.DayEnding.LocalizedName));
      stringBuilder.AppendLine(TimeRangeValidate(settings.LunchBreakBeginning, settings.LunchBreakEnding,
                                                 propertiesInfo.LunchBreakBeginning.LocalizedName, propertiesInfo.LunchBreakEnding.LocalizedName));
      
      stringBuilder.AppendLine(TimeRangeValidate(settings.DayBeginning, settings.LunchBreakBeginning, settings.DayEnding, settings.LunchBreakEnding, Resources.PeriodValidateName));
      
      return stringBuilder.ToString().Trim();
    }
    
    /// <summary>
    /// Валидация значений времени.
    /// </summary>
    /// <param name="inputValue">Значение.</param>
    /// <param name="propertyName">Наименование свойства.</param>
    /// <returns>Строка с ошибкой или пустая строка.</returns>
    public virtual string TimeRangeValidate(double? inputValue, string propertyName)
    {
      if (TimeRangeValidate(inputValue))
        return Resources.TimeRange_ErrorFormat(propertyName);
      
      return string.Empty;
    }

    /// <summary>
    /// Валидация значений времени.
    /// </summary>
    /// <param name="inputValueFrom">Значение начала.</param>
    /// <param name="inputValueTo">Значение окончания.</param>
    /// <param name="inputValueFromPropertyName">Наименование свойства начала периода.</param>
    /// <param name="inputValueToPropertyName">Наименование свойства окончания периода</param>
    /// <returns>Строка с ошибкой или пустая строка.</returns>
    public virtual string TimeRangeValidate(double? inputValueFrom, double? inputValueTo,
                                            string inputValueFromPropertyName, string inputValueToPropertyName)
    {
      if (TimeRangeValidate(inputValueFrom, inputValueTo))
        return Resources.TimeComparison_ErrorFormat(inputValueFromPropertyName, inputValueToPropertyName);
      
      return string.Empty;
    }
    
    /// <summary>
    /// Валидация значений времени (Проверка вхождения периодов времени).
    /// </summary>
    /// <param name="outerFromValue">Внешнее значение начала.</param>
    /// <param name="innerFromValue">Внутреннее значение начала.</param>
    /// <param name="outerToValue">Внешнее значение окончания.</param>
    /// <param name="innerToValue">Внутреннее значение окончания.</param>
    /// <param name="periodName">Наименование периода.</param>
    /// <returns>Строка с ошибкой или пустая строка.</returns>
    public virtual string TimeRangeValidate(double? outerFromValue, double? innerFromValue, double? outerToValue, double? innerToValue, string periodName)
    {
      if (TimeRangeValidate(outerFromValue, innerFromValue, outerToValue, innerToValue))
        return Resources.LunchTimeRange_ErrorFormat(periodName);
      
      return string.Empty;
    }
    
    /// <summary>
    /// Валидация значений времени (Проверка вхождения периодов времени).
    /// </summary>
    /// <param name="outerFromValue">Внешнее значение начала.</param>
    /// <param name="innerFromValue">Внутреннее значение начала.</param>
    /// <param name="outerToValue">Внешнее значение окончания.</param>
    /// <param name="innerToValue">Внутреннее значение окончания.</param>
    /// <returns>True/False.</returns>
    public virtual bool TimeRangeValidate(double? outerFromValue, double? innerFromValue, double? outerToValue, double? innerToValue)
    {
      return innerFromValue < outerFromValue || innerToValue > outerToValue;
    }
    
    /// <summary>
    /// Валидация значения времени.
    /// </summary>
    /// <param name="inputValue">Значение.</param>
    /// <returns>True/False.</returns>
    public virtual bool TimeRangeValidate(double? inputValue)
    {
      return inputValue < 0 || inputValue > 24;
    }
    
    /// <summary>
    /// Валидация значений времени.
    /// </summary>
    /// <param name="inputValueFrom">Значение начала.</param>
    /// <param name="inputValueTo">Значение окончания.</param>
    /// <returns>True/False.</returns>
    public virtual bool TimeRangeValidate(double? inputValueFrom, double? inputValueTo)
    {
      return inputValueFrom > inputValueTo;
    }
    #endregion

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
    
    /// <summary>
    /// Обновить календарь рабочего времени.
    /// </summary>
    /// <param name="calendar">Календарь.</param>
    /// <param name="settings">Настройка обновления.</param>
    /// <param name="withoutPreHolidays">true если исключаем предпраздничные дни.</param>
    /// <param name="preHolidays">Предпраздничные дни.</param>
    /// <param name="dateStart">Дата начала обновления.</param>
    /// <param name="dateEnd">Дата окончания обновления.</param>
    public virtual void UpdateCalendar(IWorkingTimeCalendar calendar, Structures.CalendarSettings.IUpdateSettings settings, bool withoutPreHolidays, List<DateTime> preHolidays,
                                       DateTime? dateStart, DateTime? dateEnd)
    {
      if (calendar == null || settings == null)
        return;
      
      var days = calendar.Day
        .Where(x => !x.Kind.HasValue)
        .Where(x => !dateStart.HasValue || x.Day >= dateStart)
        .Where(x => !dateEnd.HasValue || x.Day <= dateEnd);
      
      if (withoutPreHolidays && preHolidays.Any())
        days = days.Where(x => !preHolidays.Contains(x.Day));
      
      foreach (var line in days)
      {
        line.DayBeginning = settings.DayBeginning;
        line.DayEnding = settings.DayEnding;
        line.LunchBreakBeginning = settings.LunchBreakBeginning;
        line.LunchBreakEnding = settings.LunchBreakEnding;
      }
      
      calendar.Save();
    }
  }
}