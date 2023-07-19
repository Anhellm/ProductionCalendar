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
    public virtual void UpdateFromExternalService(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var settings = Functions.CalendarSettings.Remote.GetUpdateSettings();
      
      var dialog = Dialogs.CreateInputDialog(ProductionCalendars.Resources.UpdateDialog_Name);
      var service = dialog.AddSelect(ProductionCalendars.Resources.UpdateDialog_Service, true, settings.DefaultService);
      var withPreHolidays = dialog.AddBoolean(ProductionCalendars.Resources.UpdateDialog_SetPreHolidays, settings.NeedSetPreHolidays.GetValueOrDefault());
      
      if (dialog.Show() == DialogButtons.Ok)
      {
        var serviceValue = service.Value;
        settings.NeedSetPreHolidays = withPreHolidays.Value;
        
        try
        {
          var data = Functions.Module.Remote.GetWeekendData(_obj.Year.GetValueOrDefault(), serviceValue);
          
          Functions.Module.UpdateCalendar(_obj.WorkingTimeCalendar, data, settings);
          
          Functions.ProductionCalendar.SetPreHolidays(_obj, data.PreHolidays);
          _obj.HolidayInfo = data.HolidayInfo;
          _obj.UpdateInfo = ProductionCalendars.Resources.UpdateInfoFormat(serviceValue.Name, Calendar.Now.ToString());
          _obj.Save();
        }
        catch (Exception ex)
        {
          Logger.Error("ProductionCalendar. UpdateWeekends(action). Ошибка обработки данных.", ex);
          Dialogs.ShowMessage(ProductionCalendars.Resources.UpdateWeekends_ErrorFormat(ex.Message), MessageType.Error);
        }
      }
    }

    public virtual bool CanUpdateFromExternalService(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.WorkingTimeCalendar?.AccessRights?.CanUpdate() ?? false;
    }
    #endregion
  }

}