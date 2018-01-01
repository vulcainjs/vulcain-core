using Vulcain.Core.Commands.CircuitBreaker;
using Vulcain.Core.Commands.Metrics;
using Vulcain.Core.Commands.Metrics.Publishers;
using System;

namespace Vulcain.Core.Commands
{

    public interface IJellyfishContext : IServiceProvider
    {
        ICommandExecutionHook CommandExecutionHook { get; }
        MetricsPublisherFactory MetricsPublisher { get; }
        void Reset();
        RequestCache<T> GetCache<T>(string commandName);
        RequestLog GetRequestLog();
        T GetService<T>();
    }
}