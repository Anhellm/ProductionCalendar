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
    /// Получить основной календарь рабочего времени.
    /// </summary>
    /// <param name="year">Год.</param>
    /// <returns>Календарь рабочего времени за год.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IWorkingTimeCalendar GetWorkingTimeCalendar(int year)
    {
      return GetWorkingTimeCalendars().Where(x => x.Year == year).FirstOrDefault();
    }
    
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
    
    /// <summary>
    /// Получить структуру с данными по выходным.
    /// </summary>
    /// <param name="year">Год.</param>
    /// <param name="service">Сервис.</param>
    /// <param name="htmlContent">HTMl - файл.</param>
    /// <returns>Структура с данными по выходным.</returns>
    /// <remarks>htmlContent - если null то используется либо апи либо парситься сам сайт, если заполнено парситься именно файл.</remarks>
    [Remote]
    public virtual Structures.Module.IWeekendData GetWeekendData(int year, IService service, Sungero.Docflow.Structures.Module.IByteArray htmlContent)
    {
      var _logger = Logger.WithLogger(Constants.Module.LoggerPostfix)
        .WithProperty("Function", "GetWeekendData");
      
      if (service == null)
      {
        _logger.Error("Получен пустой сервис.");
        return null;
      }
      
      var requestParams = GetRequestParams(year, service);
      
      Structures.Module.IWeekendData result;
      try
      {
        if (htmlContent != null)
        {
          using (var stream = new System.IO.MemoryStream(htmlContent.Bytes))
            result = IsolatedFunctions.ExternalData.GetWeekendData(requestParams, stream, Constants.Module.LoggerPostfix);
        }
        else
          result = IsolatedFunctions.ExternalData.GetWeekendData(requestParams, Constants.Module.LoggerPostfix);
      }
      catch
      {
        throw AppliedCodeException.Create("Неопознанная ошибка. Обратитесь к администратору.");
      }
      
      return result;
    }

    /// <summary>
    /// Получить класс для получения доступа к ресурсам.
    /// </summary>
    /// <param name="year">Год.</param>
    /// <param name="service">Сервис.</param>
    /// <returns>Класс для получения доступа к ресурсам.</returns>
    public virtual Structures.Module.IRequestParams GetRequestParams(int year, IService service)
    {
      if (service == null || !service.DataSource.HasValue)
        return null;
      
      var structure = Structures.Module.RequestParams.Create();
      structure.ServiceName = service.DataSource.Value.Value;
      structure.UrlTemplate = service.Url;
      structure.Year = year;
      structure.ByApi = service.UseApi.GetValueOrDefault();
      
      return structure;
    }
  }
}