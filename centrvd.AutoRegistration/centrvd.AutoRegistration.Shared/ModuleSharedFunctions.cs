using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace centrvd.AutoRegistration.Shared
{
  public class ModuleFunctions
  {

    /// <summary>
    /// Получить журнал по умолчанию для документа.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="filteredDocRegistersIds">Список ИД доступных журналов.</param>
    /// <param name="settingType">Тип настройки.</param>
    /// <returns>Журнал регистрации по умолчанию.</returns>
    /// <remarks>Журнал подбирается сначала из настройки регистрации.
    /// Если в настройках не указан журнал, или указан недействующий, то вернётся первый журнал из доступных для документа.
    /// Если доступных журналов несколько, то вернётся пустое значение.
    public Sungero.Docflow.IDocumentRegister GetDefaultDocRegister(Sungero.Docflow.IOfficialDocument document, List<long> filteredDocRegistersIds, Enumeration? settingType)
    {
      // HACK Перекрыты и скорректированы стандартные вычисления коробочной функции GetDefaultDocRegister. Версия RX 4.10.48.0.
      var defaultDocRegister = Sungero.Docflow.DocumentRegisters.Null;

      if (document == null)
        return defaultDocRegister;

      var registrationSetting = Sungero.Docflow.PublicFunctions.RegistrationSetting.GetSettingByDocument(document, settingType);
      if (registrationSetting != null && filteredDocRegistersIds.Contains(registrationSetting.DocumentRegister.Id))
        return registrationSetting.DocumentRegister;
      
      if (filteredDocRegistersIds.Count() == 1)
        defaultDocRegister = Sungero.Docflow.PublicFunctions.DocumentRegister.Remote.GetDocumentRegister(filteredDocRegistersIds.First());
      
      return defaultDocRegister;
    }

  }
}