using System;
using NLog;

namespace Frodo.Infrastructure.Logging
{
    public sealed class NLogLogger : ILogger
    {
        private readonly Logger _nlogLogger;

        public NLogLogger(Logger nlogLogger)
        {
            _nlogLogger = nlogLogger;
        }

        public void Fatal(string message, params object[] args)
        {
            _nlogLogger.Fatal(message, args);
        }

        public void Fatal(Exception exception, string message, params object[] args)
        {
            _nlogLogger.Fatal(exception, message, args);
        }

        public void Error(string message, params object[] args)
        {
            _nlogLogger.Error(message, args);
        }

        public void Error(Exception exception, string message, params object[] args)
        {
            _nlogLogger.Error(exception, message, args);
        }

        public void Warn(string message, params object[] args)
        {
            _nlogLogger.Warn(message, args);
        }

        public void Warn(Exception exception, string message, params object[] args)
        {
            _nlogLogger.Warn(exception, message, args);
        }

        public void Info(string message, params object[] args)
        {
            _nlogLogger.Info(message, args);
        }

        public void Info(Exception exception, string message, params object[] args)
        {
            _nlogLogger.Info(exception, message, args);
        }

        public void Debug(string message, params object[] args)
        {
            _nlogLogger.Debug(message, args);
        }

        public void Debug(Exception exception, string message, params object[] args)
        {
            _nlogLogger.Debug(exception, message, args);
        }

        public void Trace(string message, params object[] args)
        {
            _nlogLogger.Trace(message, args);
        }

        public void Trace(Exception exception, string message, params object[] args)
        {
            _nlogLogger.Trace(exception, message, args);
        }
    }
}