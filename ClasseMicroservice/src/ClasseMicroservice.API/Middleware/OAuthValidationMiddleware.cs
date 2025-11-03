using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ClasseMicroservice.API.Middleware
{
    public class OAuthValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OAuthValidationMiddleware> _logger;

    // Paths to exclude from token validation
    // Note: do NOT include "/" here because StartsWith("/") would match everything.
    private readonly string[] _excludedPrefixes = new[] { "/swagger", "/api/v1/health", "/favicon", "/openapi" };

        public OAuthValidationMiddleware(RequestDelegate next, IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<OAuthValidationMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            // Exclude exact root ("/") and the configured prefixes
            if (path == "/" || string.IsNullOrEmpty(path) || _excludedPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader) || string.IsNullOrWhiteSpace(authHeader))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Missing Authorization header");
                return;
            }

            var oauthProtocol = _configuration["OAUTH_INTERNAL_PROTOCOL"] ?? "http";
            var oauthHost = _configuration["OAUTH_INTERNAL_HOST"] ?? "oauth";
            var oauthPort = _configuration["OAUTH_INTERNAL_API_PORT"] ?? "8080";
            var validateUrl = $"{oauthProtocol}://{oauthHost}:{oauthPort}/validate";

            var client = _httpClientFactory.CreateClient();

            var req = new HttpRequestMessage(HttpMethod.Post, validateUrl);

            // Forward the Authorization header (Bearer ...) but be tolerant to
            // malformed values. Common mistakes observed: the whole JSON
            // response (with access_token, expires_in, ...) was sent instead
            // of just the token. We try to extract a usable token in several
            // ways and then add the header using TryAddWithoutValidation to
            // avoid FormatException.

            string headerVal = ((string)authHeader).Trim();
            string tokenToSend = null;

            try
            {
                // If value looks like JSON, try to parse and extract access_token
                if (headerVal.StartsWith("{") || headerVal.Contains("\"access_token\""))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(headerVal);
                        if (doc.RootElement.TryGetProperty("access_token", out var at))
                        {
                            tokenToSend = at.GetString();
                        }
                    }
                    catch (JsonException)
                    {
                        // not valid JSON; we'll fall back to other heuristics below
                    }
                }

                // If still null, handle Bearer prefix or comma-separated values
                if (string.IsNullOrEmpty(tokenToSend))
                {
                    // If header contains 'Bearer ', extract what's after it
                    if (headerVal.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        tokenToSend = headerVal.Substring(7).Trim();
                    }
                    else
                    {
                        // If header contains multiple comma/space separated parts,
                        // try to pick the first plausible JWT (has two dots)
                        var parts = headerVal.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        tokenToSend = parts.FirstOrDefault(p => p.Count(c => c == '.') >= 2) ?? parts.FirstOrDefault();
                    }
                }

                if (string.IsNullOrEmpty(tokenToSend))
                {
                    _logger?.LogWarning("Could not extract token from Authorization header: {Header}", headerVal);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Invalid Authorization header");
                    return;
                }

                // Add header without strict validation to avoid FormatException
                req.Headers.TryAddWithoutValidation("Authorization", $"Bearer {tokenToSend}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error parsing Authorization header: {Header}", authHeader.ToString());
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Malformed Authorization header");
                return;
            }

            try
            {
                var resp = await client.SendAsync(req);
                if (resp.IsSuccessStatusCode)
                {
                    // Optionally we can read claims from the response and attach to context.User
                    await _next(context);
                    return;
                }
                else
                {
                    _logger?.LogWarning("Token validation failed with status {Status} for request {Path}", resp.StatusCode, path);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Invalid or expired token");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error while validating token against oauth gateway at {Url}", validateUrl);
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsync("Authentication service unavailable");
                return;
            }
        }
    }
}