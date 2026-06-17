using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using MyOS.API.Middlewares;
using MyOS.Core.Infrastructure.Extensions;
using MyOS.Core.Infrastructure.Logging;
using MyOS.Identity.Infrastructure;
using Microsoft.AspNetCore.Http.Features;
using MyOS.Modules.Notes.Infrastructure;
using MyOS.Modules.Storage.Infrastructure;
using Serilog;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((_, loggerConfiguration) =>
{
    loggerConfiguration.ConfigureSerilog();
});

builder.Services.AddCore(builder.Configuration);
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddNotesModule(builder.Configuration);
builder.Services.AddStorageModule(builder.Configuration);

// Upload limits — max size of a single uploaded file (storage module).
const long maxUploadBytes = 1L * 1024 * 1024 * 1024; // 1 GB
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = maxUploadBytes);
builder.Services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = maxUploadBytes);

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("api-version"));
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddControllers();

var swaggerModules = typeof(Program).Assembly.GetTypes()
    .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ControllerBase)))
    .Select(t => t.Namespace?.Split('.').LastOrDefault())
    .Where(m => !string.IsNullOrEmpty(m) && m != "Controllers")
    .Distinct()
    .OrderBy(m => m)
    .ToList();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    foreach (var module in swaggerModules)
        options.SwaggerDoc(module!.ToLower(), new OpenApiInfo { Title = $"MyOS {module}", Version = "v1" });

    options.DocInclusionPredicate((docName, api) =>
    {
        if (api.ActionDescriptor is ControllerActionDescriptor descriptor)
        {
            var module = descriptor.ControllerTypeInfo.Namespace?.Split('.').LastOrDefault() ?? string.Empty;
            return docName.Equals(module.ToLower(), StringComparison.Ordinal);
        }
        return false;
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT access token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
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

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation(
    "MyOS API starting in {Environment} environment at {TimeUtc} UTC",
    app.Environment.EnvironmentName,
    DateTime.UtcNow);

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    foreach (var module in swaggerModules)
        options.SwaggerEndpoint($"/swagger/{module!.ToLower()}/swagger.json", $"MyOS {module}");
    options.RoutePrefix = "swagger";
});

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<LanguageCultureMiddleware>();

app.MapControllers();

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "MyOS API failed to start");
}
finally
{
    Log.CloseAndFlush();
}
