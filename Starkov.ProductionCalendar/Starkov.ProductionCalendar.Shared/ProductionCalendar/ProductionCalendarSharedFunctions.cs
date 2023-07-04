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

    public void FillName()
    {
      _obj.Name = string.Format("Производственный календарь на {0} год", _obj.WorkingTimeCalendar?.Year);
    }
  }
}