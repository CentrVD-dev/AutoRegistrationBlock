using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using centrvd.AutoRegistration.AutoRegistrationScript;

namespace centrvd.AutoRegistration.Server
{
  partial class AutoRegistrationScriptFunctions
  {
    /// <summary>
    /// Выполнить сценарий.
    /// </summary>
    /// <param name="approvalTask">Задача на согласование по регламенту.</param>
    /// <returns>Результат выполнения сценария.</returns>
    public override Sungero.Docflow.Structures.ApprovalFunctionStageBase.ExecutionResult Execute(Sungero.Docflow.IApprovalTask approvalTask)
    {
      var result = base.Execute(approvalTask);
      
      try
      {
        var document = approvalTask.DocumentGroup.OfficialDocuments.FirstOrDefault();
        
        if (document.RegistrationState != Sungero.Docflow.OfficialDocument.RegistrationState.Registered)
        {
          if (document == null)
            return this.GetErrorResult("Не найден документ.");
          
          var autoRegistrationSetting = centrvd.AutoRegistration.AutoRegistrationSettings.GetAll().Where(d => Equals(d.DocumentKind, document.DocumentKind));
          if (autoRegistrationSetting == null)
            return this.GetErrorResult("Не найден вид документа.");

          if (autoRegistrationSetting != null)
          {
            var operation = Sungero.Docflow.RegistrationSetting.SettingType.Reservation;
            var registers = centrvd.AutoRegistration.PublicFunctions.Module.GetDocumentRegistersByDocument(document, operation);
            var defaultDocumentRegister = centrvd.AutoRegistration.PublicFunctions.Module.GetDefaultDocRegister(document, registers, operation);
            if (defaultDocumentRegister == null)
              return this.GetErrorResult("Не найден журнал регистрации.");

            //Регистрация документа.
            var regDate = document.RegistrationDate != null ? document.RegistrationDate : Calendar.Today;
            var regNumber = document.RegistrationNumber != null ? document.RegistrationNumber : string.Empty;

            if (Locks.GetLockInfo(document).IsLocked)
              Locks.Unlock(document);

            Sungero.Docflow.PublicFunctions.OfficialDocument.RegisterDocument(document, defaultDocumentRegister, regDate, regNumber, false, true);

            if (Locks.GetLockInfo(document).IsLocked)
              Locks.Unlock(document);
          }
        }
        else
        {
          Logger.Debug(string.Format("Документ с ИД: {0} ранее зарегестрирован.", document.Id));
          
          if (Locks.GetLockInfo(document).IsLocked)
            Locks.Unlock(document);
        }
      }
      catch (Exception ex)
      {
        result = this.GetErrorResult(ex.Message);
        Logger.ErrorFormat(ex.Message);
      }
      
      return result;
    }
  }
}