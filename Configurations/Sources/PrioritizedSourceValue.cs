using System;
using System.Collections.Generic;
using System.Text;

namespace Vulcain.Core.Configuration.Sources
{
    internal class PrioritizedSourceValue : IConfigurationSource
    {
        private List<IConfigurationSource> chain;
        public List<IRemoteConfigurationSource> RemoteSources { get; }


        public PrioritizedSourceValue(IEnumerable<IConfigurationSource> localSources=null, IEnumerable<IRemoteConfigurationSource> remoteSources = null)
        {
            this.chain = new List<IConfigurationSource>();
            RemoteSources = new List<IRemoteConfigurationSource>();

            if (remoteSources != null)
            {
                this.chain.AddRange(remoteSources);
                RemoteSources.AddRange(remoteSources);
            }
            if (localSources != null)
                this.chain.AddRange(localSources);
        }

        public PropertyValue Get(string name)
        {
            foreach (var pv in this.chain)
            {
                var val = pv.Get(name);
                if (val.IsDefined) return val;
            }
            return PropertyValue.Undefined;
        }
    }
}
