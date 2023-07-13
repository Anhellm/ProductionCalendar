using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ProductionCalendar.Service;

namespace Starkov.ProductionCalendar
{
  partial class ServiceServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.IsPriority = false;
      _obj.UseApi = false;
    }

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      var source = _obj.DataSource;
      if (Functions.Service.GetServices(source).Any(x => !Equals(x, _obj)))
      {
        var sourceName = _obj.Info.Properties.DataSource.GetLocalizedValue(source);
        e.AddError(Starkov.ProductionCalendar.Services.Resources.Duplicate_ErrorFormat(sourceName));
        return;
      }
      
      if (_obj.Status != Status.Active && _obj.IsPriority == true)
        _obj.IsPriority = false;
    }

    public override void Saving(Sungero.Domain.SavingEventArgs e)
    {
      if (_obj.IsPriority == true)
      {
        var services = Functions.Service.GetServices(null).Where(x => !Equals(x, _obj)).Where(x => x.IsPriority == true).ToList();
        foreach (var service in services)
          service.IsPriority = false;
      }
    }

    public override void BeforeDelete(Sungero.Domain.BeforeDeleteEventArgs e)
    {
      e.AddError(Services.Resources.Delete_Error);
    }
  }

}