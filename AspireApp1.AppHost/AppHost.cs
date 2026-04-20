var builder = DistributedApplication.CreateBuilder(args);

var sqlProbe = builder
    .AddProject<Projects.MssqlProbe_ApiService>("sql-probe")
    .WithHttpHealthCheck("/health")
    .WithEnvironment(
        "ConnectionStrings__SqlServer",
        "Server=localhost,1433;Database=AdventureWorks2025;User Id=sa;Password=Passw0rd;TrustServerCertificate=True;");

var apiService = builder
    .AddProject<Projects.AspireApp1_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(sqlProbe);

builder.AddProject<Projects.AspireApp1_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();