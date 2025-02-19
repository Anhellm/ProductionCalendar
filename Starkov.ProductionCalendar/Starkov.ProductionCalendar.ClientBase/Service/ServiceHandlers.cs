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

    public virtual void UseApiValueInput(Sungero.Presentation.BooleanValueInputEventArgs e)
    {
        e.AddWarning(Services.Resources.CheckUrl_Warn);
    }

    public virtual void UrlValueInput(Sungero.Presentation.StringValueInputEventArgs e)
    {
      var urlError = Functions.Service.ValidateUrl(e.NewValue);
      if (!string.IsNullOrEmpty(urlError))
      {
        e.AddError(urlError);
        return;
      }

    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      var properties = _obj.State.Properties;
      var dataSource = _obj.DataSource.HasValue ? _obj.DataSource.Value.Value : string.Empty;
      
      properties.UseApi.IsEnabled = IsolatedFunctions.ExternalData.CanSwitchImplementation(dataSource);
    }

  }
}