// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace RoutingSample.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
        }

        public void Configure(IApplicationBuilder app)
        {
            var endpoint1 = new RouteHandler((c) =>
            {
                return c.Response.WriteAsync($"match1, route values - {string.Join(", ", c.GetRouteData().Values)}");
            });

            var endpoint2 = new RouteHandler((c) => c.Response.WriteAsync("Hello, World!"));

            var routeBuilder = new RouteBuilder(app)
            {
                DefaultHandler = endpoint1,
            };

            routeBuilder.MapRoute("api/status/{item}", c => c.Response.WriteAsync($"{c.GetRouteValue("item")} is just fine."));


            routeBuilder.AddPrefixRoute("api/store", endpoint1);
            routeBuilder.AddPrefixRoute("hello/world", endpoint2);

            routeBuilder.AddPrefixRoute("", endpoint2);

            app.UseRouter(routeBuilder.Build());
        }

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}