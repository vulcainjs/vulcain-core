﻿using Vulcain.Core.Commands.Utils;
using System;
using System.Collections.Generic;

namespace Vulcain.Core.Commands
{
    public enum EventType
    {
        FALLBACK_REJECTION,
        FALLBACK_SUCCESS,
        FALLBACK_FAILURE,
        THREAD_POOL_REJECTED,
        TIMEOUT,
        FAILURE,
        SUCCESS,
        SHORT_CIRCUITED,
        SEMAPHORE_REJECTED,
        RESPONSE_FROM_CACHE
    }

    internal class ExecutionResult
    {
        public  List<EventType> events;
        private  int executionTime=-1;
        private  Exception exception;
        private  long commandRunStartTimeInNMs;

        public ExecutionResult()
        {
        }

        public ExecutionResult(ExecutionResult copy)
        {
            this.executionTime = copy.ExecutionTime;
            this.commandRunStartTimeInNMs = copy.commandRunStartTimeInNMs;
            this.events = new List<EventType>( copy.events );
        }

        //private  int numEmissions;
        //private  int numFallbackEmissions;

        public void AddEvent(EventType evt)
        {
            if (events == null)
                events = new List<EventType>(4);
            events.Add(evt);
        }

        public Exception Exception
        {
            get
            {
                return exception;
            }

            set
            {
                exception = value;
            }
        }

        public int ExecutionTime
        {
            get
            {
                return executionTime;
            }

            set
            {
                executionTime = value;
                if (executionTime >= 0)
                {
                    this.CommandRunStartTimeInMs = Clock.GetInstance().EllapsedTimeInMs - executionTime;// * 1000 * 1000; // 1000*1000 will convert the milliseconds to nanoseconds
                }
                else
                {
                    this.CommandRunStartTimeInMs = -1;
                }
            }
        }

        public long CommandRunStartTimeInMs
        {
            get
            {
                return commandRunStartTimeInNMs;
            }

            set
            {
                commandRunStartTimeInNMs = value;
            }
        }

        internal bool EventExists(EventType evt)
        {
            return events != null && events.Contains(evt);
        }

        /**
         * This method may be called many times for {@code EventType.EMIT} and {@code EventType.FALLBACK_EMIT}.
         * To save on storage, on the first time we see that event type, it gets added to the event list, and the count gets incremented.
         * @param eventType emission event
         * @return "updated" {@link ExecutionResult}
         */
        //public ExecutionResult addEmission(EventType eventType)
        //{
        //    switch (eventType)
        //    {
        //        case EMIT:
        //            if (events.contains(EventType.EMIT))
        //            {
        //                return new ExecutionResult(events, ExecutionTime, Exception, numEmissions + 1, numFallbackEmissions);
        //            }
        //            else
        //            {
        //                return new ExecutionResult(getUpdatedList(this.events, EventType.EMIT), ExecutionTime, Exception, numEmissions + 1, numFallbackEmissions);
        //            }
        //        case FALLBACK_EMIT:
        //            if (events.contains(EventType.FALLBACK_EMIT))
        //            {
        //                return new ExecutionResult(events, ExecutionTime, Exception, numEmissions, numFallbackEmissions + 1);
        //            }
        //            else
        //            {
        //                return new ExecutionResult(getUpdatedList(this.events, EventType.FALLBACK_EMIT), ExecutionTime, Exception, numEmissions, numFallbackEmissions + 1);
        //            }
        //        default: return this;
        //    }
        //}
    }
}
