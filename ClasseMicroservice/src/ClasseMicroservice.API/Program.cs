using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using ClasseMicroservice.API;
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
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("DatabaseSettings"));

var app = builder.Build();

// Pipeline
var enableSwagger = builder.Configuration.GetValue<bool>("EnableSwagger");

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

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();