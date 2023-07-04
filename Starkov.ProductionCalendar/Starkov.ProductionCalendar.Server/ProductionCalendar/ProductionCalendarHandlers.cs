using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ProductionCalendar.ProductionCalendar;

namespace Starkov.ProductionCalendar
{
  partial class ProductionCalendarServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      var duplicate = Functions.ProductionCalendar.GetCalendar(_obj.WorkingTimeCalendar);
      if (duplicate != null)
        e.AddError(string.Format("Представление для календаря уже существует {0}", Hyperlinks.Get(duplicate)));
    }
  }

}