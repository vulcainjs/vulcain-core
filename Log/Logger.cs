using System;
using System.Collections.Generic;
using System.Text;

namespace Vulcain.Core
{
    public interface ILogger
    {
        void Error(IRequestContext context, Exception error, Func<string> msg = null);
        void Info(IRequestContext context, Func<string> msg);
        bool Verbose(IRequestContext context, Func<string> msg);
        void LogAction(IRequestContext context, string kind, string message = null);
    }
}
