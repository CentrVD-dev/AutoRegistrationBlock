using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace centrvd.AutoRegistration.Server
{
  public class ModuleFunctions
  {

    /// <summary>
    /// Авторегистрация документа.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <returns>Результат регистрации.</returns>
    [Public]
    public Structures.Module.IDocumentAutoregistrationResult AutoRegistrationDocument(Sungero.Docflow.IOfficialDocument document)
    {
      var documentLockInfo = Locks.GetLockInfo(document);
      var result = Structures.Module.DocumentAutoregistrationResult.Create();
      
      if (documentLockInfo.IsLocked)
      {
        result.IsError = true;
        result.Message = Resources.DocumentLockFormat(documentLockInfo.OwnerName);
        return result;
      }            
      
      if (document == null)
      {
        result.IsError = true;
        result.Message = Resources.NotFoundDocument;
        return result;
      }
      
      Locks.Lock(document);
      
      if (document.RegistrationState != Sungero.Docflow.OfficialDocument.RegistrationState.Registered)
      {
        var autoRegistrationSetting = centrvd.AutoRegistration.AutoRegistrationSettings.GetAll().Where(d => Equals(d.DocumentKind, document.DocumentKind));
        if (autoRegistrationSetting == null)
        {
          result.IsError = true;
          result.Message = Resources.NotFoundSetting;
          return result;
        }
        
        var operation = Sungero.Docflow.RegistrationSetting.SettingType.Registration;
        var registersIds = Sungero.Docflow.PublicFunctions.DocumentRegister.Remote.GetDocumentRegistersIdsByParams(document.DocumentKind, document.BusinessUnit, document.Department, operation, true);
        var registers = Sungero.Docflow.PublicFunctions.OfficialDocument.GetDocumentRegistersIdsByDocument(document, Sungero.Docflow.RegistrationSetting.SettingType.Registration);
        
        var defaultDocumentRegister = centrvd.AutoRegistration.PublicFunctions.Module.GetDefaultDocRegister(document, registers, operation);
        
        if (defaultDocumentRegister == null)
        {
          result.IsError = true;
          result.Message = Resources.NotFoundRegSetting;
          return result;
        }

        // Регистрация документа.
        var regDate = document.RegistrationDate != null ? document.RegistrationDate : Calendar.Today;
        var regNumber = document.RegistrationNumber != null ? document.RegistrationNumber : string.Empty;

        Sungero.Docflow.PublicFunctions.OfficialDocument.RegisterDocument(document, defaultDocumentRegister, regDate, regNumber, false, true);
        
        Locks.Unlock(document);
        
        result.IsError = false;        
      }
      else
      {
        result.IsError = false;
        result.Message = Resources.DocumentPreviouslyRegisteredFormat(document.Id);
        
        Locks.Unlock(document);
      }
      
      return result;
    }

  }
}