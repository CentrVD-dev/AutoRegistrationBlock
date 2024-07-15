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
    ///  Получить отфильтрованные журналы регистрации по документу.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="settingType">Тип регистрации.</param>
    /// <returns>Журналы.</returns>
    [Public, Obsolete("Используйте метод GetDocumentRegistersIdsByDocument.")]
    public static List<Sungero.Docflow.IDocumentRegister> GetDocumentRegistersByDocument(Sungero.Docflow.IOfficialDocument document, Enumeration? settingType)
    {
      var emptyList = new List<Sungero.Docflow.IDocumentRegister>();
      var documentKind = document.DocumentKind;
      if (documentKind == null)
        return emptyList;
      
      //var isClerk = document.AccessRights.CanRegister();
      if (settingType == Sungero.Docflow.RegistrationSetting.SettingType.Registration) //!isClerk ||
      {
        var setting = Sungero.Docflow.PublicFunctions.Module.Remote.GetRegistrationSettings(settingType, document.BusinessUnit, documentKind, document.Department).FirstOrDefault();
        return setting != null ? new List<Sungero.Docflow.IDocumentRegister> { setting.DocumentRegister } : emptyList;
      }
      
      return Sungero.Docflow.PublicFunctions.DocumentRegister.Remote.GetDocumentRegistersByParams(document.DocumentKind, document.BusinessUnit, document.Department, settingType, true);
    }
    
    /// <summary>
    /// Получить журнал по умолчанию для документа.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="filteredDocRegisters">Список доступных журналов.</param>
    /// <param name="settingType">Тип настройки.</param>
    /// <returns>Журнал.</returns>
    [Public]
    public new static Sungero.Docflow.IDocumentRegister GetDefaultDocRegister(Sungero.Docflow.IOfficialDocument document, List<Sungero.Docflow.IDocumentRegister> filteredDocRegisters, Enumeration? settingType)
    {
      var defaultDocRegister = Sungero.Docflow.DocumentRegisters.Null;

      if (document == null)
        return defaultDocRegister;

      var registrationSetting = Sungero.Docflow.PublicFunctions.RegistrationSetting.GetSettingByDocument(document, settingType);
      if (registrationSetting != null && filteredDocRegisters.Contains(registrationSetting.DocumentRegister))
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

      if (defaultDocRegister == null || !filteredDocRegisters.Contains(defaultDocRegister) || defaultDocRegister.Status != Sungero.CoreEntities.DatabookEntry.Status.Active)
      {
        defaultDocRegister = filteredDocRegisters.Count() > 1 ? Sungero.Docflow.DocumentRegisters.Null : Sungero.Docflow.DocumentRegisters.As(filteredDocRegisters.FirstOrDefault());
      }
      
      return defaultDocRegister;
    }

  }
}