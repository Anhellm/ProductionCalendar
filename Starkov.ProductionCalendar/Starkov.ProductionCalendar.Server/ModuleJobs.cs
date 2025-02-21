using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace Starkov.ProductionCalendar.Server
{
  public class ModuleJobs
  {

    /// <summary>
    /// Обновление данных календаря на следующий год.
    /// </summary>
    public virtual void NextYearCalendar()
    {
      int year = Calendar.Today.Year + 1;
      var settings = Functions.CalendarSettings.GetUpdateSettings();
      
      var _logger = Logger.WithLogger(Constants.Module.LoggerPostfix)
        .WithProperty("Job", "NextYearCalendar")
        .WithProperty("Year", year);
      
      Structures.Module.IWeekendData data = null;
      try
      {
        data = Functions.Module.GetWeekendData(year, settings.DefaultService, null);
      }
      catch (Exception ex)
      {
        _logger.Error(ex, "Ошибка при получении данных из внешнего сервиса.");
        return;
      }
      
      var workingTimeCalendar = Functions.Module.GetWorkingTimeCalendar(year);
      if (workingTimeCalendar == null)
        workingTimeCalendar = Functions.Module.CreateWorkingTimeCalendar();
      
      var days = workingTimeCalendar.Day;
      
      // Заполнение рабочего календаря.
      days.Clear();
      for (DateTime date = new DateTime(year, 1, 1); date <= new DateTime(year, 12, 31); date = date.AddDays(1))
        days.AddNew().Day = date;
      
      try
      {
        Functions.Module.UpdateCalendar(workingTimeCalendar, data, settings);
        _logger.Debug("Календарь обновлен.");
      }
      catch (Exception ex)
      {
        _logger.Error(ex, "Ошибка при обновлении данных календаря.");
        return;
      }
      
      // Обновление производственного календаря.
      try
      {
        var productionCalendar = Functions.ProductionCalendar.GetCalendar(year) ?? Functions.ProductionCalendar.CreateCalendar(workingTimeCalendar);
        productionCalendar.WorkingTimeCalendar = workingTimeCalendar;
        Functions.ProductionCalendar.UpdateProductionCalendar(productionCalendar, data, settings.DefaultService);
      }
      catch (Exception ex)
      {
        _logger.Error(ex, "Ошибка при обновлении данных производственного календаря.");
        return;
      }
    }

  }
}