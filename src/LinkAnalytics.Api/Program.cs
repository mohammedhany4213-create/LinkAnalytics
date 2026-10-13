using LinkAnalytics.Application;
using LinkAnalytics.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' was not found.");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddControllers();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { message = "LinkAnalytics API is running" }));
app.MapControllers();

app.Run();
