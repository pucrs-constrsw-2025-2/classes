using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using ClasseMicroservice.API;
using ClasseMicroservice.API.Middleware;
using ClasseMicroservice.Domain.Entities;
using ClasseMicroservice.Domain.Interfaces;

using ClasseMicroservice.Application.Queries;   // IQueryHandler, GetClassesQuery, GetClassByIdQuery
using ClasseMicroservice.Application.Commands; // ICommandHandler, Create/Update/Delete commands

using ClasseMicroservice.Infrastructure.Repositories;
using Swashbuckle.AspNetCore.Annotations;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ClasseMicroservice API",
        Version = "v1"
    });

    // Habilita [SwaggerOperation(...)]
    c.EnableAnnotations();

    // OperationId previsível (Controller_HttpMethod_RouteTemplate)
    c.CustomOperationIds(apiDesc =>
    {
        var controller = apiDesc.ActionDescriptor.RouteValues["controller"];
        var method = apiDesc.HttpMethod;
        var relativePath = apiDesc.RelativePath?
            .Replace("/", "_")
            .Replace("{", "")
            .Replace("}", "")
            .Replace("?", "")
            .Replace("&", "_");

        return $"{controller}_{method}_{relativePath}";
    });

    // Evita colisão de nomes de schema
    c.CustomSchemaIds(t => t.FullName);

    // (Opcional) XML comments — ative no .csproj: <GenerateDocumentationFile>true</GenerateDocumentationFile>
    var xmlName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlName);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    // Add JWT Bearer Authorization to Swagger so the UI shows the Authorize button
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Auth “NoAuth” como padrão (para dev)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "NoAuth";
    options.DefaultChallengeScheme = "NoAuth";
})
.AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions,
            ClasseMicroservice.API.Authentication.AllowAnonymousAuthenticationHandler>("NoAuth", _ => { });

// DI: Repository
builder.Services.AddSingleton<IClassRepository, ClassRepository>();

// DI: Handlers (CQRS)
builder.Services.AddTransient<IQueryHandler<GetClassByIdQuery, Class>, GetClassByIdQueryHandler>();
builder.Services.AddTransient<IQueryHandler<GetClassesQuery, System.Collections.Generic.List<Class>>, GetClassesQueryHandler>();
builder.Services.AddTransient<ICommandHandler<CreateClassCommand>, CreateClassCommandHandler>();
builder.Services.AddTransient<ICommandHandler<UpdateClassCommand>, UpdateClassCommandHandler>();
builder.Services.AddTransient<ICommandHandler<DeleteClassCommand>, DeleteClassCommandHandler>();

// Settings (Mongo)
// Monta a connection string a partir das variáveis de ambiente expostas pelo container
// (não criamos novas variáveis; usamos apenas MONGODB_HOST, MONGODB_PORT,
// MONGODB_USERNAME, MONGODB_PASSWORD, MONGODB_DATABASE ou a ConnectionStrings:MongoDb se já existir)
var existingConn = builder.Configuration.GetValue<string>("ConnectionStrings:MongoDb");

