using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ProductionCalendar.Service;

namespace Starkov.ProductionCalendar.Shared
{
  partial class ServiceFunctions
  {

    /// <summary>
    /// Проверка требования для шаблона Url.
    /// </summary>
    /// <param name="url">Url.</param>
    /// <returns>Ошибка или пустая строка.</returns>
    public static string ValidateUrl(string url)
    {
      if (string.IsNullOrWhiteSpace(url))
        return Services.Resources.EmptyUrl;
      
      if (!url.Contains("{year}"))
        return Services.Resources.WithoutYear;
      
      return string.Empty;
    }

  }
}