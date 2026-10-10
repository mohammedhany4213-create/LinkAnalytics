using LinkAnalytics.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("Default")!);

var app = builder.Build();

app.MapGet("/", () => "LinkAnalytics API is running");

app.Run();