var builder = DistributedApplication.CreateBuilder(args);

builder.AddDotnetProject("api", "../JobRadar.Api/JobRadar.Api.csproj")
    .WithHttpHealthCheck("/health");

builder.Build().Run();
