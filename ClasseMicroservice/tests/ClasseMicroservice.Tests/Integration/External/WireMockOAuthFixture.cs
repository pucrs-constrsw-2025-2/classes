using System;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;
using Xunit;

namespace ClasseMicroservice.Tests.Integration.External
{
    public class WireMockOAuthFixture : IDisposable
    {
        public WireMockServer Server { get; }
        public string Protocol => "http";
        public string Host => "127.0.0.1";
        public int Port => Server.Ports[0];

        public WireMockOAuthFixture()
        {
            Server = WireMockServer.Start(new WireMockServerSettings
            {
                Urls = new[] { "http://127.0.0.1:0" },
                StartAdminInterface = false,
                ReadStaticMappings = false
            });

            // Default mapping: POST /api/v1/validate -> 200 OK
            Server.Given(Request.Create()
                    .WithPath("/api/v1/validate")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200));
        }

        public void Dispose()
        {
            try { Server.Stop(); } catch { /* ignore */ }
            Server.Dispose();
        }
    }
}
