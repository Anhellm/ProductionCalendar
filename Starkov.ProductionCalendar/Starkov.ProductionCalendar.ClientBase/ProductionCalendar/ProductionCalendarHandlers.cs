using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ProductionCalendar.ProductionCalendar;

namespace Starkov.ProductionCalendar
{
  partial class ProductionCalendarPreHolidaysClientHandlers
  {

    public virtual void PreHolidaysDateValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {
      var year = _obj.ProductionCalendar.Year;
      if (e.NewValue.HasValue && e.NewValue.Value.Year != year)
        e.AddError(ProductionCalendars.Resources.PreHolidayInput_ErrorFormat(year), e.Property);
    }
  }

  partial class ProductionCalendarClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      _obj.State.Properties.WorkingTimeCalendar.IsEnabled = _obj.WorkingTimeCalendar == null;
    }

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      if (!string.IsNullOrEmpty(_obj.UpdateInfo))
        e.AddInformation(_obj.UpdateInfo);
    }

  }
}