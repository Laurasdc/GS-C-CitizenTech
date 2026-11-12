using System.Text.Json.Serialization;
using BairroEmDia.Api.Exceptions;
using BairroEmDia.Api.Services;
using BairroEmDia.Domain.Interfaces;
using BairroEmDia.Infrastructure.Data;
using BairroEmDia.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "A conexão DefaultConnection não foi configurada.");

builder.Services.AddDbContext<BairroEmDiaDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IOcorrenciaRepository, OcorrenciaRepository>();

builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IOcorrenciaService, OcorrenciaService>();
builder.Services.AddScoped<IIndicadoresService, IndicadoresService>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("ExpoWebLocal", policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:8081",
                    "http://127.0.0.1:8081")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });
}

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseCors("ExpoWebLocal");

    app.MapOpenApi();

    app.MapScalarApiReference(options =>
        options.WithTitle("Bairro em Dia — API"));
}
else
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.Run();