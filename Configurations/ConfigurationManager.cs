// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;
using System.Linq;
using System.Reactive.Subjects;
using Vulcain.Core.Utils;
using Vulcain.Core.Configuration.Sources;

namespace Vulcain.Core.Configuration
{
    internal class ConfigurationManager
    {
        private Timer _timer;
        public bool isRunning;
        private PrioritizedSourceValue _values;
        private Dictionary<string, IDynamicProperty<object>> _dynamicProperties = new Dictionary<string, IDynamicProperty<object>>();
        private bool disposed;
        private ReplaySubject<IDynamicProperty<object>> _propertyChanged;
        private EnvironmentVariableSource _environmentVariables = new EnvironmentVariableSource();
        public int PollingIntervalInSeconds = 60;
        public int SourceTimeoutInMs = 1500;
        internal Dictionary<string, IDynamicProperty<object>> Properties => this._dynamicProperties;

        public IObservable<IDynamicProperty<object>> PropertyChanged {
            get {
                if (this._propertyChanged == null) {
                    this._propertyChanged = new ReplaySubject<IDynamicProperty<object>>(1);
                }
                return this._propertyChanged;
            }
        }

        public ConfigurationManager(
        int pollingIntervalInSeconds = 60,
        int sourceTimeoutInMs = 1500) {
            this.PollingIntervalInSeconds = pollingIntervalInSeconds;
            this.SourceTimeoutInMs = sourceTimeoutInMs;
        }

        internal PropertyValue GetValueInSources(string name)
        {
            if (this._values == null)
            { // For testing
                this._values = new PrioritizedSourceValue();
            }
            var val = this._values.Get(name);
            return val;
        }

        public IDynamicProperty<T> CreateDynamicProperty<T>(string name, T defaultValue)
        {
            return CreateDynamicPropertyInternal<T>(name, new PropertyValue(defaultValue));
        }

        public IDynamicProperty<T> CreateDynamicProperty<T>(string name)
        {
            return CreateDynamicPropertyInternal<T>(name, PropertyValue.Undefined);
        }

        private IDynamicProperty<T> CreateDynamicPropertyInternal<T>(string name, PropertyValue defaultValue) {
            var dp = new DynamicProperty<T>(this, name, defaultValue);
            if (name != null) {
                var v = this.GetValueInSources(name);
                if(v.IsDefined)
                    dp.Set(v.GetValue<T>());
            }

            return dp;
        }

        public IDynamicProperty<T> CreateChainedDynamicProperty<T>(string name, string[] properties, T defaultValue = default(T))
        {
            return CreateChainedDynamicPropertyInternal<T>(name, properties, new PropertyValue(defaultValue));
        }

        public IDynamicProperty<T> CreateChainedDynamicProperty<T>(string name, string[] properties)
        {
            return CreateChainedDynamicPropertyInternal<T>(name, properties, PropertyValue.Undefined);
        }

        public IDynamicProperty<T> CreateChainedDynamicPropertyInternal<T>(string name, string[] properties, PropertyValue defaultValue ) {
            if (properties != null)
                properties = properties.Where(n => n != null).ToArray(); // remove null property
            if (properties == null || properties.Length == 0)
                return this.CreateDynamicPropertyInternal<T>(name, defaultValue);

            var dp = new ChainedDynamicProperty<T>(this, name, properties, defaultValue);
            var v = this.GetValueInSources(name);
            if (v.IsDefined)
                dp.Set(v.GetValue<T>());
            return dp;
        }

        private T GetValueFromEnvironmentVariable<T>(string name) {
            var pv = this._environmentVariables.Get(name);
            return pv.GetValue<T>();
        }

        public IDynamicProperty<T> GetProperty<T>(string name) {
            if(!this._dynamicProperties.TryGetValue(name, out IDynamicProperty<object> prop))
            {
                var pv = this._environmentVariables.Get(name);
                if (pv.IsDefined)
                    return (IDynamicProperty<T>)this.CreateDynamicProperty(name, pv.GetValue<T>());
            }

            return (IDynamicProperty<T>)prop;
        }

