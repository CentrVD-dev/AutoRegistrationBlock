using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace centrvd.AutoRegistration.Structures.Module
{
  [Public]
  partial class DocumentAutoregistrationResult 
  {
    public bool IsError { get; set; }
    
    public bool IsLocked { get; set; }
    
    public string Message { get; set; }
  }
  
}