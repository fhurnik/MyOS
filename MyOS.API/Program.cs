using MyOS.Core.Infrastructure.Extensions;
using MyOS.Core.Infrastructure.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((_, loggerConfiguration) =>
{
    loggerConfiguration.ConfigureSerilog();
});

builder.Services.AddCore(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation(
"MyOS API starting in {Environment} environment at {TimeUtc} UTC",
app.Environment.EnvironmentName,
DateTime.UtcNow);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

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