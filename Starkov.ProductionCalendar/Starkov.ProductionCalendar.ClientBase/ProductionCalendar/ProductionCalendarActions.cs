using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ProductionCalendar.ProductionCalendar;

namespace Starkov.ProductionCalendar.Client
{
  partial class ProductionCalendarActions
  {
    #region Показать праздники.
    public virtual void ShowHolidays(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Dialogs.ShowMessage(_obj.HolidayInfo);
    }

    public virtual bool CanShowHolidays(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !string.IsNullOrEmpty(_obj.HolidayInfo);
    }
    #endregion

    #region Обновить праздники.
    public virtual void UpdateWeekends(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var settings = Functions.CalendarSettings.Remote.GetSettings();
      
      var dialog = Dialogs.CreateInputDialog("Параметры");
      var service = dialog.AddSelect("Сервис", true, settings?.DefaultService);
      var withPreHolidays = dialog.AddBoolean("Обработать предпраздничные дни", settings?.NeedSetPreHolidays.GetValueOrDefault() ?? false);
      
      if (dialog.Show() == DialogButtons.Ok)
      {
        var serviceValue = service.Value;
        
        try
        {
          var data = Functions.Module.Remote.GetWeekendData(_obj.Year.GetValueOrDefault(), serviceValue);
          
          Functions.Module.UpdateCalendar(_obj.WorkingTimeCalendar, data, withPreHolidays.Value.GetValueOrDefault(), settings);
          _obj.HolidayInfo = data.HolidayInfo;
          _obj.UpdateInfo = string.Format("Данные получены из сервиса {0}: {1}", serviceValue.Name, Calendar.Now.ToString());
          _obj.Save();
        }
        catch (Exception ex)
        {
          Logger.Error("ProductionCalendar. UpdateWeekends(action). Ошибка обработки данных.", ex);
          Dialogs.ShowMessage(ProductionCalendars.Resources.UpdateWeekends_ErrorFormat(ex.Message), MessageType.Error);
        }
      }
    }

    public virtual bool CanUpdateWeekends(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.WorkingTimeCalendar?.AccessRights?.CanUpdate() ?? false;
    }
    #endregion
  }

}