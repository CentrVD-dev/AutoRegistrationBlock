using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace centrvd.AutoRegistration.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      CreateAutoRegistrationScript();
    }
    
    /// <summary>
    /// Создание записи нового типа сценария "Авторегистрация документов".
    /// </summary>
    public static void CreateAutoRegistrationScript()
    {
      InitializationLogger.DebugFormat("Init: Create stage for automatic registration script.");
      if (centrvd.AutoRegistration.AutoRegistrationScripts.GetAll().Any())
        return;
      
      var stage = centrvd.AutoRegistration.AutoRegistrationScripts.Create();
      stage.Name = "Сценарий авторегистрации документов";
      stage.TimeoutInHours = 1;
      stage.Save();
    }
  }
}
