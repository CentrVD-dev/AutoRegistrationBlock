using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;

namespace centrvd.AutoRegistration.Server.AutoRegistrationBlocks
{
  partial class ScriptBlockHandlers
  {

    public virtual void ScriptBlockExecute()
    {
      try
      {
        if (!this.BlockHasDocuments())
        {
          //this.ExecutionDone();
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
        
        Logger.Debug(ex.Message);
        Logger.Debug(ex.StackTrace);
        return;
      }
    }
    
    /// <summary>
    /// Проверить, переданы ли в блок документы.
    /// </summary>
    /// <returns>True - переданы, False - не переданы.</returns>
    public virtual bool BlockHasDocuments()
    {
      var hasDocuments = _block.Document != null;
      if (!hasDocuments)
        Logger.Debug("Documents are not provided");
      
      return hasDocuments;
    }
    
    /// <summary>
    /// Настроить выходные параметры блока при возникновении ошибки процесса авторегистрации документа.
    /// </summary>
    /// <param name="errorMessage">Текст ошибки.</param>
    public virtual void SetBlockErrorResult(string errorMessage)
    {
      _block.OutProperties.ErrorMessage = errorMessage;
      _block.OutProperties.ExecutionResult = ExecutionResult.RegError;
    }
  }

}