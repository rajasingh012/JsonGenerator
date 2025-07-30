
namespace SampleWebApp.Logging;

using log4net.Core;
using log4net;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;


/// <summary>
/// Class Log4NetLogger.
/// </summary>
/// <typeparam name="T"></typeparam>
internal class Log4NetLogger<T> : ILogger<T>
{

    /// <summary>
    /// The log
    /// </summary>
    private readonly ILog _log;


    /// <summary>
    /// Initializes a new instance of the <see cref="Log4NetLogger" /> class.
    /// </summary>
    /// <param name="log">The log.</param>
    public Log4NetLogger(ILog log)
    {
        _log = log;
    }


    /// <summary>
    /// Logs the specified log level.
    /// </summary>
    /// <typeparam name="TState">The type of the t state.</typeparam>
    /// <param name="logLevel">The log level.</param>
    /// <param name="eventId">The event identifier.</param>
    /// <param name="state">The state.</param>
    /// <param name="exception">The exception.</param>
    /// <param name="formatter">The formatter.</param>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        var logEvent = new LoggingEvent(new LoggingEventData
        {
            Level = MapLevel(logLevel),
            Message = formatter(state, exception),
            ExceptionString = exception?.ToString(),
            TimeStampUtc = DateTime.UtcNow,
            LoggerName = _log.Logger.Name,
            LocationInfo = new LocationInfo(typeof(T))
        });

        _log.Logger.Log(logEvent);
    }


    /// <summary>
    /// Determines whether the specified log level is enabled.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <returns><c>true</c> if the specified log level is enabled; otherwise, <c>false</c>.</returns>
    public bool IsEnabled(LogLevel logLevel)
    {
        return _log.Logger.IsEnabledFor(MapLevel(logLevel));
    }


    /// <summary>
    /// Begins the scope.
    /// </summary>
    /// <typeparam name="TState">The type of the t state.</typeparam>
    /// <param name="state">The state.</param>
    /// <returns>IDisposable.</returns>
    public IDisposable BeginScope<TState>(TState state)
    {
        return new DisposableScope();
    }


    /// <summary>
    /// Maps the level.
    /// </summary>
    /// <param name="level">The level.</param>
    /// <returns>Level.</returns>
    private Level? MapLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Critical => Level.Critical,
            LogLevel.Error => Level.Error,
            LogLevel.Warning => Level.Warn,
            LogLevel.Information => Level.Info,
            LogLevel.Debug => Level.Debug,
            LogLevel.Trace => Level.Trace,
            LogLevel.None => Level.Debug,
            _ => null,
        };
    }


    /// <summary>
    /// Class DisposableScope.
    /// Implements the <see cref="IDisposable" />
    /// </summary>
    /// <seealso cref="IDisposable" />
    private class DisposableScope : IDisposable
    {

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() { }
    }
}


/// <summary>
/// Class Log4NetLogger.
/// Implements the <see cref="ILogger{T}" />
/// </summary>
/// <seealso cref="ILogger{T}" />
internal class Log4NetLogger : Microsoft.Extensions.Logging.ILogger
{

    /// <summary>
    /// The log
    /// </summary>
    private readonly ILog _log;


    /// <summary>
    /// Initializes a new instance of the <see cref="Log4NetLogger"/> class.
    /// </summary>
    /// <param name="log">The log.</param>
    public Log4NetLogger(ILog log)
    {
        _log = log;
    }


    /// <summary>
    /// Writes a log entry.
    /// </summary>
    /// <typeparam name="TState">The type of the object to be written.</typeparam>
    /// <param name="logLevel">Entry will be written on this level.</param>
    /// <param name="eventId">Id of the event.</param>
    /// <param name="state">The entry to be written. Can be also an object.</param>
    /// <param name="exception">The exception related to this entry.</param>
    /// <param name="formatter">Function to create a <see cref="T:System.String" /> message of the <paramref name="state" /> and <paramref name="exception" />.</param>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        //debug and release version's StackTrace is difference.
#if DEBUG
        StackFrame sf = (new StackTrace(true)).GetFrame(3);
#else
    StackFrame sf = (new System.Diagnostics.StackTrace(true)).GetFrame(2);
#endif

        var logEvent = new LoggingEvent(new LoggingEventData
        {
            Level = MapLevel(logLevel),
            Message = formatter(state, exception),
            ExceptionString = exception?.ToString(),
            TimeStampUtc = DateTime.Now.ToUniversalTime(),
            LoggerName = _log.Logger.Name,
            LocationInfo = new LocationInfo(sf.GetFileName(), sf.GetMethod().Name, Path.GetFileName(sf.GetFileName()), sf.GetFileLineNumber().ToString()),

        });

        _log.Logger.Log(logEvent);
    }


    /// <summary>
    /// Checks if the given <paramref name="logLevel" /> is enabled.
    /// </summary>
    /// <param name="logLevel">Level to be checked.</param>
    /// <returns><c>true</c> if enabled.</returns>
    public bool IsEnabled(LogLevel logLevel)
    {
        return _log.Logger.IsEnabledFor(MapLevel(logLevel));
    }


    /// <summary>
    /// Begins a logical operation scope.
    /// </summary>
    /// <typeparam name="TState">The type of the state to begin scope for.</typeparam>
    /// <param name="state">The identifier for the scope.</param>
    /// <returns>An <see cref="T:System.IDisposable" /> that ends the logical operation scope on dispose.</returns>
    public IDisposable BeginScope<TState>(TState state)
    {
        return new DisposableScope();
    }


    /// <summary>
    /// Maps the level.
    /// </summary>
    /// <param name="level">The level.</param>
    /// <returns>Level.</returns>
    private Level? MapLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Critical => Level.Critical,
            LogLevel.Error => Level.Error,
            LogLevel.Warning => Level.Warn,
            LogLevel.Information => Level.Info,
            LogLevel.Debug => Level.Debug,
            LogLevel.Trace => Level.Trace,
            LogLevel.None => Level.Debug,
            _ => null,
        };
    }


    /// <summary>
    /// Class DisposableScope.
    /// Implements the <see cref="IDisposable" />
    /// </summary>
    /// <seealso cref="IDisposable" />
    private class DisposableScope : IDisposable
    {

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() { }
    }
}
