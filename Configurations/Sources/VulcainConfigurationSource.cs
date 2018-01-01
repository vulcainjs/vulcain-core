using System.Net.Http;
using System.Threading.Tasks;

namespace Vulcain.Core.Configuration.Sources
{
    internal class VulcainConfigurationSource : HttpConfigurationSource
    {

        public VulcainConfigurationSource(string uri, object options) : base(uri)
        {
        }

        protected override void PrepareRequest(HttpClient client)
        {
            if (Service.VulcainToken != null)
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("ApiKey ", Service.VulcainToken);
        }

        protected override string CreateRequestUrl()
        {
            return this.uri + "?$query=" + JSON.Stringify(new { lastUpdate = this.lastUpdate });
        }

        public override Task<DataSource> PollProperties(int timeoutInMs = 0)
        {

            if (Service.VulcainToken == null && !Service.IsTestEnvironnment)
            {
                return Task.FromResult<DataSource>(null);
            }

            return base.PollProperties(timeoutInMs);
        }
    }
}