using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Vulcain.Core.Configuration.Sources
{
    internal class MemoryConfigurationSource : ILocalConfigurationSource
    {
        private Dictionary<string, ConfigurationItem> _values = new Dictionary<string, ConfigurationItem>();
        public Task<DataSource> ReadProperties(int timeout = 0) {
            return Task.FromResult(new DataSource(this._values.Values));
        }

        /// <summary>
        /// Set a update a new property
        /// </summary>
        /// <param name="name">Property name</param>
        /// <param name="value">Property value</param>
        public void set(string name, object value)
        {
            this._values[name] = new ConfigurationItem { Value = value, Key = name };
        }

        public PropertyValue Get(string name)
        {
            if (!this._values.TryGetValue(name, out ConfigurationItem item) && !item.Deleted)
            {
                return new PropertyValue(item.Value);
            }
            return PropertyValue.Undefined;
        }
    }

    internal class MockConfigurationSource : MemoryConfigurationSource, IRemoteConfigurationSource
    {
        public Task<DataSource> PollProperties(int timeout = 0)
        {
            return ReadProperties();
        }
    }
}
