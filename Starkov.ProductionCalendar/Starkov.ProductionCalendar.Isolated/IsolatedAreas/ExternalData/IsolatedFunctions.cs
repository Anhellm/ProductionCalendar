using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sungero.Core;
using Starkov.ProductionCalendar.Structures.Module;

namespace Starkov.ProductionCalendar.Isolated.ExternalData
{
  public class IsolatedFunctions
  {

    /// <summary>
    /// Получить структуру с данными по выходным/праздникам.
    /// </summary>
    /// <param name="requestParams">Параметры запроса.</param>
    /// <param name="loggerPostfix">Постфикс для логгера.</param>
    /// <returns>Структура данных.</returns>
    [Public]
    public virtual Structures.Module.IWeekendData GetWeekendData(Structures.Module.IRequestParams requestParams, string loggerPostfix)
    {
      var logger = Logger.WithLogger(loggerPostfix);
      
      try
      {
        logger.WithObject(requestParams).Debug("Запрос данных внешнего сервиса.");
        
        var service = GetService(requestParams);
        service.ServiceLogger = logger;
        
        var result = service.GetWeekendInfo();
        if (result == null)
          throw new InvalidOperationException("Получен пустой набор данных.");
        
        return result;
      }
      catch (Exception ex)
      {
        logger.Error(ex);
        throw;
      }
    }
    
    /// <summary>
    /// Получить сервис.
    /// </summary>
    /// <param name="requestParams">Параметры запроса.</param>
    /// <returns>Сервис.</returns>
    public virtual IService GetService(Structures.Module.IRequestParams requestParams)
    {
      switch (requestParams.ServiceName)
      {
        case "XMLCalendar":
          return new XMLCalendar(requestParams.UrlTemplate, requestParams.Year, requestParams.ByApi);
        case "Consultant":
          return new ConsultantPlus(requestParams.UrlTemplate, requestParams.Year);
        case "HeadHunter":
          return new HeadHunter(requestParams.UrlTemplate, requestParams.Year);
        default:
          throw new NotImplementedException("Сервис не определен.");
      }
    }
    
    #region Проверка доступности реализации.
    /// <summary>
    /// Возможность изменения реализации для сервиса.
    /// </summary>
    /// <param name="name">Наименование сервиса.</param>
    /// <returns>True/False.</returns>
    [Public]
    public virtual bool CanSwitchImplementation(string name)
    {
      var type = GetServiceType(name);
      return CanUseApi(type) && CanUseParse(type);
    }
    
    /// <summary>
    /// Возможность использования сервиса через API.
    /// </summary>
    /// <param name="name">Наименование сервиса.</param>
    /// <returns>True/False.</returns>
    [Public]
    public virtual bool CanUseApi(string name)
    {
      var type = GetServiceType(name);
      return CanUseApi(type);
    }
    
    /// <summary>
    /// Возможность использования сервиса через API.
    /// </summary>
    /// <param name="serviceType">Тип.</param>
    /// <returns>True/False.</returns>
    public virtual bool CanUseApi(Type serviceType)
    {
      return serviceType?.GetInterfaces().Any(x => Equals(x, typeof(IApiService))) ?? false;
    }
    
    /// <summary>
    /// Возможность использования сервиса через парсер.
    /// </summary>
    /// <param name="name">Наименование сервиса.</param>
    /// <returns>True/False.</returns>
    [Public]
    public virtual bool CanUseParse(string name)
    {
      var type = GetServiceType(name);
      return CanUseParse(type);
    }
    
    /// <summary>
    /// Возможность использования сервиса через парсер.
    /// </summary>
    /// <param name="serviceType">Тип.</param>
    /// <returns>True/False.</returns>
    public virtual bool CanUseParse(Type serviceType)
    {
      return serviceType?.GetInterfaces().Any(x => Equals(x, typeof(IParseService))) ?? false;
    }
    
    /// <summary>
    /// Получить тип по наименованию.
    /// </summary>
    /// <param name="name">Наименование сервиса.</param>
    /// <returns>Тип.</returns>
    public virtual Type GetServiceType(string name)
    {
      switch (name)
      {
        case "XMLCalendar":
          return typeof(XMLCalendar);
        case "Consultant":
          return typeof(ConsultantPlus);
        case "HeadHunter":
          return typeof(HeadHunter);
        default:
          return null;
      }
    }
    #endregion
  }
}