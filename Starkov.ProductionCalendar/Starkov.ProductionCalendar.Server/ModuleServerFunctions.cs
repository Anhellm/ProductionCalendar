using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace Starkov.ProductionCalendar.Server
{
  public class ModuleFunctions
  {

    #region Основные календари рабочего времени
    /// <summary>
    /// Получить основные календари рабочего времени.
    /// </summary>
    /// <returns>Основные календари рабочего времени.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IQueryable<IWorkingTimeCalendar> GetWorkingTimeCalendars()
    {
      return WorkingTime.GetAll(x => !PrivateWorkingTimeCalendars.Is(x));
    }
    
    /// <summary>
    /// Получить основные календари рабочего времени.
    /// </summary>
    /// <returns>Основные календари рабочего времени.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IWorkingTimeCalendar CreateWorkingTimeCalendar()
    {
      if (!WorkingTime.AccessRights.CanCreate())
        throw AppliedCodeException.Create(Resources.AccessRightsCreate_Error);
      
      return WorkingTime.Create();
    }
    #endregion
  }
}