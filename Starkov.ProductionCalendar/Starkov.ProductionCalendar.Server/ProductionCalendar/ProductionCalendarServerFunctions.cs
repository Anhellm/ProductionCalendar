using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ProductionCalendar.ProductionCalendar;

namespace Starkov.ProductionCalendar.Server
{
  partial class ProductionCalendarFunctions
  {
    #region Получение данных.
    /// <summary>
    /// Получить основные производственные календари.
    /// </summary>
    /// <returns>Производственные календари.</returns>
    [Public, Remote(IsPure = true)]
    public static IQueryable<IProductionCalendar> GetCalendars()
    {
      return ProductionCalendars.GetAll(x => x.IsPrivate != true);
    }
    
    /// <summary>
    /// Получить основной производственный календарь.
    /// </summary>
    /// <param name="year">Год.</param>
    /// <returns>Производственный календарь на конкретный год.</returns>
    [Public, Remote(IsPure = true)]
    public static IProductionCalendar GetCalendar(int? year)
    {
      if (!year.HasValue)
        return ProductionCalendars.Null;
      
      return GetCalendars().Where(x => x.Year == year).FirstOrDefault();
    }
    
    /// <summary>
    /// Получить основной производственный календарь.
    /// </summary>
    /// <param name="workingTimeCalendar">Календарь рабочего времени.</param>
    /// <returns>Получить производственный календарь на основе календаря рабочего времени.</returns>
    [Public, Remote(IsPure = true)]
    public static IProductionCalendar GetCalendar(IWorkingTimeCalendar workingTimeCalendar)
    {
      if (workingTimeCalendar == null)
        return ProductionCalendars.Null;
      
      return GetCalendars().Where(x => Equals(x.WorkingTimeCalendar, workingTimeCalendar)).FirstOrDefault();
    }
    
    #region Частные календари.
    /// <summary>
    /// Получить частные производственные календари.
    /// </summary>
    /// <returns>Производственные календари.</returns>
    [Public, Remote(IsPure = true)]
    public static IQueryable<IProductionCalendar> GetPrivateCalendars()
    {
      return ProductionCalendars.GetAll(x => x.IsPrivate == true);
    }
    
    /// <summary>
    /// Получить частные производственные календари.
    /// </summary>
    /// <param name="year">Год.</param>
    /// <returns>Производственные календари.</returns>
    [Public, Remote(IsPure = true)]
    public static IQueryable<IProductionCalendar> GetPrivateCalendars(int? year)
    {
      return GetPrivateCalendars().Where(x => x.Year == year);
    }
    #endregion
    #endregion
    
    #region Создание
    /// <summary>
    /// Создать производственный календарь.
    /// </summary>
    /// <param name="workingTimeCalendar">Календарь рабочего времени.</param>
    /// <returns>Производственный календарь.</returns>
    [Public, Remote]
    public static IProductionCalendar CreateCalendar(IWorkingTimeCalendar workingTimeCalendar)
    {
      if (workingTimeCalendar == null)
        return ProductionCalendars.Null;
      
      var calendar = ProductionCalendars.Create();
      calendar.WorkingTimeCalendar = workingTimeCalendar;
      return calendar;
    }
    #endregion
    
    #region StateView календаря.
    /// <summary>
    /// Сформировать представление календаря.
    /// </summary>
    [Remote]
    public virtual StateView GetProductionCalendarState()
    {
      var stateView = StateView.Create();
      if (_obj.WorkingTimeCalendar == null)
        return stateView;
      
      AddColumns(stateView);
      
      var calendar = _obj.WorkingTimeCalendar.Day.ToList();
      for (int i = 0; i <= 4; i++)
        AddBlock(stateView, i, calendar);
      
      return stateView;
    }
    
    /// <summary>
    /// Добавить заголовки.
    /// </summary>
    /// <param name="stateView">StateView календаря.</param>
    public virtual void AddColumns(StateView stateView)
    {
      var columnBlock = stateView.AddBlock();
      var headerStyle = Sungero.Docflow.PublicFunctions.Module.CreateHeaderStyle(false);
      
      foreach (var header in new string[] { string.Empty, ProductionCalendars.Resources.DaysCount_Header, string.Empty, ProductionCalendars.Resources.WorkingHours_Header, string.Empty })
      {
        var content = columnBlock.AddContent();
        content.AddLabel(header, headerStyle);
      }
      
      AddEmptyContent(columnBlock);
    }
    
