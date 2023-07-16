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
    public virtual void UpdateWeekends(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var dialog = Dialogs.CreateInputDialog("Параметры");
      var service = dialog.AddSelect("Сервис", true, Functions.Service.Remote.GetPriorityService());
      var withPreHolidays = dialog.AddBoolean("Обработать предпраздничные дни", false);
      
      if (dialog.Show() == DialogButtons.Ok)
      {
        var serviceValue = service.Value;
        var data = Functions.Module.Remote.GetWeekendData(_obj.Year.GetValueOrDefault(), serviceValue);
        
        Functions.Module.UpdateCalendar(_obj.WorkingTimeCalendar, data, withPreHolidays.Value.GetValueOrDefault());
        _obj.HolidayInfo = data.HolidayInfo;
        _obj.UpdateInfo = string.Format("Данные получены из сервиса {0}: {1}", serviceValue.Name, Calendar.Now.ToString());
        _obj.Save();
      }
    }

    public virtual bool CanUpdateWeekends(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.WorkingTimeCalendar?.AccessRights?.CanUpdate() ?? false;
    }

  }

}