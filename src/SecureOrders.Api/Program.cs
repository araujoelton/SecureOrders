using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Text.Json;
using SecureOrders.Api.Swagger;
using SecureOrders.Application.Auth;
using SecureOrders.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe: Bearer {seu_token}"
    });

    c.AddSecurityRequirement((document) =>
    {
        var schemeRef = new OpenApiSecuritySchemeReference("Bearer", document, "Bearer");
        return new OpenApiSecurityRequirement
        {
            { schemeRef, new List<string>() }
        };
    });

    c.OperationFilter<AllowAnonymousOperationFilter>();
});

var authOptions = builder.Configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>()
    ?? throw new InvalidOperationException("Auth section not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authOptions.Jwt.Issuer,

            ValidateAudience = true,
            ValidAudience = authOptions.Jwt.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.Jwt.SigningKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddInfrastructure(builder.Configuration);

var healthChecksBuilder = builder.Services.AddHealthChecks();
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
var redisConn = builder.Configuration["Redis:ConnectionString"];

if (!string.IsNullOrWhiteSpace(conn))
    healthChecksBuilder.AddNpgSql(conn, name: "postgres", tags: new[] { "ready" });

if (!string.IsNullOrWhiteSpace(redisConn))
    healthChecksBuilder.AddRedis(redisConn, name: "redis", tags: new[] { "ready" });

healthChecksBuilder.AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Se estiver rodando somente em HTTP no Docker e der redirect/erro, remova ou condicione isso.
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
async Task WriteHealthResponse(HttpContext context, HealthReport report)
{
    var result = new
    {
        status = report.Status.ToString(),
        checks = report.Entries.Select(e => new
        {
            name = e.Key,
            status = e.Value.Status.ToString(),
            description = e.Value.Description
        })
    };

    context.Response.ContentType = "application/json";
    await context.Response.WriteAsJsonAsync(result, jsonOptions);
}

app.MapControllers();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live"),
    ResponseWriter = WriteHealthResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready"),
    ResponseWriter = WriteHealthResponse
});

app.Run();
