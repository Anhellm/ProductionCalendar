using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sungero.Core;
using Starkov.ProductionCalendar.Structures.Module;
using System.Xml.Serialization;
using System.Globalization;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace Starkov.ProductionCalendar.Isolated.ExternalData
{
  public delegate Structures.Module.IWeekendData GetWeekendData();
  public delegate Structures.Module.IWeekendData CreateWeekendData(IHtmlCollection<IElement> months, IElement holidayInfo);
  
  #region Интерфейсы.
  /// <summary>
  /// Базовый интерфейс сервиса получения данных.
  /// </summary>
  public interface IService
  {
    /// <summary>
    /// Логгер.
    /// </summary>
    ILogger Logger { get; set; }
    
    /// <summary>
    /// Адрес доступа.
    /// </summary>
    string Url { get; }
    
    /// <summary>
    /// Получить информацию по выходным/праздничным дням.
    /// </summary>
    /// <returns>Структура с информацией по выходным/праздничным дням.</returns>
    Structures.Module.IWeekendData GetWeekendInfo();
  }
  
  /// <summary>
  /// Базовый интерфейс получения данных через API.
  /// </summary>
  public interface IApiService : IService
  {
    /// <summary>
    /// Тип содержимого для заголовка.
    /// </summary>
    string ContentType { get; }
    
    /// <summary>
    /// Получить информацию по выходным/праздничным дням.
    /// </summary>
    /// <returns>Структура с информацией по выходным/праздничным дням.</returns>
    Structures.Module.IWeekendData Get();
  }
  
  /// <summary>
  /// Базовый интерфейс получения данных через парсинг html-страницы.
  /// </summary>
  public interface IParseService : IService
  {
    /// <summary>
    /// Селектор для месяцев.
    /// </summary>
    string MonthsSelector { get; set; }
    
    /// <summary>
    /// Селектор для праздников.
    /// </summary>
    string HolidaySelector { get; set; }
    
    /// <summary>
    /// Получить информацию по выходным/праздничным дням.
    /// </summary>
    /// <returns>Структура с информацией по выходным/праздничным дням.</returns>
    Structures.Module.IWeekendData Parse();
    
    /// <summary>
    /// Создать структура с информацией по выходным/праздничным дням.
    /// </summary>
    /// <param name="months">Список элементов с данными по месяцам.</param>
    /// <param name="holidayInfo">Элемент с информацией о праздниках.</param>
    /// <returns>Структура с информацией по выходным/праздничным дням.</returns>
    Structures.Module.IWeekendData CreateWeekendData(IHtmlCollection<IElement> months, IElement holidayInfo);
  }
  
  /// <summary>
  /// Интерфейс для классов конвертируемых в структуру с информацией по выходным/праздничным дням.
  /// </summary>
  public interface IConvertable
  {
    /// <summary>
    /// Конвертировать данные класса в структуру с информацией по выходным/праздничным дням.
    /// </summary>
    /// <returns>Структура с информацией по выходным/праздничным дням.</returns>
    Structures.Module.IWeekendData Convert();
  }
  
  /// <summary>
  /// Интерфейс для классов с внешней реализацией конвертации в структуру с информацией по выходным/праздничным дням.
  /// </summary>
  public interface IExtConvertable : IConvertable
  {
    /// <summary>
    /// Валидация данных класса.
    /// </summary>
    /// <returns>Строка с ошибкой или пустая строка.</returns>
    string Validate();
    
    /// <summary>
    /// Внешняя реализация конвертации в структуру с информацией по выходным/праздничным дням.
    /// </summary>
    CreateWeekendData ConvertImplementation { get; }
  }
  #endregion
  
  #region Классы для промежуточной обработки.
  /// <summary>
  /// Результат парсинга страницы.
  /// </summary>
  internal class HTMLMonthsInfo : IExtConvertable
  {    
    /// <summary>
    /// Элементы месяца.
    /// </summary>
    internal IHtmlCollection<IElement> Months { get; set; }

    /// <summary>
    /// Инфо о праздниках.
    /// </summary>
    internal IElement HolidaysInfo { get; set; }

    /// <summary>
    /// Реализация преобразования данных в структуру с результатом.
    /// </summary>
    public CreateWeekendData ConvertImplementation { get; private set; }
    
    /// <summary>
    /// Валидация полученного набора данных.
    /// </summary>
    /// <returns>Строка с ошибкой или пустая строка.</returns>
    public virtual string Validate()
    {
      if (Months == null)
        return "Данные по месяцам не определены.";

      if (Months.Length != 12)
        return string.Format("Получено некорректное количество месяцев - {0}", Months.Length);

      return string.Empty;
    }
    
    /// <summary>
    /// Конвертировать данные класса в структуру с информацией по выходным/праздничным дням.
    /// </summary>
    /// <returns>Структура с информацией по выходным/праздничным дням.</returns>
    public Structures.Module.IWeekendData Convert()
    {
      return ConvertImplementation?.Invoke(Months, HolidaysInfo);
    }
    
    internal HTMLMonthsInfo(IHtmlCollection<IElement> months, IElement holidaysInfo, CreateWeekendData implementation)
    {
      Months = months;
      HolidaysInfo = holidaysInfo;
      ConvertImplementation = implementation;
    }
  }
  
  #region XMLCalendar.
  /// <summary>
  /// Общий класс календаря.
  /// </summary>
  [XmlRoot(ElementName = "calendar")]
  [Serializable()]
  public class XMLC_Calendar : IConvertable
  {
    /// <summary>
    /// Год.
    /// </summary>
    [XmlAttribute("year")]
    public int Year { get; set; }

    /// <summary>
    /// Названия праздников.
    /// </summary>
    [XmlElement("holidays")]
    public XMLC_Holidays Holidays { get; set; }

    /// <summary>
    /// Массив дней  в виде строк (праздники/короткие дни/рабочие дни).
    /// </summary>
    [XmlElement("days")]
    public XMLC_Days Days { get; set; }
    
    /// <summary>
    /// Преобразовать данные класса в выходную структуру.
    /// </summary>
    /// <returns>Структура с выходными/праздничными днями.</returns>
    public Structures.Module.IWeekendData Convert()
    {
      var result = Structures.Module.WeekendData.Create();
      
      try
      {
        result.HolidayInfo = GetHolidayInfo();
        FillDays(result);
      }
      catch
      {
        throw;
      }
      
      return result;
    }
    
    /// <summary>
    /// Заполнить даты в структуре.
    /// </summary>
    /// <param name="structure">Структура с данными.</param>
    private void FillDays(Structures.Module.IWeekendData structure)
    {
      var dayInfos = GetDayInfo();
      
      for (byte i = 1; i <= 12; i++)
      {
        var days = dayInfos.Where(x => x.Date.Month == i);
        
        // Предпраздничные дни.
        var preHolidays = days.Where(x => x.Type == 2).Select(x => x.Date);
        structure.PreHolidays.AddRange(preHolidays);

        // Выходные дни.
        var beginnigOfMonth = new DateTime(Year, i, 1);
        var allWeekends = CommonFunctions.GetWeekends(beginnigOfMonth, beginnigOfMonth.AddDays(DateTime.DaysInMonth(Year, i)));
        
        var weekends = allWeekends.Where(x => !days.Select(d => d.Date).Contains(x));
        structure.Weekends.AddRange(weekends);
        
        // Праздничные дни.
        var holidays = days.Where(x => x.Type == 1).Select(x => x.Date);
        structure.Holidays.AddRange(holidays);
      }
    }
    
    /// <summary>
    /// Получить текстовое представление по праздникам.
    /// </summary>
    /// <returns>Текстовое представление по праздникам.</returns>
    private string GetHolidayInfo()
    {
      var stringBuilder = new System.Text.StringBuilder();
      foreach (var holiday in Holidays.Holiday)
      {
        var days = Days.Day.Where(x => x.HolidayId == holiday.Id).Select(x => x.Date);
        var info = string.Format("{0} - {1}", string.Join(", ", days), holiday.Title);
        stringBuilder.AppendLine(info);
      }

      return stringBuilder.ToString().Trim();
    }
    
    /// <summary>
    /// Получить данные по датам в виде массива структур.
    /// </summary>
    /// <returns>Массив данных с датой и типом (выходной/праздник).</returns>
    DayInfo[] GetDayInfo()
    {
      return Days.Day
        .Select(x =>
                {
                  var dayInfo = new DayInfo();
                  
                  dayInfo.Type = x.Type;
                  dayInfo.Date = DateTime.TryParseExact($"{x.Date}.{Year}", "MM.dd.yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime date) ?
                    date :
                    DateTime.MinValue;
                  
                  return dayInfo;
                })
        .Where(x => x.Date > DateTime.MinValue)
        .ToArray();
    }
    
    /// <summary>
    /// Структура описания дня.
    /// </summary>
    struct DayInfo
    {
      /// <summary>
      /// Дата.
      /// </summary>
      internal DateTime Date { get; set; }

      /// <summary>
      /// Тип дня.
      /// </summary>
      /// <remarks>1 - выходной день, 2 - рабочий и сокращенный (может быть использован для любого дня недели), 3 - рабочий день (суббота/воскресенье)</remarks>
      internal byte Type { get; set; }
    }
  }

  /// <summary>
  /// Класс описания названий праздников.
  /// </summary>
  [XmlRoot()]
  [Serializable()]
  public class XMLC_Holidays
  {
    /// <summary>
    /// Названия праздников.
    /// </summary>
    [XmlElement("holiday")]
    public XMLC_Holiday[] Holiday { get; set; }
  }

  /// <summary>
  /// Класс описания дней.
  /// </summary>
  [XmlRoot()]
  [Serializable()]
  public class XMLC_Days
  {
    /// <summary>
    /// Массив дней  в виде строк (праздники/короткие дни/рабочие дни).
    /// </summary>
    [XmlElement("day")]
    public XMLC_Day[] Day { get; set; }
  }

  /// <summary>
  /// Класс описания праздника.
  /// </summary>
  [XmlType("holiday")]
  [Serializable()]
  public class XMLC_Holiday
  {
    /// <summary>
    /// Ид.
    /// </summary>
    [XmlAttribute("id")]
    public byte Id { get; set; }

    /// <summary>
    /// Название праздника.
    /// </summary>
    [XmlAttribute("title")]
    public string Title { get; set; }
  }

  /// <summary>
  /// Класс описания дня.
  /// </summary>
  [XmlType("day")]
  [Serializable()]
  public class XMLC_Day
  {
    /// <summary>
    /// Тип дня.
    /// </summary>
    /// <remarks>1 - выходной день, 2 - рабочий и сокращенный (может быть использован для любого дня недели), 3 - рабочий день (суббота/воскресенье)</remarks>
    [XmlAttribute("t")]
    public byte Type { get; set; }

    /// <summary>
    /// День в формате ММ.ДД.
    /// </summary>
    [XmlAttribute("d")]
    public string Date { get; set; }

    /// <summary>
    /// Номер праздника (ссылка на атрибут id тэга holiday)
    /// </summary>
    [XmlAttribute("h")]
    public byte HolidayId { get; set; }
  }
  #endregion
  #endregion
  
  #region Базовые классы.
  /// <summary>
  /// Базовая реализация сервиса.
  /// </summary>
  public abstract class ServiceBase : IService
  {
    /// <summary>
    /// Логгер.
    /// </summary>
    public virtual ILogger Logger { get; set; }
    
    /// <summary>
    /// Адрес доступа.
    /// </summary>
    public virtual string Url { get; private set; }
    
    /// <summary>
    /// Получить информацию по выходным/праздничным дням.
    /// </summary>
    /// <returns>Структура с информацией по выходным/праздничным дням.</returns>
    public abstract Structures.Module.IWeekendData GetWeekendInfo();
    
    /// <summary>
    /// Конструктор по умолчанию.
    /// </summary>
    /// <param name="urlTemplate">Шаблон для адреса.</param>
    /// <param name="year">Год.</param>
    public ServiceBase(string urlTemplate, int year)
    {
      Url = string.Format(urlTemplate, year);
      Logger = Logger.WithLogger("CalendarService");
    }
  }
  
  /// <summary>
  /// Базовая реализация API-сервиса.
  /// </summary>
  public abstract class ApiServiceBase : ServiceBase, IApiService
  {
    /// <summary>
    /// Тип содержимого для заголовка.
    /// </summary>
    public string ContentType { get; }
    
    /// <summary>
    /// Получить информацию по выходным/праздничным дням.
    /// </summary>
    /// <returns>Структура с информацией по выходным/праздничным дням.</returns>
    public override Structures.Module.IWeekendData GetWeekendInfo()
    {
      return Get();
    }
    
    /// <summary>
    /// Получить информацию по выходным/праздничным дням.
    /// </summary>
    /// <returns>Структура с информацией по выходным/праздничным дням.</returns>
    public abstract Structures.Module.IWeekendData Get();
    
    /// <summary>
    /// Базовая реализация получения информации по выходным/праздничным дням через API.
    /// </summary>
    /// <typeparam name="T">Десериализуемый класс с конвертацией в структуру данных.</typeparam>
    /// <param name="service">Сервис.</param>
    /// <returns>Структура с информацией по выходным/праздничным дням.</returns>
    public static Structures.Module.IWeekendData GetBase<T>(IApiService service) where T : IConvertable
    {
      if (service == null)
        throw new ArgumentNullException("Пустые параметры.");
      
      service.Logger?.Debug("Запрос данных к API. {url}", service.Url);
      
      // Получаем данные с сайта.
      bool success = CommonFunctions.Get(service.Url, service.ContentType, out string result);
      if (!success)
      {
        service.Logger?.Error(result);
        return null;
      }
      
      // Десериализация.
      T deserialized;
      try
      {
        deserialized = CommonFunctions.Deserialize<T>(result);
      }
      catch (Exception ex)
      {
        service.Logger?.Error(ex, "Ошибка при десериализации данных.");
        return null;
      }

      return deserialized.Convert();
    }
    
    public ApiServiceBase(string urlTemplate, int year) : base(urlTemplate, year)
    {
      
    }
  }
  
  /// <summary>
  /// Базовая реализация сервиса-парсера.
  /// </summary>
  public abstract class ParseServiceBase : ServiceBase, IParseService
  {
    /// <summary>
    /// Селектор для месяцев.
    /// </summary>
    public string MonthsSelector { get; set; }
    
    /// <summary>
    /// Селектор для праздников.
    /// </summary>
    public string HolidaySelector { get; set; }
    
    /// <summary>
    /// Селектор для информации по праздникам.
    /// </summary>
    protected abstract string HolidayInfoSelector { get; }
    
    /// <summary>
    /// Получить информацию по выходным/праздничным дням.
    /// </summary>
    /// <returns>Структура с информацией по выходным/праздничным дням.</returns>
    public override Structures.Module.IWeekendData GetWeekendInfo()
    {
      return Parse();
    }
    
    /// <summary>
    /// Получить информацию по выходным/праздничным дням.
    /// </summary>
    /// <returns>Структура с информацией по выходным/праздничным дням.</returns>
    public virtual Structures.Module.IWeekendData Parse()
    {
      return ParseServiceBase.ParseBase(this);
    }
    
    /// <summary>
    /// Создать структуру с информацией по выходным/праздничным дням.
    /// </summary>
    /// <param name="months">HTML данные по месяцам.</param>
    /// <param name="holidayInfo">HTML данные по праздникам.</param>
    /// <returns>Структуру с информацией по выходным/праздничным дням.</returns>
    public abstract Structures.Module.IWeekendData CreateWeekendData(IHtmlCollection<IElement> months, IElement holidayInfo);
    
    /// <summary>
    /// Базовая реализация получения информации по выходным/праздничным дням через парсинг.
    /// </summary>
    /// <param name="service">Сервис.</param>
    /// <returns>Структура с информацией по выходным/праздничным дням.</returns>
    public static Structures.Module.IWeekendData ParseBase(IParseService service)
    {
      if (service == null)
        throw new ArgumentNullException("Пустые параметры данных.");
      
      HTMLMonthsInfo htmlMonthsInfo;

      service.Logger?.Debug("Запрос данных сайта. {url}", service.Url);
      
      // Получаем данные с сайта.
      try
      {
        htmlMonthsInfo = CommonFunctions.GetHTMLMonthsInfo(null, service.Url, service.MonthsSelector, service.HolidaySelector, service.CreateWeekendData);
      }
      catch (Exception ex)
      {
        service.Logger?.Error(ex, "Ошибка при получении данных со страницы");
        return null;
      }

      // Валидация данных.
      string validate = htmlMonthsInfo.Validate();
      if (!string.IsNullOrEmpty(validate))
      {
        service.Logger?.Error(validate);
        return null;
      }

      return htmlMonthsInfo.Convert();
    }
    
    /// <summary>
    /// Получить информацию по праздникам.
    /// </summary>
    /// <param name="info">Элемент с данными по праздникам.</param>
    /// <returns>Информация по праздникам.</returns>
    public virtual string GetHolidayInfo(IElement info)
    {
      return GetHolidayInfo(info, HolidayInfoSelector);
    }
    
    /// <summary>
    /// Получить информацию по праздникам.
    /// </summary>
    /// <param name="info">Элемент с данными по праздникам.</param>
    /// <param name="holidayInfoSelector">Селектор данных.</param>
    /// <returns>Информация по праздникам.</returns>
    public static string GetHolidayInfo(IElement info, string holidayInfoSelector)
    {
      if (info == null)
        return string.Empty;

      var holidays = info.QuerySelectorAll(holidayInfoSelector);
      return string.Join(Environment.NewLine, holidays.Select(x => x.TextContent));
    }
    
    public ParseServiceBase(string urlTemplate, int year) : base(urlTemplate, year)
    {
      
    }
  }
  
  /// <summary>
  /// Базовый класс с реализацией нескольких сервисов.
  /// </summary>
  public abstract partial class MultipleServiceBase : ServiceBase, IApiService, IParseService
  {
    private GetWeekendData _implementation;
    
    /// <summary>
    /// Используемая реализация получения данных.
    /// </summary>
    public GetWeekendData Implementation
    {
      get
      {
        return _implementation ?? Get;
      }
      
      set
      {
        if (value == null)
          throw new ArgumentNullException("Значение не может быть null;");
        
        _implementation = value;
      }
    }
    
    #region API.
    /// <summary>
    /// Тип содержимого для заголовка.
    /// </summary>
    public abstract string ContentType { get; }
    
    /// <summary>
    /// Получить информацию по выходным/праздничным дням.
    /// </summary>
    /// <returns>Структура с информацией по выходным/праздничным дням.</returns>
    public abstract Structures.Module.IWeekendData Get();
    #endregion
    
    #region Парсинг.
    /// <summary>
    /// Селектор для информации по праздникам.
    /// </summary>
    protected abstract string HolidayInfoSelector { get; }
    
    /// <summary>
    /// Селектор для месяцев.
    /// </summary>
    public string MonthsSelector { get; set; }
    
    /// <summary>
    /// Селектор для праздников.
    /// </summary>
    public string HolidaySelector { get; set; }
    
    /// <summary>
    /// Получить информацию по выходным/праздничным дням.
    /// </summary>
    /// <returns>Структура с информацией по выходным/праздничным дням.</returns>
    public virtual Structures.Module.IWeekendData Parse()
    {
      return ParseServiceBase.ParseBase(this);
    }
    
    /// <summary>
    /// Получить информацию по праздникам.
    /// </summary>
    /// <param name="info">Элемент с данными по праздникам.</param>
    /// <returns>Информация по праздникам.</returns>
    public virtual string GetHolidayInfo(IElement info)
    {
      return ParseServiceBase.GetHolidayInfo(info, HolidayInfoSelector);
    }
    
    /// <summary>
    /// Создать структуру с информацией по выходным/праздничным дням.
    /// </summary>
    /// <param name="months">HTML данные по месяцам.</param>
    /// <param name="holidayInfo">HTML данные по праздникам.</param>
    /// <returns>Структуру с информацией по выходным/праздничным дням.</returns>
    public abstract Structures.Module.IWeekendData CreateWeekendData(IHtmlCollection<IElement> months, IElement holidayInfo);
    #endregion
    
    /// <summary>
    /// Получить информацию по выходным/праздничным дням.
    /// </summary>
    /// <returns>Структура с информацией по выходным/праздничным дням.</returns>
    public override Structures.Module.IWeekendData GetWeekendInfo()
    {
      return Implementation.Invoke();
    }
    
    public MultipleServiceBase(string urlTemplate, int year, bool byAPI) : base(urlTemplate, year)
    {
      if (!byAPI)
        Implementation = Parse;
    }
  }
  #endregion
  
  #region Реализация сервисов.
  public class XMLCalendar : MultipleServiceBase
  {
    /// <summary>
    /// Логгер.
    /// </summary>
    public override ILogger Logger
    {
      set
      {
        if (value != null)
          value = value.WithProperty("Service", "XMLCalendar");
      }
    }
    
    #region API.
    /// <summary>
    /// Тип содержимого для заголовка.
    /// </summary>
    public override string ContentType
    {
      get
      {
        return "application/xml";
      }
    }
    
    /// <summary>
    /// Получить информацию по выходным/праздничным дням.
    /// </summary>
    /// <returns>Структура с информацией по выходным/праздничным дням.</returns>
    public override Structures.Module.IWeekendData Get()
    {
      return ApiServiceBase.GetBase<XMLC_Calendar>(this);
    }
    #endregion
    
    #region Парсинг.
    /// <summary>
    /// Наименование класса с выходными.
    /// </summary>
    const string WeekendClassName = "pcal-day pcal-day-holiday";
    
    /// <summary>
    /// Наименование класса с предпраздничными днями.
    /// </summary>
    const string PreHolidayClassName = "pcal-day pcal-day-short";
    
    /// <summary>
    /// Селектор для информации по праздникам.
    /// </summary>
    protected override string HolidayInfoSelector
    {
      get
      {
        return "li";
      }
    }
    
    /// <summary>
    /// Создать структуру с информацией по выходным/праздничным дням.
    /// </summary>
    /// <param name="months">HTML данные по месяцам.</param>
    /// <param name="holidayInfo">HTML данные по праздникам.</param>
    /// <returns>Структуру с информацией по выходным/праздничным дням.</returns>
    public override Structures.Module.IWeekendData CreateWeekendData(IHtmlCollection<IElement> months, IElement holidayInfo)
    {
      var structure = Structures.Module.WeekendData.Create();
      structure.HolidayInfo = GetHolidayInfo(holidayInfo);
      
      try
      {
        foreach (var month in months)
        {
          // Предпраздничные дни.
          var preHolidays = month.GetElementsByClassName(PreHolidayClassName)
            .Select(x => (x as IHtmlTableDataCellElement))
            .Select(x => DateTime.Parse(x.Dataset["date"]));
          structure.PreHolidays.AddRange(preHolidays);
          
          // Выходные дни.
          var allWeekends = month.GetElementsByClassName(WeekendClassName)
            .Select(x => (x as IHtmlTableDataCellElement))
            .Select(x => DateTime.Parse(x.Dataset["date"]));
          structure.Weekends.AddRange(allWeekends);
          
          // Праздничные дни явно не опеределить.
          //structure.Holidays = ;
        }
      }
      catch
      {
        throw;
      }
      
      return structure;
    }
    #endregion
    
    public XMLCalendar(string urlTemplate, int year, bool byAPI) : base(urlTemplate, year, byAPI)
    {
      HolidaySelector = ".pcal-holidays-container";
      MonthsSelector = ".pcal-month";
    }
  }
  
  public class ConsultantPlus : ParseServiceBase
  {
    /// <summary>
    /// Год.
    /// </summary>
    private readonly int _year;
    
    /// <summary>
    /// Наименование класса с выходными.
    /// </summary>
    const string WeekendClassName = "weekend";
    
    /// <summary>
    /// Наименование класса с предпраздничными днями.
    /// </summary>
    const string PreHolidayClassName = "preholiday";
    
    /// <summary>
    /// Наименование класса с праздниками.
    /// </summary>
    const string HolidayClassName = "holiday weekend";
    
    /// <summary>
    /// Селектор для информации по праздникам.
    /// </summary>
    protected override string HolidayInfoSelector
    {
      get
      {
        return "p";
      }
    }
    
    /// <summary>
    /// Создать структуру с информацией по выходным/праздничным дням.
    /// </summary>
    /// <param name="months">HTML данные по месяцам.</param>
    /// <param name="holidayInfo">HTML данные по праздникам.</param>
    /// <returns>Структуру с информацией по выходным/праздничным дням.</returns>
    public override Structures.Module.IWeekendData CreateWeekendData(IHtmlCollection<IElement> months, IElement holidayInfo)
    {
      var structure = Structures.Module.WeekendData.Create();
      structure.HolidayInfo = GetHolidayInfo(holidayInfo);
      
      var listMonth = months.ToList();
      try
      {
        foreach (var month in listMonth)
        {
          var index = listMonth.IndexOf(month);
          
          // Предпраздничные дни.
          var preHolidays = month.GetElementsByClassName(PreHolidayClassName)
            .Select(x => x.TextContent.Trim('*'))
            .Select(x => DateTime.Parse($"{x}.{index}.{_year}"));
          structure.PreHolidays.AddRange(preHolidays);
          
          var allWeekends = month.GetElementsByClassName(WeekendClassName)
            .Where(x => x is IHtmlTableCellElement && !(x is IHtmlTableHeaderCellElement));
          
          // Выходные дни.
          var weekends = allWeekends.Where(x => x.ClassName == WeekendClassName)
            .Select(x => x.TextContent)
            .Select(x => DateTime.Parse($"{x}.{index}.{_year}"));
          structure.Weekends.AddRange(weekends);
          
          // Праздничные дни.
          var holidays = allWeekends.Where(x => x.ClassName == HolidayClassName)
            .Select(x => x.TextContent)
            .Select(x => DateTime.Parse($"{x}.{index}.{_year}"));
          structure.Holidays.AddRange(holidays);
        }
      }
      catch
      {
        throw;
      }
      
      return structure;
    }
    
    public ConsultantPlus(string urlTemplate, int year) : base(urlTemplate, year)
    {
      _year = year;
      HolidaySelector = "blockquote:first-of-type";
      MonthsSelector = ".cal";
    }
  }
  
  public class HeadHunter : ParseServiceBase
  {
    /// <summary>
    /// Год.
    /// </summary>
    private readonly int _year;
    
    /// <summary>
    /// Наименование класса с выходными.
    /// </summary>
    const string WeekendClassName = "calendar-list__numbers__item calendar-list__numbers__item_day-off";
    
    /// <summary>
    /// Наименование класса с предпраздничными днями.
    /// </summary>
    const string PreHolidayClassName = "calendar-list__numbers__item calendar-list__numbers__item_shortened";

    /// <summary>
    /// Селектор для информации по праздникам.
    /// </summary>
    protected override string HolidayInfoSelector
    {
      get
      {
        return ".calendar-info-list__item";
      }
    }
    
    /// <summary>
    /// Создать структуру с информацией по выходным/праздничным дням.
    /// </summary>
    /// <param name="months">HTML данные по месяцам.</param>
    /// <param name="holidayInfo">HTML данные по праздникам.</param>
    /// <returns>Структуру с информацией по выходным/праздничным дням.</returns>
    public override Structures.Module.IWeekendData CreateWeekendData(IHtmlCollection<IElement> months, IElement holidayInfo)
    {
      var structure = Structures.Module.WeekendData.Create();
      structure.HolidayInfo = GetHolidayInfo(holidayInfo);
      
      var listMonth = months.ToList();
      try
      {
        foreach (var month in listMonth)
        {
          var index = listMonth.IndexOf(month);
          
          // Предпраздничные дни.
          var preHolidays = month.GetElementsByClassName(PreHolidayClassName)
            .Select(x => GetDayFromContent(x.TextContent))
            .Select(x => DateTime.Parse($"{x}.{index}.{_year}"));
          structure.PreHolidays.AddRange(preHolidays);
          
          var allWeekends = month.GetElementsByClassName(WeekendClassName);
          
          // Выходные дни.
          var weekends = allWeekends.Where(x => x.FirstElementChild?.TextContent == "Выходной день")
            .Select(x => GetDayFromContent(x.TextContent))
            .Select(x => DateTime.Parse($"{x}.{index}.{_year}"));
          structure.Weekends.AddRange(weekends);
          
          // Праздничные дни.
          var holidays = allWeekends
            .Select(x => GetDayFromContent(x.TextContent))
            .Select(x => DateTime.Parse($"{x}.{index}.{_year}"))
            .Where(x => !weekends.Contains(x));
          structure.Holidays.AddRange(holidays);
        }
      }
      catch
      {
        throw;
      }
      
      return structure;
    }
    
    /// <summary>
    /// Получить значения дня из содержимого.
    /// </summary>
    /// <param name="content">Содержимое.</param>
    /// <returns>Строка с днем.</returns>
    private static string GetDayFromContent(string content)
    {
      if (string.IsNullOrEmpty(content))
        return string.Empty;

      string pattern = @"\d+";
      var matches = System.Text.RegularExpressions.Regex.Matches(content, pattern);
      if (matches.Count > 0)
        return matches[0].Value;

      return string.Empty;
    }
    
    public HeadHunter(string urlTemplate, int year) : base(urlTemplate, year)
    {
      _year = year;
      HolidaySelector = ".calendar-info-list";
      MonthsSelector = ".calendar-list__item-body:nth-child(2n+1)";
    }
  }
  #endregion
  
  /// <summary>
  /// Общие функции.
  /// </summary>
  internal class CommonFunctions
  {
    /// <summary>
    /// Получить коллекцию с данными по месяцам.
    /// </summary>
    /// <param name="config">Конфигурация (если null используется стандарнтная).</param>
    /// <param name="url">Адресс.</param>
    /// <param name="monthSelector">Селектор для месяцев.</param>
    /// <param name="holidaySelector">Селектор праздников.</param>
    /// <param name="convertImplementation">Реализация преобразования в выходную структуру.</param>
    /// <returns>Коллекция с данными по месяцам.</returns>
    /// <exception cref="ArgumentNullException">Некорректные входные параметры.</exception>
    internal static HTMLMonthsInfo GetHTMLMonthsInfo(AngleSharp.IConfiguration config, string url, string monthSelector, string holidaySelector, CreateWeekendData convertImplementation)
    {
      if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(monthSelector) || string.IsNullOrEmpty(holidaySelector))
        throw new ArgumentNullException("Переданы пустые параметры.");

      if (config == null)
        config = AngleSharp.Configuration.Default.WithDefaultLoader();

      using (var context = AngleSharp.BrowsingContext.New(config))
      {
        using (var document = context.OpenAsync(url))
        {
          var htmlResult = document.Result;
          return new HTMLMonthsInfo(htmlResult.QuerySelectorAll(monthSelector), htmlResult.QuerySelector(holidaySelector), convertImplementation);
        }
      }
    }
    
    /// <summary>
    /// Get Запрос к API
    /// </summary>
    /// <param name="url">Строка запроса.</param>
    /// <param name="contentType">Тип содержимого.</param>
    /// <param name="result">Значение с результатам запроса (страка с данными/ошибка).</param>
    /// <returns>Признак успешного получения данных.</returns>
    internal static bool Get(string url, string contentType, out string result)
    {
      result = string.Empty;

      HttpClient client = new HttpClient();
      client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
      
      try
      {
        HttpResponseMessage response = client.GetAsync(url).Result;
        if (response.IsSuccessStatusCode)
        {
          var data = response.Content.ReadAsStringAsync();
          
          result = data.Result;
          return true;
        }
        
        result = $"{(int)response.StatusCode} ({response.ReasonPhrase})";
        return false;
      }
      catch(Exception ex)
      {
        result = ex.Message;
        return false;
      }
      finally
      {
        client.Dispose();
      }
    }
    
    /// <summary>
    /// Десериализовать.
    /// </summary>
    /// <typeparam name="T">Класс десериализации.</typeparam>
    /// <param name="content">строка с содержимым.</param>
    /// <returns>Десериализованный класс.</returns>
    /// <exception cref="ArgumentNullException">Пустое значение параметра.</exception>
    internal static T Deserialize<T>(string content)
    {
      if (string.IsNullOrEmpty(content))
        throw new ArgumentNullException("Передано пустое содержимое");

      XmlSerializer formater = new XmlSerializer(typeof(T));
      using (var reader = new StringReader(content))
        return (T)formater.Deserialize(reader);
    }
    
    /// <summary>
    /// Перечисление дат.
    /// </summary>
    /// <param name="start">Дата начала.</param>
    /// <param name="end">Дата окончания.</param>
    /// <returns>Перечисление дат.</returns>
    internal static IEnumerable<DateTime> GetDaysBetween(DateTime start, DateTime end)
    {
      for (DateTime i = start; i < end; i = i.AddDays(1))
        yield return i;
    }

    /// <summary>
    /// Перечисление выходных.
    /// </summary>
    /// <param name="start">Дата начала.</param>
    /// <param name="end">Дата окончания.</param>
    /// <returns>Перечисление выходных.</returns>
    internal static IEnumerable<DateTime> GetWeekends(DateTime start, DateTime end)
    {
      return GetDaysBetween(start, end)
        .Where(d => d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday);
    }
  }
}