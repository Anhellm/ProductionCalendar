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
      
      _obj.State.Controls.CalendarState.Refresh();
      Functions.ProductionCalendar.FillName(_obj);
    }

  }
}