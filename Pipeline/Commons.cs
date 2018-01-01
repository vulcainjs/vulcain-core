using System;
using System.Collections.Generic;
using System.Text;

namespace Vulcain.Core.Pipeline
{

    public enum Pipeline
    {
        Event,
        AsyncTask,
        HttpRequest,
        Test
    }


    public interface ICustomEvent
    {
        string Action { get; }
        string Schema { get; }
        object Params { get; }
    }

    public interface IRequestContext : IDisposable
    {
        /**
         * Span tracker
         */
        ISpanTracker Tracker { get; }

        /**
         * Current user or null
         *
         * @type {UserContext}
         */
        UserContext User { get; }

        /**
         * Scoped container
         *
         * @type {IContainer}
         */
        IContainer Container { get; }

        /**
         * Current locale
         *
         * @type {string}
         * @memberOf RequestContext
         */
        string Locale { get; }

        /**
         * Request host name
         *
         * @type {string}
         * @memberOf RequestContext
         */
        string HostName { get; }
        RequestData RequestData { get; }
        HttpRequest Request { get; }
        IRequestContext Parent { get; }
        /**
         * Send custom event from current service
         *
         * @param {string} action action event
         * @param {*} [params] action parameters
         * @param {string} [schema] optional schema
         */
        void SendCustomEvent(string action, object parms = null, string schema = null);

        /**
         * Create a new command
         * Throws an exception if the command is unknown
         *
         * @param {string} name Command name
         * @returns {ICommand} A command
         */
        T GetCommand<T>(string name, params object[] args) where T : ICommand;
        /**
          * Log an error
          *
          * @param {Error} error Error instance
          * @param {string} [msg] Additional message
          *
          */
        void LogError(Exception error, Func<string> msg = null);

        /**
         * Log a message info
         *
         * @param {string} msg Message format (can include %s, %j ...)
         * @param {...Array<string>} params Message parameters
         *
         */
        void LogInfo(Func<string> msg);

        /**
         * Log a verbose message. Verbose message are enable by service configuration property : enableVerboseLog
         *
         * @param {string} msg Message format (can include %s, %j ...)
         *
         */
        void LogVerbose(Func<string> msg);
    }

    public class RequestData
    {
        public string VulcainVerb { get; }
        public string CorrelationId { get; }
        public string Action { get; }
        public string Domain { get; }
        public string Schema { get; }
        public object Params { get; }
        public int MaxByPage { get; }
        public int Page { get; }
        public string InputSchema { get; }
        public object Body { get; }
    }
}
