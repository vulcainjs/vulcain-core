using System;

namespace Vulcain.Core.Commands
{
    internal class IllegalStateException : Exception
    {
        public IllegalStateException(string message) : base(message)
        {
        }
    }
}