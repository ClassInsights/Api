using System.Threading.RateLimiting;
using Api;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerConfiguration();

// register own services
builder.Services.AddDbConfiguration(builder.Configuration);

// register authentications
var authentication = builder.Services.AddAuthentication(c =>
{
    c.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    c.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    c.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
});

authentication.AddJwtAuthentication(builder.Configuration);
authentication.AddWinAuthentication();

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
app.UseSwagger(c => { c.RouteTemplate = "docs/{documentName}/docs.json"; });

app.UseRateLimiter();

app.UseSwaggerUI(c =>
{
    c.DocumentTitle = "API Documentation - ClassInsights";
    c.RoutePrefix = "docs";
    c.SwaggerEndpoint("/docs/v1/docs.json", "API v1");
    c.InjectStylesheet("/swagger-ui/SwaggerDark.css");
});

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.UseWebSockets();

app.MapControllers().RequireAuthorization().RequireRateLimiting("defaultUserRateLimit");

app.Run();