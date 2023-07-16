using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ProductionCalendar.CalendarSettings;

namespace Starkov.ProductionCalendar.Shared
{
  partial class CalendarSettingsFunctions
  {

    /// <summary>
    /// Задать состояние свойств.
    /// </summary>       
    public virtual void SetPropertiesState()
    {
      var properties = _obj.State.Properties;
      
      properties.LunchBreakBeginning.IsRequired = _obj.LunchBreakEnding.HasValue;
      properties.LunchBreakEnding.IsRequired = _obj.LunchBreakBeginning.HasValue;
    }

  }
}