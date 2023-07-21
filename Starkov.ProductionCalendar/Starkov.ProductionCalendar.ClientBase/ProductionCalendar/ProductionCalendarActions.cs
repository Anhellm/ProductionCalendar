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
    #region Изменить график работы
    public virtual void WorkScheduleChange(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var preHolidays = Functions.ProductionCalendar.GetPreHolidays(_obj);
      bool hasPreHolidays = preHolidays.Any();
      
      var dialog = Dialogs.CreateInputDialog(ProductionCalendars.Resources.WorkScheduleChangeDialog_Name);
      
      var propertiesInfo = _obj.WorkingTimeCalendar.Info.Properties.Day.Properties;
      var settings = Functions.CalendarSettings.Remote.GetUpdateSettings();
      
      var dayBeginning = dialog.AddDouble(propertiesInfo.DayBeginning.LocalizedName, true, settings.DayBeginning);
      var dayEnding = dialog.AddDouble(propertiesInfo.DayEnding.LocalizedName, true, settings.DayEnding);
      var lunchBreakBeginning = dialog.AddDouble(propertiesInfo.LunchBreakBeginning.LocalizedName, false, settings.LunchBreakBeginning);
      var lunchBreakEnding = dialog.AddDouble(propertiesInfo.LunchBreakEnding.LocalizedName, false, settings.LunchBreakEnding);
      var withoutPreHolidays = dialog.AddBoolean(ProductionCalendars.Resources.WorkScheduleChangeDialog_WithPreHolidays, hasPreHolidays);
      var dateStart = dialog.AddDate(ProductionCalendars.Resources.WorkScheduleChangeDialog_PeriodFrom, false);
      var dateEnd = dialog.AddDate(ProductionCalendars.Resources.WorkScheduleChangeDialog_PeriodTo, false);
      var needChangeSettings = dialog.AddBoolean(ProductionCalendars.Resources.WorkScheduleChangeDialog_NeedChangeSettings, hasPreHolidays);
      
      dialog.SetOnRefresh(
        (args) =>
        {
          lunchBreakBeginning.IsRequired = lunchBreakEnding.Value.HasValue;
          lunchBreakEnding.IsRequired = lunchBreakBeginning.Value.HasValue;
          withoutPreHolidays.IsVisible = hasPreHolidays;
          
          settings.DayBeginning = dayBeginning.Value;
          settings.DayEnding = dayEnding.Value;
          settings.LunchBreakBeginning = lunchBreakBeginning.Value;
          settings.LunchBreakEnding = lunchBreakEnding.Value;
          
          var validate = Functions.Module.Validate(settings);
          if (!string.IsNullOrEmpty(validate))
            args.AddError(validate);

          Functions.ProductionCalendar.ChangeDialogValidateInputValue(_obj, args, dateStart.Value, dateEnd.Value,
                                                                      new CommonLibrary.IDialogControl[] { dateStart, dateEnd });
        });
      
      if (dialog.Show() == DialogButtons.Ok)
      {
        // Обновление настроек.
        if (needChangeSettings.Value == true)
        {
          var settingsEntity = Functions.CalendarSettings.Remote.GetSettings();
          if (settings != null)
            Functions.CalendarSettings.UpdateSettings(settingsEntity, settings);
        }
        
        // Обновление календаря.
        var preholidays = Functions.ProductionCalendar.GetPreHolidays(_obj);
        
        try
        {
          Functions.Module.UpdateCalendar(_obj.WorkingTimeCalendar, settings, withoutPreHolidays.Value.GetValueOrDefault(), 
                                          preholidays, dateStart.Value, dateEnd.Value);
          _obj.State.Controls.CalendarState.Refresh();
          Dialogs.ShowMessage(ProductionCalendars.Resources.WorkScheduleChangeSuccess_Info);
        }
        catch (Exception ex)
        {
          Dialogs.ShowMessage(ex.Message, MessageType.Error);
        }
      }
    }

    public virtual bool CanWorkScheduleChange(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      bool canUpdateCalendar = _obj.WorkingTimeCalendar?.AccessRights?.CanUpdate() ?? false;
      bool canUpdateSettings = CalendarSettingses.AccessRights.CanUpdate();
      
      return canUpdateCalendar && canUpdateSettings;
    }
    #endregion
    
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

    #region Обновить из внешнего сервиса.
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
      bool canUpdateCalendar = _obj.WorkingTimeCalendar?.AccessRights?.CanUpdate() ?? false;
      bool canReadSettings = CalendarSettingses.AccessRights.CanRead();
      
      return canUpdateCalendar && canReadSettings;
    }
    #endregion
  }

}