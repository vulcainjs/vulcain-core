using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Vulcain.Core.Configuration.Sources
{
    public class HttpConfigurationSource : AbstractRemoteSource
    {
        protected string lastUpdate;
        protected string uri;

        public HttpConfigurationSource(string uri)
        {
            this.uri = uri;
        }

        protected virtual void PrepareRequest(HttpClient request)
        {
        }

        protected virtual string CreateRequestUrl()
        {
            var uri = this.uri;
            if (this.lastUpdate != null)
            {
                uri = uri + "?lastUpdate=" + this.lastUpdate;
            }
            return uri;
        }

        public override async Task<DataSource> PollProperties(int timeout = 0)
        {
            var uri = this.CreateRequestUrl();
                Dictionary<string, ConfigurationItem> values = null;

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders
                      .Accept
                      .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.BaseAddress = new Uri(uri);

                    this.PrepareRequest(client);

                    dynamic response = await client.GetStringAsync("");

                    if (response.body.error)
                    {
                        if (!Service.IsDevelopment)
                        {
                            Service.Log.Info(null, () => $"HTTP CONFIG: error when polling properties on { uri} - { response.body.error.message}");
                        }
                    }
                    else
                    {
                        values = new Dictionary<string, ConfigurationItem>();
                        var data = response.body?.value;
                        if (data)
                        {
                            foreach (var (p, v) in JSObject.PropertiesOf((object)data))
                            {
                                values[p] = (ConfigurationItem)v;
                            }
                            this.lastUpdate = DateTime.UtcNow.ToString("o");
                            MergeChanges(values);
                        }
                    }
                }
            }
            catch
            {
                Service.Log.Info(null, () => $"HTTP CONFIG: error when polling properties on { uri} - { (response.error && response.error.message) || response.status}");
            }
            return values != null ? new DataSource(values.Values) : null;
        }
    }
}
