using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using Api;
using Api.Models.Database;
using Api.Services;
using Api.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson();

// Add database connection
builder.Services.AddDbContext<ClassInsightsContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("npgsql"), o => o.UseNodaTime());
});

var settingsService = new SettingsService();

// Add Settings Service
builder.Services.AddSingleton(settingsService);

// Read or generate jwt key
var jwtKey = settingsService.GetSettingAsync("JwtKey").GetAwaiter().GetResult();
if (string.IsNullOrWhiteSpace(jwtKey))
{
    jwtKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(128));
    settingsService.SetSettingAsync("JwtKey", jwtKey).GetAwaiter().GetResult();
}

// add authentication
builder.Services.AddAuthentication(c =>
{
    c.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    c.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    c.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidIssuer = "ClassInsights",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// generate lowercase URLs
builder.Services.Configure<RouteOptions>(options => { options.LowercaseUrls = true; });

builder.Services.AddAuthorization();

// Auto Mapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

if (builder.Environment.IsProduction())
{
    // Update Untis Data regularly
    builder.Services.AddSingleton<UntisService>();
    builder.Services.AddHostedService<UntisService>(provider => provider.GetRequiredService<UntisService>());
}

// configure swagger
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        Description = "Please enter a valid token",
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Name = "Bearer",
                In = ParameterLocation.Header,
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

// Configure ssl
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ConfigureHttpsDefaults(options =>
    {
        options.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
    });

    /*serverOptions.ListenAnyIP(7061, listenOptions =>
    {// todo: create ssl cert depending on domain
        if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "ssl.pfx")))
            CertificateUtils.SaveCertificate(CertificateUtils.CreateClientCertificate("ClassInsights"), "ssl.pfx");

        listenOptions.UseHttps("ssl.pfx");
    });*/
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
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = "docs";
    });
}

app.UseRateLimiter();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.UseWebSockets();

app.MapControllers().RequireAuthorization().RequireRateLimiting("defaultUserRateLimit");

app.Run();