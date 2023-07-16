using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ProductionCalendar.Service;

namespace Starkov.ProductionCalendar.Server
{
  partial class ServiceFunctions
  {

    #region Получение данных.
    /// <summary>
    /// Получить доступные сервисы.
    /// </summary>
    /// <param name="dataSource">Источник данных.</param>
    /// <returns>Доступные сервисы.</returns>
    [Public, Remote(IsPure = true)]
    public static IQueryable<IService> GetServices(Enumeration? dataSource)
    {
      var services = Services.GetAll(x => x.Status == Status.Active);
      if (dataSource.HasValue)
        services = services.Where(x => x.DataSource == dataSource);
      
      return services;
    }
    #endregion
    
    /// <summary>
    /// Создать сервис.
    /// </summary>
    /// <param name="name">Имя.</param>
    /// <param name="url">Адрес.</param>
    /// <param name="dataSource">Тип источника данных.</param>
    /// <param name="canUseApi">Использовать API.</param>
    /// <returns>Созданный сервис.</returns>
    [Public, Remote(IsPure = true)]
    public static IService CreateService(string name, string url, Enumeration dataSource, bool canUseApi)
    {
      if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(url))
        return Services.Null;
      
      var service = Services.Create();
      service.Name = name;
      service.Url = url;
      service.DataSource = dataSource;
      service.UseApi = canUseApi;
      
      try
      {
        service.Save();
        return service;
      }
      catch (Exception ex)
      {
        Logger.Error("Ошибка при создании сервиса доступа к производственным календарям", ex);
      }
      
      return Services.Null;
    }

  }
}