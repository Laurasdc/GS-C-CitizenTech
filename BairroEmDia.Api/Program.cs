using System.Threading.RateLimiting;
using BairroEmDia.Api.Authentication;
using BairroEmDia.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
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

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IPasswordHasher<ContaUsuario>, PasswordHasher<ContaUsuario>>();
builder.Services.AddScoped<IAutenticacaoRepository, AutenticacaoRepository>();
builder.Services.AddScoped<AutenticacaoService>();
builder.Services.AddAuthentication(SessaoAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, SessaoAuthenticationHandler>(SessaoAuthenticationHandler.SchemeName, _ => { });
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options => {
    options.RejectionStatusCode = 429;
    options.AddPolicy("autenticacao", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "local",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});


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

var origens = builder.Configuration.GetSection("Frontend:Origins").Get<string[]>() ?? [];
if (builder.Environment.IsDevelopment()) origens = ["http://localhost:8081", "http://127.0.0.1:8081", ..origens];
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.WithOrigins(origens).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors("Frontend");

if (app.Environment.IsDevelopment())
{

    app.MapOpenApi();

    app.MapScalarApiReference(options =>
        options.WithTitle("Bairro em Dia — API"));
}
else
{
    app.UseHttpsRedirection();
}

// Requisições do navegador que alteram dados devem vir do frontend autorizado.
app.Use(async (context, next) => {
    if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method) && !HttpMethods.IsOptions(context.Request.Method))
    {
        var origin = context.Request.Headers.Origin.ToString();
        var usaCookie = context.Request.Cookies.ContainsKey(SessaoAuthenticationHandler.CookieName)
            && !context.Request.Headers.ContainsKey("Authorization");
        if ((origin.Length > 0 && !origens.Contains(origin, StringComparer.OrdinalIgnoreCase)) || (usaCookie && origin.Length == 0))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { title = "Origem da solicitação não permitida.", status = 403 });
            return;
        }
    }
    await next();
});
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
public partial class Program { }