// If explicit MONGODB_* environment variables are provided, prefer them over the
// default value in appsettings.json (which may be localhost). This avoids the
// service trying to connect to localhost when running inside Docker where the
// real mongo host is a different container.
var envHost = builder.Configuration["MONGODB_HOST"] ?? builder.Configuration["MONGODB_INTERNAL_HOST"];
if (!string.IsNullOrEmpty(envHost))
{
    var host = envHost;
    var port = builder.Configuration["MONGODB_PORT"] ?? builder.Configuration["MONGODB_INTERNAL_PORT"] ?? "27017";
    var user = builder.Configuration["MONGODB_USERNAME"];
    var pass = builder.Configuration["MONGODB_PASSWORD"];
    var db = builder.Configuration["MONGODB_DATABASE"] ?? builder.Configuration["MONGODB_DB"] ?? "classes";

    string conn;
    if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass))
        conn = $"mongodb://{Uri.EscapeDataString(user)}:{Uri.EscapeDataString(pass)}@{host}:{port}/{db}";
    else
        conn = $"mongodb://{host}:{port}/{db}";

    var inMemory = new Dictionary<string, string>
    {
        // Override ConnectionStrings:MongoDb so MongoClient uses the container host
        ["ConnectionStrings:MongoDb"] = conn,
        ["DatabaseSettings:ConnectionString"] = conn,
        ["DatabaseSettings:DatabaseName"] = db,
        ["MongoDbSettings:DatabaseName"] = db,
        ["MongoDbSettings:CollectionName"] = db
    };
    builder.Configuration.AddInMemoryCollection(inMemory);
}
else if (string.IsNullOrEmpty(existingConn))
{
    // No env and no existing connection string: fallback to localhost default
    var host = "mongodb";
    var port = builder.Configuration["MONGODB_PORT"] ?? builder.Configuration["MONGODB_INTERNAL_PORT"] ?? "27017";
    var db = builder.Configuration["MONGODB_DATABASE"] ?? builder.Configuration["MONGODB_DB"] ?? "classes";
    var conn = $"mongodb://{host}:{port}/{db}";

    var inMemory = new Dictionary<string, string>
    {
        ["ConnectionStrings:MongoDb"] = conn,
        ["DatabaseSettings:ConnectionString"] = conn,
        ["DatabaseSettings:DatabaseName"] = db,
        ["MongoDbSettings:DatabaseName"] = db,
        ["MongoDbSettings:CollectionName"] = db
    };
    builder.Configuration.AddInMemoryCollection(inMemory);
}
else
{
    // existingConn is present (from appsettings.json or other providers) and
    // no explicit MONGODB_* env vars were provided: keep existingConn but set
    // DatabaseSettings fallbacks.
    var db = builder.Configuration["DatabaseSettings:DatabaseName"] ?? builder.Configuration["MONGODB_DATABASE"] ?? builder.Configuration["MONGODB_DB"] ?? "classes";
    var inMemory = new Dictionary<string, string>
    {
        ["DatabaseSettings:ConnectionString"] = existingConn,
        ["DatabaseSettings:DatabaseName"] = db,
        ["MongoDbSettings:DatabaseName"] = db,
        ["MongoDbSettings:CollectionName"] = db
    };
    builder.Configuration.AddInMemoryCollection(inMemory);
}

builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("DatabaseSettings"));

// Register HttpClient factory so middleware can call the oauth gateway
builder.Services.AddHttpClient();

// Configure Health Checks (equivalente ao Spring Boot Actuator)
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDb") ?? 
    builder.Configuration["ConnectionStrings:MongoDb"] ?? "";
if (!string.IsNullOrEmpty(mongoConnectionString))
{
    builder.Services.AddHealthChecks()
        .AddMongoDb(
            mongoConnectionString,
            name: "mongodb",
            tags: new[] { "db", "nosql", "mongodb" }
        );
}
else
{
    // Fallback: health check simples se não houver connection string
    builder.Services.AddHealthChecks();
}

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? "classes", serviceVersion: "1.0.0"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddPrometheusExporter());

var app = builder.Build();

// Expose Prometheus metrics
app.UseOpenTelemetryPrometheusScrapingEndpoint();

// Pipeline
// Lê EnableSwagger da configuração ou da variável de ambiente PROVIDA (ENABLE_SWAGGER)
// Isso garante compatibilidade com o docker-compose que expõe ENABLE_SWAGGER
bool enableSwagger = false;
{
    // Primeiro tenta pela configuração normal (appsettings ou providers já carregados)
    var cfgVal = builder.Configuration.GetValue<bool?>("EnableSwagger");
    if (cfgVal.HasValue)
    {
        enableSwagger = cfgVal.Value;
    }
    else
    {
        // Tenta também pela variável de ambiente em maiúsculas com underscore
        var envVal = Environment.GetEnvironmentVariable("ENABLE_SWAGGER") ?? builder.Configuration["ENABLE_SWAGGER"];
        if (!string.IsNullOrEmpty(envVal) && bool.TryParse(envVal, out var parsed))
        {
            enableSwagger = parsed;
        }
    }
}

if (app.Environment.IsDevelopment() || enableSwagger)
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ClasseMicroservice API v1");
        c.DocumentTitle = "ClasseMicroservice API";
        // Opcional: exibir sem precisar clicar no "v1"
        c.RoutePrefix = "swagger";
    });
}

// Register OAuth token validation middleware - it will call the gateway (`oauth` service)
app.UseMiddleware<ClasseMicroservice.API.Middleware.OAuthValidationMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint padronizado (formato compatível com Actuator)
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status == HealthStatus.Healthy ? "UP" : "DOWN",
            components = report.Entries.ToDictionary(
                e => e.Key,
                e => new
                {
                    status = e.Value.Status == HealthStatus.Healthy ? "UP" : "DOWN",
                    details = e.Value.Description
                }
            )
        });
        await context.Response.WriteAsync(result);
    }
});

app.Run();