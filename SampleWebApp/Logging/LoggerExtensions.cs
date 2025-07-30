using log4net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;


/// <summary>
/// Logger Extensions
/// </summary>
public static class LoggerExtensions
{

    /// <summary>
    /// Log States
    /// </summary>
    public enum LogState
    {
        GenerateJson
    }


    /// <summary>
    ///   Exception
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="exception"></param>
    /// <param name="logState"></param>
    /// <param name="datas"></param>
    public static void JsonLogException(this ILogger logger, System.Exception exception, LogState logState, params object?[] datas)
    {
        StringBuilder ExceptionString = new StringBuilder(exception.ToString());
        ExceptionString.Append(exception.StackTrace);
        if (exception.InnerException != null)
        {
            ExceptionString.Append(exception.InnerException.ToString());
            ExceptionString.Append(exception.InnerException.StackTrace);
        }
        JsonLog(logger, LogLevel.Error, ExceptionString.ToString(), logState, datas);
    }


    /// <summary>
    /// Static method for logging
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="message"></param>
    /// <param name="additionalData"></param>
    public static void JsonLog(this ILogger logger, LogLevel level, string message, LogState logState, params object?[] datas)
    {
        Dictionary<string, object> additionalData = new Dictionary<string, object>(); 
        foreach (var data in datas)
        {
            if (data != null)
            {
                PropertyInfo[] properties = data.GetType().GetProperties();
                foreach (PropertyInfo prop in properties)
                { 
                    if (prop.Name == "Exception")
                    {
                        var exception = prop.GetValue(data) as Exception;
                        if (exception != null)
                        {
                            StringBuilder exceptionString = new StringBuilder(exception.ToString());
                            exceptionString.Append(exception.StackTrace);
                            if (exception.InnerException != null)
                            {
                                exceptionString.Append(exception.InnerException.ToString());
                                exceptionString.Append(exception.InnerException.StackTrace);
                            }
                            additionalData["ExceptionMessage"] = exception.Message;
                            additionalData["ExceptionString"] = exceptionString.ToString();
                        }
                    }
                    else
                    {
                        additionalData[prop.Name] = prop.GetValue(data);
                    }
                }
            }
        }
        string contentJson = JsonConvert.SerializeObject(additionalData, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });
        Dictionary<string, object>? contentDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(contentJson);
        LogicalThreadContext.Properties["AdditionalData"] = contentDict;
        LogicalThreadContext.Properties["MethodName"] = GetCallingMethodName();
        LogicalThreadContext.Properties["LogState"] = logState; 

        switch (level)
        {
            case LogLevel.Debug:
                logger.LogDebug(message);
                break;
            case LogLevel.Information:
                logger.LogInformation(message);
                break;
            case LogLevel.Warning:
                logger.LogWarning(message);
                break;
            case LogLevel.Error:
                logger.LogError(message);
                break;
            case LogLevel.Critical:
                logger.LogCritical(message);
                break;
            default:
                logger.LogInformation(message);
                break;
        }
        LogicalThreadContext.Properties.Remove("AdditionalData");
        LogicalThreadContext.Properties.Remove("MethodName");
        LogicalThreadContext.Properties.Remove("LogState");
        LogicalThreadContext.Properties.Remove("HelpCode");
    }


    public static void JsonLog(this ILogger logger, LogLevel level, string message, LogState logState, long timeTaken, params object?[] datas)
    {

        Dictionary<string, object> additionalData = new Dictionary<string, object>();
        foreach (var data in datas)
        {
            if (data != null)
            {
                PropertyInfo[] properties = data.GetType().GetProperties();
                foreach (PropertyInfo prop in properties)
                {
                    additionalData[prop.Name] = prop.GetValue(data);
                }
            }
        }
        string contentJson = JsonConvert.SerializeObject(additionalData, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });
        Dictionary<string, object>? contentDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(contentJson);
        LogicalThreadContext.Properties["AdditionalData"] = contentDict;
        LogicalThreadContext.Properties["MethodName"] = GetCallingMethodName();
        LogicalThreadContext.Properties["LogState"] = logState;

        LogicalThreadContext.Properties["TimeTaken"] = timeTaken;

        switch (level)
        {
            case LogLevel.Debug:
                logger.LogDebug(message);
                break;
            case LogLevel.Information:
                logger.LogInformation(message);
                break;
            case LogLevel.Warning:
                logger.LogWarning(message);
                break;
            case LogLevel.Error:
                logger.LogError(message);
                break;
            case LogLevel.Critical:
                logger.LogCritical(message);
                break;
            default:
                logger.LogInformation(message);
                break;
        }
        LogicalThreadContext.Properties.Remove("AdditionalData");
        LogicalThreadContext.Properties.Remove("MethodName");
        LogicalThreadContext.Properties.Remove("LogState");
        LogicalThreadContext.Properties.Remove("TimeTaken");
    }


    /// <summary>
    /// Get Calling MethodName
    /// </summary>
    /// <returns></returns>
    private static string GetCallingMethodName()
    {
        StackTrace stackTrace = new StackTrace();
        StackFrame[] stackFrames = stackTrace.GetFrames();
        if (stackFrames != null && stackFrames.Length > 2)
        {
            StackFrame callingFrame = stackFrames[2];
            string methodName = callingFrame.GetMethod()?.Name ?? string.Empty;
            int lastIndex = methodName.LastIndexOf('.');
            if (lastIndex >= 0)
            {
                methodName = methodName[(lastIndex + 1)..];
            }
            return methodName;
        }
        return "Unknown";
    }
}
