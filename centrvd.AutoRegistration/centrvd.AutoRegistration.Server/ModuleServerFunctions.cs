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
        var operation = Sungero.Docflow.RegistrationSetting.SettingType.Registration;
        
        // Регистрация документа.
        var regDate = document.RegistrationDate != null ? document.RegistrationDate : Calendar.Today;
        var regNumber = document.RegistrationNumber != null ? document.RegistrationNumber : string.Empty;
        var leadDocumentId = document.LeadingDocument != null ? document.LeadingDocument.Id : 0;
        var leadDocumentNumber = string.Empty;        
        var departmentId = document.Department != null ? document.Department.Id : 0;
        var departmentCode = document.Department != null ? document.Department.Code : string.Empty;
        var businessUnitId = document.BusinessUnit != null ? document.BusinessUnit.Id : 0;
        var businessUnitCode = document.BusinessUnit != null ? document.BusinessUnit.Code : string.Empty;
        var docKindCode = document.DocumentKind != null ? document.DocumentKind.Code : string.Empty;
        var caseFileIndex = document.CaseFile != null ? document.CaseFile.Index : string.Empty;
        var isClerk = document.AccessRights.CanRegister();
        var counterpartyCode = Sungero.Docflow.PublicFunctions.OfficialDocument.GetCounterpartyCode(document);
        var currentRegistrationDate = document.RegistrationDate ?? Calendar.UserToday;
        
        var registersIds = Sungero.Docflow.PublicFunctions.OfficialDocument.GetDocumentRegistersIdsByDocument(document, operation);
        var defaultDocumentRegister = Functions.Module.GetDefaultDocRegister(document, registersIds, operation);
        var index = this.GetNextIndex(defaultDocumentRegister, currentRegistrationDate, leadDocumentId, departmentId, businessUnitId, document).ToString();
        string nextNumber = string.Empty;
        if (defaultDocumentRegister != null)
          nextNumber = Sungero.Docflow.PublicFunctions.DocumentRegister.GenerateRegistrationNumber(defaultDocumentRegister, currentRegistrationDate, index, departmentCode, businessUnitCode, caseFileIndex, docKindCode,
                                                                                                  counterpartyCode, leadDocumentNumber);
        
        var isRegistered = Sungero.Docflow.PublicFunctions.OfficialDocument.TryExternalRegister(document, nextNumber, regDate);
        if (!isRegistered)
        {
          result.IsError = true;
          result.Message = Resources.NotFoundRegSetting;
          return result;
        }        
        
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
    
    /// <summary>
    /// Получить следующий порядковый номер для журнала.
    /// </summary>
    /// <param name="date">Дата.</param>
    /// <param name="leadDocumentId">Ведущий документ.</param>
    /// <param name="departmentId">Подразделение.</param>
    /// <param name="businessUnitId">НОР.</param>
    /// <param name="document">Текущий документ.</param>
    /// <returns>Порядковый номер.
    /// Дублирование коробочной функции т.к. функция не публичная</returns>
    public virtual int? GetNextIndex(Sungero.Docflow.IDocumentRegister documentRegister, DateTime date, long leadDocumentId, long departmentId, long businessUnitId, Sungero.Docflow.IOfficialDocument document)
    {
      var index = this.GetCurrentNumber(documentRegister, date, leadDocumentId, departmentId, businessUnitId) + 1;
      var documentsList = Sungero.Docflow.PublicFunctions.DocumentRegister.GetOtherDocumentsInPeriodBySections(documentRegister, document, date)
        .Where(l => l.Index >= index)
        .Where(l => leadDocumentId == 0 || l.LeadingDocument != null && Equals(l.LeadingDocument.Id, leadDocumentId))
        .Where(l => departmentId == 0 || l.Department != null && Equals(l.Department.Id, departmentId))
        .Where(l => businessUnitId == 0 || l.BusinessUnit != null && Equals(l.BusinessUnit.Id, businessUnitId))
        .Select(l => l.Index)
        .ToList();
      
      // Вернуть следующий номер, если он не занят.
      if (!documentsList.Contains(index))
        return index;
      
      // Найти следующий незанятый номер.
      index = documentsList.Where(d => !documentsList.Contains(d.Value + 1)).Min(d => d.Value);
      return index + 1;
    }
    
    /// <summary>
    /// Получить текущий порядковый номер для журнала.
    /// </summary>
    /// <param name="date">Дата.</param>
    /// <param name="leadDocumentId">ID ведущего документа.</param>
    /// <param name="departmentId">ID подразделения.</param>
    /// <param name="businessUnitId">ID НОР.</param>
    /// <returns>Порядковый номер.
    /// Дублирование коробочной функции т.к. функция не публичная</returns>
    [Remote(IsPure = true)]
    public virtual int GetCurrentNumber(Sungero.Docflow.IDocumentRegister documentRegister, DateTime date, long leadDocumentId, long departmentId, long businessUnitId)
    {
      var month = Sungero.Docflow.PublicFunctions.DocumentRegister.GetCurrentMonth(documentRegister, date);
      var year = Sungero.Docflow.PublicFunctions.DocumentRegister.GetCurrentYear(documentRegister, date);
      var quarter = Sungero.Docflow.PublicFunctions.DocumentRegister.GetCurrentQuarter(documentRegister, date);
      var day = Sungero.Docflow.PublicFunctions.DocumentRegister.GetCurrentDay(documentRegister, date);
      
      if (documentRegister.NumberingSection != Sungero.Docflow.DocumentRegister.NumberingSection.LeadingDocument)
        leadDocumentId = 0;
      
      if (documentRegister.NumberingSection != Sungero.Docflow.DocumentRegister.NumberingSection.Department)
        departmentId = 0;
      
      if (documentRegister.NumberingSection != Sungero.Docflow.DocumentRegister.NumberingSection.BusinessUnit)
        businessUnitId = 0;
      
      var command = string.Format(Queries.Module.GetCurrentNumber,
                                  documentRegister.Id, month, year, leadDocumentId, quarter, departmentId, businessUnitId, day);
      
      var executionResult = Sungero.Docflow.PublicFunctions.Module.ExecuteScalarSQLCommand(command);
      var result = 0;
      if (!(executionResult is DBNull) && executionResult != null)
        int.TryParse(executionResult.ToString(), out result);
      
      return result;
    }

    
  }
}