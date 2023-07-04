using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ProductionCalendar.ProductionCalendar;

namespace Starkov.ProductionCalendar
{
  partial class ProductionCalendarSharedHandlers
  {

    public virtual void WorkingTimeCalendarChanged(Starkov.ProductionCalendar.Shared.ProductionCalendarWorkingTimeCalendarChangedEventArgs e)
    {
      if (Equals(e.NewValue, e.OldValue))
        return;
      
      _obj.IsPrivate = PrivateWorkingTimeCalendars.Is(e.NewValue);
      _obj.Year = e.NewValue?.Year;
      
      Functions.ProductionCalendar.FillName(_obj);
      _obj.State.Controls.CalendarState.Refresh();
    }

  }
}