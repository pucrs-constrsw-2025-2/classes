using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ClasseMicroservice.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace ClasseMicroservice.Tests.Integration.External
{
    [Trait("Category", "External")]
    public class ClassesApiExternalIntegrationTests : IClassFixture<MongoContainerFixture>, IClassFixture<WireMockOAuthFixture>
    {
        private readonly MongoContainerFixture _mongo;
        private readonly WireMockOAuthFixture _oauth;

        private sealed class OAuthStubHandler : DelegatingHandler
        {
            private readonly string _host;
            private readonly int _port;
            public OAuthStubHandler(string host, int port, HttpMessageHandler inner) : base(inner)
            {
                _host = host;
                _port = port;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                var uri = request.RequestUri;
                if (uri != null && string.Equals(uri.Host, _host, StringComparison.OrdinalIgnoreCase) && uri.Port == _port && uri.AbsolutePath.Equals("/api/v1/validate", StringComparison.OrdinalIgnoreCase) && request.Method == HttpMethod.Post)
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                }
                return base.SendAsync(request, cancellationToken);
            }
        }

        private sealed class NoProxyHttpClientFactory : IHttpClientFactory
        {
            private readonly string _host;
            private readonly int _port;
            public NoProxyHttpClientFactory(string host, int port)
            {
                _host = host;
                _port = port;
            }
            public HttpClient CreateClient(string name)
            {
                var inner = new System.Net.Http.SocketsHttpHandler { UseProxy = false, AllowAutoRedirect = false };
                var stub = new OAuthStubHandler(_host, _port, inner);
                return new HttpClient(stub, disposeHandler: true)
                {
                    Timeout = TimeSpan.FromSeconds(10)
                };
            }
        }

        public ClassesApiExternalIntegrationTests(MongoContainerFixture mongo, WireMockOAuthFixture oauth)
        {
            _mongo = mongo;
            _oauth = oauth;
        }

        private HttpClient CreateClient(bool disableAuth = false)
        {
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((ctx, cfg) =>
                    {
                        var dict = new Dictionary<string, string?>
                        {
                            ["ConnectionStrings:MongoDb"] = _mongo.ConnectionString,
                            ["MongoDbSettings:DatabaseName"] = _mongo.DatabaseName,
                            ["MongoDbSettings:CollectionName"] = "Classes",
                            ["DatabaseSettings:ConnectionString"] = _mongo.ConnectionString,
                            ["DatabaseSettings:DatabaseName"] = _mongo.DatabaseName,
                            ["OAUTH_INTERNAL_PROTOCOL"] = _oauth.Protocol,
                            ["OAUTH_INTERNAL_HOST"] = _oauth.Host,
                            ["OAUTH_INTERNAL_API_PORT"] = _oauth.Port.ToString(),
                            ["ENABLE_SWAGGER"] = "false"
                        };
                        if (disableAuth)
                        {
                            dict["DISABLE_AUTH"] = "true";
                        }
                        cfg.AddInMemoryCollection(dict!);
                    });
                    builder.ConfigureServices(services =>
                    {
                        // Override IHttpClientFactory to ensure no proxy and stub OAuth validate calls
                        services.RemoveAll<IHttpClientFactory>();
                        services.AddSingleton<IHttpClientFactory>(sp => new NoProxyHttpClientFactory(_oauth.Host, _oauth.Port));
                    });
                });

            var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = true });
            client.DefaultRequestHeaders.Add("Authorization", "Bearer external.test.token");
            return client;
        }

        [Fact]
        public async Task Create_Then_GetById_Persists_In_Mongo()
        {
            var client = CreateClient(disableAuth: true);

            var dto = new
            {
                ClassNumber = "EXT-01",
                Year = 2025,
                Semester = 2,
                Schedule = "Mon 18:00",
                Course = new { Id = "COURSE-EXT" }
            };

            var respCreate = await client.PostAsJsonAsync("/api/v1/classes", dto);
            Assert.Equal(HttpStatusCode.Created, respCreate.StatusCode);
            var created = await respCreate.Content.ReadFromJsonAsync<Class>();
            Assert.NotNull(created);

            var get = await client.GetAsync($"/api/v1/classes/{created!.Id}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var fetched = await get.Content.ReadFromJsonAsync<Class>();
            Assert.Equal(created.Id, fetched!.Id);
        }

        [Fact]
        public async Task Returns401_When_Missing_Authorization_Header()
        {
            var client = CreateClient();
            client.DefaultRequestHeaders.Remove("Authorization");
            var resp = await client.GetAsync("/api/v1/classes");
            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }
    }
}
