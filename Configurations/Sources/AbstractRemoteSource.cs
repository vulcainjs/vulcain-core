using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Vulcain.Core.Configuration.Sources
{
    public abstract class AbstractRemoteSource : IRemoteConfigurationSource
    {
        private Dictionary<string, ConfigurationItem> _values = new Dictionary<string, ConfigurationItem>();

        public PropertyValue Get(string name)
        {
            if (!this._values.TryGetValue(name, out ConfigurationItem item) && !item.Deleted)
            {
                return new PropertyValue(item.Value);
            }
            return PropertyValue.Undefined;
        }

        public abstract Task<DataSource> PollProperties(int timeout = 0);

        protected void MergeChanges(Dictionary<string, ConfigurationItem> changes)
        {
            if (changes == null)
                return;

            foreach (var item in changes.Values)
            {
                if (!item.Deleted)
                    this._values[item.Key] = item;
                else
                    this._values.Remove(item.Key);
            }
        }
    }
}
