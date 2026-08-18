using System;
using Sungero.Core;

namespace Starkov.ProductionCalendar.Constants
{
  public static class Module
  {

    /// <summary>
    /// Постфикс для логирования модуля.
    /// </summary>
    public const string LoggerPostfix = "CalendarService";

    /// <summary>
    /// Константы инициализации модуля.
    /// </summary>
    public static class Init
    {
      public static class ProdCalendar
      {
        /// <summary>
        /// Название параметра для хранения проинициализированной версии кода модуля.
        /// </summary>
        public const string Name = "InitProdCalendarUpdate";
        
        public const string FirstInitVersion = "4.12.0.0";
      }
    }
  }
}