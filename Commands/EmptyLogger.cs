using System;
using Microsoft.Extensions.Logging;

namespace Vulcain.Core.Commands
{
    internal class NullDisposable : IDisposable
    {
        public static IDisposable Instance = new NullDisposable();

        private NullDisposable()
        {
        }

        public void Dispose()
        {
        }
    }

    internal class EmptyLogger : ILogger
    {
        public static ILogger Instance = new EmptyLogger();

        private EmptyLogger()
        {
        }

        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            return NullDisposable.Instance;
        }

        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            return false;
        }

        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
        }
    }
}