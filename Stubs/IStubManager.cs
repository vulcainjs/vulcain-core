using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Vulcain.Core.Stubs
{
    public interface IStubManager
    {
        bool Enabled { get; }
        void Initialize(object sessions, Func<IDictionary<string,object>, Task> saveHandler);
        object ApplyHttpStub(string url, string verb);
        object ApplyServiceStub(string serviceName, string serviceVersion, string verb, object data);
        Task<HttpResponse> TryGetMockValue(RequestContext ctx, ActionMetadata metadata, string verb, object args);
        Task SaveStub(RequestContext ctx, ActionMetadata metadata, string verb, object args, HttpResponse result);
    }

    internal class DummyStubManager : IStubManager
    {
        public bool Enabled => false;

        public void Initialize(object sessions, Action saveHandler)
        {
        }

        public object ApplyHttpStub(string url, string verb)
        {
            return null;
        }

        public object ApplyServiceStub(string serviceName, string serviceVersion, string verb, object data)
        {
            return null;
        }

        public Task<HttpResponse> TryGetMockValue(RequestContext ctx, ActionMetadata metadata, string verb, object args)
        {
            return Task.FromResult<HttpResponse>(null);
        }

        public Task SaveStub(RequestContext ctx, ActionMetadata metadata, string verb, object args, HttpResponse result)
        {
            return Task.CompletedTask;
        }
    }
}
