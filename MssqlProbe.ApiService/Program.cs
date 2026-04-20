using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHealthChecks()
    .AddSqlServer(
        "Server=localhost,1433;Database=master;User Id=sa;Password=Passw0rd;TrustServerCertificate=True;",
        name: "external-sql",
        failureStatus: HealthStatus.Unhealthy,
        timeout: TimeSpan.FromSeconds(5));

var app = builder.Build();

app.MapDefaultEndpoints();

app.Run();