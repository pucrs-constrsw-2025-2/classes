using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ClasseMicroservice.API.Repositories;
using ClasseMicroservice.Domain.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace ClasseMicroservice.Tests.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((ctx, cfg) =>
            {
                // Provide predictable OAuth endpoint values (not strictly required for the fake client)
                var dict = new[]
                {
                    new KeyValuePair<string,string>("OAUTH_INTERNAL_PROTOCOL","http"),
                    new KeyValuePair<string,string>("OAUTH_INTERNAL_HOST","oauth"),
                    new KeyValuePair<string,string>("OAUTH_INTERNAL_API_PORT","8080"),
                    new KeyValuePair<string,string>("ENABLE_SWAGGER","true")
                };
                cfg.AddInMemoryCollection(dict);
            });

            builder.ConfigureServices(services =>
            {
                // Replace repository with in-memory implementation for integration tests
                var toRemove = services.Where(d => d.ServiceType == typeof(IClassRepository)).ToList();
                foreach (var d in toRemove) services.Remove(d);
                services.AddSingleton<IClassRepository, InMemoryClassRepository>();

                // Replace IHttpClientFactory with a fake that always returns 200 OK for /validate
                services.AddSingleton<IHttpClientFactory>(sp =>
                {
                    var handler = new StubHandler(req =>
                    {
                        if (req.RequestUri != null && req.RequestUri.AbsolutePath.Contains("/validate", StringComparison.OrdinalIgnoreCase))
                        {
                            return new HttpResponseMessage(HttpStatusCode.OK);
                        }
                        return new HttpResponseMessage(HttpStatusCode.NotFound);
                    });
                    return new TestHttpClientFactory(new HttpClient(handler));
                });
            });
        }

        private sealed class TestHttpClientFactory : IHttpClientFactory
        {
            private readonly HttpClient _client;
            public TestHttpClientFactory(HttpClient client) => _client = client;
            public HttpClient CreateClient(string name = null) => _client;
        }

        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;
            public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) => _responder = responder;
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => Task.FromResult(_responder(request));
        }
    }
}
