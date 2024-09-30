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
    /// <remarks>Журнал подбирается сначала из настройки регистрации, потом из персональных настроек пользователя.
    /// Если в настройках не указан журнал, или указан недействующий, то вернётся первый журнал из доступных для документа.
    /// Если доступных журналов несколько, то вернётся пустое значение.
    /// Дублирование коробочной функции т.к. не публичная функция. Версия RX 4.10.48.0</remarks>
    public static Sungero.Docflow.IDocumentRegister GetDefaultDocRegister(Sungero.Docflow.IOfficialDocument document, List<long> filteredDocRegistersIds, Enumeration? settingType)
    {
      var defaultDocRegister = Sungero.Docflow.DocumentRegisters.Null;

      if (document == null)
        return defaultDocRegister;

      var registrationSetting = Sungero.Docflow.PublicFunctions.RegistrationSetting.GetSettingByDocument(document, settingType);
      if (registrationSetting != null && filteredDocRegistersIds.Contains(registrationSetting.DocumentRegister.Id))
        return registrationSetting.DocumentRegister;
      
      var personalSettings = Sungero.Docflow.PublicFunctions.PersonalSetting.GetPersonalSettings(null);
      if (personalSettings != null)
      {
        var documentKind = document.DocumentKind;

        if (documentKind.DocumentFlow == Sungero.Docflow.DocumentKind.DocumentFlow.Incoming)
          defaultDocRegister = personalSettings.IncomingDocRegister;
        if (documentKind.DocumentFlow == Sungero.Docflow.DocumentKind.DocumentFlow.Outgoing)
          defaultDocRegister = personalSettings.OutgoingDocRegister;
        if (documentKind.DocumentFlow == Sungero.Docflow.DocumentKind.DocumentFlow.Inner)
          defaultDocRegister = personalSettings.InnerDocRegister;
        if (documentKind.DocumentFlow == Sungero.Docflow.DocumentKind.DocumentFlow.Contracts)
          defaultDocRegister = personalSettings.ContractDocRegister;
      }

      if (defaultDocRegister == null || !filteredDocRegistersIds.Contains(defaultDocRegister.Id) || defaultDocRegister.Status != Sungero.CoreEntities.DatabookEntry.Status.Active)
      {
        defaultDocRegister = null;
        if (filteredDocRegistersIds.Count() == 1)
          defaultDocRegister = Sungero.Docflow.PublicFunctions.DocumentRegister.Remote.GetDocumentRegister(filteredDocRegistersIds.First());
      }
      
      return defaultDocRegister;
    }

  }
}