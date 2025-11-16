using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClasseMicroservice.API.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClasseMicroservice.Tests.Middleware
{
    public class OAuthValidationMiddlewareTests
    {
        private static DefaultHttpContext MakeContext(string path, string? authHeader = null)
        {
            var ctx = new DefaultHttpContext();
            ctx.Request.Path = path;
            if (!string.IsNullOrEmpty(authHeader))
                ctx.Request.Headers["Authorization"] = authHeader;
            return ctx;
        }

        private class StubHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _func;
            public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> func) => _func = func;
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => Task.FromResult(_func(request));
        }

        private static IHttpClientFactory MakeFactory(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            var handler = new StubHandler(responder);
            var client = new HttpClient(handler);
            var f = new Mock<IHttpClientFactory>();
            f.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);
            return f.Object;
        }

        [Fact]
        public async Task SkipsValidation_ForExcludedPaths()
        {
            var ctx = MakeContext("/swagger/index.html");
            var calledNext = false;
            var middleware = new OAuthValidationMiddleware(
                _ => { calledNext = true; return Task.CompletedTask; },
                MakeFactory(_ => new HttpResponseMessage(HttpStatusCode.OK)),
                new ConfigurationBuilder().Build(),
                Mock.Of<ILogger<OAuthValidationMiddleware>>()
            );

            await middleware.InvokeAsync(ctx);
            calledNext.Should().BeTrue();
        }

        [Fact]
        public async Task Returns401_WhenMissingAuthorization()
        {
            var ctx = MakeContext("/api/v1/classes");
            var middleware = new OAuthValidationMiddleware(
                _ => Task.CompletedTask,
                MakeFactory(_ => new HttpResponseMessage(HttpStatusCode.OK)),
                new ConfigurationBuilder().Build(),
                Mock.Of<ILogger<OAuthValidationMiddleware>>()
            );

            await middleware.InvokeAsync(ctx);
            ctx.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }

        [Fact]
        public async Task SkipsValidation_ForRootPath()
        {
            var ctx = MakeContext("/");
            var calledNext = false;
            var middleware = new OAuthValidationMiddleware(
                _ => { calledNext = true; return Task.CompletedTask; },
                MakeFactory(_ => new HttpResponseMessage(HttpStatusCode.OK)),
                new ConfigurationBuilder().Build(),
                Mock.Of<ILogger<OAuthValidationMiddleware>>()
            );

            await middleware.InvokeAsync(ctx);
            calledNext.Should().BeTrue();
        }

        [Fact]
        public async Task ExtractsTokenFromJsonAuthorizationHeader()
        {
            var ctx = MakeContext("/api/v1/classes", "{\"access_token\":\"abc.def.ghi\"}");
            var factory = MakeFactory(req =>
            {
                req.Headers.Authorization.Should().NotBeNull();
                req.Headers.Authorization!.Scheme.Should().Be("Bearer");
                req.Headers.Authorization.Parameter.Should().Be("abc.def.ghi");
                return new HttpResponseMessage(HttpStatusCode.OK);
            });
            var cfg = new ConfigurationBuilder().AddInMemoryCollection(new[]
            {
                new KeyValuePair<string,string>("OAUTH_INTERNAL_PROTOCOL","http"),
                new KeyValuePair<string,string>("OAUTH_INTERNAL_HOST","oauth"),
                new KeyValuePair<string,string>("OAUTH_INTERNAL_API_PORT","8080")
            }).Build();

            var calledNext = false;
            var middleware = new OAuthValidationMiddleware(
                _ => { calledNext = true; return Task.CompletedTask; },
                factory,
                cfg,
                Mock.Of<ILogger<OAuthValidationMiddleware>>()
            );

            await middleware.InvokeAsync(ctx);
            calledNext.Should().BeTrue();
        }

        [Fact]
        public async Task ForJsonHeaderWithoutAccessToken_FallsBackAndProceeds()
        {
            var ctx = MakeContext("/api/v1/classes", "{\"expires_in\":3600}");
            var calledNext = false;
            var middleware = new OAuthValidationMiddleware(
                _ => { calledNext = true; return Task.CompletedTask; },
                MakeFactory(_ => new HttpResponseMessage(HttpStatusCode.OK)),
                new ConfigurationBuilder().Build(),
                Mock.Of<ILogger<OAuthValidationMiddleware>>()
            );

            await middleware.InvokeAsync(ctx);
            calledNext.Should().BeTrue();
        }

        [Fact]
        public async Task ExtractsTokenFromSpaceSeparatedParts()
        {
            var ctx = MakeContext("/api/v1/classes", "foo abc.def.ghi bar");
            var calledNext = false;
            var middleware = new OAuthValidationMiddleware(
                _ => { calledNext = true; return Task.CompletedTask; },
                MakeFactory(_ => new HttpResponseMessage(HttpStatusCode.OK)),
                new ConfigurationBuilder().Build(),
                Mock.Of<ILogger<OAuthValidationMiddleware>>()
            );

            await middleware.InvokeAsync(ctx);
            calledNext.Should().BeTrue();
        }

        [Fact]
        public async Task Returns401_WhenTokenInvalid()
        {
            var ctx = MakeContext("/api/v1/classes", "Bearer invalid");
            var middleware = new OAuthValidationMiddleware(
                _ => Task.CompletedTask,
                MakeFactory(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)),
                new ConfigurationBuilder().Build(),
                Mock.Of<ILogger<OAuthValidationMiddleware>>()
            );

            await middleware.InvokeAsync(ctx);
            ctx.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }

        [Fact]
        public async Task Returns503_WhenAuthServiceUnavailable()
        {
            var ctx = MakeContext("/api/v1/classes", "Bearer token");
            var factory = MakeFactory(_ => throw new HttpRequestException("down"));
            var middleware = new OAuthValidationMiddleware(
                _ => Task.CompletedTask,
                factory,
                new ConfigurationBuilder().Build(),
                Mock.Of<ILogger<OAuthValidationMiddleware>>()
            );

            await middleware.InvokeAsync(ctx);
            ctx.Response.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
        }
    }
}
