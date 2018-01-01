using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vulcain.Core.Configuration.Sources;

namespace Vulcain.Core.Configuration
{
    /**
     * Helper for adding configuration source providing by DynamicConfiguration.init
     */
    internal class ConfigurationSourceBuilder
    {
        private List<IConfigurationSource> _sources;
            private ConfigurationManager _configurationManager;

    internal ConfigurationSourceBuilder(ConfigurationManager configurationManager) {
            _configurationManager = configurationManager;
            this._sources = new List<IConfigurationSource>();
        this.AddVulcainSource();
    }

    public ConfigurationSourceBuilder AddSource(IRemoteConfigurationSource source)
    {
        this._sources.Add(source);
        return this;
    }

        private ConfigurationSourceBuilder AddVulcainSource()
        {
            if (Service.VulcainServer != null)
            {
                if (Service.VulcainToken == null && !Service.IsTestEnvironnment)
                {
                    Service.Log.Info(null, () => "No token defined for reading configuration properties. Vulcain configuration source is ignored.");
                }
                else
                {
                    var uri = $"http://{Service.VulcainServer}/api/configforservice";
                    var options = new {
                        Environment = Service.StagingEnvironment,
                        Service = Service.ServiceName,
                        Version = Service.ServiceVersion,
                        Domain = Service.DomainName
                    };
                    this.AddSource(new VulcainConfigurationSource(uri, options));
                }
            }

            return this;
        }

    /*public addRestSource(uri:string)
    {
        this.addSource(new HttpConfigurationSource(uri));
        return this;
    }*/

    public ConfigurationSourceBuilder AddFileSource(string path, ConfigurationDataType  mode = ConfigurationDataType.Json)
    {
        this._sources.Add(new FileConfigurationSource(path, mode));
        return this;
    }

    public Task StartPolling()
    {
        return this._configurationManager.StartPolling(this._sources);
    }
}
}
