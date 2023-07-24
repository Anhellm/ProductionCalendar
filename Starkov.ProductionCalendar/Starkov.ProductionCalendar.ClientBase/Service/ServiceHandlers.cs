using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ProductionCalendar.Service;

namespace Starkov.ProductionCalendar
{
  partial class ServiceClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      var properties = _obj.State.Properties;
      var dataSource = _obj.DataSource.HasValue ? _obj.DataSource.Value.ToString() : string.Empty;
      
      properties.UseApi.IsEnabled = Functions.Module.Remote.HasApi(dataSource) && Functions.Module.Remote.HasParser(dataSource);
    }

  }
}