using System;

namespace Frodo.Infrastructure.Logging
{
    public sealed class NLogManager : ILogManager
    {
        private static readonly Lazy<ILogManager> _instance = new Lazy<ILogManager>(() => new NLogManager(), true);

        public static ILogManager Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        public ILogger GetLogger(string name = null)
        {
            var nlogLogger = name == null ? NLog.LogManager.GetCurrentClassLogger() : NLog.LogManager.GetLogger(name);
            return new NLogLogger(nlogLogger);
        }
    }
}