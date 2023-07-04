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

    /// <summary>
    /// Заполнить имя календаря.
    /// </summary>
    public void FillName()
    {
      string privateCalendarLabel = _obj.IsPrivate == true ? ProductionCalendars.Resources.PrivateLabel_Name : " ";
      _obj.Name = ProductionCalendars.Resources.NameTemplateFormat(privateCalendarLabel, _obj.Year);
    }
  }
}