        /**
     * Initialize source(s) and return only when all sources are initialized
     * @param sources List of sources
     * @returns {Promise<T>}
     */
        internal async Task StartPolling(List<IConfigurationSource> sources, bool pollSources = true)
        {
            var localSources = new List<IConfigurationSource>();
            var remoteSources = new List<IRemoteConfigurationSource>();

            sources.Add(new FileConfigurationSource(Files.FindConfigurationFile(), ConfigurationDataType.VulcainConfig));

            foreach (var source in sources)
            {
                // Local properties has loaded first (less priority)
                if (source is ILocalConfigurationSource)
                {
                    localSources.Add(source);
                    await ((ILocalConfigurationSource)source).ReadProperties();
                }
                else
                {
                    var s = source as IRemoteConfigurationSource;
                    if (remoteSources.IndexOf(s) < 0)
                    {
                        remoteSources.Add(s);
                    }
                }
            }

            this._values = new PrioritizedSourceValue(localSources, remoteSources);

            // Run initialization
            var tries = 2;
            while (tries > 0)
            {
                if (await this.Polling(3000, false))
                {
                    // All sources are OK
                    if (pollSources)
                        this.RepeatPolling();
                    this.isRunning = true;
                    return;
                }

                tries--;
                if (tries > 0)
                    Service.Log.Info(null, () => "CONFIG: Some dynamic properties sources failed. Retry polling.");
            }

            if (!Service.IsDevelopment)
            {
                throw new Exception("CONFIG: Cannot read properties from sources. Program is stopped.");
            }
            else
            {
                Service.Log.Info(null, () => "CONFIG: Cannot read properties from sources.");
            }
        }

        /**
         * for test only
         */
        internal async void ForcePolling(IRemoteConfigurationSource src = null, bool reset = false)
        {
            if (reset)
                this._values = null;

            if (src != null)
            {
                if (this._values == null)
                {
                    this._values = new PrioritizedSourceValue(null, new IRemoteConfigurationSource[] { src });
                }
                else {
                    this._values.RemoteSources.Add(src);
                }
            }
            await this.Polling(3000, false);
        }

        /**
         * Pull properties for all sources
         *
         * @private
         * @param {any} [timeout]
         * @returns
         *
         * @memberOf ConfigurationManager
         */
        private async Task<bool> Polling(int timeout = 0, bool pollSources = true)
        {
            var ok = true;

            try
            {
                var list = this._values.RemoteSources;
                if (this.disposed || list == null) return false;

                var tasks = new Task<DataSource>[list.Count];
                var i = 0;
                foreach (var src in list)
                {
                    tasks[i++] =
                        // pollProperties cannot failed
                        src.PollProperties(timeout > 0 ? timeout : this.SourceTimeoutInMs);
                }

                await Task.WhenAll(tasks);

                // Ignore null result
                foreach(var res in tasks) {

                    if (res.Result == null)
                    {
                        ok = false;
                    }
                    else
                        this.OnPropertiesChanged(res.Result);
                }
            }
            catch (Exception e)
            {
                ok = false;
                Service.Log.Error(null, e, () => "CONFIG: Error when polling sources");
            }

            // Restart
            if (pollSources)
                this.RepeatPolling();

            return ok;
        }

        private void OnPropertiesChanged(DataSource data)
        {
            if (data.Values == null)
                return;

            foreach (var item in data.Values)
            {
                if(!this._dynamicProperties.TryGetValue(item.Key, out IDynamicProperty<object> dp))
                {
                    dp = this.CreateDynamicProperty<object>(item.Key);
                }
                else if (dp is IUpdatableProperty)
                {
                    ((IUpdatableProperty)dp).UpdateValue(item);
                }
            }
        }

        internal void OnPropertyChanged(IDynamicProperty<object> dp)
        {
            var tmp = this._propertyChanged;
            if (tmp != null)
                tmp.OnNext(dp);
        }

        private void RepeatPolling()
        {
            if (!this.disposed && this._values.RemoteSources.Count > 0)
            {
                _timer = new Timer(new TimerCallback(async (s)=> await Polling()), null, 0, this.PollingIntervalInSeconds * 1000);
            }
            else if(_timer!=null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        /**
/// Reset configuration and properties.
/// All current properties will be invalid and all current sources will be lost.
/// </summary>
/// <param name="pollingIntervalInSeconds"></param>
/// <param name="sourceTimeoutInMs"></param>
*/
        public void Reset(int pollingIntervalInSeconds = 0, int sourceTimeoutInMs = 0)
        {
            if (pollingIntervalInSeconds > 0)
                this.PollingIntervalInSeconds = pollingIntervalInSeconds;
            if (sourceTimeoutInMs > 0)
                this.SourceTimeoutInMs = sourceTimeoutInMs;

            //this._propertyChanged.dispose();
            this._propertyChanged = null;

            var tmp = this._dynamicProperties;

            if (tmp != null)
            {
                foreach (var prop in tmp.Values)
                {
                    if (prop is IDisposable)
                        ((IDisposable)prop).Dispose();
                }
                tmp.Clear();
            }
        }

        public void Dispose()
        {
            if (_timer != null)
                _timer.Dispose();
            _timer = null;
            this.Reset();
            this.disposed = true;
        }
    }
}