    /// <summary>
    /// Добавить блок со статистикой по дням/часам.
    /// </summary>
    /// <param name="stateView">StateView календаря.</param>
    /// <param name="number">Номер квартала (если 0 - информация за год)</param>
    /// <param name="days">Коллекция дней производственного календаря.</param>
    public virtual void AddBlock(StateView stateView, int number, List<Sungero.CoreEntities.IWorkingTimeCalendarDay> days)
    {
      var block = stateView.AddBlock();
      
      string blockName = string.Empty;
      
      bool isQuarterInfo = number > 0;
      if (isQuarterInfo)
      {
        blockName = GetQuarterName(number);
        
        days = days.Where(x => (x.Day.Month + 2) / 3 == number).ToList();
        
        bool isCurrent = days.Any(x => x.Day == Calendar.Today);
        block.IsExpanded = isCurrent;
        if (isCurrent)
          block.Background = Sungero.Core.Colors.Common.LightGreen;
      }
      
      block.AddLabel(blockName, Sungero.Docflow.PublicFunctions.Module.CreateHeaderStyle(false));
      
      int workingDays = days.Where(x => !x.Kind.HasValue).Count();
      int daysCount = days.Count;
      
      #region Количество дней.
      var daysInfoName = new string[]
      {
        ProductionCalendars.Resources.Calendar_Title,
        ProductionCalendars.Resources.Weekend_Title,
        ProductionCalendars.Resources.Working_Title
      };
      AddContent(block, daysInfoName);
      
      var daysInfoValue = new string[]
      {
        daysCount.ToString(),
        (daysCount - workingDays).ToString(),
        workingDays.ToString()
      };
      AddContent(block, daysInfoValue);
      #endregion
      
      #region Количество часов.
      var rates = new int[] { 40, 36, 24 };
      var hoursInfoName = rates.Select(x => ProductionCalendars.Resources.HoursLabel_TitleFormat(x).ToString()).ToArray();
      AddContent(block, hoursInfoName);
      
      var hoursInfoValue = rates.Select(x => x / 5 * workingDays).Select(x => x.ToString()).ToArray();
      AddContent(block, hoursInfoValue);
      #endregion
      
      AddEmptyContent(block);
      
      if (isQuarterInfo)
        AddMonthsQuarterBlock(block, days);
    }
    
    /// <summary>
    /// Получить наименование для квартала.
    /// </summary>
    /// <param name="number">Номер квартала.</param>
    /// <returns>Наименование квартала.</returns>
    public virtual string GetQuarterName(int number)
    {
      switch (number)
      {
        case 1:
          return ProductionCalendars.Resources.QuarterLabel_HeaderFormat("I");
        case 2:
          return ProductionCalendars.Resources.QuarterLabel_HeaderFormat("II");
        case 3:
          return ProductionCalendars.Resources.QuarterLabel_HeaderFormat("III");
        case 4:
          return ProductionCalendars.Resources.QuarterLabel_HeaderFormat("IV");
      }
      
      return string.Empty;
    }
    
    /// <summary>
    /// Добавить данные в столбец с информацией.
    /// </summary>
    /// <param name="block">Блок.</param>
    /// <param name="data">Массив данных для столбца.</param>
    public virtual void AddContent(Sungero.Core.StateBlock block, string[] data)
    {
      var content = block.AddContent();
      foreach (var line in data)
      {
        content.AddLabel(line);
        content.AddLineBreak();
      }
    }

