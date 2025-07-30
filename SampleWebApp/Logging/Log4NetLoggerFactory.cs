 
namespace SampleWebApp.Logging;
using Microsoft.Extensions.Logging;


/// <summary>
/// Log4Net Logger Factory
/// </summary>
public class Log4NetLoggerFactory : ILoggerFactory
{

    /// <summary>
    /// Private _logProvider
    /// </summary>
    private ILoggerProvider _logProvider;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="provider"></param>
    public void AddProvider(ILoggerProvider provider)
    {
        _logProvider = provider;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="categoryName"></param>
    /// <returns></returns>
    public ILogger CreateLogger(string categoryName)
    {
        _logProvider ??= new Log4NetLoggerProvider();

        return _logProvider.CreateLogger(categoryName);
    }


    /// <summary>
    /// Dispose 
    /// </summary>
    public void Dispose()
    {
        _logProvider.Dispose();
    }
}
