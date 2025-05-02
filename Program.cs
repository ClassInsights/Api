using System.Text;
using System.Threading.RateLimiting;
using Api.Extensions;
using Api.Models.Database;
using Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables("CI_");

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));

// Add the clock instance
builder.Services.AddSingleton<IClock>(SystemClock.Instance);

// Add database connection
builder.Services.AddDbContext<ClassInsightsContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("npgsql"), o => o.UseNodaTime());
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
            PermitLimit = 60,
            QueueLimit = 0,
            Window = TimeSpan.FromMinutes(3)
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
        options.Title = "ClassInsights API";
        options.Theme = ScalarTheme.Kepler;
        options.Authentication = new ScalarAuthenticationOptions
        {
            PreferredSecurityScheme = IdentityConstants.BearerScheme
        };
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