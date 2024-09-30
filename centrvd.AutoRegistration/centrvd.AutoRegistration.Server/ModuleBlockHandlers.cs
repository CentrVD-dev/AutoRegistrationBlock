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
        if (_block.Document == null)
        {
          this.SetBlockErrorResult(Resources.NoDocument);
          return;
        }
        
        var document = _block.Document;
        
        var result = PublicFunctions.Module.AutoRegistrationDocument(document);
        
        if (!result.IsError)
          _block.OutProperties.ExecutionResult = ExecutionResult.Success;
        else
          this.SetBlockErrorResult(result.Message);
      }
      catch (Exception ex)
      {
        this.SetBlockErrorResult(ex.Message);
        
        return;
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
      Logger.Debug(errorMessage);
    }
  }

}