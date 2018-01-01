﻿// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Vulcain.Core.Commands.CircuitBreaker
{
    public interface ICircuitBreaker
    {
        bool AllowRequest { get; }

        void MarkSuccess();
        bool IsOpen();
    }
}
