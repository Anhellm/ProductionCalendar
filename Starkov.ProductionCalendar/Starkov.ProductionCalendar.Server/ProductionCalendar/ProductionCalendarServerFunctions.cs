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
    /// Получить производственные календари.
    /// </summary>
    /// <returns>Производственные календари.</returns>
    [Public, Remote(IsPure = true)]
    public static IQueryable<IProductionCalendar> GetCalendars()
    {
      return ProductionCalendars.GetAll();
    }
    
    /// <summary>
    /// Получить производственный календарь.
    /// </summary>
    /// <param name="year">Год.</param>
    /// <returns>Производственный календарь на конкретный год.</returns>
    [Public, Remote(IsPure = true)]
    public static IProductionCalendar GetCalendar(int? year)
    {
      if (!year.HasValue)
        return ProductionCalendars.Null;
      
      return GetCalendars().Where(x => x.WorkingTimeCalendar != null && x.WorkingTimeCalendar.Year == year).FirstOrDefault();
    }
    
    /// <summary>
    /// Получить производственный календарь.
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
    #endregion

    #region StateView календаря.
    /// <summary>
    /// 
    /// </summary>
    [Remote]
    public StateView GetProductionCalendarState()
    {
      var stateView = StateView.Create();
      if (_obj.WorkingTimeCalendar == null)
        return stateView;
      
      var calendar = _obj.WorkingTimeCalendar.Day;
      for (int i = 1; i <= 4; i++)
      {
        AddQuarter(stateView, GetQuarterNumber(i), calendar.Where(x => (x.Day.Month + 2) / 3 == i).ToList());
      }
      
      return stateView;
    }
    
    private static void AddQuarter(StateView stateView, string number, List<Sungero.CoreEntities.IWorkingTimeCalendarDay> days)
    {
      var quarter = stateView.AddBlock();
      quarter.IsExpanded = days.Any(x => x.Day == Calendar.Today);
      
      var headerStyle = Sungero.Docflow.PublicFunctions.Module.CreateHeaderStyle(false);
      quarter.AddLabel(number + " КВАРТАЛ", headerStyle);
      
      int workingDays = days.Where(x => !x.Kind.HasValue).Count();
      
      var daysInfoName = quarter.AddContent();
      daysInfoName.AddLabel("Количество дней", headerStyle);
      daysInfoName.AddLineBreak();
      daysInfoName.AddLabel("Календарных");
      daysInfoName.AddLineBreak();
      daysInfoName.AddLabel("Выходных и праздничных");
      daysInfoName.AddLineBreak();
      daysInfoName.AddLabel("Рабочих");
      
      var daysInfoValue = quarter.AddContent();
      daysInfoValue.AddEmptyLine();
      daysInfoValue.AddLineBreak();
      daysInfoValue.AddLabel(days.Count.ToString());
      daysInfoValue.AddLineBreak();
      daysInfoValue.AddLabel(days.Where(x => x.Kind.HasValue).Count().ToString());
      daysInfoValue.AddLineBreak();
      daysInfoValue.AddLabel(workingDays.ToString());

      quarter.AddContent().AddEmptyLine();
      
      var hoursInfoName = quarter.AddContent();
      hoursInfoName.AddLabel("Рабочие часы", headerStyle);
      hoursInfoName.AddLineBreak();
      hoursInfoName.AddLabel("40-часовая неделя");
      hoursInfoName.AddLineBreak();
      hoursInfoName.AddLabel("36-часовая неделя");
      hoursInfoName.AddLineBreak();
      hoursInfoName.AddLabel("24-часовая неделя");
      
      var hoursInfoValue = quarter.AddContent();
      hoursInfoValue.AddEmptyLine();
      hoursInfoValue.AddLineBreak();
      hoursInfoValue.AddLabel((40 / 5 * workingDays).ToString());
      hoursInfoValue.AddLineBreak();
      hoursInfoValue.AddLabel((36 / 5 * workingDays).ToString());
      hoursInfoValue.AddLineBreak();
      hoursInfoValue.AddLabel((24 / 5 * workingDays).ToString());
      
      quarter.AddContent().AddEmptyLine();
      
      AddMonthsQuarterBlock(quarter, days);
    }
    
    private string GetQuarterNumber(int number)
    {
      switch (number)
      {
        case 1:
          return "I";
        case 2:
          return "II";
        case 3:
          return "III";
        case 4:
          return "IV";
      }
      
      return string.Empty;
    }
    
    private static void AddMonthsQuarterBlock(StateBlock block, List<Sungero.CoreEntities.IWorkingTimeCalendarDay> days)
    {
      
      var headerStyle = Sungero.Docflow.PublicFunctions.Module.CreateHeaderStyle(false);
      var noteStyle = Sungero.Docflow.PublicFunctions.Module.CreateNoteStyle(true);
      
      var deafaultStyle = Sungero.Docflow.PublicFunctions.Module.CreateStyle(Sungero.Core.Colors.Common.Black);
      var weekendStyle = Sungero.Docflow.PublicFunctions.Module.CreateStyle(Sungero.Core.Colors.Common.Red);
      
      foreach (var month in days.GroupBy(x => x.Day.Month))
      {
        var child = block.AddChildBlock();
        child.AddLabel(month.FirstOrDefault().Day.ToString("MMMM"), headerStyle);
        child.AddLineBreak();
        
        var contentBlock = child.AddChildBlock();
        var first = month.FirstOrDefault().Day.DayOfWeek;
        foreach (var dayOfWeek in month.GroupBy(x => x.Day.DayOfWeek).OrderBy(x => x.Key))
        {
          var content = contentBlock.AddContent();
          content.AddLabel(dayOfWeek.Key.ToString(), noteStyle);
          content.AddLineBreak();
          
          foreach (var day in dayOfWeek)
          {
            if (day.Day == dayOfWeek.Select(x => x.Day).Min() && dayOfWeek.Key < first)
            {
              content.AddEmptyLine();
              content.AddLineBreak();
            }
            
            var style = day.Kind.HasValue ? weekendStyle : deafaultStyle;
            content.AddLabel(day.Day.Day.ToString(), style);
            content.AddLineBreak();
            
          }
        }
      }
    }
    #endregion
  }
}