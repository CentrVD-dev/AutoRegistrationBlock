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
    /// <param name="documentRegister">Журнал регистрации.</param>
    /// <returns>Результат регистрации.</returns>
    [Public]
    public Structures.Module.IDocumentAutoregistrationResult AutoRegistrationDocument(Sungero.Docflow.IOfficialDocument document,
                                                                                      Sungero.Docflow.IDocumentRegister documentRegister)
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
          var defaultDocumentRegister = Sungero.Docflow.DocumentRegisters.Null;
          if (documentRegister == null)
          {
            var operation = Sungero.Docflow.RegistrationSetting.SettingType.Registration;
            var registersIds = Sungero.Docflow.PublicFunctions.OfficialDocument.GetDocumentRegistersIdsByDocument(document, operation);
            defaultDocumentRegister = Functions.Module.GetDefaultDocRegister(document, registersIds, operation);
          }
          else
            defaultDocumentRegister = documentRegister;
          
          if (defaultDocumentRegister != null)
          {
            var regDate = document.RegistrationDate != null ? document.RegistrationDate : Calendar.Today;
            defaultDocumentRegisterId = defaultDocumentRegister.Id;
            // Не вычисляем рег. номер до сохранения, чтобы он вычислялся стандартными вычислениями коробки в событии Saving.
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