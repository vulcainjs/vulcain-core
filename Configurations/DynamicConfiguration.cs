// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;

namespace Vulcain.Core.Configuration
{
    /// <summary>
    /// Provides dynamic properties updating when the config is changed.
    /// Accessing a dynamic property is very fast and thread safe. The last value is cached and updated on the fly from a <see cref="IConfigurationSource"/> at fixed interval.
    /// Updates are made using polling requests on a list of sources.
    /// <para>
    /// Dynamic properties are read only. You can set a value but it will be valid only as a default value.
    /// </para>
    /// <para>
    /// DynamicProperty objects are not subject to normal garbage collection.
    /// They should be used only as a static value that lives for the
    /// lifetime of the program.
    /// </para>
    /// <code>
    /// var i = DynamicProperties.Instance.GetProperty<int>("prop1");
    /// var i2 = DynamicProperties.Instance.GetOrDefaultProperty<int>("prop1", 1);
    /// </code>
    /// </summary>
    public static class DynamicConfiguration
    {
        /**
         * For test only - Do not use directly
         */
        private static ConfigurationManager _manager = new ConfigurationManager();
        private static ConfigurationSourceBuilder _builder;

        /**
         * subscribe for a global property changed
         */
        public static IObservable<IDynamicProperty<object>> PropertyChanged
        {
            get
            {
                return DynamicConfiguration._manager.PropertyChanged;
            }
        }

        /**
         * Get a property
         */
        public static IDynamicProperty<T> GetProperty<T>(string name, T value = default(T))
        {
            var p = DynamicConfiguration._manager.GetProperty<T>(name);
            if (p == null)
            {
                p = DynamicConfiguration._manager.CreateDynamicProperty(name, value);
            }
            return p;
        }

        public static IDynamicProperty<T> GetChainedProperty<T>(string name, T defaultValue, params string[] fallbackPropertyNames)
        {
            var p = DynamicConfiguration._manager.GetProperty<T>(name);
            if (p == null)
            {
                p = DynamicConfiguration._manager.CreateChainedDynamicProperty(name, fallbackPropertyNames, defaultValue);
            }
            return p;
        }

        /**
         * get a chained property for the current service.
         * Properties chain is: service.version.name->service.name->domain.name->name
         * @param name property name
         * @param defaultValue
         * @returns {IDynamicProperty<T>}
         */
        public static IDynamicProperty<T> GetChainedConfigurationProperty<T>(string name, T defaultValue = default(T), string commandName = null)
        {
            var p = DynamicConfiguration._manager.GetProperty<T>(name);
            if (p != null)
                return p;
            Service.RegisterPropertyAsDependency(name, defaultValue);

            var fullName = commandName != null ? commandName + "." + name : name;
            var chain = new List<string>() {
                    Service.ServiceName + "." + Service.ServiceVersion + "." + fullName,
                    Service.ServiceName + "." + fullName
                };

            if (commandName != null)
            {
                chain.Add(fullName);
            }

            if (Service.DomainName != null)
                chain.Add(Service.DomainName + "." + name);

            chain.Add(name);

            return DynamicConfiguration.GetChainedProperty<T>(
                name,
                defaultValue,
                chain.ToArray());
        }

        /**
         * Get a property value by name
         *
         * @static
         * @template T
         * @param {string} name
         * @returns
         *
         * @memberOf DynamicConfiguration
         */
        public static T GetPropertyValue<T>(string name)
        {
            var p = DynamicConfiguration.GetProperty<T>(name);
            return p.Value;
        }

        /// <summary>
        /// Initialize dynamic properties configuration. Can be call only once and before any call to DynamicProperties.instance.
        /// </summary>
        /// <param name="pollingIntervalInSeconds">Polling interval in seconds (default 60)</param>
        /// <param name="sourceTimeoutInMs">Max time allowed to a source to retrieve new values (Cancel the request but doesn't raise an error)</param>
        /// <returns>ConfigurationSourceBuilder</returns>
        internal static ConfigurationSourceBuilder GetBuilder(int pollingIntervalInSeconds = 0, int sourceTimeoutInMs = 0)
        {
            if (pollingIntervalInSeconds > 0)
                DynamicConfiguration._manager.PollingIntervalInSeconds = pollingIntervalInSeconds;
            if (sourceTimeoutInMs > 0)
                DynamicConfiguration._manager.SourceTimeoutInMs = sourceTimeoutInMs;

            if (DynamicConfiguration._builder == null)
            {
                DynamicConfiguration._builder = new ConfigurationSourceBuilder(DynamicConfiguration._manager);
            }
            return DynamicConfiguration._builder;
        }

        /**
         *
         * @param pollingIntervalInSeconds For test only
         */
        internal static ConfigurationManager Reset(int pollingIntervalInSeconds = 0)
        {
            DynamicConfiguration._manager = new ConfigurationManager();
            if (pollingIntervalInSeconds > 0)
                DynamicConfiguration._manager.PollingIntervalInSeconds = pollingIntervalInSeconds;
            return DynamicConfiguration._manager;
        }
    }
}