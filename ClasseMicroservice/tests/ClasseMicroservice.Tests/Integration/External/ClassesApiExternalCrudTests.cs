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
    public class ClassesApiExternalCrudTests : IClassFixture<MongoContainerFixture>, IClassFixture<WireMockOAuthFixture>
    {
        private readonly MongoContainerFixture _mongo;
        private readonly WireMockOAuthFixture _oauth;

        private sealed class OAuthStubHandler : DelegatingHandler
        {
            private readonly string _host;
            private readonly int _port;
            public OAuthStubHandler(string host, int port, HttpMessageHandler inner) : base(inner)
            {
                _host = host; _port = port;
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
            private readonly string _host; private readonly int _port;
            public NoProxyHttpClientFactory(string host, int port) { _host = host; _port = port; }
            public HttpClient CreateClient(string name)
            {
                var inner = new System.Net.Http.SocketsHttpHandler { UseProxy = false, AllowAutoRedirect = false };
                var stub = new OAuthStubHandler(_host, _port, inner);
                return new HttpClient(stub, disposeHandler: true) { Timeout = TimeSpan.FromSeconds(15) };
            }
        }

        public ClassesApiExternalCrudTests(MongoContainerFixture mongo, WireMockOAuthFixture oauth)
        {
            _mongo = mongo; _oauth = oauth;
        }

        private HttpClient CreateClient()
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
                        cfg.AddInMemoryCollection(dict!);
                    });
                    builder.ConfigureServices(services =>
                    {
                        services.RemoveAll<IHttpClientFactory>();
                        services.AddSingleton<IHttpClientFactory>(sp => new NoProxyHttpClientFactory(_oauth.Host, _oauth.Port));
                    });
                });

            var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = true });
            client.DefaultRequestHeaders.Add("Authorization", "Bearer external.test.token");
            return client;
        }

        [Fact]
        public async Task Crud_Class_And_Patch_Then_Delete()
        {
            var client = CreateClient();

            var dto = new { ClassNumber = "EXT-CRUD", Year = 2024, Semester = 1, Schedule = "Tue 08:00" };
            var create = await client.PostAsJsonAsync("/api/v1/classes", dto);
            Assert.Equal(HttpStatusCode.Created, create.StatusCode);
            var created = await create.Content.ReadFromJsonAsync<Class>();
            Assert.NotNull(created);

            var patch = new Dictionary<string, object> { ["Year"] = 2026, ["Schedule"] = "Fri 09:00" };
            var patchedResp = await client.PatchAsJsonAsync($"/api/v1/classes/{created!.Id}", patch);
            Assert.Equal(HttpStatusCode.OK, patchedResp.StatusCode);
            var patched = await patchedResp.Content.ReadFromJsonAsync<Class>();
            Assert.Equal(2026, patched!.Year);
            Assert.Equal("Fri 09:00", patched.Schedule);

            var del = await client.DeleteAsync($"/api/v1/classes/{created.Id}");
            Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);
            var afterDel = await client.GetAsync($"/api/v1/classes/{created.Id}");
            Assert.Equal(HttpStatusCode.NotFound, afterDel.StatusCode);
        }

        [Fact]
        public async Task Exams_Subresource_Flow_Add_List_Get_Update_Delete()
        {
            var client = CreateClient();

            var create = await client.PostAsJsonAsync("/api/v1/classes", new { ClassNumber = "EXT-EXAMS", Year = 2025, Semester = 2, Schedule = "Thu 14:00" });
            Assert.Equal(HttpStatusCode.Created, create.StatusCode);
            var cls = await create.Content.ReadFromJsonAsync<Class>();

            var exam = new Exam { Name = "Prova EXT", Date = DateTime.UtcNow, Weight = 50 };
            var add = await client.PostAsJsonAsync($"/api/v1/classes/{cls!.Id}/exams", exam);
            Assert.Equal(HttpStatusCode.Created, add.StatusCode);

            var list = await client.GetAsync($"/api/v1/classes/{cls.Id}/exams");
            Assert.Equal(HttpStatusCode.OK, list.StatusCode);
            var exams = await list.Content.ReadFromJsonAsync<List<Exam>>();
            Assert.NotNull(exams);
            Assert.Single(exams!);
            var e = exams![0];

            var get = await client.GetAsync($"/api/v1/classes/{cls.Id}/exams/{e.Id}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var single = await get.Content.ReadFromJsonAsync<Exam>();
            Assert.Equal("Prova EXT", single!.Name);

            var updated = new Exam { Name = "Prova EXT Editada", Date = single.Date, Weight = 60 };
            var put = await client.PutAsJsonAsync($"/api/v1/classes/{cls.Id}/exams/{e.Id}", updated);
            Assert.Equal(HttpStatusCode.OK, put.StatusCode);
            var afterPut = await put.Content.ReadFromJsonAsync<Exam>();
            Assert.Equal(60, afterPut!.Weight);

            var del = await client.DeleteAsync($"/api/v1/classes/{cls.Id}/exams/{e.Id}");
            Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);
            var getAfter = await client.GetAsync($"/api/v1/classes/{cls.Id}/exams/{e.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getAfter.StatusCode);
        }
    }
}
