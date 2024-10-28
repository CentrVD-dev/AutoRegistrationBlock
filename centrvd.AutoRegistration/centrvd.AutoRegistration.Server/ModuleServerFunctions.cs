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
      var result = Structures.Module.DocumentAutoregistrationResult.Create();
      result.IsLocked = false;
      result.IsError = false;
      
      if (document == null)
      {
        result.IsError = true;
        result.Message = Resources.NoDocument;
        return result;
      }
      
      var documentLockInfo = Locks.GetLockInfo(document);
      if (documentLockInfo.IsLockedByOther)
      {
        result.IsLocked = true;
        result.Message = Resources.DocumentIsLockedFormat(document.Id, documentLockInfo.OwnerName);
        return result;
      }
      
      long defaultDocumentRegisterId = 0;
      try
      {
        Locks.Lock(document);
        if (document.RegistrationState != Sungero.Docflow.OfficialDocument.RegistrationState.Registered)
        {
          // Регистрация документа.
          var operation = Sungero.Docflow.RegistrationSetting.SettingType.Registration;
          var regDate = document.RegistrationDate != null ? document.RegistrationDate : Calendar.Today;
                    
          var registersIds = Sungero.Docflow.PublicFunctions.OfficialDocument.GetDocumentRegistersIdsByDocument(document, operation);
          var defaultDocumentRegister = Functions.Module.GetDefaultDocRegister(document, registersIds, operation);
          
          string nextNumber = string.Empty;
          if (defaultDocumentRegister != null)
          {            
            defaultDocumentRegisterId = defaultDocumentRegister.Id;            
            Sungero.Docflow.PublicFunctions.OfficialDocument.RegisterDocument(document, defaultDocumentRegister, regDate, string.Empty, false, true);                        
          }
          else
          {
            result.IsError = true;
            result.Message = Resources.NotFoundRegSettingFormat(document.Id);
          }
        }
        else
        {
          result.Message = Resources.DocumentPreviouslyRegisteredFormat(document.Id);
        }
      }
      catch (Exception ex)
      {
        result.IsError = true;
        result.Message = centrvd.AutoRegistration.Resources.FailedToRegisterFormat(document.Id, defaultDocumentRegisterId, ex.Message);
      }
      finally
      {
        if (Locks.GetLockInfo(document).IsLockedByMe)
          Locks.Unlock(document);
      }
      return result;
    }    
  }
}