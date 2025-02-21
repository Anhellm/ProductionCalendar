using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ProductionCalendar.CalendarSettings;

namespace Starkov.ProductionCalendar
{
  partial class CalendarSettingsServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      var structure = Functions.CalendarSettings.GetUpdateSettings(_obj);
      
      var validate = Functions.Module.Validate(structure);
      if (!string.IsNullOrEmpty(validate))
        e.AddError(validate);
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Name = CalendarSettingses.Resources.CalendarSettingsName;
      _obj.CanAsyncUpdate = false;
    }
  }

}