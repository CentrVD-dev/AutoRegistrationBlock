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
    /// Авторегистрация документа
    /// </summary>
    /// <param name="document">Документ</param>
    [Public]
    public Structures.Module.IDocumentAutoregistrationResult AutoRegistrationDocument(Sungero.Docflow.IOfficialDocument document)
    {
      var result = Structures.Module.DocumentAutoregistrationResult.Create();
      
      if (document.RegistrationState != Sungero.Docflow.OfficialDocument.RegistrationState.Registered)
      {
        if (document == null)
        {
          result.IsError = true;
          result.Message = Resources.NotFoundDocument;
          return result;
        }
        
        var autoRegistrationSetting = centrvd.AutoRegistration.AutoRegistrationSettings.GetAll().Where(d => Equals(d.DocumentKind, document.DocumentKind));
        if (autoRegistrationSetting == null)
        {
          result.IsError = true;
          result.Message = Resources.NotFoundSetting;
          return result;
        }
        
        var operation = Sungero.Docflow.RegistrationSetting.SettingType.Registration;
        var registers = centrvd.AutoRegistration.PublicFunctions.Module.GetDocumentRegistersByDocument(document, operation);
        var defaultDocumentRegister = centrvd.AutoRegistration.PublicFunctions.Module.GetDefaultDocRegister(document, registers, operation);
        
        if (defaultDocumentRegister == null)
        {
          result.IsError = true;
          result.Message = Resources.NotFoundRegSetting;
          return result;
        }

        //Регистрация документа.
        var regDate = document.RegistrationDate != null ? document.RegistrationDate : Calendar.Today;
        var regNumber = document.RegistrationNumber != null ? document.RegistrationNumber : string.Empty;

        if (Locks.GetLockInfo(document).IsLocked)
          Locks.Unlock(document);

        Sungero.Docflow.PublicFunctions.OfficialDocument.RegisterDocument(document, defaultDocumentRegister, regDate, regNumber, false, true);

        if (Locks.GetLockInfo(document).IsLocked)
          Locks.Unlock(document);
        
        result.IsError = false;
        return result;        
      }
      else
      {
        result.IsError = false;
        result.Message = Resources.DocumentPreviouslyRegisteredFormat(document.Id);
        
        if (Locks.GetLockInfo(document).IsLocked)
          Locks.Unlock(document);
        
        return result;
      }
    }

  }
}