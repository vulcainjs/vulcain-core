// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Vulcain.Core.Commands.CircuitBreaker;
using System;

namespace Vulcain.Core.Commands.Metrics.Publishers
{
    public interface IMetricsPublisherCommand
    {
        void Run(Action<string> handler);
    }
}