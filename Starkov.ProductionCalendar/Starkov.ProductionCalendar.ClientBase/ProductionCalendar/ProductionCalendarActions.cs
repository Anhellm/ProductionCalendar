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
    #region Открыть календарь на след. год.
    public virtual void PreviousYear(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (_obj.Year.HasValue)
        Functions.Module.ShowOrCreateMainCalendar(_obj.Year.Value - 1);
    }

    public virtual bool CanPreviousYear(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.Year.HasValue;
    }
    #endregion

    #region Открыть календарь на след. год.
    public virtual void NextYear(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (_obj.Year.HasValue)
        Functions.Module.ShowOrCreateMainCalendar(_obj.Year.Value + 1);
    }

    public virtual bool CanNextYear(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.Year.HasValue;
    }
    #endregion

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
      var defualtService = settings.DefaultService;
      
      var dialog = Dialogs.CreateInputDialog(ProductionCalendars.Resources.UpdateDialog_Name);
      var service = dialog.AddSelect(ProductionCalendars.Resources.UpdateDialog_Service, true, defualtService);
      var withPreHolidays = dialog.AddBoolean(ProductionCalendars.Resources.UpdateDialog_SetPreHolidays, settings.NeedSetPreHolidays.GetValueOrDefault());
      var html = dialog.AddFileSelect(ProductionCalendars.Resources.UpdateDialog_File, false)
        .WithFilter(ProductionCalendars.Resources.UpdateDialog_FileFilterName, "html", "htm");
      
      var ayncButton = dialog.Buttons.AddCustom(ProductionCalendars.Resources.UpdateDialog_AsyncButton);
      dialog.Buttons.AddOkCancel();
      
      ayncButton.IsVisible = settings.CanAsyncUpdate.GetValueOrDefault();
      
      bool canParse = IsolatedFunctions.ExternalData.CanUseParse(defualtService?.DataSource.Value.Value);
      bool hasContent = false;
      
      #region Обработчики диалога.
      service.SetOnValueChanged(
        (args) =>
        {
          if (!Equals(args.NewValue, args.OldValue))
            canParse = args.NewValue?.DataSource != null ? IsolatedFunctions.ExternalData.CanUseParse(args.NewValue.DataSource.Value.Value) : false;
        });
      dialog.SetOnRefresh(
        (args) =>
        {
          html.IsVisible = canParse;
          hasContent = html.Value?.Content?.Length > 0;
          ayncButton.IsEnabled = !hasContent && args.IsValid;
        });
      #endregion
      
      dialog.SetOnButtonClick(
        (args) =>
        {
          var btn = args.Button;
          
          #region Ok.
          if (btn == DialogButtons.Ok)
          {
            var serviceValue = service.Value;
            settings.NeedSetPreHolidays = withPreHolidays.Value;
            
            var _logger = Logger.WithLogger(Constants.Module.LoggerPostfix)
              .WithProperty("Action", "UpdateFromExternalService");
            
            try
            {
              // Структура с массивом байт файла.
              Sungero.Docflow.Structures.Module.IByteArray structure = null;
              if (canParse && hasContent)
                structure = Sungero.Docflow.Structures.Module.ByteArray.Create(html.Value.Content);
              
              var data = Functions.Module.Remote.GetWeekendData(_obj.Year.GetValueOrDefault(), serviceValue, structure);
              Functions.Module.UpdateCalendar(_obj.WorkingTimeCalendar, data, settings);
              Functions.ProductionCalendar.UpdateProductionCalendar(_obj, data, service.Value);
            }
            catch (Exception ex)
            {
              _logger.Error(ex, "Ошибка обработки данных.");
              Dialogs.ShowMessage(ProductionCalendars.Resources.UpdateWeekends_ErrorFormat(ex.Message), MessageType.Error);
            }
          }
          #endregion
          
          #region АО.
          if (btn == ayncButton)
          {
            e.CloseFormAfterAction = true;
            
            var handler = AsyncHandlers.UpdateCalendar.Create();
            handler.ProductionCalendarId = _obj.Id;
            handler.ServiceId = service.Value.Id;
            handler.ExecuteAsync(ProductionCalendars.Resources.AsyncUpdate_CompleteNotification);
          }
          #endregion
        });
      
      dialog.Show();
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