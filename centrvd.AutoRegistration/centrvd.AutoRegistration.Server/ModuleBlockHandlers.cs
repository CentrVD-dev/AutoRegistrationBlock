using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;

namespace centrvd.AutoRegistration.Server.AutoRegistrationBlocks
{
  partial class AutoRegistrationScriptBlockHandlers
  {

    public virtual void AutoRegistrationScriptBlockExecute()
    {
      try
      {
        var result = PublicFunctions.Module.AutoRegistrationDocument(_block.Document, _block.DocumentRegister);
        if (result.IsLocked)
        {
          _block.RetrySettings.Retry = true;
          LogAction(result.Message);
        }
        else if (result.IsError)
          this.SetBlockErrorResult(result.Message);
        else
        {
          _block.OutProperties.ExecutionResult = ExecutionResult.Success;
          LogAction(result.Message);
        }
        
      }
      catch (Exception ex)
      {
        this.SetBlockErrorResult(ex.Message);
      }
    }
    
    /// <summary>
    /// Настроить выходные параметры блока при возникновении ошибки процесса авторегистрации документа.
    /// </summary>
    /// <param name="errorMessage">Текст ошибки.</param>
    public void SetBlockErrorResult(string errorMessage)
    {
      _block.OutProperties.ErrorMessage = errorMessage;
      _block.OutProperties.ExecutionResult = ExecutionResult.RegError;
      LogAction(errorMessage);
    }
    
    /// <summary>
    /// Логирование сообщения.
    /// </summary>
    /// <param name="message">Сообщение.</param>
    public void LogAction(string message)
    {
      if (string.IsNullOrWhiteSpace(message))
        return;
      var parts = new List<string>();
      parts.Add("AutoRegistrationScriptBlock");
      parts.Add(message);
      parts.Add($"Task (id={_obj.MainTaskId}).");      
      parts.Add($"RetryIteration: {_block.RetrySettings.RetryIteration}");
      Logger.Debug(string.Join(" ", parts));
    }
  }

}