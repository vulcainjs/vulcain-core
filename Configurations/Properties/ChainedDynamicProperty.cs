// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;

namespace Vulcain.Core.Configuration
{
    /// <summary>
    /// Create a chained property composed with a dynamic property and fallback properties used if the main property is not defined.
    /// A chained property works with fallback values. If the first is not defined, the value is founded in the first property values defined in the fallback list
    /// and then the default value.
    /// </summary>
    /// <typeparam name="T">Property type</typeparam>
    [System.Diagnostics.DebuggerDisplay("{Value}")]
    internal class ChainedDynamicProperty<T> : DynamicProperty<T>
    {
        private object _sync = new object();
        private string[] _fallbackProperties;
        private IDynamicProperty<T> _activeProperty;
        private IDisposable _subscription;

        internal ChainedDynamicProperty(ConfigurationManager manager, string name, string[] fallbackProperties, PropertyValue defaultValue): base(manager, name, defaultValue)
        {
            if (!fallbackProperties.Contains(name))
            {
                _fallbackProperties = new string[fallbackProperties.Length + 1];
                _fallbackProperties[0] = name;
                Array.Copy(fallbackProperties, 0, _fallbackProperties, 1, fallbackProperties.Length);
            }
            else
            {
                _fallbackProperties = fallbackProperties;
            }
            if (_fallbackProperties.Length < 1) throw new ArgumentException("You must provided at least 2 properties.");

            Reset();

            _subscription = manager.PropertyChanged.Subscribe(dp => this.Reset(dp));
        }

        public override T Value
        {
            get
            {
                T v;
                if (this.IsDefined)
                    v = this.value;
                else if (this._activeProperty != null)
                    v = this._activeProperty.Value;
                else
                 v = this.defaultValue.GetValue<T>();
                return v;
            }
        }

        protected void Reset(IDynamicProperty<object> dp = null)
        {
            lock (_sync)
            {
                if (this.notifying)
                    return;

                if (dp != null && this._fallbackProperties.Contains(dp.Name))
                    return;

                this.notifying = true;
                this._activeProperty = null;
                var oldValue = this.value;

                // Find first property value in the chain
                foreach (var propertyName in this._fallbackProperties)
                {
                    var p = this.manager.GetProperty<T>(propertyName);
                    if (p != null && p.IsDefined)
                    {
                        this._activeProperty = p;
                        break;
                    }
                }

                if (!Object.Equals(oldValue, this.Value))
                    this.OnPropertyChanged();

                this.notifying = false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_subscription != null)
            {
                _subscription.Dispose();
                _subscription = null;
            }

            base.Dispose(disposing);
        }
    }
}