    /// <summary>
    /// Добавить данные по представениям месяцев квартала.
    /// </summary>
    /// <param name="block">Основной блок квартала.</param>
    /// <param name="days">Коллекция дней производственного календаря.</param>
    public virtual void AddMonthsQuarterBlock(Sungero.Core.StateBlock block, List<Sungero.CoreEntities.IWorkingTimeCalendarDay> days)
    {
      #region Стили.
      var headerStyle = Sungero.Docflow.PublicFunctions.Module.CreateHeaderStyle(false);
      
      var headerWeekendStyle = Sungero.Docflow.PublicFunctions.Module.CreateHeaderStyle(false);
      headerWeekendStyle.Color = Sungero.Core.Colors.Common.Red;
      
      var deafaultStyle = Sungero.Docflow.PublicFunctions.Module.CreateStyle(Sungero.Core.Colors.Common.Black);
      var weekendStyle = Sungero.Docflow.PublicFunctions.Module.CreateStyle(Sungero.Core.Colors.Common.Red);
      var preholidayStyle = Sungero.Docflow.PublicFunctions.Module.CreateStyle(Sungero.Core.Colors.Common.Blue);
      #endregion
      
      var preholidays = Functions.ProductionCalendar.GetPreHolidays(_obj);
      
      // Группируем данные по месяцам в квартале.
      foreach (var month in days.GroupBy(x => x.Day.Month))
      {
        var firstMonthDay = month.FirstOrDefault().Day;
        
        var child = block.AddChildBlock();
        
        bool isCurrent = month.Any(x => x.Day == Calendar.Today);
        child.IsExpanded = isCurrent;
        if (isCurrent)
          child.Background = Sungero.Core.Colors.Common.LightGreen;
        
        child.AddLabel(firstMonthDay.ToString("MMMM"), headerStyle);
        
        // Блок календаря.
        var contentBlock = child.AddChildBlock();
        contentBlock.ShowBorder = false;
        if (isCurrent)
          contentBlock.Background = Sungero.Core.Colors.Common.LightGreen;
        
        // Первый день недели месяца.
        var firstDayOfMonth = GetDayOfWeekOrder((byte)firstMonthDay.DayOfWeek);
        
        // Группируем по дням недели.
        foreach (var dayOfWeek in month.GroupBy(x => x.Day.DayOfWeek).OrderBy(x => GetDayOfWeekOrder((byte)x.Key)))
        {
          var firstWeekDay = dayOfWeek.FirstOrDefault().Day;
          
          var key = GetDayOfWeekOrder((byte)dayOfWeek.Key);
          var columnStyle = key > 4 ? headerWeekendStyle : headerStyle;
          string dayName = Sungero.Docflow.PublicFunctions.Module.ReplaceFirstSymbolToUpperCase(firstWeekDay.ToString("dddd"));
          
          var content = contentBlock.AddContent();
          content.AddLabel(dayName, columnStyle);
          content.AddLineBreak();
          
          // Первый день недели.
          var firstDayOfWeek = firstWeekDay.Day;
          foreach (var day in dayOfWeek)
          {
            // Пропуск для выравнивания таблицы.
            if (day.Day.Day == firstDayOfWeek && key < firstDayOfMonth)
            {
              content.AddEmptyLine();
              content.AddLineBreak();
            }
            
            // Стиль отображения дня.
            var style = day.Kind.HasValue ? weekendStyle : deafaultStyle;
            if (preholidays.Contains(day.Day))
              style = preholidayStyle;
            
            content.AddLabel(day.Day.Day.ToString(), style);
            content.AddLineBreak();
          }
        }
        
        AddEmptyContent(contentBlock);
      }
    }
    
    /// <summary>
    /// Задать порядок вывода дней недели.
    /// </summary>
    /// <param name="day">Число дня недели.</param>
    /// <returns>Число в соответствующем порядке.</returns>
    private byte GetDayOfWeekOrder(byte day)
    {
      return (byte)((day + 6) % 7);
    }
    
    /// <summary>
    /// Добавить пустую колонку.
    /// </summary>
    /// <param name="block">Блок.</param>
    private void AddEmptyContent(StateBlock block)
    {
      var empty = block.AddContent();
      empty.AddLabel();
    }
    #endregion
    
    /// <summary>
    /// Необходимость обновления сериализованных данных календаря.
    /// </summary>
    /// <returns>True/False.</returns>
    [Remote(IsPure = true)]
    public virtual bool NeedSerialize()
    {
      var wtc = _obj.WorkingTimeCalendar;
      return wtc == null || GetLastDateChange(wtc) > GetLastDateChange(_obj);
    }
    
    /// <summary>
    /// Получить последнюю дату изменения сущности.
    /// </summary>
    /// <param name="entity">Сущность.</param>
    /// <returns>Дата.</returns>
    [Remote(IsPure = true)]
    public static DateTime? GetLastDateChange(Sungero.Domain.Shared.IEntity entity)
    {
      if (entity == null)
        return null;
      
      return entity.History.GetAll()
        .Where(x => x.Action == Sungero.CoreEntities.History.Action.Update && x.HistoryDate.HasValue)
        .Select(x => x.HistoryDate.Value)
        .OrderByDescending(x => x)
        .FirstOrDefault();
    }
    
    /// <summary>
    /// Сериализовать календаря в json.
    /// </summary>
    /// <param name="withSave">Необходимость сохранения.</param>
    [Remote]
    public virtual void SerializeCalendar(bool withSave)
    {
      var data = _obj.WorkingTimeCalendar?.Day
        ?.Select(x => Starkov.ProductionCalendar.Structures.Module.DateInfo.Create(x.Day, x.Kind.HasValue ? x.Kind.Value.Value : string.Empty))
        ?.ToList();
      var preHolidays = _obj.PreHolidays.Where(x => x.Date.HasValue).Select(x => x.Date.Value).ToList();
      
      _obj.JsonInfo = IsolatedFunctions.Serialize.SerializeCalendar(data, preHolidays, Constants.Module.LoggerPostfix);
      if (withSave)
        _obj.Save();
    }
  }
}