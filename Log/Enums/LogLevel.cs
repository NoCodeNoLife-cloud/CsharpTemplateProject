namespace CustomSerilogImpl.InstanceVal.Service.Enums;

/// <summary>
/// Log level enumeration for controlling output verbosity
/// Lower numeric values represent more verbose/debug levels
/// Higher numeric values represent more severe/critical levels
/// </summary>
public enum LogLevel
{
    Debug,        // Value 0 - Most verbose level, detailed diagnostic information
    Information,  // Value 1 - Default level, general operational information  
    Warning,      // Value 2 - Warning conditions that may require attention
    Error,        // Value 3 - Error events that might still allow the application to continue
    Critical      // Value 4 - Critical conditions that may cause application failure
}