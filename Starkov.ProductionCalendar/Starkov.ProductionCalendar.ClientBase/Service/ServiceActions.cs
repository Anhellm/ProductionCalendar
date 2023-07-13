using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ProductionCalendar.Service;

namespace Starkov.ProductionCalendar.Client
{
  partial class ServiceCollectionActions
  {
    public override void DeleteEntities(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.DeleteEntities(e);
    }

    public override bool CanDeleteEntities(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return false;
    }

  }

  partial class ServiceActions
  {
    public override void DeleteEntity(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.DeleteEntity(e);
    }

    public override bool CanDeleteEntity(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return false;
    }

  }

}