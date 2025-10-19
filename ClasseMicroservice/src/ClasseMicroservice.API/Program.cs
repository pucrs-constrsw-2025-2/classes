using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using ClasseMicroservice.API;
using ClasseMicroservice.Domain.Interfaces;
using ClasseMicroservice.Infrastructure.Repositories;
using ClasseMicroservice.Application.Queries;
using ClasseMicroservice.Application.Commands;
using ClasseMicroservice.Application.Queries; // IQueryHandler, GetClassesQuery, GetClassByIdQuery
using ClasseMicroservice.Application.Commands; // ICommandHandler, Create/Update/Delete commands
using ClasseMicroservice.Domain.Entities;
using ClasseMicroservice.Application.Queries; 
// handlers estão nos namespaces de Queries e Commands

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ClasseMicroservice API", Version = "v1" });
});

// Registrar um handler de autenticação permissivo (apenas para desenvolvimento local)
builder.Services.AddAuthentication("NoAuth").AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, ClasseMicroservice.API.Authentication.AllowAnonymousAuthenticationHandler>("NoAuth", options => { });

// Injeção de dependência para CQRS
// Registrar implementação de repositório da camada Infrastructure (IClassRepository)
// Se estiver em desenvolvimento ou UseInMemoryRepository = true, usar repositório em memória para evitar dependência de MongoDB local
// Registrar ClassRepository (usa MongoDB)
builder.Services.AddSingleton<IClassRepository, ClassRepository>();

// Registrar handlers da camada Application
// Registrar handlers da camada Application
builder.Services.AddTransient<IQueryHandler<GetClassByIdQuery, Class>, GetClassByIdQueryHandler>();
builder.Services.AddTransient<IQueryHandler<GetClassesQuery, System.Collections.Generic.List<Class>>, GetClassesQueryHandler>();
builder.Services.AddTransient<ICommandHandler<CreateClassCommand>, CreateClassCommandHandler>();
builder.Services.AddTransient<ICommandHandler<UpdateClassCommand>, UpdateClassCommandHandler>();
builder.Services.AddTransient<ICommandHandler<DeleteClassCommand>, DeleteClassCommandHandler>();

// Configure MongoDB connection
// Bind DatabaseSettings from configuration
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("DatabaseSettings"));

var app = builder.Build();

// Configure the HTTP request pipeline.
var enableSwagger = builder.Configuration.GetValue<bool>("EnableSwagger");
if (app.Environment.IsDevelopment() || enableSwagger)
{
    app.UseDeveloperExceptionPage();
    if (enableSwagger)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ClasseMicroservice API v1"));
    }
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();