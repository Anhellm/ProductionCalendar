using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ProductionCalendar.ProductionCalendar;

namespace Starkov.ProductionCalendar
{
  partial class ProductionCalendarCreatingFromServerHandler
  {

    public override void CreatingFrom(Sungero.Domain.CreatingFromEventArgs e)
    {
      e.Without(_info.Properties.WorkingTimeCalendar);
      e.Without(_info.Properties.Name);
      e.Without(_info.Properties.IsPrivate);
    }
  }

  partial class ProductionCalendarServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      // Дубли.
      var duplicates = Functions.ProductionCalendar.GetCalendars()
        .Where(x => Equals(x.WorkingTimeCalendar, _obj.WorkingTimeCalendar))
        .Where(x => !Equals(x, _obj));
      if (duplicates.Any())
        e.AddError(ProductionCalendars.Resources.Duplicate_Error);
      
      // Проверка предпраздничных дней.
      if (Functions.ProductionCalendar.GetPreHolidays(_obj).Any(x => x.Year != _obj.Year))
        e.AddError(ProductionCalendars.Resources.PreHolidayInput_ErrorFormat(_obj.Year));
    }
  }

}