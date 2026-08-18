using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sungero.Core;
using Starkov.ProductionCalendar.Structures.Module;

namespace Starkov.ProductionCalendar.Isolated.Serialize
{

  public static class Serializer
  {
    /// <summary>
    /// Сериализовать данные в строку.
    /// </summary>
    /// <param name="entity">Сущность для сериализации.</param>
    /// <returns>Json-строка.</returns>
    public static string Serialize(object entity, bool withTypeNameHandling)
    {
      if (entity == null)
        throw AppliedCodeException.Create("Передано пустое значение.");

      var settings = new JsonSerializerSettings();
      settings.Formatting = Formatting.Indented;
      if (withTypeNameHandling)
        settings.TypeNameHandling = TypeNameHandling.Auto;
      
      try
      {
        return JsonConvert.SerializeObject(entity, settings);
      }
      catch
      {
        throw;
      }
    }
  }
  
  /// <summary>
  /// Структура для описания даты.
  /// </summary>
  public struct DateInfo
  {
    public DateTime Date;
    public DateType Type;
    
    public DateInfo(DateTime date, DateType type)
    {
      Date = date;
      Type = type;
    }
  }
  
  /// <summary>
  /// Перечисление с типом дат.
  /// </summary>
  public enum DateType
  {
    Empty,
    Work,
    Weekend,
    Holiday,
    Preholiday,
  }
}