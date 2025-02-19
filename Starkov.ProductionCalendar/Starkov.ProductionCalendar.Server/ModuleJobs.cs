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
      
      Structures.Module.IWeekendData data = null;
      try
      {
        data = Functions.Module.GetWeekendData(year, settings.DefaultService);
      }
      catch (Exception ex)
      {
        Logger.Error("Ошибка при получении данных из внешнего сервиса.", ex);
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
        Logger.DebugFormat("Обновлен календарь на {0} год.", year);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("Ошибка при обновлении данных календаря на {0} год.", ex, year);
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
        Logger.ErrorFormat("Ошибка при обновлении данных производственного календаря на {0} год.", ex, year);
        return;
      }
    }

  }
}