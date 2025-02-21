using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace Starkov.ProductionCalendar.Server
{
  public class ModuleAsyncHandlers
  {

    /// <summary>
    /// Обновление производственного календаря.
    /// </summary>
    /// <param name="args"></param>
    public virtual void UpdateCalendar(Starkov.ProductionCalendar.Server.AsyncHandlerInvokeArgs.UpdateCalendarInvokeArgs args)
    {
      var _logger = Logger.WithLogger(Constants.Module.LoggerPostfix)
        .WithProperty("AsyncHandler", "UpdateCalendar")
        .WithProperty("CalendarId", args.ProductionCalendarId)
        .WithProperty("ServiceId", args.ServiceId);
      
      _logger.Debug("Старт обработки календаря.");
      
      #region Проверка данных.
      var calendar = ProductionCalendars.GetAll(x => x.Id == args.ProductionCalendarId).FirstOrDefault();
      if (calendar == null)
      {
        _logger.Error("Календарь не найден.");
        return;
      }
      
      var service = Services.GetAll(x => x.Id == args.ServiceId).FirstOrDefault();
      if (service == null)
      {
        _logger.Error("Сервис не найден.");
        return;
      }
      
      var workingTimeCalendar = calendar.WorkingTimeCalendar;
      if (workingTimeCalendar == null)
      {
        _logger.Error("Не заполнен календарь рабочего времени.");
        return;
      }
      #endregion
      
      var settings = Functions.CalendarSettings.GetUpdateSettings();
      
      try
      {
        #region Блокировки.
        if (!Locks.TryLock(calendar))
        {
          _logger.Error("Не удалось установить блокировку записи справочника.");
          args.Retry = true;
          return;
        }
        
        if (!Locks.TryLock(workingTimeCalendar))
        {
          _logger.Error("Не удалось установить блокировку календаря рабочего времени.");
          args.Retry = true;
          return;
        }
        #endregion
        
        var externalData = Functions.Module.GetWeekendData(calendar.Year.GetValueOrDefault(), service, null);
        Functions.Module.UpdateCalendar(workingTimeCalendar, externalData, settings);
        Functions.ProductionCalendar.UpdateProductionCalendar(calendar, externalData, service);
        
        _logger.Debug("Обработка успешно завершена.");
      }
      catch (Exception ex)
      {
        _logger.Error(ex, "Ошибка при обработке.");
        return;
      }
      finally
      {
        if (Locks.GetLockInfo(calendar).IsLockedByMe)
          Locks.Unlock(calendar);
        if (Locks.GetLockInfo(workingTimeCalendar).IsLockedByMe)
          Locks.Unlock(workingTimeCalendar);
      }
      
    }

  }
}