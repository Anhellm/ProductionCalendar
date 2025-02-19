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
        service.Logger = logger;
        
        return service.GetWeekendInfo();
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

  }
}