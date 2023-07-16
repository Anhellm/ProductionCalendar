using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ProductionCalendar.ProductionCalendar;

namespace Starkov.ProductionCalendar
{
  partial class ProductionCalendarClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      if (!string.IsNullOrEmpty(_obj.UpdateInfo))
        e.AddInformation(_obj.UpdateInfo);
    }

  }
}