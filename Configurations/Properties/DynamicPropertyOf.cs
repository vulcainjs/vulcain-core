// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reactive.Subjects;

namespace Vulcain.Core.Configuration
{
    [System.Diagnostics.DebuggerDisplay("{value}")]
    internal class DynamicProperty<T> : IDynamicProperty<T>, IUpdatableProperty
    {
        private bool _isDefined;
        public bool IsDefined => _isDefined && !removed;

        protected T value;
        protected bool notifying;
        private bool removed = false;
        private Lazy<ReplaySubject<IDynamicProperty<T>>> propertyChanged = new Lazy<ReplaySubject<IDynamicProperty<T>>>(() => new ReplaySubject<IDynamicProperty<T>>());
        protected PropertyValue defaultValue;
        protected ConfigurationManager manager;
        public string Name { get; }

        internal DynamicProperty(ConfigurationManager manager, string name, PropertyValue defaultValue)
        {
            this.Name = name;
            this.manager = manager;
            this.defaultValue = defaultValue;
            manager.Properties[name] = (IDynamicProperty<object>)this;
            if (defaultValue.IsDefined)
                this.OnPropertyChanged();
        }

        /// <summary>
        /// Current value
        /// </summary>
        public virtual T Value
        {
            get
            {
                return _isDefined ? (T)value : defaultValue.GetValue<T>();
            }
        }

        public IObservable<IDynamicProperty<T>> PropertyChanged
        {
            get
            {
                return propertyChanged.Value;
            }
        }

        /// <summary>
        /// Update local property value. This value can be overrided by a <see cref="IConfigurationSource"/>. 
        /// Doesn't update source values.
        /// </summary>
        /// <param name="value">Property value</param>
        public void Set(T value)
        {
            if (!Object.Equals(this.value, value))
            {
                this.value = value;
                _isDefined = true;
                OnPropertyChanged();
            }
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            OnPropertyChanged();
            propertyChanged = null;
            removed = true;
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }

        public override string ToString()
        {
            return Value != null ? Value.ToString() : String.Empty;
        }

        protected void OnPropertyChanged()
        {
            if (this.Name == null || this.notifying)
                return;

            this.notifying = true;

            try
            {
                propertyChanged.Value.OnNext(this);
                this.manager.OnPropertyChanged((IDynamicProperty<object>)this);
            }
            finally
            {
                this.notifying = false;
            }
        }

        void IUpdatableProperty.UpdateValue(ConfigurationItem item)
        {
            if (item.Deleted)
            {
                this.removed = true;
                Service.Log.info(null, () =>$"CONFIG: Removing property value for key { this.Name}");
                this.OnPropertyChanged();
                return;
            }

            if (!Object.Equals(this.value, item.Value))
            {
                if(item.Encrypted)
                {
                    this.value = (T)Convert.ChangeType(Service.Decrypt(item.Value as string), typeof(T));
                }
                else
                {
                    this.value = (T)item.Value;
                }
                var v = item.Encrypted ? "********" : item.Value;
                Service.Log.Info(null, () => $"CONFIG: Setting property value '{v}' for key { this.Name}");
                this.OnPropertyChanged();
                return;
            }
        }
        #endregion
    }
}
