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
      CreateDefaultServices();
      
      GrantDefaultAccessRights();
    }
    
    /// <summary>
    /// Создать сервисы доступа по-умолчанию.
    /// </summary>
    public virtual void CreateDefaultServices()
    {
      CreateDefaultService(Services.Resources.Consultant_DefaultName, Services.Resources.Consultant_DefaultURL,
                           Starkov.ProductionCalendar.Service.DataSource.Consultant, true, false);
      CreateDefaultService(Services.Resources.HeadHunter_DefaultName, Services.Resources.HeadHunter_DefaultURL,
                           Starkov.ProductionCalendar.Service.DataSource.HeadHunter, false, false);
    }
    
    /// <summary>
    /// Создать сервис по-умолчанию.
    /// </summary>
    /// <param name="name">Имя.</param>
    /// <param name="url">Адрес.</param>
    /// <param name="dataSource">Тип источника данных.</param>
    /// <param name="isPriority">Приоритетная.</param>
    /// <param name="canUseApi">Использовать API.</param>
    public virtual void CreateDefaultService(string name, string url, Enumeration dataSource, bool isPriority, bool canUseApi)
    {
      if (!Functions.Service.GetServices(dataSource).Any())
        Functions.Service.CreateService(name, url, dataSource, isPriority, canUseApi);
    }
    
    /// <summary>
    /// Назначить права доступа по-умолчанию.
    /// </summary>
    public virtual void GrantDefaultAccessRights()
    {
      var all = Roles.AllUsers;
      
      // Справочник Производственные календари.
      var calendarAccessRights = ProductionCalendars.AccessRights;
      calendarAccessRights.Grant(all, DefaultAccessRightsTypes.Read);
      calendarAccessRights.Save();
    }
  }
}
