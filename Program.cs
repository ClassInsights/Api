using Api;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Https;

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


// Auto Mapper Configurations
var mapperConfig = new MapperConfiguration(mc => { mc.AddProfile(new MappingProfile()); });

builder.Services.AddSingleton(mapperConfig.CreateMapper());

// Enable Memory Cache
builder.Services.AddMemoryCache();

// Allow client certs
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ConfigureHttpsDefaults(options =>
    {
        options.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline and Swagger
app.UseSwagger(c => { c.RouteTemplate = "docs/{documentName}/docs.json"; });

app.UseSwaggerUI(c =>
{
    c.DocumentTitle = "API Documentation - ClassInsights";
    c.RoutePrefix = "docs";
    c.SwaggerEndpoint("/docs/v1/docs.json", "API v1");
    c.InjectStylesheet("/swagger-ui/SwaggerDark.css");
});

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseWebSockets();

app.MapControllers().RequireAuthorization();

app.Run();