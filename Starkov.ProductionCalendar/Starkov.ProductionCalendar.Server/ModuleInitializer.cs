using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace Starkov.ProductionCalendar.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      Sungero.Commons.PublicInitializationFunctions.Module.ModuleVersionInit(this.FirstInitializing, Constants.Module.Init.ProdCalendar.Name, Version.Parse(Constants.Module.Init.ProdCalendar.FirstInitVersion));
    }
    
    /// <summary>
    /// Начальная инициализация модуля после установки.
    /// </summary>
    public virtual void FirstInitializing()
    {
      CreateDefaultServices();
      CreateDefaultSettings();
      
      GrantDefaultAccessRights();
    }
    
    #region Сервисы.
    /// <summary>
    /// Создать сервисы доступа по-умолчанию.
    /// </summary>
    public virtual void CreateDefaultServices()
    {
      InitializationLogger.Debug("Init ProductionCalendar: Create default services.");
      
      CreateDefaultService(Services.Resources.Consultant_DefaultName, Services.Resources.Consultant_DefaultURL,
                           Starkov.ProductionCalendar.Service.DataSource.Consultant, false);
      CreateDefaultService(Services.Resources.HeadHunter_DefaultName, Services.Resources.HeadHunter_DefaultURL,
                           Starkov.ProductionCalendar.Service.DataSource.HeadHunter, false);
      CreateDefaultService(Services.Resources.XMLCalendar_DefaultName, Services.Resources.XMLCalendarDefaultURL,
                           Starkov.ProductionCalendar.Service.DataSource.XMLCalendar, true);
    }
    
    /// <summary>
    /// Создать сервис по-умолчанию.
    /// </summary>
    /// <param name="name">Имя.</param>
    /// <param name="url">Адрес.</param>
    /// <param name="dataSource">Тип источника данных.</param>
    /// <param name="canUseApi">Использовать API.</param>
    public virtual void CreateDefaultService(string name, string url, Enumeration dataSource, bool canUseApi)
    {
      if (!Functions.Service.GetServices(dataSource).Any())
        Functions.Service.CreateService(name, url, dataSource, canUseApi);
    }
    #endregion
    
    /// <summary>
    /// Создать настройки по умолчанию.
    /// </summary>
    public virtual void CreateDefaultSettings()
    {
      InitializationLogger.Debug("Init ProductionCalendar: Create default settings.");
      
      if (Functions.CalendarSettings.GetSettings() != null)
        return;
      
      Functions.CalendarSettings.CreateDefaultSettings();
    }
    
    /// <summary>
    /// Назначить права доступа по-умолчанию.
    /// </summary>
    public virtual void GrantDefaultAccessRights()
    {
      InitializationLogger.Debug("Init ProductionCalendar: Grant default access rignts.");
      
      var all = Roles.AllUsers;
      
      // Справочник Производственные календари.
      var calendarAccessRights = ProductionCalendars.AccessRights;
      calendarAccessRights.Grant(all, DefaultAccessRightsTypes.Read);
      calendarAccessRights.Save();
    }
  }
}
