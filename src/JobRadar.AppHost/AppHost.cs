var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithContainerName("jobradar-postgres")        // readable Docker name instead of a hash suffix
    .WithDataVolume("jobradar-postgres-data")      // data survives restarts
    .WithLifetime(ContainerLifetime.Persistent)    // container keeps running between AppHost runs
    .WithHostPort(17300);                          // stable port for a DB client
var database = postgres.AddDatabase("jobradar");

builder.AddDotnetProject("api", "../JobRadar.Api/JobRadar.Api.csproj")
    .WithReference(database)                       // passes the connection string
    .WaitFor(database)                             // Api starts once the DB is ready
    .WithHttpHealthCheck("/health");

builder.Build().Run();
