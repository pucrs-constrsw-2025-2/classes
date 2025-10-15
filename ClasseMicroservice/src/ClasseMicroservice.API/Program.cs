using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Injeção de dependência para CQRS
builder.Services.AddSingleton<ClasseMicroservice.API.Data.ClasseRepository>();
builder.Services.AddTransient<ClasseMicroservice.API.Queries.IQueryHandler<ClasseMicroservice.API.Queries.GetClasseByIdQuery, ClasseMicroservice.API.Models.Classe>, ClasseMicroservice.API.Queries.GetClasseByIdQueryHandler>();
builder.Services.AddTransient<ClasseMicroservice.API.Commands.ICommandHandler<ClasseMicroservice.API.Commands.CreateClasseCommand>, ClasseMicroservice.API.Commands.CreateClasseCommandHandler>();

// Configure MongoDB connection
builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("DatabaseSettings"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();