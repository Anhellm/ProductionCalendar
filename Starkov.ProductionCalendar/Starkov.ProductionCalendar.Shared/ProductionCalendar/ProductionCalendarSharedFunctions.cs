using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ProductionCalendar.ProductionCalendar;

namespace Starkov.ProductionCalendar.Shared
{
  partial class ProductionCalendarFunctions
  {
    /// <summary>
    /// Валидация значений диалога.
    /// </summary>
    /// <param name="args">Аргументы.</param>
    /// <param name="inputValueFrom">Значение начала.</param>
    /// <param name="inputValueTo">Значение окончания.</param>
    /// <param name="controls">Массив контролов.</param>
    public virtual void ChangeDialogValidateInputValue(CommonLibrary.InputDialogRefreshEventArgs args, DateTime? inputValueFrom, DateTime? inputValueTo, CommonLibrary.IDialogControl[] controls)
    {
      var year = _obj.Year.GetValueOrDefault();
      
      string error = string.Empty;
      if (inputValueFrom > inputValueTo)
        error = ProductionCalendars.Resources.DateComparisonDialog_Error;
      
      if ((inputValueFrom.HasValue && inputValueFrom.Value.Year != year) || (inputValueTo.HasValue && inputValueTo.Value.Year != year))
        error = ProductionCalendars.Resources.YearDialog_Error;
      
      if (!string.IsNullOrEmpty(error))
        args.AddError(error, controls);
    }
    
    /// <summary>
    /// Обновить производственный календарь.
    /// </summary>
    /// <param name="data">Данные из внешнего сервиса.</param>
    public virtual void UpdateProductionCalendar(Structures.Module.IWeekendData data, IService service)
    {
      if (data == null)
        return;
      
      SetPreHolidays(data.PreHolidays);
      _obj.HolidayInfo = data.HolidayInfo;
      _obj.UpdateInfo = ProductionCalendars.Resources.UpdateInfoFormat(service?.Name, Calendar.Now.ToString());
       Functions.ProductionCalendar.Remote.SerializeCalendar(_obj, false);
       
      _obj.Save();
    }
    
    #region Предпраздничные дни.
    /// <summary>
    /// Заполнить предпраздничные дни.
    /// </summary>
    /// <param name="dates">Список дат.</param>
    public virtual void SetPreHolidays(List<DateTime> dates)
    {
      _obj.PreHolidays.Clear();
      
      foreach (var date in dates)
        _obj.PreHolidays.AddNew().Date = date;
    }
    
    /// <summary>
    /// Получить список предпраздничных дней.
    /// </summary>
    /// <returns>Список предпраздничных дней.</returns>
    [Public]
    public virtual List<DateTime> GetPreHolidays()
    {
      return _obj.PreHolidays.Where(x => x.Date.HasValue).Select(x => x.Date.Value).ToList();
    }
    #endregion
    
    /// <summary>
    /// Заполнить имя календаря.
    /// </summary>
    public void FillName()
    {
      string privateCalendarLabel = _obj.IsPrivate == true ? ProductionCalendars.Resources.PrivateLabel_Name : " ";
      _obj.Name = ProductionCalendars.Resources.NameTemplateFormat(privateCalendarLabel, _obj.Year);
    }
  }
}