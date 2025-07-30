using Microsoft.Extensions.Logging;
using SampleWebApp.Logging;


/// <summary>
/// Log4Net Logger Provider
/// </summary>
namespace SampleWebApp.Logging;
internal class Log4NetLoggerProvider : ILoggerProvider
{

    /// <summary>
    /// 
    /// </summary>
    public void Dispose() { }


    /// <summary>
    /// Creates the logger.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>ILogger.</returns>
    public ILogger<T> CreateLogger<T>()
    {
        return new Log4NetLogger<T>(log4net.LogManager.GetLogger(typeof(T)));
    }


    /// <summary>
    /// Creates a new <see cref="T:Microsoft.Extensions.Logging.ILogger" /> instance.
    /// </summary>
    /// <param name="categoryName">The category name for messages produced by the logger.</param>
    /// <returns>The instance of <see cref="T:Microsoft.Extensions.Logging.ILogger" /> that was created.</returns>
    public ILogger CreateLogger(string categoryName)
    {
        return new Log4NetLogger(log4net.LogManager.GetLogger(categoryName));
    }
}
