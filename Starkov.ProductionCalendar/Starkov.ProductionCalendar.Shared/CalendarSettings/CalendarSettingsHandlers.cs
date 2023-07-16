using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ProductionCalendar.CalendarSettings;

namespace Starkov.ProductionCalendar
{
  partial class CalendarSettingsSharedHandlers
  {

    public virtual void LunchBreakEndingChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      Functions.CalendarSettings.SetPropertiesState(_obj);
    }

    public virtual void LunchBreakBeginningChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      Functions.CalendarSettings.SetPropertiesState(_obj);
    }

  }
}