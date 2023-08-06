using Microsoft.EntityFrameworkCore;
using Api.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});
builder.Services.AddDbContext<ClassInsightsContext>(opt =>
{
    //opt.EnableSensitiveDataLogging();
    opt.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});

var app = builder.Build();

// Configure the HTTP request pipeline and Swagger
app.UseSwagger(c =>
{
    c.RouteTemplate = "docs/{documentName}/docs.json";
});

app.UseSwaggerUI(c =>
{
    c.DocumentTitle = "API Documentation - ClassInsights";
    c.RoutePrefix = "docs";
    c.SwaggerEndpoint("/docs/v1/docs.json", "API v1");
    c.InjectStylesheet("/swagger-ui/SwaggerDark.css");
});

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseWebSockets();

app.MapControllers();

app.Run();
