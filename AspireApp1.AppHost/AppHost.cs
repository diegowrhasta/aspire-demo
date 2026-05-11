var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("env");

// Add RabbitMQ container
var rabbit = builder
    .AddRabbitMQ(
        "rabbitmq",
        userName: builder.AddParameter("username", "admin", secret: true),
        password: builder.AddParameter("password", "123456", secret: true))
    .WithManagementPlugin();

var seq = builder.AddSeq("seq")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithLifetime(ContainerLifetime.Persistent);

var sqlProbe = builder
    .AddProject<Projects.MssqlProbe_ApiService>("sql-probe")
    .WithHttpHealthCheck("/health")
    .WithEnvironment(
        "ConnectionStrings__SqlServer",
        "Server=localhost,1433;Database=AdventureWorks2025;User Id=sa;Password=Passw0rd;TrustServerCertificate=True;")
    .WithReference(seq)
    .WaitFor(seq);

var apiService = builder
    .AddProject<Projects.AspireApp1_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(sqlProbe)
    .WithReference(rabbit)
    .WithEnvironment(
        "ConnectionStrings__SqlServer",
        "Server=localhost,1433;Database=AdventureWorks2025;User Id=sa;Password=Passw0rd;TrustServerCertificate=True;")
    .WithReference(seq)
    .WaitFor(seq);

builder
    .AddProject<Projects.AspireApp1_RabbitConsumer>("rabbitConsumer")
    .WithHttpHealthCheck("/health")
    .WithReference(rabbit)
    .WithReference(seq)
    .WaitFor(seq);;

builder.AddProject<Projects.AspireApp1_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithReference(seq)
    .WaitFor(seq);;

// var tunnel = builder.AddDevTunnel("my-tunnel")
//     .WithAnonymousAccess()
//     .WithReference(web);

builder.Build().Run();