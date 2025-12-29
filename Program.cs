using System.Text;
using System.Threading.RateLimiting;
using Api.Extensions;
using Api.Handlers;
using Api.Models.Database;
using Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using Npgsql;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables("CI_");

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));

// Add the clock instance
builder.Services.AddSingleton<IClock>(SystemClock.Instance);

// Add database connection
var connectionStringBuilder = new NpgsqlConnectionStringBuilder(builder.Configuration.GetConnectionString("npgsql"))
    {
        // https://www.npgsql.org/doc/security.html#gss-session-encryption-gss-api
        GssEncryptionMode = GssEncryptionMode.Disable
    };
builder.Services.AddDbContext<ClassInsightsContext>(options =>
{
    options.UseNpgsql(connectionStringBuilder.ToString(), o => o.UseNodaTime());
});

// Add Settings Service
builder.Services.AddSingleton<SettingsService>();

builder.Services.AddHttpClient();

// add authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateAudience = false,
        ValidIssuer = "ClassInsights",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtKey"]!))
    };
});

// generate lowercase URLs
builder.Services.Configure<RouteOptions>(options => { options.LowercaseUrls = true; });

builder.Services.AddAuthorization();

// let owner bypass all auth roles
builder.Services.AddSingleton<IAuthorizationHandler, OwnerBypassAuthorizationHandler>();

if (builder.Environment.IsProduction())
{
    // Update Untis Data regularly
    builder.Services.AddSingleton<UntisService>();
    builder.Services.AddHostedService<UntisService>(provider => provider.GetRequiredService<UntisService>());
}

// Add identity Service
builder.Services.AddHostedService<IdentityService>();

// Add OpenAPI
builder.Services.AddOpenApi(options => options.AddBearerTokenAuthentication());

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin();
        policy.AllowAnyMethod();
        policy.AllowAnyHeader();
    });
});

// Configure Rate Limits
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("defaultUserRateLimit", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Request.Headers.Authorization.FirstOrDefault() ?? context.Request.Headers.Host.ToString(),
        _ => new FixedWindowRateLimiterOptions
        {
            AutoReplenishment = true,
            PermitLimit = 120,
            QueueLimit = 0,
            Window = TimeSpan.FromMinutes(1)
        }));
});

var app = builder.Build();

// Apply migrations at startup
if (app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ClassInsightsContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline and Swagger
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("ClassInsights API")
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
            .WithTheme(ScalarTheme.Kepler)
            .WithFavicon("https://classinsights.at/favicon.ico")
            .AddPreferredSecuritySchemes(IdentityConstants.BearerScheme);
    });
}

app.UseRateLimiter();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.UseWebSockets();

var controllers = app.MapControllers();
if (!app.Environment.IsDevelopment()) controllers.RequireAuthorization().RequireRateLimiting("defaultUserRateLimit");

app.Run();