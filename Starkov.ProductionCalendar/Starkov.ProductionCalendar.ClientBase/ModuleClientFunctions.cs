using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace Starkov.ProductionCalendar.Client
{
  public class ModuleFunctions
  {

    #region Обложка.
    /// <summary>
    /// Открыть настройки календарей.
    /// </summary>
    public virtual void ShowCalendarSettings()
    {
      var settings = Functions.CalendarSettings.Remote.GetSettings();
      if (settings != null)
        settings.Show();
    }
    
    /// <summary>
    /// Показать основные производственные календари.
    /// </summary>
    [Public]
    public virtual void ShowMainCalendars()
    {
      Functions.ProductionCalendar.Remote.GetCalendars().Show();
    }
    
    /// <summary>
    /// Показать частные производственные календари.
    /// </summary>
    [Public]
    public virtual void ShowPrivateCalendars()
    {
      Functions.ProductionCalendar.Remote.GetPrivateCalendars().Show();
    }
    
    /// <summary>
    /// Открыть или создать основной производственный календарь на текущий год.
    /// </summary>
    [Public]
    public virtual void ShowOrCreateCurrentYearCalendar()
    {
      ShowOrCreateMainCalendar(Calendar.Today.Year);
    }
    
    /// <summary>
    /// Открыть или создать основной производственный календарь на следующий год.
    /// </summary>
    [Public]
    public virtual void ShowOrCreateNextYearCalendar()
    {
      ShowOrCreateMainCalendar(Calendar.Today.Year + 1);
    }

    /// <summary>
    /// Создать частный производственный календарь.
    /// </summary>
    public virtual void CreatePrivateCalendar()
    {
      var privateCalendar = PrivateWorkingTimeCalendars.GetAll().ShowSelect();
      if (privateCalendar == null)
        return;
      
      // Ищем существующий календарь, иначе создаем.
      var productionCalendar = Functions.ProductionCalendar.Remote.GetCalendar(privateCalendar);
      if (productionCalendar == null)
      {
        if (!ProductionCalendars.AccessRights.CanCreate())
        {
          Dialogs.ShowMessage(Resources.AccessRightsCreate_Error, MessageType.Warning);
          return;
        }
        
        productionCalendar = Functions.ProductionCalendar.Remote.CreateCalendar(privateCalendar);
      }
      
      if (productionCalendar == null)
      {
        Dialogs.ShowMessage(Resources.NullCalendar_Error, MessageType.Warning);
        return;
      }
      
      productionCalendar.Show();
    }
    #endregion
    
    /// <summary>
    /// Открыть или создать основной производственный календарь на текущий год.
    /// </summary>
    /// <param name="year">Год.</param>
    [Public]
    public virtual void ShowOrCreateMainCalendar(int year)
    {
      var calendar = Functions.ProductionCalendar.Remote.GetCalendar(year);
      if (calendar != null)
      {
        calendar.Show();
        return;
      }
      
      // Находим основной рабочий календарь, иначе создаем.
      var workingTimeCalendar = Functions.Module.Remote.GetWorkingTimeCalendars()
        .Where(x => x.Year == year)
        .FirstOrDefault();
      
      if (workingTimeCalendar == null)
      {
        try
        {
          workingTimeCalendar = Functions.Module.Remote.CreateWorkingTimeCalendar();
        }
        catch (Exception ex)
        {
          Dialogs.ShowMessage(Resources.CalendarCreate_ErrorFormat(ex.Message), MessageType.Error);
          return;
        }
        
        workingTimeCalendar.Year = year;
        workingTimeCalendar.Show();
        
        if (workingTimeCalendar.State.IsInserted)
        {
          Dialogs.ShowMessage(Resources.CalendarNotSaved_Message, MessageType.Warning);
          return;
        }
      }
      
      var productionCalendar = Functions.ProductionCalendar.Remote.CreateCalendar(workingTimeCalendar);
      if (productionCalendar == null)
      {
        Dialogs.ShowMessage(Resources.NullCalendar_Error, MessageType.Warning);
        return;
      }
      
      productionCalendar.Show();
    }
  }
}