using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;

namespace StudentsMcpServer;

public class FileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;
    private readonly LogLevel _minimumLevel;
    private readonly object _lock = new object();

    public FileLoggerProvider(string filePath, LogLevel minimumLevel)
    {
        _filePath = filePath;
        _minimumLevel = minimumLevel;
        
        // Ensure directory exists
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(categoryName, _filePath, _minimumLevel);
    }

    public void Dispose()
    {
        // Nothing to dispose
    }

    private class FileLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly string _filePath;
        private readonly LogLevel _minimumLevel;
        private readonly object _lock = new object();

        public FileLogger(string categoryName, string filePath, LogLevel minimumLevel)
        {
            _categoryName = categoryName;
            _filePath = filePath;
            _minimumLevel = minimumLevel;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _minimumLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            var logEntry = FormatLogEntry(logLevel, _categoryName, message, exception);

            lock (_lock)
            {
                try
                {
                    File.AppendAllText(_filePath, logEntry + Environment.NewLine, Encoding.UTF8);
                }
                catch
                {
                    // Silently fail if we can't write to file
                }
            }
        }

        private static string FormatLogEntry(LogLevel logLevel, string categoryName, string message, Exception? exception)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logLevelString = logLevel.ToString().ToUpper().PadRight(5);
            var entry = $"[{timestamp}] [{logLevelString}] [{categoryName}] {message}";

            if (exception != null)
            {
                entry += Environment.NewLine + exception.ToString();
            }

            return entry;
        }
    }
}

