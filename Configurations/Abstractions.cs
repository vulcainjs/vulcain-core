// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vulcain.Core.Configuration
{
    public interface IConfigurationSource
    {
        PropertyValue Get(string name);
    }

    public interface ILocalConfigurationSource : IConfigurationSource
    {
        Task<DataSource> ReadProperties(int timeout = 0);
    }

    public interface IRemoteConfigurationSource : IConfigurationSource
    {
        Task<DataSource> PollProperties(int timeout = 0);
    }

    public struct ConfigurationItem
    {
        public string Key;
        public object Value;
        public string LastUpdate;
        public bool Encrypted;
        public bool Deleted;

        public ConfigurationItem(string key, object value, string lastUpdate = null, bool encrypted = false, bool deleted = false)
        {
            this.Key = key;
            this.Value = value;
            this.LastUpdate = lastUpdate;
            this.Encrypted = encrypted;
            this.Deleted = deleted;
        }
    }

    /// <summary>
    /// This class represents a result from a poll of configuration source
    /// </summary>
    public class DataSource
    {
        public IEnumerable<ConfigurationItem> Values { get; }

        public DataSource(IEnumerable<ConfigurationItem> values)
        {
            this.Values = values;
        }
    }

    /// <summary>
    /// A dynamic property created with <see cref="IDynamicProperties"/>
    /// </summary>
    /// <typeparam name="T">Property type</typeparam>
    public interface IDynamicProperty<T>
    {
        /// <summary>
        /// Property name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Current value
        /// </summary>
        T Value { get; }

        /// <summary>
        /// Update local property value. This value can be overrided by a <see cref="IConfigurationSource"/>. 
        /// Doesn't update source values.
        /// </summary>
        /// <param name="value">Property value</param>
        void Set(T value);

        IObservable<IDynamicProperty<T>> PropertyChanged { get; }

        bool IsDefined { get; }
    }

    internal interface IUpdatableProperty
    { // Internal interface
        void UpdateValue(ConfigurationItem item);
    }

    public struct PropertyValue
    {
        public static PropertyValue Undefined { get; } = new PropertyValue(null, false); 
        private object _value;
        public bool IsDefined { get; }

        public T GetValue<T>()
        {
            return IsDefined ? (T)_value : default(T);
        }

        public PropertyValue(object value, bool defined=true)
        {
            this._value = value;
            this.IsDefined = defined;
        }
    }
}