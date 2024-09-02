using System.Threading.RateLimiting;
using Api;
using Api.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models.ExternalConnectors;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerConfiguration();

// register own services
builder.Services.AddDbContext<ClassInsightsContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("npgsql"));
});

// register authentications
var authentication = builder.Services.AddAuthentication(c =>
{
    c.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    c.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    c.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
});

authentication.AddJwtAuthentication(builder.Configuration);

// generate lowercase URLs
builder.Services.Configure<RouteOptions>(options => { options.LowercaseUrls = true; });

builder.Services.AddAuthorization();

// Add Microsoft Graph
builder.Services.AddAuthentication()
    .AddMicrosoftIdentityWebApp(builder.Configuration)
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddMicrosoftGraph()
    .AddInMemoryTokenCaches();

// Auto Mapper Configurations
var mapperConfig = new MapperConfiguration(mc => { mc.AddProfile(new MappingProfile()); });

builder.Services.AddSingleton(mapperConfig.CreateMapper());

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        //policy.AllowAnyOrigin();
        policy.AllowAnyMethod();
        policy.AllowAnyHeader();
        policy.AllowCredentials();
        policy.WithOrigins("https://classinsights.t-la.lokal", "http://localhost:3000", "https://admin.projekt.lokal");
        //policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost");
    });
});

// Allow client certs
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ConfigureHttpsDefaults(options =>
    {
        options.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
    });
});

// Configure Rate Limits
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("defaultUserRateLimit", context => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: context.Request.Headers.Authorization.FirstOrDefault() ?? context.Request.Headers.Host.ToString(),
        factory: _ => new FixedWindowRateLimiterOptions
        {
            AutoReplenishment = true,
            PermitLimit = 60,
            QueueLimit = 0,
            Window = TimeSpan.FromMinutes(3)
        }));
});

var app = builder.Build();

// Configure the HTTP request pipeline and Swagger

app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = "docs";        
    });
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.UseWebSockets();

app.MapControllers().RequireAuthorization().RequireRateLimiting("defaultUserRateLimit");

app.Run();